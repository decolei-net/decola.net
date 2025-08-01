using Decolei.net.DTOs;
using Decolei.net.Interfaces;
using Decolei.net.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Decolei.net.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReservaController : ControllerBase
    {
        private readonly IReservaRepository _reservaRepository;
        private readonly IPacoteRepository _pacoteRepository;
        private readonly ILogger<ReservaController> _logger;

        public ReservaController(IReservaRepository reservaRepository, IPacoteRepository pacoteRepository, ILogger<ReservaController> logger)
        {
            _reservaRepository = reservaRepository;
            _pacoteRepository = pacoteRepository;
            _logger = logger;
        }

        // Endpoint para ATENDENTE e ADMIN verem TODAS as reservas.
        [HttpGet]
        [Authorize(Roles = "ATENDENTE,ADMIN")]
        public async Task<ActionResult<IEnumerable<ReservaDetalhesDto>>> GetAll()
        {
            try
            {
                var reservas = await _reservaRepository.ObterTodasAsync();
                var reservasDto = reservas.Select(r => MapearParaDto(r)).ToList();
                return Ok(reservasDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro inesperado ao buscar todas as reservas.");
                return StatusCode(500, new { erro = "Ocorreu um erro interno no servidor." });
            }
        }

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
                var minhasReservasDto = minhasReservas.Select(r => MapearParaDto(r)).ToList();
                return Ok(minhasReservasDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao buscar as reservas do usuário {UsuarioId}.", idUsuarioLogado);
                return StatusCode(500, new { erro = "Ocorreu um erro interno ao buscar suas reservas." });
            }
        }

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
                    return Forbid();
                }

                return Ok(MapearParaDto(reserva));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro inesperado ao buscar a reserva com ID {ReservaId}.", id);
                return StatusCode(500, new { erro = "Ocorreu um erro interno no servidor." });
            }
        }

        // Endpoint para CRIAR uma nova reserva.
        [HttpPost]
        public async Task<IActionResult> CriarReserva([FromBody] CriarReservaDto criarReservaDto)
        {
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

                var viajantesNestaReserva = 1 + (criarReservaDto.Viajantes?.Count ?? 0);
                var reservasDoPacote = await _reservaRepository.ObterPorPacoteIdAsync(criarReservaDto.PacoteViagemId);
                var vagasOcupadas = reservasDoPacote.Sum(r => 1 + r.Viajantes.Count);

                if ((vagasOcupadas + viajantesNestaReserva) > pacote.QuantidadeVagas)
                {
                    return BadRequest(new { erro = "Não há vagas disponíveis para este pacote." });
                }

                var valorTotalCalculado = pacote.Valor * viajantesNestaReserva;

                var reserva = new Reserva
                {
                    Usuario_Id = idUsuarioLogado,
                    PacoteViagem_Id = criarReservaDto.PacoteViagemId,
                    Data = DateTime.UtcNow,
                    ValorTotal = valorTotalCalculado,
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

                var novaReserva = await _reservaRepository.ObterPorIdAsync(reserva.Id);

                return CreatedAtAction(nameof(GetById), new { id = novaReserva!.Id }, MapearParaDto(novaReserva!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao criar reserva para o usuário {UsuarioId} no pacote {PacoteId}.", idUsuarioLogado, criarReservaDto.PacoteViagemId);
                return StatusCode(500, new { erro = "Ocorreu um erro interno ao criar a reserva." });
            }
        }

        // Endpoint para ATUALIZAR os viajantes de uma reserva PENDENTE.
        [HttpPut("{id}/viajantes")]
        [Authorize(Roles = "CLIENTE,ADMIN")]
        public async Task<IActionResult> AtualizarViajantes(int id, [FromBody] List<ViajanteDto> novosViajantes)
        {
            var idUsuarioLogadoString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idUsuarioLogadoString, out var idUsuarioLogado))
            {
                return Unauthorized(new { erro = "Token de usuário inválido." });
            }

            var reserva = await _reservaRepository.ObterPorIdAsync(id);
            if (reserva == null) return NotFound(new { erro = "Reserva não encontrada." });
            if (reserva.Usuario_Id != idUsuarioLogado && !User.IsInRole("ADMIN")) return Forbid();
            if (reserva.Status?.ToUpper() != "PENDENTE") return BadRequest(new { erro = "Não é possível alterar uma reserva que não está mais pendente." });

            try
            {
                var pacote = await _pacoteRepository.ObterPorIdAsync(reserva.PacoteViagem_Id);
                if (pacote == null) return BadRequest(new { erro = "O pacote associado a esta reserva não foi encontrado." });

                var viajantesNestaReserva = 1 + (novosViajantes?.Count ?? 0);
                var reservasDoPacote = await _reservaRepository.ObterPorPacoteIdAsync(reserva.PacoteViagem_Id);
                var vagasOcupadasPorOutros = reservasDoPacote.Where(r => r.Id != reserva.Id).Sum(r => 1 + r.Viajantes.Count);
                if ((vagasOcupadasPorOutros + viajantesNestaReserva) > pacote.QuantidadeVagas)
                {
                    return BadRequest(new { erro = "Não há vagas disponíveis para adicionar estes viajantes." });
                }

                reserva.Viajantes.Clear();
                if (novosViajantes != null)
                {
                    foreach (var viajanteDto in novosViajantes)
                    {
                        reserva.Viajantes.Add(new Viajante { Nome = viajanteDto.Nome, Documento = viajanteDto.Documento });
                    }
                }

                reserva.ValorTotal = pacote.Valor * viajantesNestaReserva;
                await _reservaRepository.AtualizarAsync(reserva);

                return Ok(MapearParaDto(reserva));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao atualizar viajantes da reserva {ReservaId}", id);
                return StatusCode(500, new { erro = "Erro interno ao atualizar os viajantes." });
            }
        }

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
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao atualizar a reserva com ID {ReservaId}.", id);
                return StatusCode(500, new { erro = "Ocorreu um erro interno no servidor." });
            }
        }

        // Método auxiliar privado para mapear a entidade Reserva para o DTO de retorno
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
                    Destino = reserva.PacoteViagem.Destino,
                    DataInicio = (DateTime)reserva.PacoteViagem.DataInicio,
                    DataFim = (DateTime)reserva.PacoteViagem.DataFim
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