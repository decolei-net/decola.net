using Decolei.net.Data;
using Decolei.net.DTOs;
using Decolei.net.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Decolei.net.Controllers
{
    [ApiController]
    [Route("api/avaliacoes")]
    public class AvaliacaoController : ControllerBase
    {
        private readonly DecoleiDbContext _context;

        public AvaliacaoController(DecoleiDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = "CLIENTE")]
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

            // ***** A CORREÇÃO ESTÁ AQUI *****
            // Definimos quais status de reserva permitem uma avaliação.
            var statusValidosParaAvaliacao = new[] { "confirmada", "concluida" };

            var reservaValida = await _context.Reservas
                .AnyAsync(r =>
                    r.Usuario_Id == request.Usuario_Id &&
                    r.PacoteViagem_Id == request.PacoteViagem_Id &&
                    // A verificação agora checa se o status da reserva está na lista de status válidos.
                    statusValidosParaAvaliacao.Contains(r.Status.ToLower()));

            if (!reservaValida)
            {
                // Mensagem de erro mais clara para o frontend.
                return BadRequest(new { erro = "Você só pode avaliar pacotes com reservas confirmadas ou concluídas." });
            }

            var avaliacao = new Avaliacao
            {
                Usuario_Id = request.Usuario_Id,
                PacoteViagem_Id = request.PacoteViagem_Id,
                Nota = request.Nota,
                Comentario = request.Comentario,
                Data = DateTime.Now,
                Aprovada = false // Por padrão, toda nova avaliação chega como pendente.
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
                .Include(a => a.Usuario)
                .Select(a => new
                {
                    a.Id,
                    Usuario = a.Usuario.NomeCompleto,
                    a.Nota,
                    a.Comentario,
                    a.Data
                })
                .ToListAsync();

            return Ok(avaliacoes);
        }

        [HttpGet("aprovadas")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> ListarAvaliacoesAprovadas([FromQuery] string? destino)
        {
            var query = _context.Avaliacoes
                .Include(a => a.Usuario)
                .Include(a => a.PacoteViagem)
                .Where(a => a.Aprovada == true);

            if (!string.IsNullOrEmpty(destino))
            {
                query = query.Where(a => a.PacoteViagem.Destino.Contains(destino));
            }

            var aprovadas = await query
                .Select(a => new
                {
                    a.Id,
                    UsuarioId = a.Usuario_Id,
                    PacoteId = a.PacoteViagem_Id,
                    Usuario = a.Usuario.NomeCompleto,
                    Pacote = a.PacoteViagem.Titulo,
                    a.Nota,
                    a.Comentario,
                    a.Data
                })
                .ToListAsync();

            return Ok(aprovadas);
        }

        [HttpGet("pendentes")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> ListarAvaliacoesPendentes([FromQuery] string? destino)
        {
            var query = _context.Avaliacoes
                .Include(a => a.Usuario)
                .Include(a => a.PacoteViagem)
                .Where(a => a.Aprovada == false);

            if (!string.IsNullOrEmpty(destino))
            {
                query = query.Where(a => a.PacoteViagem.Destino.Contains(destino));
            }

            var pendentes = await query
                .Select(a => new
                {
                    a.Id,
                    UsuarioId = a.Usuario_Id,
                    PacoteId = a.PacoteViagem_Id,
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
        [Authorize(Roles = "ADMIN")]
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

        [HttpGet("minhas-avaliacoes")]
        [Authorize(Roles = "CLIENTE")]
        public async Task<IActionResult> GetMinhasAvaliacoes()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Não foi possível identificar o usuário a partir do token.");
            }

            var avaliacoesDoUsuario = await _context.Avaliacoes
                .Where(a => a.Usuario_Id == userId)
                .Include(a => a.PacoteViagem)
                .Select(a => new
                {
                    Id = a.Id,
                    Nota = a.Nota,
                    Comentario = a.Comentario,
                    DataCriacao = a.Data, // Renomeado para consistência, se preferir
                    Status = (bool)a.Aprovada ? "APROVADA" : "PENDENTE", // Adicionando o status
                    Pacote = new
                    {
                        Id = a.PacoteViagem.Id,
                        Titulo = a.PacoteViagem.Titulo,
                        Destino = a.PacoteViagem.Destino
                    }
                })
                .OrderByDescending(a => a.DataCriacao)
                .ToListAsync();

            return Ok(avaliacoesDoUsuario);
        }
    }
}