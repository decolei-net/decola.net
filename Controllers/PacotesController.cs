using Decolei.net.DTOs;
using Decolei.net.Interfaces;
using Decolei.net.Models;
using Microsoft.AspNetCore.Mvc;

namespace Decolei.net.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PacotesController : ControllerBase
    {
        private readonly IPacoteRepository _pacoteRepository;

        public PacotesController(IPacoteRepository pacoteRepository)
        {
            _pacoteRepository = pacoteRepository;
        }

        // MÉTODO GET MODIFICADO PARA ACEITAR FILTROS OPCIONAIS
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string? destino,
            [FromQuery] decimal? precoMin,
            [FromQuery] decimal? precoMax,
            [FromQuery] DateTime? dataInicio,
            [FromQuery] DateTime? dataFim)
        {
            try
            {
                bool hasFilters = !string.IsNullOrWhiteSpace(destino) ||
                                  precoMin.HasValue || precoMax.HasValue ||
                                  dataInicio.HasValue || dataFim.HasValue;

                IEnumerable<PacoteViagem> pacotes;

                if (hasFilters)
                {
                    pacotes = await _pacoteRepository.GetByFiltersAsync(
                        destino, precoMin, precoMax, dataInicio, dataFim);
                }
                else
                {
                    pacotes = await _pacoteRepository.ListarTodosAsync();
                }

                return Ok(pacotes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocorreu um erro interno: {ex.Message}");
            }
        }

        // MÉTODO GET PARA OBTER UM PACOTE PELO ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var pacote = await _pacoteRepository.ObterPorIdAsync(id);
                if (pacote == null)
                {
                    return NotFound($"Pacote com ID {id} não encontrado.");
                }
                return Ok(pacote);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocorreu um erro interno: {ex.Message}");
            }
        }

        // MÉTODO POST PARA CRIAR UM NOVO PACOTE
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CriarPacoteViagemDto criarPacoteDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Mapeamento correto do DTO para o Modelo de domínio
                var pacote = new PacoteViagem
                {
                    Titulo = criarPacoteDto.Titulo,
                    Descricao = criarPacoteDto.Descricao,
                    ImagemURL = criarPacoteDto.ImagemURL,
                    VideoURL = criarPacoteDto.VideoURL,
                    Destino = criarPacoteDto.Destino,
                    Valor = criarPacoteDto.Valor,
                    DataInicio = criarPacoteDto.DataInicio,
                    DataFim = criarPacoteDto.DataFim
                };

                await _pacoteRepository.AdicionarAsync(pacote);

                return CreatedAtAction(nameof(GetById), new { id = pacote.Id }, pacote);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocorreu um erro interno ao criar o pacote: {ex.Message}");
            }
        }
    }
}