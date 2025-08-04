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
    public class PacotesController : ControllerBase
    {
        private readonly IPacoteRepository _pacoteRepository;
        private readonly ILogger<PacotesController> _logger;

        public PacotesController(IPacoteRepository pacoteRepository, ILogger<PacotesController> logger)
        {
            _pacoteRepository = pacoteRepository;
            _logger = logger;
        }

        // MÉTODO DE MAPEAMENTO ATUALIZADO PARA MÍDIA
        private object MapearParaDtoComVagas(PacoteViagem pacote)
        {
            var vagasOcupadas = pacote.Reservas?.Sum(r => 1 + (r.Viajantes?.Count ?? 0)) ?? 0;
            var vagasDisponiveis = pacote.QuantidadeVagas - vagasOcupadas;

            return new
            {
                pacote.Id,
                pacote.Titulo,
                pacote.Descricao,
                Imagens = pacote.Imagens?.Select(midia => new { midia.Url, midia.IsVideo }).ToList(),
                pacote.Destino,
                pacote.Valor,
                pacote.DataInicio,
                pacote.DataFim,
                pacote.UsuarioId,
                pacote.QuantidadeVagas,
                VagasDisponiveis = vagasDisponiveis,
                Usuario = pacote.Usuario == null ? null : new
                {
                    pacote.Usuario.Id,
                    pacote.Usuario.UserName,
                    pacote.Usuario.Email
                },
                Avaliacoes = pacote.Avaliacoes?.Select(a => new {
                    a.Id,
                    a.Comentario,
                    a.Nota,
                    UsuarioNome = a.Usuario?.NomeCompleto,
                    Data = a.Data
                }).ToList(),
                Reservas = pacote.Reservas?.Select(r => new {
                    r.Id,
                    r.Usuario_Id,
                    r.Data,
                    r.ValorTotal,
                    r.Status,
                    r.Numero,
                    Viajantes = r.Viajantes?.Select(v => new { v.Id, v.Nome, v.Documento }).ToList()
                }).ToList()
            };
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? destino, [FromQuery] decimal? precoMin, [FromQuery] decimal? precoMax, [FromQuery] DateTime? dataInicio, [FromQuery] DateTime? dataFim)
        {
            try
            {
                var pacotes = await _pacoteRepository.GetByFiltersAsync(destino, precoMin, precoMax, dataInicio, dataFim);
                var pacotesComVagas = pacotes.Select(MapearParaDtoComVagas);
                return Ok(pacotesComVagas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar pacotes com filtros.");
                return StatusCode(500, new { erro = "Erro interno no servidor." });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var pacote = await _pacoteRepository.ObterPorIdAsync(id);
                if (pacote == null)
                {
                    return NotFound(new { erro = $"Pacote com ID {id} não encontrado." });
                }
                var pacoteComVagas = MapearParaDtoComVagas(pacote);
                return Ok(pacoteComVagas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar pacote por ID {PacoteId}.", id);
                return StatusCode(500, new { erro = "Erro interno no servidor." });
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN,ADMINISTRADOR")]
        public async Task<ActionResult<PacoteViagem>> CriarPacote([FromBody] CriarPacoteViagemDto criarPacoteDto)
        {
            // Lembre-se de remover a propriedade VideoURL do seu DTO CriarPacoteViagemDto
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var idUsuarioLogado))
            {
                return Unauthorized(new { erro = "Não foi possível identificar o usuário a partir do token." });
            }
            try
            {
                var pacote = new PacoteViagem
                {
                    Titulo = criarPacoteDto.Titulo,
                    Descricao = criarPacoteDto.Descricao,
                    Destino = criarPacoteDto.Destino,
                    Valor = criarPacoteDto.Valor,
                    DataInicio = criarPacoteDto.DataInicio,
                    DataFim = criarPacoteDto.DataFim,
                    UsuarioId = idUsuarioLogado,
                    QuantidadeVagas = criarPacoteDto.QuantidadeVagas,

                };
                await _pacoteRepository.AdicionarAsync(pacote);
                return CreatedAtAction(nameof(GetById), new { id = pacote.Id }, pacote);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao criar um novo pacote para o usuário {UsuarioId}.", idUsuarioLogado);
                return StatusCode(500, new { erro = "Erro interno ao criar o pacote." });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN,ADMINISTRADOR")]
        public async Task<IActionResult> AtualizarPacote(int id, [FromBody] UpdatePacoteViagemDto dto)
        {
            // Lembre-se de remover a propriedade VideoURL do seu DTO UpdatePacoteViagemDto
            var pacote = await _pacoteRepository.ObterPorIdAsync(id);
            if (pacote == null) return NotFound();

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var idUsuarioLogado) || (pacote.UsuarioId != idUsuarioLogado && !User.IsInRole("ADMIN")))
            {
                return Forbid();
            }

            if (dto.Titulo != null) pacote.Titulo = dto.Titulo;
            if (dto.Descricao != null) pacote.Descricao = dto.Descricao;
            if (dto.Destino != null) pacote.Destino = dto.Destino;
            if (dto.Valor.HasValue) pacote.Valor = dto.Valor.Value;
            if (dto.DataInicio.HasValue) pacote.DataInicio = dto.DataInicio.Value;
            if (dto.DataFim.HasValue) pacote.DataFim = dto.DataFim.Value;

            await _pacoteRepository.AtualizarAsync(pacote);
            return Ok(new { mensagem = "Pacote atualizado com sucesso!" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN,ADMINISTRADOR")]
        public async Task<IActionResult> DeletarPacote(int id)
        {
            var pacote = await _pacoteRepository.ObterPorIdAsync(id);
            if (pacote == null) return NotFound();

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var idUsuarioLogado) || (pacote.UsuarioId != idUsuarioLogado && !User.IsInRole("ADMIN")))
            {
                return Forbid();
            }

            await _pacoteRepository.RemoverAsync(pacote);
            return NoContent();
        }
    }
}