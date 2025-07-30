using Decolei.net.Data;
using Decolei.net.DTOs;
using Decolei.net.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Decolei.net.Controllers
{
    [ApiController]
    [Route("avaliacoes")]
    public class AvaliacaoController : ControllerBase
    {
        private readonly DecoleiDbContext _context;

        public AvaliacaoController(DecoleiDbContext context)
        {
            _context = context;
        }

        [HttpPost]

        public async Task<IActionResult> AvaliarPacote([FromBody] AvaliacaoRequest request)

        {

            if (request.Nota < 1 || request.Nota > 5)

                return BadRequest("Nota deve estar entre 1 e 5.");

            var pacote = await _context.PacotesViagem

                .FirstOrDefaultAsync(p => p.Id == request.PacoteViagem_Id);

            if (pacote == null)

                return NotFound("Pacote de viagem não encontrado.");

            if (DateTime.Now < pacote.DataFim)

                return BadRequest("Você só pode avaliar esse pacote após o término da viagem.");

            var avaliacaoExistente = await _context.Avaliacoes

                .AnyAsync(a => a.Usuario_Id == request.Usuario_Id && a.PacoteViagem_Id == request.PacoteViagem_Id);

            if (avaliacaoExistente)

                return BadRequest("Você já avaliou este pacote anteriormente.");

            var reservaValida = await _context.Reservas

                .AnyAsync(r =>
                    r.Usuario_Id == request.Usuario_Id &&
                    r.PacoteViagem_Id == request.PacoteViagem_Id &&
                    r.Status.ToLower() == "aprovado");

            if (!reservaValida)

                return BadRequest("Você só pode avaliar pacotes que você reservou e estão confirmadas.");

            var avaliacao = new Avaliacao

            {
                Usuario_Id = request.Usuario_Id,
                PacoteViagem_Id = request.PacoteViagem_Id,
                Nota = request.Nota,
                Comentario = request.Comentario,
                Data = DateTime.Now,
                Aprovada = false
            };

            _context.Avaliacoes.Add(avaliacao);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Avaliação registrada com sucesso." });

        }



        [HttpGet("pacote/{id}")]
        public async Task<IActionResult> AvaliacoesAprovadasPorPacote(int id)
        {
            var avaliacoes = await _context.Avaliacoes
                .Where(a => a.PacoteViagem_Id == id && a.Aprovada == true)
                .Select(a => new {
                    a.Id,
                    Usuario = a.Usuario.NomeCompleto,
                    a.Nota,
                    a.Comentario,
                    a.Data
                })
                .ToListAsync();

            return Ok(avaliacoes);
        }


        [HttpGet("pendentes")]
        public async Task<IActionResult> ListarAvaliacoesPendentes()
        {
            var pendentes = await _context.Avaliacoes
                .Where(a => a.Aprovada == false)
                .Select(a => new {
                    a.Id,
                    Usuario = a.Usuario.NomeCompleto,
                    Pacote = a.PacoteViagem.Titulo,
                    a.Nota,
                    a.Comentario,
                    a.Data
                })
                .ToListAsync();

            return Ok(pendentes);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarStatusAvaliacao(int id, [FromBody] AvaliacaoAcaoDto dto)
        {
            var avaliacao = await _context.Avaliacoes.FindAsync(id);

            if (avaliacao == null)
                return NotFound("Avaliação não encontrada.");

            if (dto.Acao?.ToLower() == "aprovar")
            {
                if (avaliacao.Aprovada == true)
                    return BadRequest("Avaliação já está aprovada.");

                avaliacao.Aprovada = true;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Avaliação aprovada com sucesso." });
            }
            else if (dto.Acao?.ToLower() == "rejeitar")
            {
                _context.Avaliacoes.Remove(avaliacao);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Avaliação rejeitada e excluída com sucesso." });
            }

            return BadRequest("Ação inválida. Use 'aprovar' ou 'rejeitar'.");
        }


    }

}
