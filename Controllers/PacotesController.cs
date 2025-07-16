using Decolei.net.DTOs;
using System.Security.Claims; // para ClaimsPrincipal 
using Decolei.net.Interfaces; // MUDOU AQUI
using Microsoft.AspNetCore.Authorization; // Para usar [Authorize]
using Decolei.net.Models;
using Microsoft.AspNetCore.Mvc;

namespace Decolei.net.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PacotesController : ControllerBase
    {
        private readonly IPacoteRepository _pacoteRepository; // MUDOU AQUI

        public PacotesController(IPacoteRepository pacoteRepository)
        {
            _pacoteRepository = pacoteRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PacoteViagem>>> GetPacotes()
        {
            var pacotes = await _pacoteRepository.ListarTodosAsync(); // MUDOU AQUI
            return Ok(pacotes);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<PacoteViagem>> CriarPacote([FromBody] CriarPacoteViagemDto pacoteDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // --- INÍCIO DA MUDANÇA ---

            // 1. Pegar o ID do usuário logado a partir do token.
            // A claim 'NameIdentifier' guarda o ID do usuário por padrão no Identity.
            var idUsuarioLogadoString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idUsuarioLogadoString))
            {
                return Unauthorized("Não foi possível identificar o usuário.");
            }
            var idUsuarioLogado = int.Parse(idUsuarioLogadoString);

            // --- FIM DA MUDANÇA ---

            var novoPacote = new PacoteViagem
            {
                // ... mapeamento igual ao de antes
                Titulo = pacoteDto.Titulo,
                Descricao = pacoteDto.Descricao,
                ImagemURL = pacoteDto.ImagemURL,
                VideoURL = pacoteDto.VideoURL,
                Destino = pacoteDto.Destino,
                Valor = pacoteDto.Valor,
                DataInicio = pacoteDto.DataInicio,
                DataFim = pacoteDto.DataFim,
                UsuarioId = idUsuarioLogado
            };

            await _pacoteRepository.AdicionarAsync(novoPacote); // MUDOU AQUI

            // A lógica de retorno continua a mesma
            return CreatedAtAction(nameof(GetPacote), new { id = novoPacote.Id }, novoPacote);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PacoteViagem>> GetPacote(int id)
        {
            var pacote = await _pacoteRepository.ObterPorIdAsync(id); // MUDOU AQUI

            if (pacote == null)
            {
                return NotFound();
            }

            return Ok(pacote);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> AtualizarPacote(int id, [FromBody] UpdatePacoteViagemDto dto)
        {
            var pacote = await _pacoteRepository.ObterPorIdAsync(id);
            if(pacote == null)
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