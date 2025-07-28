using Decolei.net.Data;
using Decolei.net.DTOs;
using Decolei.net.Models;
using Decolei.net.Enums;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Decolei.net.Services
{
    public class PagamentoService
    {
        private readonly EmailService emailService;
        private readonly DecoleiDbContext dbContext;
        private readonly IServiceScopeFactory scopeFactory; // serve para criar escopos de serviço, útil para injeção de dependências em tarefas agendadas -  como coleto

        public PagamentoService(EmailService emailService, DecoleiDbContext dbContext, IServiceScopeFactory scopeFactory)
        {
            this.emailService = emailService;
            this.dbContext = dbContext;
            this.scopeFactory = scopeFactory;
        }

        // Método principal para processar o pagamento
        public async Task<PagamentoDto> RealizarPagamentoAsync(PagamentoEntradaDTO dto, ClaimsPrincipal user)
        {
            var idUsuarioLogadoString = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idUsuarioLogadoString, out var idUsuarioLogado))
            {
                throw new ArgumentException("Token de usuário inválido.");
            }

            var reserva = await dbContext.Reservas.FirstOrDefaultAsync(r => r.Id == dto.ReservaId);
            if (reserva == null)
            {
                throw new ArgumentException("Reserva não encontrada.");
            }

            if (reserva.Usuario_Id != idUsuarioLogado)
            {
                throw new ArgumentException("Você não tem permissão para pagar por esta reserva.");
            }

            var pagamentoExistente = await dbContext.Pagamentos.FirstOrDefaultAsync(p => p.Reserva_Id == dto.ReservaId && p.Status == "APROVADO");
            if (pagamentoExistente != null)
            {
                throw new ArgumentException("Esta reserva já possui um pagamento aprovado.");
            }

            // 1. Simula o gateway de pagamento com os dados recebidos
            var gateway = new GatewayPagamentoService
            {
                NomeCompleto = dto.NomeCompleto,
                Cpf = dto.Cpf,
                Metodo = dto.Metodo,
                ValorTotal = dto.Valor,
                Parcelas = dto.Parcelas,
                NumeroCartaoMascarado = dto.NumeroCartao
            };

            // Processa o pagamento (define o status de acordo com o método)
            gateway.ProcessarPagamento();

            // 2. Cria o objeto de pagamento com os dados simulados
            var pagamento = new Pagamento
            {
                Reserva_Id = dto.ReservaId,
                Forma = ConverterMetodoParaBanco(gateway.Metodo),
                Status = gateway.Status,
                ComprovanteURL = $"https://decolei.net/comprovante/{gateway.IdTransacao}",
                Data = DateTime.Now
            };

            // 3. Salva o pagamento no banco de dados
            await dbContext.Pagamentos.AddAsync(pagamento);
            await dbContext.SaveChangesAsync();

            // 4. Se o pagamento for aprovado de imediato, atualiza o status da reserva também
            if (reserva != null)
            {
                // Apenas atualiza imediatamente se NÃO for boleto
                if (gateway.Metodo != MetodoPagamento.Boleto && gateway.Status == "APROVADO")
                {
                    reserva.Status = "APROVADO";
                    reserva.Reserva_StatusPagamento = "APROVADO";
                    dbContext.Reservas.Update(reserva);
                }
            }

            await dbContext.SaveChangesAsync();

            // 5. Envia e-mail imediato com o status atual (aprovado ou pendente)
            var emailInicial = $@"
                                <html>
                                <body style='font-family: Arial, sans-serif;'>
                                    <h2>Olá {dto.NomeCompleto},</h2>
                                    <p>Recebemos sua solicitação de pagamento e ela foi processada com o seguinte status:</p>
                                    <p><strong>Status:</strong> {gateway.Status}</p>
                                    <p><strong>Número da transação:</strong> {gateway.IdTransacao}</p>
                                    <br />
                                    <p>Agradecemos por escolher a <strong>Decolei.NET</strong>!</p>
                                    <p>Se tiver qualquer dúvida, estamos à disposição.</p>
                                    <br />
                                    <p>Atenciosamente,</p>
                                    <p><em>Equipe Decolei.NET</em></p>
                                </body>
                                </html>";

            await emailService.EnviarEmailAsync(dto.Email, "Confirmação de Pagamento", emailInicial);

            // 6. Se for boleto, agenda a aprovação automática após 60 segundos
            if (gateway.Metodo == MetodoPagamento.Boleto)
            {
                var tarefaBoleto = Task.Run(async () =>
                {
                    // Espera 60 segundos simulando compensação do boleto
                    await Task.Delay(TimeSpan.FromSeconds(60));

                    using var scope = scopeFactory.CreateScope();
                    var scopedContext = scope.ServiceProvider.GetRequiredService<DecoleiDbContext>();

                    // Busca o pagamento salvo no banco
                    var pagamentoNoBanco = await scopedContext.Pagamentos
                        .Include(p => p.Reserva)
                        .FirstOrDefaultAsync(p => p.Id == pagamento.Id);

                    // Se ainda estiver pendente, atualiza para aprovado
                    if (pagamentoNoBanco != null && pagamentoNoBanco.Status == "PENDENTE")
                    {
                        pagamentoNoBanco.Status = "APROVADO";

                        if (pagamentoNoBanco.Reserva != null)
                        {
                            pagamentoNoBanco.Reserva.Reserva_StatusPagamento = "APROVADO";
                            pagamentoNoBanco.Reserva.Status = "APROVADO";
                        }
                        await scopedContext.SaveChangesAsync();

                        // Envia novo e-mail confirmando a aprovação do boleto
                        var corpoBoleto = $@"
                                        <html>
                                        <body style='font-family: Arial, sans-serif;'>
                                            <h2>Olá {dto.NomeCompleto}!</h2>
                                            <p>Temos uma ótima notícia: o seu <strong>boleto foi compensado com sucesso</strong>.</p>
                                            <p><strong>Valor:</strong> R$ {dto.Valor:F2}</p>
                                            <br />
                                            <p>Agradecemos por viajar com a <strong>Decolei.NET</strong>!</p>
                                            <p>Se precisar de algo, estamos aqui para ajudar.</p>
                                            <br />
                                            <p>Atenciosamente,</p>
                                            <p><em>Equipe Decolei.NET</em></p>
                                        </body>
                                        </html>";

                        await emailService.EnviarEmailAsync(dto.Email, "Pagamento aprovado (Boleto)", corpoBoleto);
                    }
                });
            }

            // 7. Retorna o status final
            return new PagamentoDto
            {
                Id = pagamento.Id,
                Reserva_Id = pagamento.Reserva_Id,
                Forma = pagamento.Forma,
                Status = pagamento.Status,
                ComprovanteURL = pagamento.ComprovanteURL,
                Data = (DateTime)pagamento.Data
            };
        }

        // Converte o enum para o valor esperado pelo banco de dados
        private string ConverterMetodoParaBanco(MetodoPagamento metodo)
        {
            return metodo switch
            {
                MetodoPagamento.Pix => "PIX",
                MetodoPagamento.Boleto => "BOLETO",
                MetodoPagamento.Credito => "CARTAO_CREDITO",
                MetodoPagamento.Debito => "CARTAO_DEBITO",
                _ => "INDEFINIDO"
            };
        }
    }
}
