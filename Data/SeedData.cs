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
                Descricao = " A ilha é reconhecida como um dos melhores pontos de mergulho do mundo e a Baía do Sancho foi eleita a Melhor Praia do Mundo por diversas vezes pelo TripAdvisor. ",
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

            var pacoteJoaoPessoa = new PacoteViagem
            {
                Titulo = "João Pessoa, mais que uma cidade, uma experiência",
                Descricao = " João Pessoa tem ganhado destaque internacional, sendo apontada como o 3º lugar com maior aumento na procura entre destinos de todo o mundo ",
                Destino = "João Pessoa, Paraíba",
                Valor = 3000.00m,
                DataInicio = new DateTime(2025, 08, 30),
                DataFim = new DateTime(2025, 09, 07),
                UsuarioId = adminUser.Id,
                Imagens = new List<Imagem>
                {
                    new Imagem { Url = "uploads/pacotes/joaopessoa-1.jpg", IsVideo = false },
                    new Imagem { Url = "uploads/pacotes/joaopessoa-2.jpg", IsVideo = false },
                    new Imagem { Url = "uploads/pacotes/joaopessoa-3.jpg", IsVideo = false },
                    new Imagem { Url = "uploads/pacotes/joaopessoa-4.jpg", IsVideo = false }
                }
            };

            var pacoteBalneario = new PacoteViagem
            {
                Titulo = "Dubai Brasileira",
                Descricao = "  Também conhecida como a \"Dubai Brasileira\", é um destino turístico popular no litoral norte de Santa Catarina, famoso por suas praias, vida noturna agitada e arranha-céus imponentes. ",
                Destino = "Balneário Camboriú, Santa Catarina",
                Valor = 5100.00m,
                DataInicio = new DateTime(2025, 08, 30),
                DataFim = new DateTime(2025, 09, 07),
                UsuarioId = adminUser.Id,
                Imagens = new List<Imagem>
                {
                    new Imagem { Url = "uploads/pacotes/balneario-1.jpg", IsVideo = false },
                    new Imagem { Url = "uploads/pacotes/balneario-2.jpg", IsVideo = false },
                    new Imagem { Url = "uploads/pacotes/balneario-3.jpg", IsVideo = false },
                    new Imagem { Url = "uploads/pacotes/balneario-4.jpg", IsVideo = false }
                }
            };

            var pacoteSaoPaulo = new PacoteViagem
            {
                Titulo = "A Metrópole que Nunca Dorme",
                Descricao = "São Paulo é o coração financeiro do Brasil, repleta de arranha-céus, centros culturais, gastronomia premiada e vida noturna intensa. Ideal para quem busca cultura, arte urbana e experiências cosmopolitas.",
                Destino = "São Paulo, Brasil",
                Valor = 9500.00m,
                DataInicio = new DateTime(2025, 10, 10),
                DataFim = new DateTime(2025, 10, 15),
                UsuarioId = adminUser.Id,
                Imagens = new List<Imagem>
    {
                    new Imagem { Url = "uploads/pacotes/saopaulo-1.jpg", IsVideo = false },
                    new Imagem { Url = "uploads/pacotes/saopaulo-2.jpg", IsVideo = false }
    }
            };

            var pacoteRio = new PacoteViagem
            {
                Titulo = "Cidade Maravilhosa",
                Descricao = "O Rio de Janeiro encanta com suas paisagens deslumbrantes, praias icônicas como Copacabana e Ipanema, além do Cristo Redentor e do Pão de Açúcar. Uma mistura perfeita de natureza, cultura e alegria carioca.",
                Destino = "Rio de Janeiro, Brasil",
                Valor = 11000.00m,
                DataInicio = new DateTime(2025, 11, 5),
                DataFim = new DateTime(2025, 11, 10),
                UsuarioId = adminUser.Id,
                Imagens = new List<Imagem>
    {
                    new Imagem { Url = "uploads/pacotes/rio-1.jpg", IsVideo = false },
                    new Imagem { Url = "uploads/pacotes/rio-2.jpg", IsVideo = false },
                    new Imagem { Url = "uploads/pacotes/rio-3.jpg", IsVideo = false },
                    new Imagem { Url = "uploads/pacotes/rio-4.jpg", IsVideo = false }
    }
            };

            var pacoteBeloHorizonte = new PacoteViagem
            {
                Titulo = "Cultura e Sabores das Gerais",
                Descricao = "Belo Horizonte une o charme das montanhas com a rica culinária mineira e um forte cenário artístico e musical. Ideal para quem busca tradição, hospitalidade e gastronomia de excelência.",
                Destino = "Belo Horizonte, Minas Gerais",
                Valor = 8900.00m,
                DataInicio = new DateTime(2025, 12, 1),
                DataFim = new DateTime(2025, 12, 6),
                UsuarioId = adminUser.Id,
                Imagens = new List<Imagem>
    {
                    new Imagem { Url = "uploads/pacotes/bh-1.jpg", IsVideo = false },
                    new Imagem { Url = "uploads/pacotes/bh-2.jpg", IsVideo = false }
    }
            };

            var pacoteFortaleza = new PacoteViagem
            {
                Titulo = "Sol, Mar e Cultura Cearense",
                Descricao = "Fortaleza é famosa por suas belas praias, como a Praia do Futuro, e uma vibrante vida cultural. Uma cidade acolhedora, com culinária marcante e um clima ensolarado o ano inteiro.",
                Destino = "Fortaleza, Ceará",
                Valor = 9800.00m,
                DataInicio = new DateTime(2025, 10, 20),
                DataFim = new DateTime(2025, 10, 25),
                UsuarioId = adminUser.Id,
                Imagens = new List<Imagem>
    {
                    new Imagem { Url = "uploads/pacotes/fortaleza-1.jpg", IsVideo = false },
                    new Imagem { Url = "uploads/pacotes/fortaleza-2.jpg", IsVideo = false },
                    new Imagem { Url = "uploads/pacotes/fortaleza-3.jpg", IsVideo = false }
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

            var pacoteArgentina = new PacoteViagem
            {
                Titulo = " A Paris da América do Sul",
                Descricao = "Uma cidade vibrante e cosmopolita, conhecida como a \"Paris da América do Sul\" por sua arquitetura europeia e rica vida cultural. A cidade oferece uma mistura única de história, arte, tango e gastronomia, com diversos bairros cheios de personalidade. ",
                Destino = "Buenos Aires, Argentina",
                Valor = 12000.00m,
                DataInicio = new DateTime(2025, 9, 20),
                DataFim = new DateTime(2025, 9, 25),
                UsuarioId = adminUser.Id,
                Imagens = new List<Imagem>
                {
                    new Imagem { Url = "uploads/pacotes/buenos-1.jpg", IsVideo = false },
                    new Imagem { Url = "uploads/pacotes/buenos-2.jpg", IsVideo = false },
                    new Imagem { Url = "uploads/pacotes/buenos-3.jpg", IsVideo = false },
                    new Imagem { Url = "uploads/pacotes/buenos-4.jpg", IsVideo = false }
                }
            };

            var pacoteNatal = new PacoteViagem
            {
                Titulo = "Paraíso nas Dunas",
                Descricao = "Natal encanta com suas dunas douradas, praias de águas mornas e o famoso passeio de buggy. Um destino perfeito para relaxar, curtir o sol e se encantar com as belezas do Nordeste.",
                Destino = "Natal, Rio Grande do Norte",
                Valor = 9300.00m,
                DataInicio = new DateTime(2025, 11, 15),
                DataFim = new DateTime(2025, 11, 20),
                UsuarioId = adminUser.Id,
                Imagens = new List<Imagem>
    {
                    new Imagem { Url = "uploads/pacotes/natal-1.jpg", IsVideo = false },
                    new Imagem { Url = "uploads/pacotes/natal-2.jpg", IsVideo = false },
    }
            };

            var pacoteMaragogi = new PacoteViagem
            {
                Titulo = "O Caribe Brasileiro",
                Descricao = "Maragogi é conhecido por suas piscinas naturais de águas cristalinas e recifes de corais. Ideal para quem busca tranquilidade, mergulho e um visual paradisíaco.",
                Destino = "Maragogi, Alagoas",
                Valor = 10500.00m,
                DataInicio = new DateTime(2025, 12, 10),
                DataFim = new DateTime(2025, 12, 15),
                UsuarioId = adminUser.Id,
                Imagens = new List<Imagem>
    {
                    new Imagem { Url = "uploads/pacotes/maragogi-1.jpg", IsVideo = false },
                    new Imagem { Url = "uploads/pacotes/maragogi-2.jpg", IsVideo = false },
                    new Imagem { Url = "uploads/pacotes/maragogi-3.jpg", IsVideo = false }
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
                    new Imagem { Url = "https://www.youtube.com/embed/aNC3UOYOejI?si=UCXT6BMqvSefHarj", IsVideo = true },
                    new Imagem { Url = "uploads/pacotes/quioto-fushimi-inari.jpg", IsVideo = false },
                    new Imagem { Url = "uploads/pacotes/quioto-kinkaku-ji.jpg", IsVideo = false }
                }
            };

            var pacoteGramado = new PacoteViagem
            {
                Titulo = "Inverno Mágico em Gramado",
                Descricao = "Curta o charme europeu da Serra Gaúcha, com fondues, vinhos e paisagens de tirar o fôlego.",
                Destino = "Gramado, Rio Grande do Sul",
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