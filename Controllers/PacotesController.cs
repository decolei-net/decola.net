using Decolei.net.DTOs;
using Decolei.net.Interfaces;
using Decolei.net.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string? destino, [FromQuery] decimal? precoMin, [FromQuery] decimal? precoMax,
            [FromQuery] DateTime? dataInicio, [FromQuery] DateTime? dataFim)
        {
            try
            {
                var pacotes = await _pacoteRepository.GetByFiltersAsync(destino, precoMin, precoMax, dataInicio, dataFim);
                return Ok(pacotes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao buscar os pacotes com filtros.");
                return StatusCode(500, new { erro = "Ocorreu um erro inesperado no servidor. Tente novamente mais tarde." });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { erro = "O ID do pacote deve ser um número positivo." });
            }

            try
            {
                var pacote = await _pacoteRepository.ObterPorIdAsync(id);
                if (pacote == null)
                {
                    return NotFound(new { erro = $"Pacote com ID {id} não encontrado." });
                }
                return Ok(pacote);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao buscar o pacote por ID {PacoteId}.", id);
                return StatusCode(500, new { erro = "Ocorreu um erro inesperado no servidor." });
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN,ADMINISTRADOR")]
        public async Task<ActionResult<PacoteViagem>> CriarPacote([FromBody] CriarPacoteViagemDto criarPacoteDto)
        {
            // O [ApiController] já trata o `ModelState.IsValid`, retornando um 400 Bad Request detalhado.

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
                    ImagemURL = criarPacoteDto.ImagemURL,
                    VideoURL = criarPacoteDto.VideoURL,
                    Destino = criarPacoteDto.Destino,
                    Valor = criarPacoteDto.Valor,
                    DataInicio = criarPacoteDto.DataInicio,
                    DataFim = criarPacoteDto.DataFim,
                    UsuarioId = idUsuarioLogado
                };

                await _pacoteRepository.AdicionarAsync(pacote);

                return CreatedAtAction(nameof(GetById), new { id = pacote.Id }, pacote);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao criar um novo pacote para o usuário {UsuarioId}.", idUsuarioLogado);
                return StatusCode(500, new { erro = "Ocorreu um erro interno ao criar o pacote." });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN,ADMINISTRADOR")]
        public async Task<IActionResult> AtualizarPacote(int id, [FromBody] UpdatePacoteViagemDto dto)
        {
            if (id <= 0)
            {
                return BadRequest(new { erro = "O ID do pacote deve ser um número positivo." });
            }

            try
            {
                var pacote = await _pacoteRepository.ObterPorIdAsync(id);
                if (pacote == null)
                {
                    return NotFound(new { erro = $"Pacote com ID {id} não encontrado para atualização." });
                }

                if (dto.Titulo != null) pacote.Titulo = dto.Titulo;
                if (dto.Descricao != null) pacote.Descricao = dto.Descricao;
                if (dto.ImagemURL != null) pacote.ImagemURL = dto.ImagemURL;
                if (dto.VideoURL != null) pacote.VideoURL = dto.VideoURL;
                if (dto.Destino != null) pacote.Destino = dto.Destino;
                if (dto.Valor.HasValue) pacote.Valor = dto.Valor.Value;
                if (dto.DataInicio.HasValue) pacote.DataInicio = dto.DataInicio.Value;
                if (dto.DataFim.HasValue) pacote.DataFim = dto.DataFim.Value;

                await _pacoteRepository.AtualizarAsync(pacote);
                return Ok(new { mensagem = "Pacote atualizado com sucesso!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao atualizar o pacote com ID {PacoteId}.", id);
                return StatusCode(500, new { erro = "Ocorreu um erro interno ao atualizar o pacote." });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN,ADMINISTRADOR")]
        public async Task<IActionResult> DeletarPacote(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { erro = "O ID do pacote deve ser um número positivo." });
            }

            try
            {
                var pacote = await _pacoteRepository.ObterPorIdAsync(id);
                if (pacote == null)
                {
                    return NotFound(new { erro = $"Pacote com ID {id} não encontrado para exclusão." });
                }

                await _pacoteRepository.RemoverAsync(pacote);
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Tentativa de deletar pacote com ID {PacoteId} que possui reservas atreladas.", id);
                return Conflict(new { erro = "Este pacote não pode ser excluído pois possui reservas ou outros registros associados." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao deletar o pacote com ID {PacoteId}.", id);
                return StatusCode(500, new { erro = "Ocorreu um erro interno ao deletar o pacote." });
            }
        }
    }
}