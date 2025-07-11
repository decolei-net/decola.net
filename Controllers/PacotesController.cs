using Decolei.net.DTOs;
using Decolei.net.Interfaces; // MUDOU AQUI
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
        public async Task<ActionResult<PacoteViagem>> CriarPacote([FromBody] CriarPacoteViagemDto pacoteDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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
                DataFim = pacoteDto.DataFim
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
    }
}