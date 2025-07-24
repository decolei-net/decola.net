using Microsoft.AspNetCore.Mvc;
using Decolei.net.Data; // ou seu namespace correto
using Decolei.net.Models;

namespace Decolei.net.Controllers
{
    [ApiController]
    [Route("pagamentos")]
    public class PagamentosController : ControllerBase
    {
        private readonly DecoleiDbContext _context;

        public PagamentosController(DecoleiDbContext context)
        {
            _context = context;
        }

        [HttpGet("status/{id}")]
        public IActionResult ObterStatusPagamento(int id)
        {
            var reserva = _context.Reservas.FirstOrDefault(r => r.Id == id);
            if (reserva == null)
                return NotFound("Reserva não encontrada.");

            return Ok(new
            {
                ReservaId = reserva.Id,
                StatusPagamento = reserva.Reserva_StatusPagamento
            });
        }
    }
}
