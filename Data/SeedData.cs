using Decolei.net.Data;
using Decolei.net.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Decolei.net.Data
{
    /// <summary>
    /// Classe estática responsável por popular o banco de dados com dados iniciais.
    /// Renomeada para SeedData para maior clareza.
    /// </summary>
    public static class SeedData
    {
        /// <summary>
        /// Executa o processo de seeding completo.
        /// </summary>
        public static async Task SeedAllAsync(IServiceProvider serviceProvider)
        {
            // Obtém os serviços necessários do contêiner de injeção de dependência
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<Usuario>>();
            var context = serviceProvider.GetRequiredService<DecoleiDbContext>();

            // Garante que o banco de dados esteja criado
            await context.Database.EnsureCreatedAsync();

            // 1. Cria os perfis (Roles) se eles não existirem
            await SeedRoleAsync(roleManager, "ADMIN");
            await SeedRoleAsync(roleManager, "CLIENTE");
            await SeedRoleAsync(roleManager, "ATENDENTE");

            // 2. Cria os usuários padrão se eles não existirem
            await SeedUserAsync(userManager, "admin@decolei.net", "SenhaAdmin123!", "Administrador Master", "00000000000", "ADMIN");
            await SeedUserAsync(userManager, "cliente@decolei.net", "SenhaCliente123!", "Cliente de Teste", "11111111111", "CLIENTE");
            await SeedUserAsync(userManager, "atendente@decolei.net", "SenhaAtendente123!", "Atendente de Teste", "22222222222", "ATENDENTE");

            // 3. Cria os pacotes de viagem iniciais se não houver nenhum
            var adminUser = await userManager.FindByEmailAsync("admin@decolei.net");
            if (adminUser != null)
            {
                await SeedPacotesAsync(context, adminUser.Id);
            }
        }

        private static async Task SeedRoleAsync(RoleManager<IdentityRole<int>> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                Console.WriteLine($"Perfil '{roleName}' criado com sucesso.");
            }
        }

        private static async Task SeedUserAsync(UserManager<Usuario> userManager, string email, string password, string nomeCompleto, string documento, string roleName)
        {
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new Usuario
                {
                    UserName = email,
                    Email = email,
                    NomeCompleto = nomeCompleto,
                    Documento = documento,
                    Perfil = roleName,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, roleName);
                    Console.WriteLine($"Usuário '{email}' criado com sucesso e adicionado ao perfil '{roleName}'.");
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    Console.WriteLine($"Erro ao criar usuário '{email}': {errors}");
                }
            }
        }

        private static async Task SeedPacotesAsync(DecoleiDbContext context, int adminUserId)
        {
            if (await context.PacotesViagem.AnyAsync())
            {
                return;
            }

            var pacotes = new List<PacoteViagem>
            {
                new PacoteViagem
                {
                    Titulo = "Verão em Fernando de Noronha",
                    Descricao = "Aproveite 7 dias no paraíso de Fernando de Noronha, com praias deslumbrantes e mergulho com golfinhos.",
                    ImagemURL = "https://exemplo.com/noronha.jpg",
                    VideoURL = "https://youtube.com/noronha",
                    Destino = "Fernando de Noronha, PE",
                    Valor = 4500.00m,
                    DataInicio = new DateTime(2025, 11, 10),
                    DataFim = new DateTime(2025, 11, 17),
                    UsuarioId = adminUserId
                },
                new PacoteViagem
                {
                    Titulo = "Fim de Semana em Porto de Galinhas",
                    Descricao = "Escapada de 3 dias para as piscinas naturais de Porto de Galinhas.",
                    ImagemURL = "https://exemplo.com/porto.jpg",
                    VideoURL = "https://youtube.com/porto",
                    Destino = "Porto de Galinhas, PE",
                    Valor = 1200.00m,
                    DataInicio = new DateTime(2025, 9, 5),
                    DataFim = new DateTime(2025, 9, 8),
                    UsuarioId = adminUserId
                },
                new PacoteViagem
                {
                    Titulo = "Aventura na Chapada Diamantina",
                    Descricao = "5 dias de trilhas, cachoeiras e paisagens incríveis na Bahia.",
                    ImagemURL = "https://exemplo.com/chapada.jpg",
                    VideoURL = "https://youtube.com/chapada",
                    Destino = "Chapada Diamantina, BA",
                    Valor = 2800.00m,
                    DataInicio = new DateTime(2025, 10, 15),
                    DataFim = new DateTime(2025, 10, 20),
                    UsuarioId = adminUserId
                }
            };

            await context.PacotesViagem.AddRangeAsync(pacotes);
            await context.SaveChangesAsync();
            Console.WriteLine("Pacotes de viagem iniciais criados com sucesso.");
        }
    }
}
