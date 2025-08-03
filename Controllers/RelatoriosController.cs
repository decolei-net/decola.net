using System;
using System.Linq;
using System.Threading.Tasks;
using Decolei.net.Interfaces; // Verifique se o namespace está correto
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Decolei.net.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RelatoriosController : ControllerBase
    {
        private readonly IReservaRepository _reservaRepository;
        private readonly ILogger<RelatoriosController> _logger;

        public RelatoriosController(IReservaRepository reservaRepository, ILogger<RelatoriosController> logger)
        {
            _reservaRepository = reservaRepository;
            _logger = logger;
        }

        [HttpGet("reservas/pdf")]
        [Authorize(Roles = "ATENDENTE,ADMIN")]
        public async Task<IActionResult> ExportarReservasPdf()
        {
            try
            {
                QuestPDF.Settings.License = LicenseType.Community;

                var reservas = await _reservaRepository.ObterTodasAsync();

                if (!reservas.Any())
                {
                    return NotFound(new { mensagem = "Nenhuma reserva encontrada para gerar o relatório." });
                }

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(1.5f, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                        // Cabeçalho do documento
                        page.Header()
                            .PaddingBottom(0.5f, Unit.Centimetre)
                            .Text("Relatório Geral de Reservas - Decolei.net")
                            .SemiBold().FontSize(18).AlignCenter();

                        // Rodapé com paginação e data de geração
                        page.Footer()
                            .AlignCenter()
                            .Text(text =>
                            {
                                text.Span("Página ").FontSize(8);
                                text.CurrentPageNumber().FontSize(8);
                                text.Span(" de ").FontSize(8);
                                text.TotalPages().FontSize(8);
                                text.EmptyLine();
                                text.Span($"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm:ss}").FontSize(8);
                            });

                        // Conteúdo principal com a tabela
                        page.Content().Table(table =>
                        {
                            // Definição das colunas da tabela
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(0.5f); // ID
                                columns.RelativeColumn(1.5f); // Número
                                columns.RelativeColumn(2f);   // Cliente
                                columns.RelativeColumn(2f);   // Pacote
                                columns.RelativeColumn(1.5f); // Destino
                                columns.RelativeColumn(1f);   // Status
                                columns.RelativeColumn(1f);   // Valor
                                columns.RelativeColumn(1.5f); // Data
                            });

                            // Cabeçalho da tabela com fundo cinza
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("ID").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Número").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Cliente").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Pacote").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Destino").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Status").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Valor").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Data").Bold();
                            });

                            // Linhas da tabela com cores alternadas para melhor legibilidade
                            foreach (var (reserva, index) in reservas.Select((value, i) => (value, i)))
                            {
                                var backgroundColor = index % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                                table.Cell().BorderBottom(1, Unit.Point).BorderColor(Colors.Grey.Lighten2).Background(backgroundColor).Padding(4).Text(reserva.Id.ToString());
                                table.Cell().BorderBottom(1, Unit.Point).BorderColor(Colors.Grey.Lighten2).Background(backgroundColor).Padding(4).Text(reserva.Numero);
                                table.Cell().BorderBottom(1, Unit.Point).BorderColor(Colors.Grey.Lighten2).Background(backgroundColor).Padding(4).Text(reserva.Usuario?.NomeCompleto ?? "N/A");
                                table.Cell().BorderBottom(1, Unit.Point).BorderColor(Colors.Grey.Lighten2).Background(backgroundColor).Padding(4).Text(reserva.PacoteViagem?.Titulo ?? "N/A");
                                table.Cell().BorderBottom(1, Unit.Point).BorderColor(Colors.Grey.Lighten2).Background(backgroundColor).Padding(4).Text(reserva.PacoteViagem?.Destino ?? "N/A");
                                table.Cell().BorderBottom(1, Unit.Point).BorderColor(Colors.Grey.Lighten2).Background(backgroundColor).Padding(4).Text(reserva.Status ?? "N/A");
                                table.Cell().BorderBottom(1, Unit.Point).BorderColor(Colors.Grey.Lighten2).Background(backgroundColor).Padding(4).Text($"R$ {(reserva.ValorTotal ?? 0):N2}");
                                table.Cell().BorderBottom(1, Unit.Point).BorderColor(Colors.Grey.Lighten2).Background(backgroundColor).Padding(4).Text(reserva.Data?.ToString("dd/MM/yyyy") ?? "N/A");
                            }
                        });
                    });
                });

                var pdfBytes = document.GeneratePdf();
                var nomeArquivo = $"RelatorioGeralReservas_{DateTime.Now:yyyy-MM-dd}.pdf";

                return File(pdfBytes, "application/pdf", nomeArquivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao gerar o relatório de reservas em PDF.");
                return StatusCode(500, new { erro = "Ocorreu um erro interno ao gerar o relatório." });
            }
        }
    }
}