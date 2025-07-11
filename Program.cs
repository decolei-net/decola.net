// Usings necess�rios para o Entity Framework, Identity, Swagger e seus modelos.
using Decolei.net.Data;
using Decolei.net.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

// No seu tipo de projeto, toda a l�gica de inicializa��o precisa
// estar dentro de uma classe, por conven��o chamada 'Program'.
public class Program
{
    // O m�todo 'Main' � o ponto de entrada da sua aplica��o.
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // --- CONFIGURA��O DOS SERVI�OS ---

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        // 1. REGISTRAR O DBCONTEXT
        builder.Services.AddDbContext<DecoleiDbContext>(options =>
            options.UseSqlServer(connectionString));

        builder.Services.AddScoped<Decolei.net.Interfaces.IPacoteRepository, Decolei.net.Repositories.PacoteRepository>();
        // 2. REGISTRAR E CONFIGURAR O ASP.NET IDENTITY
        builder.Services.AddIdentity<Usuario, IdentityRole<int>>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.SignIn.RequireConfirmedAccount = false;
        })
        .AddEntityFrameworkStores<DecoleiDbContext>();

        // 3. REGISTRAR SERVI�OS DE AUTENTICA��O E AUTORIZA��O
        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();

        // 4. REGISTRAR CONTROLLERS E SERVI�OS DO SWAGGER
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        // ESTA � A CONFIGURA��O CORRETA E COMPLETA DO SWAGGERGEN
        builder.Services.AddSwaggerGen(c =>
        {
            // Esta linha cria um "documento" de API e fornece as informa��es
            // essenciais que o Swagger precisa para renderizar a UI.
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Decolei.net API", Version = "v1" });
        });

        // --- FIM DA CONFIGURA��O DE SERVI�OS ---

        var app = builder.Build();

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

        // A ordem destes middlewares � fundamental
        app.UseRouting(); // Adicionado para garantir o roteamento correto
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
