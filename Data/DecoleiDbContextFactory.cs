using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using Decolei.net.Data; // Certifique-se que este 'using' está correto para sua estrutura

public class DecoleiDbContextFactory : IDesignTimeDbContextFactory<DecoleiDbContext>
{
    public DecoleiDbContext CreateDbContext(string[] args)
    {
        // Constrói o caminho para o appsettings.json a partir da localização atual
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        // Cria o DbContextOptionsBuilder
        var builder = new DbContextOptionsBuilder<DecoleiDbContext>();

        // Pega a connection string do appsettings.json
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Configura o builder para usar o SQL Server com a string de conexão
        builder.UseSqlServer(connectionString);

        // Retorna uma nova instância do seu DbContext
        return new DecoleiDbContext(builder.Options);
    }
}