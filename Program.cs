// Usings necess�rios para o Entity Framework, Identity, Swagger e seus modelos.
using Decolei.net.Data;
using Decolei.net.Interfaces;
using Decolei.net.Models;
using Decolei.net.Repository;
using Decolei.net.Repository.Decolei.net.Repository;

// Usings adicionais para JWT
using Microsoft.AspNetCore.Authentication; // ESSENCIAL para AddAuthentication()
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

public class Program
{
    // O m�todo 'Main' agora � ass�ncrono para permitir await
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // --- CONFIGURA��O DOS SERVI�OS ---

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        // 1. REGISTRAR O DBCONTEXT
        builder.Services.AddDbContext<DecoleiDbContext>(options =>
            options.UseSqlServer(connectionString));

        builder.Services.AddScoped<Decolei.net.Interfaces.IPacoteRepository, PacoteRepository>();
        // 2. REGISTRAR E CONFIGURAR O ASP.NET IDENTITY
        builder.Services.AddIdentity<Usuario, IdentityRole<int>>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.SignIn.RequireConfirmedAccount = false;
            // Configura��es para UserName e Email (j� que Email ser� o login)
            options.User.RequireUniqueEmail = true; // Garante que o email seja �nico
            options.SignIn.RequireConfirmedEmail = false; // N�o exige confirma��o de email para login
        })
        .AddEntityFrameworkStores<DecoleiDbContext>();

        // 3. REGISTRAR SERVI�OS DE AUTENTICA��O E AUTORIZA��O (CONFIGURADO PARA JWT)
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                ValidAudience = builder.Configuration["JwtSettings:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]!))
            };
        });
        builder.Services.AddAuthorization();

        // 4. REGISTRAR CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        // 5. REGISTRAR CONTROLLERS E SERVI�OS DO SWAGGER
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Decolei.net API", Version = "v1" });
            // Adicionado para permitir autoriza��o JWT no Swagger UI
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });

        // --- FIM DA CONFIGURA��O DE SERVI�OS ---
        builder.Services.AddScoped<IReservaRepository, ReservaRepository>();

        var app = builder.Build();

        // --- Seeding Inicial do Admin (DEVE VIR ANTES DE app.Run()) ---
        using (var scope = app.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

            // Garante que a role "ADMIN" existe
            if (!await roleManager.RoleExistsAsync("ADMIN"))
            {
                await roleManager.CreateAsync(new IdentityRole<int>("ADMIN"));
            }
            // Garante que a role "CLIENTE" existe (pode ser criada no registro tbm)
            if (!await roleManager.RoleExistsAsync("CLIENTE"))
            {
                await roleManager.CreateAsync(new IdentityRole<int>("CLIENTE"));
            }
            // Garante que a role "ATENDENTE" existe
            if (!await roleManager.RoleExistsAsync("ATENDENTE"))
            {
                await roleManager.CreateAsync(new IdentityRole<int>("ATENDENTE"));
            }


            // Cria o primeiro usu�rio Admin se ele n�o existir
            var adminUser = await userManager.FindByEmailAsync("admin@decolei.net");
            if (adminUser == null)
            {
                adminUser = new Usuario
                {
                    UserName = "admin@decolei.net", // UserName ser� o email
                    Email = "admin@decolei.net",
                    Documento = "00000000000",
                    Perfil = "ADMIN", // Atribu�do ao perfil customizado
                    PhoneNumber = "999999999",
                    NomeCompleto = "Administrador Master" // Nome completo
                };
                var createResult = await userManager.CreateAsync(adminUser, "SenhaAdmin123!"); // Escolha uma senha segura!
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "ADMIN"); // Adiciona � role do Identity
                    Console.WriteLine("Usu�rio Admin inicial criado!");
                }
                else
                {
                    Console.WriteLine($"Erro ao criar usu�rio Admin: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }
            }
        }
        // --- Fim do Seeding ---


        // --- CONFIGURA��O DO PIPELINE DE REQUISI��O ---

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            // Esta configura��o garante que a UI do Swagger funcione corretamente.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Decolei.net API V1");
                // Opcional: Torna a UI do Swagger a p�gina inicial
                c.RoutePrefix = string.Empty;
            });
        }

        // app.UseHttpsRedirection(); // Mantenha comentado se estiver dando erro de porta HTTPS

        // permitindo que front-end, mesmo rodando em outra porta ou dom�nio, consiga fazer requisi��es para a API.
        app.UseCors("AllowAll"); // <<<<< CORS habilitado aqui

        // A ordem destes middlewares � fundamental
        app.UseRouting();
        app.UseAuthentication(); // Deve vir ANTES de UseAuthorization
        app.UseAuthorization();  // Deve vir DEPOIS de UseAuthentication


        app.MapControllers();

        await app.RunAsync(); // Use await aqui para o Main ass�ncrono
    }
}