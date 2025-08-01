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

            // Busca os usuários que vamos usar para criar os dados
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
            // Só executa o seed se não houver NENHUM pacote, para evitar duplicatas.
            if (await context.PacotesViagem.AnyAsync())
            {
                return;
            }

            // PACOTES FUTUROS (os que você já tinha)
            var pacoteQuioto = new PacoteViagem
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
            };

            var pacoteAmalfi = new PacoteViagem
            {
                Titulo = "Aventura na Costa Amalfitana",
                Descricao = "Viagem de carro pelas vilas coloridas de Positano, Amalfi e Ravello, com vistas espetaculares do Mediterrâneo.",
                VideoURL = "https://youtube.com/noronha",
                Destino = "Costa Amalfitana, Itália",
                Valor = 7500.00m,
                DataInicio = new DateTime(2025, 9, 15),
                DataFim = new DateTime(2025, 9, 22),
                UsuarioId = adminUser.Id,
                Imagens = new List<Imagem>
                {
                    new Imagem { Url = "uploads/pacotes/amalfi-coast-geral.jpg" },
                    new Imagem { Url = "uploads/pacotes/amalfi-positano.jpg" },
                    new Imagem { Url = "uploads/pacotes/amalfi-ravello.jpg" }
                }
            };

            // PACOTE PASSADO (o novo pacote que você pediu)
            var pacoteGramado = new PacoteViagem
            {
                Titulo = "Inverno Mágico em Gramado",
                Descricao = "Curta o charme europeu da Serra Gaúcha, com fondues, vinhos e paisagens de tirar o fôlego.",
                VideoURL = "http://googleusercontent.com/youtube.com/3",
                Destino = "Gramado, RS",
                Valor = 2500.00m,
                DataInicio = new DateTime(2025, 6, 12), // Data no passado
                DataFim = new DateTime(2025, 6, 19),   // Data no passado
                UsuarioId = adminUser.Id,
                Imagens = new List<Imagem>
                {
                    new Imagem { Url = "uploads/pacotes/gramado-rua-torta.jpg" },
                    new Imagem { Url = "uploads/pacotes/gramado-lago-negro.jpg" },
                }
            };

            // CRIAÇÃO DA RESERVA, PAGAMENTO E AVALIAÇÃO PARA O PACOTE DE GRAMADO
            var reservaGramado = new Reserva
            {
                PacoteViagem = pacoteGramado, // Associa a reserva ao pacote
                Usuario = clienteUser,        // Associa ao "Cliente de Teste"
                Data = new DateTime(2025, 5, 20),
                ValorTotal = pacoteGramado.Valor,
                Status = "Finalizada",
                Numero = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper(),
                Viajantes = new List<Viajante>
                {
                    // O próprio cliente é o viajante
                    new Viajante { Nome = clienteUser.NomeCompleto, Documento = clienteUser.Documento }
                },
                Pagamentos = new List<Pagamento>
                {
                    new Pagamento { Forma = "PIX", Status = "Confirmado", Data = new DateTime(2025, 5, 20) }
                }
            };

            // Cria a avaliação do cliente para o pacote de Gramado
            var avaliacaoGramado = new Avaliacao
            {
                PacoteViagem = pacoteGramado,
                Usuario = clienteUser,
                Nota = 4, // 4 estrelas, como solicitado
                Comentario = "Viagem incrível! Gramado é uma cidade linda e o hotel era muito confortável. Só não dou 5 estrelas porque o tempo de um dos passeios foi um pouco curto. Mas recomendo!",
                Data = new DateTime(2025, 6, 22), // Data da avaliação após a viagem
                Aprovada = true
            };

            // Adiciona tudo ao contexto do banco de dados
            context.PacotesViagem.AddRange(pacoteQuioto, pacoteAmalfi, pacoteGramado);
            context.Reservas.Add(reservaGramado);
            context.Avaliacoes.Add(avaliacaoGramado);

            // Salva todas as alterações de uma vez
            await context.SaveChangesAsync();
            Console.WriteLine("Pacotes de viagem futuros e histórico de viagem criados com sucesso.");
        }
    }
}