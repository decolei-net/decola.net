using Decolei.net.DTOs;
using System.Security.Claims; // para ClaimsPrincipal 
using Decolei.net.Interfaces;
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
                // Esta lógica de filtros pode ser movida para o repositório no futuro para um código mais limpo.
                var pacotes = await _pacoteRepository.ListarTodosAsync();

                if (!string.IsNullOrWhiteSpace(destino))
                {
                    pacotes = pacotes.Where(p => p.Destino!.Contains(destino, StringComparison.OrdinalIgnoreCase));
                }
                if (precoMin.HasValue)
                {
                    pacotes = pacotes.Where(p => p.Valor >= precoMin.Value);
                }
                if (precoMax.HasValue)
                {
                    pacotes = pacotes.Where(p => p.Valor <= precoMax.Value);
                }
                if (dataInicio.HasValue)
                {
                    pacotes = pacotes.Where(p => p.DataInicio >= dataInicio.Value);
                }
                if (dataFim.HasValue)
                {
                    pacotes = pacotes.Where(p => p.DataFim <= dataFim.Value);
                }

                return Ok(pacotes.ToList());
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
        [Authorize(Roles = "ADMINISTRADOR,ADMIN")] // Aceita ambos os nomes para garantir
        public async Task<ActionResult<PacoteViagem>> CriarPacote([FromBody] CriarPacoteViagemDto criarPacoteDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var idUsuarioLogadoString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(idUsuarioLogadoString))
                {
                    return Unauthorized("Não foi possível identificar o usuário.");
                }
                var idUsuarioLogado = int.Parse(idUsuarioLogadoString);

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
                    // --- AQUI ESTÁ A CORREÇÃO CRÍTICA ---
                    UsuarioId = idUsuarioLogado
                };

                await _pacoteRepository.AdicionarAsync(pacote);

                return CreatedAtAction(nameof(GetById), new { id = pacote.Id }, pacote);
            }
            catch (Exception ex)
            {
                // Fornece um erro mais detalhado no log do servidor (console) para depuração
                Console.WriteLine($"ERRO AO CRIAR PACOTE: {ex.ToString()}");
                return StatusCode(500, $"Ocorreu um erro interno ao criar o pacote. Verifique os logs para mais detalhes.");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMINISTRADOR,ADMIN")]
        public async Task<IActionResult> AtualizarPacote(int id, [FromBody] UpdatePacoteViagemDto dto)
        {
            var pacote = await _pacoteRepository.ObterPorIdAsync(id);
            if (pacote == null)
            {
                return NotFound("Pacote Não encontrado.");
            }

            // Atualiza apenas os campos que foram fornecidos no DTO
            if (dto.Titulo != null) pacote.Titulo = dto.Titulo;
            if (dto.Descricao != null) pacote.Descricao = dto.Descricao;
            if (dto.ImagemURL != null) pacote.ImagemURL = dto.ImagemURL;
            if (dto.VideoURL != null) pacote.VideoURL = dto.VideoURL;
            if (dto.Destino != null) pacote.Destino = dto.Destino;
            if (dto.Valor.HasValue) pacote.Valor = dto.Valor.Value;
            if (dto.DataInicio.HasValue) pacote.DataInicio = dto.DataInicio.Value;
            if (dto.DataFim.HasValue) pacote.DataFim = dto.DataFim.Value;

            await _pacoteRepository.AtualizarAsync(pacote);
            return Ok(new { mensagem = "Pacote atualizado com sucesso!", pacote });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMINISTRADOR,ADMIN")]
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
