using Decolei.net.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Decolei.net.Data
{
    public static class SeedData
    {
        public static async Task SeedAllAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<Usuario>>();
            var context = serviceProvider.GetRequiredService<DecoleiDbContext>();

            await context.Database.EnsureCreatedAsync();

            await SeedRoleAsync(roleManager, "ADMIN");
            await SeedRoleAsync(roleManager, "CLIENTE");
            await SeedRoleAsync(roleManager, "ATENDENTE");

            await SeedUserAsync(userManager, "admin@decolei.net", "SenhaAdmin123!", "Administrador Master", "00000000000", "ADMIN");
            await SeedUserAsync(userManager, "cliente@decolei.net", "SenhaCliente123!", "Cliente de Teste", "11111111111", "CLIENTE");
            await SeedUserAsync(userManager, "atendente@decolei.net", "SenhaAtendente123!", "Atendente de Teste", "22222222222", "ATENDENTE");

            var adminUser = await userManager.FindByEmailAsync("admin@decolei.net");
            var clienteUser = await userManager.FindByEmailAsync("cliente@decolei.net");

            if (adminUser != null && clienteUser != null)
            {
                await SeedPacotesEHistoricoAsync(context, adminUser, clienteUser);
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

        private static async Task SeedPacotesEHistoricoAsync(DecoleiDbContext context, Usuario adminUser, Usuario clienteUser)
        {
            if (await context.PacotesViagem.AnyAsync())
            {
                return;
            }

            var pacoteMaceio = new PacoteViagem
            {
                Titulo = "O paraíso alagoano te espera de braços abertos",
                Descricao = " Um destino turístico encantador com suas praias paradisíacas, cultura rica e culinária saborosa. ",
                Destino = "Maceió, Alagoas",
                Valor = 3800.00m,
                DataInicio = new DateTime(2025, 07, 30),
                DataFim = new DateTime(2025, 08, 03),
                UsuarioId = adminUser.Id,
                Imagens = new List<Imagem>
                {
                    new Imagem { Url = "uploads/pacotes/maceio-1.jpg", IsVideo = false },
                    new Imagem { Url = "uploads/pacotes/maceio-2.jpg", IsVideo = false },
                    new Imagem { Url = "uploads/pacotes/maceio-3.jpg", IsVideo = false }
                }
            };

            var pacoteSalvador = new PacoteViagem
            {
                Titulo = "Descobrindo Salvador: Roteiros para todos os gostos",
                Descricao = " Um dos destinos mais intensos do Brasil. Repleta de história, cultura, tradições, cores e sabores, a cidade é um convite a curtir cada minuto. ",
                Destino = "Salvador, Bahia",
                Valor = 4600.00m,
                DataInicio = new DateTime(2025, 08, 01),
                DataFim = new DateTime(2025, 08, 03),
                UsuarioId = adminUser.Id,
                Imagens = new List<Imagem>
                {
                    new Imagem { Url = "uploads/pacotes/salvador-1.jpg", IsVideo = false },
                    new Imagem { Url = "uploads/pacotes/salvador-2.jpg", IsVideo = false },
                    new Imagem { Url = "uploads/pacotes/salvador-3.jpg", IsVideo = false }
                }
            };

            var pacoteNoronha = new PacoteViagem
            {
                Titulo = "Noronha: O paraíso das águas cristalinas",
                Descricao = " a ilha é reconhecida como um dos melhores pontos de mergulho do mundo e a Baía do Sancho foi eleita a Melhor Praia do Mundo por diversas vezes pelo TripAdvisor. ",
                Destino = "Fernando de Noronha, Pernambuco",
                Valor = 9100.00m,
                DataInicio = new DateTime(2025, 08, 09),
                DataFim = new DateTime(2025, 08, 15),
                UsuarioId = adminUser.Id,
                Imagens = new List<Imagem>
                {
                    new Imagem { Url = "uploads/pacotes/noronha-1.jpg", IsVideo = false },
                    new Imagem { Url = "uploads/pacotes/noronha-2.jpg", IsVideo = false },
                    new Imagem { Url = "uploads/pacotes/noronha-3.jpg", IsVideo = false }
                }
            };

            var pacoteQuioto = new PacoteViagem
            {
                Titulo = "Semana Mágica em Quioto",
                Descricao = "Explore os templos antigos, jardins zen e a cultura gueixa na antiga capital do Japão.",
                Destino = "Quioto, Japão",
                Valor = 9800.00m,
                DataInicio = new DateTime(2025, 10, 05),
                DataFim = new DateTime(2025, 10, 12),
                UsuarioId = adminUser.Id,
                Imagens = new List<Imagem>
                {
                    new Imagem { Url = "uploads/pacotes/quioto-arashiyama.jpg", IsVideo = false },
                    new Imagem { Url = "https://www.youtube.com/embed/aNC3UOYOejI?si=UCXT6BMqvSefHarj", IsVideo = true }, // VÍDEO DE QUIOTO
                    new Imagem { Url = "uploads/pacotes/quioto-fushimi-inari.jpg", IsVideo = false },
                    new Imagem { Url = "uploads/pacotes/quioto-kinkaku-ji.jpg", IsVideo = false }
                }
            };

            var pacoteAmalfi = new PacoteViagem
            {
                Titulo = "Aventura na Costa Amalfitana",
                Descricao = "Viagem de carro pelas vilas coloridas de Positano, Amalfi e Ravello, com vistas espetaculares do Mediterrâneo.",
                Destino = "Costa Amalfitana, Itália",
                Valor = 7500.00m,
                DataInicio = new DateTime(2025, 9, 15),
                DataFim = new DateTime(2025, 9, 22),
                UsuarioId = adminUser.Id,
                Imagens = new List<Imagem>
                {
                    new Imagem { Url = "uploads/pacotes/amalfi-coast-geral.jpg", IsVideo = false },
                    new Imagem { Url = "uploads/pacotes/amalfi-positano.jpg", IsVideo = false },
                    new Imagem { Url = "uploads/pacotes/amalfi-ravello.jpg", IsVideo = false }
                }
            };

            var pacoteGramado = new PacoteViagem
            {
                Titulo = "Inverno Mágico em Gramado",
                Descricao = "Curta o charme europeu da Serra Gaúcha, com fondues, vinhos e paisagens de tirar o fôlego.",
                Destino = "Gramado, RS",
                Valor = 2500.00m,
                DataInicio = new DateTime(2025, 6, 12),
                DataFim = new DateTime(2025, 6, 19),
                UsuarioId = adminUser.Id,
                Imagens = new List<Imagem>
                {
                    new Imagem { Url = "uploads/pacotes/gramado-rua-torta.jpg", IsVideo = false },
                    new Imagem { Url = "https://www.youtube.com/embed/8Qghrljp9q0?si=5z7Jjngbsl8MTzo5", IsVideo = true }, // VÍDEO DE GRAMADO
                    new Imagem { Url = "uploads/pacotes/gramado-lago-negro.jpg", IsVideo = false },
                }
            };

            var reservaGramado = new Reserva
            {
                PacoteViagem = pacoteGramado,
                Usuario = clienteUser,
                Data = new DateTime(2025, 5, 20),
                ValorTotal = pacoteGramado.Valor,
                Status = "Finalizada",
                Numero = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper(),
                Viajantes = new List<Viajante> { new Viajante { Nome = clienteUser.NomeCompleto, Documento = clienteUser.Documento } },
                Pagamentos = new List<Pagamento> { new Pagamento { Forma = "PIX", Status = "Confirmado", Data = new DateTime(2025, 5, 20) } }
            };

            var avaliacaoGramado = new Avaliacao
            {
                PacoteViagem = pacoteGramado,
                Usuario = clienteUser,
                Nota = 4,
                Comentario = "Viagem incrível! Gramado é uma cidade linda e o hotel era muito confortável. Só não dou 5 estrelas porque o tempo de um dos passeios foi um pouco curto. Mas recomendo!",
                Data = new DateTime(2025, 6, 22),
                Aprovada = true
            };

            context.PacotesViagem.AddRange(pacoteQuioto, pacoteAmalfi, pacoteGramado);
            context.Reservas.Add(reservaGramado);
            context.Avaliacoes.Add(avaliacaoGramado);

            await context.SaveChangesAsync();
            Console.WriteLine("Pacotes de viagem futuros e histórico de viagem criados com sucesso.");
        }
    }
}