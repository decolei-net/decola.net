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
                // *** ÚNICA PARTE ALTERADA ***
                await SeedPacotesAsync(context, adminUser);
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

        // ******** MÉTODO CORRIGIDO CONFORME SOLICITADO ********
        private static async Task SeedPacotesAsync(DecoleiDbContext context, Usuario adminUser)
        {
            if (await context.PacotesViagem.AnyAsync())
            {
                return;
            }

            var pacotes = new List<PacoteViagem>
            {
                // Pacote 1 com 3 imagens
                new PacoteViagem
                {
                    Titulo = "Semana Mágica em Quioto",
                    Descricao = "Explore os templos antigos, jardins zen e a cultura gueixa na antiga capital do Japão.",
                    VideoURL = "https://youtube.com/chapada",
                    Destino = "Quioto, Japão",
                    Valor = 9800.00m,
                    DataInicio = new DateTime(2025, 10, 05),
                    DataFim = new DateTime(2025, 10, 12),
                    UsuarioId = adminUser.Id,
                    Imagens = new List<Imagem>
                    {
                        new Imagem { Url = "uploads/pacotes/quioto-arashiyama.jpg" },
                        new Imagem { Url = "uploads/pacotes/quioto-fushimi-inari.jpg" },
                        new Imagem { Url = "uploads/pacotes/quioto-kinkaku-ji.jpg" }
                    }
                },
                // Pacote 2 com 3 imagens
                new PacoteViagem
                {
                    Titulo = "Aventura na Costa Amalfitana",
                    Descricao = "Viagem de carro pelas vilas coloridas de Positano, Amalfi e Ravello, com vistas espetaculares do Mediterrâneo.",
                    VideoURL = "https://youtube.com/noronha",
                    Destino = "Costa Amalfitana, Itália",
                    Valor = 7500.00m,
                    DataInicio = new DateTime(2025, 09, 15),
                    DataFim = new DateTime(2025, 09, 22),
                    UsuarioId = adminUser.Id,
                    Imagens = new List<Imagem>
                    {
                        new Imagem { Url = "uploads/pacotes/amalfi-coast-geral.jpg" },
                        new Imagem { Url = "uploads/pacotes/amalfi-positano.jpg" },
                        new Imagem { Url = "uploads/pacotes/amalfi-ravello.jpg" }
                    }
                }
            };

            await context.PacotesViagem.AddRangeAsync(pacotes);
            await context.SaveChangesAsync();
            Console.WriteLine("Pacotes de viagem iniciais criados com sucesso.");
        }
    }
}