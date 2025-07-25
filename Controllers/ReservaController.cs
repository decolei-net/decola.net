﻿using Decolei.net.DTOs;
using Decolei.net.Interfaces;
using Decolei.net.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Decolei.net.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // A autorização é exigida para todos os endpoints por padrão
    public class ReservaController : ControllerBase
    {
        private readonly IReservaRepository _reservaRepository;
        private readonly IPacoteRepository _pacoteRepository;
        private readonly ILogger<ReservaController> _logger; // INJEÇÃO DO LOGGER

        public ReservaController(IReservaRepository reservaRepository, IPacoteRepository pacoteRepository, ILogger<ReservaController> logger)
        {
            _reservaRepository = reservaRepository;
            _pacoteRepository = pacoteRepository;
            _logger = logger; // ATRIBUIÇÃO DO LOGGER
        }

        // GET: api/Reserva
        // Endpoint para ATENDENTE e ADMIN verem TODAS as reservas.
        [HttpGet]
        [Authorize(Roles = "ATENDENTE,ADMIN")]
        public async Task<ActionResult<IEnumerable<ReservaDetalhesDto>>> GetAll()
        {
            try
            {
                var reservas = await _reservaRepository.ObterTodasAsync();
                // Mapeia a lista de Reserva para uma lista de ReservaDetalhesDto
                var reservasDto = reservas.Select(r => MapearParaDto(r)).ToList();
                return Ok(reservasDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro inesperado ao buscar todas as reservas.");
                return StatusCode(500, new { erro = "Ocorreu um erro interno no servidor." });
            }
        }

        // GET: api/Reserva/MinhasReservas
        // Endpoint para o CLIENTE logado ver apenas as SUAS reservas.
        [HttpGet("minhas-reservas")]
        public async Task<ActionResult<IEnumerable<ReservaDetalhesDto>>> GetMinhasReservas()
        {
            var idUsuarioLogadoString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idUsuarioLogadoString, out var idUsuarioLogado))
            {
                return Unauthorized(new { erro = "Token de usuário inválido." });
            }

            try
            {
                var minhasReservas = await _reservaRepository.ObterPorUsuarioIdAsync(idUsuarioLogado);
                // Mapeia a lista de Reserva para uma lista de ReservaDetalhesDto
                var minhasReservasDto = minhasReservas.Select(r => MapearParaDto(r)).ToList();
                return Ok(minhasReservasDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao buscar as reservas do usuário {UsuarioId}.", idUsuarioLogado);
                return StatusCode(500, new { erro = "Ocorreu um erro interno ao buscar suas reservas." });
            }
        }

        // GET: api/Reserva/{id}
        // Endpoint para buscar uma reserva específica pelo ID.
        [HttpGet("{id}")]
        public async Task<ActionResult<ReservaDetalhesDto>> GetById(int id)
        {
            if (id <= 0) return BadRequest(new { erro = "O ID da reserva deve ser um número positivo." });

            try
            {
                var reserva = await _reservaRepository.ObterPorIdAsync(id);
                if (reserva == null)
                {
                    return NotFound(new { erro = $"Reserva com ID {id} não encontrada." });
                }

                var idUsuarioLogadoString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(idUsuarioLogadoString, out var idUsuarioLogado))
                {
                    return Unauthorized(new { erro = "Token de usuário inválido." });
                }

                if (reserva.Usuario_Id != idUsuarioLogado && !User.IsInRole("ADMIN") && !User.IsInRole("ATENDENTE"))
                {
                    return Forbid(); // Proíbe o acesso se não for o dono ou um perfil autorizado
                }

                // Mapeia a entidade Reserva para o DTO e retorna
                return Ok(MapearParaDto(reserva));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro inesperado ao buscar a reserva com ID {ReservaId}.", id);
                return StatusCode(500, new { erro = "Ocorreu um erro interno no servidor." });
            }
        }

        // POST: api/Reserva
        // Endpoint para CRIAR uma nova reserva.
        [HttpPost]
        public async Task<IActionResult> CriarReserva([FromBody] CriarReservaDto criarReservaDto)
        {
            // O [ApiController] já trata o `ModelState.IsValid`, retornando 400 se necessário.

            var idUsuarioLogadoString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idUsuarioLogadoString, out var idUsuarioLogado))
            {
                return Unauthorized(new { erro = "Não foi possível identificar o usuário a partir do token." });
            }

            try
            {
                var pacote = await _pacoteRepository.ObterPorIdAsync(criarReservaDto.PacoteViagemId);
                if (pacote == null)
                {
                    return NotFound(new { erro = $"Pacote de viagem com ID {criarReservaDto.PacoteViagemId} não encontrado." });
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

                // Busca a reserva completa para retornar os detalhes no DTO
                var novaReserva = await _reservaRepository.ObterPorIdAsync(reserva.Id);

                // CORREÇÃO: Retorna o DTO em vez da entidade para evitar o ciclo
                return CreatedAtAction(nameof(GetById), new { id = novaReserva!.Id }, MapearParaDto(novaReserva!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao criar reserva para o usuário {UsuarioId} no pacote {PacoteId}.", idUsuarioLogado, criarReservaDto.PacoteViagemId);
                return StatusCode(500, new { erro = "Ocorreu um erro interno ao criar a reserva." });
            }
        }

        // PUT: api/Reserva/{id}
        // Endpoint para ATUALIZAR o status de uma reserva.
        [HttpPut("{id}")]
        [Authorize(Roles = "ATENDENTE,ADMIN")]
        public async Task<IActionResult> UpdateReserva(int id, [FromBody] UpdateReservaDto updateReservaDto)
        {
            if (id <= 0) return BadRequest(new { erro = "O ID da reserva deve ser um número positivo." });
            if (string.IsNullOrWhiteSpace(updateReservaDto.Status))
            {
                return BadRequest(new { erro = "O campo 'Status' não pode ser vazio." });
            }

            try
            {
                var reserva = await _reservaRepository.ObterPorIdAsync(id);
                if (reserva == null)
                {
                    return NotFound(new { erro = $"Reserva com ID {id} não encontrada." });
                }

                reserva.Status = updateReservaDto.Status.ToUpper();
                await _reservaRepository.AtualizarAsync(reserva);
                return NoContent(); // Este método está correto, não precisa de DTO.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro inesperado ao atualizar a reserva com ID {ReservaId}.", id);
                return StatusCode(500, new { erro = "Ocorreu um erro interno no servidor." });
            }
        }

        // Método auxiliar privado para mapear a entidade Reserva para o DTO
        private ReservaDetalhesDto MapearParaDto(Reserva reserva)
        {
            return new ReservaDetalhesDto
            {
                Id = reserva.Id,
                Data = reserva.Data,
                ValorTotal = reserva.ValorTotal,
                Status = reserva.Status,
                Numero = reserva.Numero,
                PacoteViagem = reserva.PacoteViagem != null ? new PacoteReservaDto
                {
                    Id = reserva.PacoteViagem.Id,
                    Titulo = reserva.PacoteViagem.Titulo,
                    Destino = reserva.PacoteViagem.Destino
                } : null,
                Usuario = reserva.Usuario != null ? new UsuarioReservaDto
                {
                    Id = reserva.Usuario.Id,
                    NomeCompleto = reserva.Usuario.NomeCompleto,
                    Email = reserva.Usuario.Email
                } : null,
                Viajantes = reserva.Viajantes.Select(v => new ViajanteDto
                {
                    Nome = v.Nome,
                    Documento = v.Documento
                }).ToList()
            };
        }
    }
}