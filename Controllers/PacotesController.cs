using Decolei.net.DTOs;
using System.Security.Claims; // para ClaimsPrincipal 
using Decolei.net.Interfaces; // MUDOU AQUI
using Microsoft.AspNetCore.Authorization; // Para usar [Authorize]
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
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<PacoteViagem>> CriarPacote([FromBody] CriarPacoteViagemDto criarPacoteDto) // Corrigido o nome do parâmetro
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // 1. Pegar o ID do usuário logado a partir do token.
                // A claim 'NameIdentifier' guarda o ID do usuário por padrão no Identity.
                var idUsuarioLogadoString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(idUsuarioLogadoString))
                {
                    return Unauthorized("Não foi possível identificar o usuário.");
                }
                var idUsuarioLogado = int.Parse(idUsuarioLogadoString);

                // 2. Criar o novo pacote de viagem
                var pacote = new PacoteViagem
                {
                    Titulo = criarPacoteDto.Titulo, // Corrigido o nome do parâmetro
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

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> AtualizarPacote(int id, [FromBody] UpdatePacoteViagemDto dto)
        {
            var pacote = await _pacoteRepository.ObterPorIdAsync(id);
            if (pacote == null)
            {
                return NotFound("Pacote Não encontrado.");
            }
            if (dto.Titulo != null) pacote.Titulo = dto.Titulo;
            if (dto.Descricao != null) pacote.Descricao = dto.Descricao;
            if (dto.ImagemURL != null) pacote.ImagemURL = dto.ImagemURL;
            if (dto.VideoURL != null) pacote.VideoURL = dto.VideoURL;
            if (dto.Destino != null) pacote.Destino = dto.Destino;
            if (dto.Valor.HasValue) pacote.Valor = dto.Valor;
            if (dto.DataInicio.HasValue) pacote.DataInicio = dto.DataInicio;
            if (dto.DataFim.HasValue) pacote.DataFim = dto.DataFim;

            await _pacoteRepository.AtualizarAsync(pacote);
            return Ok(new { mensagem = "Pacote atualizado com sucesso!", pacote });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeletarPacote(int id)
        {
            var pacote = await _pacoteRepository.ObterPorIdAsync(id);
            if (pacote == null)
            {
                return NotFound("Pacote não encontrado");
            }

            await _pacoteRepository.RemoverAsync(pacote);
            return Ok(new { mensagem = "Pacote excluído com sucesso!" });
        }
    }
}