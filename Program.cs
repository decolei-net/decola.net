// Usings necessários para o Entity Framework, Identity, Swagger e seus modelos.
using Decolei.net.Data;
using Decolei.net.Interfaces;
using Decolei.net.Models;
using Decolei.net.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

// ===================================================================
// INÍCIO DA ALTERAÇÃO ESTRUTURAL PARA SUPORTE A TESTES
// ===================================================================
// Tornamos a classe Program pública e parcial.
// Isso permite que o projeto de teste se "conecte" a ela antes da inicialização.
public partial class Program
{
    // O método 'Main' agora é assíncrono para permitir await
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // --- CONFIGURAÇÃO DOS SERVIÇOS ---

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        // 1. REGISTRAR O DBCONTEXT
        if (!builder.Environment.IsEnvironment("Testing"))
        {
            builder.Services.AddDbContext<DecoleiDbContext>(options =>
                options.UseSqlServer(connectionString));
        }


        // Adiciona o repositório ao contêiner de injeção de dependência
        builder.Services.AddScoped<IPacoteRepository, PacoteRepository>();

        // 2. REGISTRAR E CONFIGURAR O ASP.NET IDENTITY
        builder.Services.AddIdentity<Usuario, IdentityRole<int>>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.SignIn.RequireConfirmedAccount = false;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false;
        })
        .AddEntityFrameworkStores<DecoleiDbContext>();

        // 3. REGISTRAR SERVIÇOS DE AUTENTICAÇÃO E AUTORIZAÇÃO (CONFIGURADO PARA JWT)
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

        // 5. REGISTRAR CONTROLLERS E SERVIÇOS DO SWAGGER
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Decolei.net API", Version = "v1" });
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

        // --- FIM DA CONFIGURAÇÃO DE SERVIÇOS ---

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
            // Garante que a role "CLIENTE" existe
            if (!await roleManager.RoleExistsAsync("CLIENTE"))
            {
                await roleManager.CreateAsync(new IdentityRole<int>("CLIENTE"));
            }
            // Garante que a role "ATENDENTE" existe
            if (!await roleManager.RoleExistsAsync("ATENDENTE"))
            {
                await roleManager.CreateAsync(new IdentityRole<int>("ATENDENTE"));
            }

            // Cria o primeiro usuário Admin se ele não existir
            var adminUser = await userManager.FindByEmailAsync("admin@decolei.net");
            if (adminUser == null)
            {
                adminUser = new Usuario
                {
                    UserName = "admin@decolei.net",
                    Email = "admin@decolei.net",
                    Documento = "00000000000",
                    Perfil = "ADMIN",
                    PhoneNumber = "999999999",
                    NomeCompleto = "Administrador Master"
                };
                var createResult = await userManager.CreateAsync(adminUser, "SenhaAdmin123!");
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "ADMIN");
                    Console.WriteLine("Usuário Admin inicial criado!");
                }
                else
                {
                    Console.WriteLine($"Erro ao criar usuário Admin: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }
            }
        }
        // --- Fim do Seeding ---

        // --- CONFIGURAÇÃO DO PIPELINE DE REQUISIÇÃO ---

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Decolei.net API V1");
                c.RoutePrefix = string.Empty;
            });
        }

        app.UseCors("AllowAll");
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        await app.RunAsync();
    }
}
// ===================================================================
// FIM DA ALTERAÇÃO ESTRUTURAL
// ===================================================================
