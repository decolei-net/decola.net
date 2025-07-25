using Decolei.net.Data;
using Decolei.net.DTOs;
using Decolei.net.Enums;
using Decolei.net.Models;
using Decolei.net.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Necessário para usar metodos asíncronos como FirstOrDefaultAsync e SaveChangesAsync

namespace Decolei.net.Controllers
{
    [ApiController]
    [Route("pagamentos")]
    public class PagamentosController : ControllerBase
    {
        private readonly DecoleiDbContext _context;
        private readonly EmailService _emailService; // serviço de email para enviar notificações
        private readonly PagamentoService _pagamentoService; // serviço de pagamento para processar pagamentos

        public PagamentosController(DecoleiDbContext context, EmailService emailService, PagamentoService pagamentoService)
        {
            _context = context;
            _emailService = emailService;
            _pagamentoService = pagamentoService;
        }

        // GET - Verifica o status do pagamento de uma reserva
        [HttpGet("status/{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> ObterStatusPagamento(int id)
        {
            var pagamento = await _context.Pagamentos
                .Include(p => p.Reserva)
                .FirstOrDefaultAsync(p => p.Id == id); // ID do pagamento

            if (pagamento == null)
                return NotFound("Pagamento não encontrado.");

            return Ok(new
            {
                PagamentoId = pagamento.Id,// ID do pagamento
                StatusPagamento = pagamento.Status, // aqui pego o status do pagamento
                StatusReserva = pagamento.Reserva?.Reserva_StatusPagamento // aq pego o status da reserva relacionada ao pagamento
            });
        }

        [HttpPost] // criar um novo pagamento para reserva existente
        [Authorize(Roles = "ADMIN, CLIENTE")]
        public async Task<IActionResult> CriarPagamento(PagamentoEntradaDTO dto)
        {
            // Verifica se o dto é válido
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var pagamento = await _pagamentoService.RealizarPagamentoAsync(dto);
                return Ok(pagamento);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    erro = "Erro interno ao processar o pagamento",
                    detalhe = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        [HttpPut("{id}")] // atualizar o status do pagamento
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> AtualizarStatusPagamento(int id, [FromBody] AtualizarStatusPagamentoDto dto)
        {
            // verifica se o dto é válido
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrEmpty(dto.Status))
            {
                // Se você não enviar o novo status, o sistema te dá um erro claro.
                return BadRequest(new { message = "O campo 'Status' é obrigatório." });
            }

            // verifica se o pagamento existe
            var pagamento = await _context.Pagamentos
                .Include(p => p.Reserva) // inclui a reserva relacionada ao pagamento
                .FirstOrDefaultAsync(p => p.Id == id); // busca o pagamento pelo ID

            if (pagamento == null)
            {
                return NotFound("Pagamento não encontrado.");
            }

            var novoStatus = dto.Status.Trim().ToUpper(); // converte o status para maiúsculas pq na tabela(Reserva) ele é armazenado em maiúsculas
                                                          // trim() remove espaços em branco no início e no final da string

            // atualiza o status do pagamento de pagamento e reserva
            pagamento.Status = novoStatus;

            if (pagamento.Reserva != null)
            {
                pagamento.Reserva.Reserva_StatusPagamento = novoStatus; // atualiza o status da reserva também
            }

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Status atualizado com sucesso.", novoStatus });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, "Erro ao atualizar o status do pagamento: " + ex.Message);
            }
        }
    }
}
