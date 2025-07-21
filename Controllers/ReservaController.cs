using Decolei.net.DTOs;
using Decolei.net.Interfaces;
using Decolei.net.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Decolei.net.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // A autorização é exigida para todos os endpoints por padrão
    public class ReservaController : ControllerBase
    {
        private readonly IReservaRepository _reservaRepository;
        private readonly IPacoteRepository _pacoteRepository;

        public ReservaController(IReservaRepository reservaRepository, IPacoteRepository pacoteRepository)
        {
            _reservaRepository = reservaRepository;
            _pacoteRepository = pacoteRepository;
        }

        // GET: api/Reserva
        // Endpoint para ATENDENTE e ADMIN verem TODAS as reservas.
        [HttpGet]
        [Authorize(Roles = "ATENDENTE,ADMIN")]
        public async Task<ActionResult<IEnumerable<Reserva>>> GetAll()
        {
            var reservas = await _reservaRepository.ObterTodasAsync();
            return Ok(reservas);
        }

        // GET: api/Reserva/MinhasReservas
        // Endpoint para o CLIENTE logado ver apenas as SUAS reservas.
        [HttpGet("MinhasReservas")]
        public async Task<ActionResult<IEnumerable<Reserva>>> GetMinhasReservas()
        {
            var idUsuarioLogadoString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idUsuarioLogadoString, out var idUsuarioLogado))
            {
                return Unauthorized("Token de usuário inválido.");
            }

            var minhasReservas = await _reservaRepository.ObterPorUsuarioIdAsync(idUsuarioLogado);
            return Ok(minhasReservas);
        }

        // GET: api/Reserva/{id}
        // Endpoint para buscar uma reserva específica pelo ID.
        [HttpGet("{id}")]
        public async Task<ActionResult<Reserva>> GetById(int id)
        {
            var reserva = await _reservaRepository.ObterPorIdAsync(id);
            if (reserva == null)
            {
                return NotFound();
            }

            var idUsuarioLogado = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (reserva.Usuario_Id != idUsuarioLogado && !User.IsInRole("ADMIN") && !User.IsInRole("ATENDENTE"))
            {
                return Forbid(); // Proíbe o acesso se não for o dono ou um perfil autorizado
            }

            return Ok(reserva);
        }

        // POST: api/Reserva
        // Endpoint para CRIAR uma nova reserva.
        [HttpPost]
        public async Task<IActionResult> CriarReserva([FromBody] CriarReservaDto criarReservaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var idUsuarioLogadoString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idUsuarioLogadoString, out var idUsuarioLogado))
            {
                return Unauthorized("Token de usuário inválido.");
            }

            var pacote = await _pacoteRepository.ObterPorIdAsync(criarReservaDto.PacoteViagemId);
            if (pacote == null)
            {
                return NotFound("Pacote de viagem não encontrado.");
            }

            var reserva = new Reserva
            {
                Usuario_Id = idUsuarioLogado,
                PacoteViagem_Id = criarReservaDto.PacoteViagemId,
                Data = DateTime.UtcNow,
                ValorTotal = pacote.Valor,
                Status = "PENDENTE",
                Numero = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper()
            };

            if (criarReservaDto.Viajantes != null && criarReservaDto.Viajantes.Any())
            {
                foreach (var viajanteDto in criarReservaDto.Viajantes)
                {
                    reserva.Viajantes.Add(new Viajante
                    {
                        Nome = viajanteDto.Nome,
                        Documento = viajanteDto.Documento
                    });
                }
            }

            await _reservaRepository.AdicionarAsync(reserva);
            return CreatedAtAction(nameof(GetById), new { id = reserva.Id }, reserva);
        }

        // PUT: api/Reserva/{id}
        // Endpoint para ATUALIZAR o status de uma reserva.
        [HttpPut("{id}")]
        [Authorize(Roles = "ATENDENTE,ADMIN")]
        public async Task<IActionResult> UpdateReserva(int id, [FromBody] UpdateReservaDto updateReservaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var reserva = await _reservaRepository.ObterPorIdAsync(id);
            if (reserva == null)
            {
                return NotFound("Reserva não encontrada.");
            }

            reserva.Status = updateReservaDto.Status.ToUpper();
            await _reservaRepository.AtualizarAsync(reserva);
            return NoContent();
        }
    }
}