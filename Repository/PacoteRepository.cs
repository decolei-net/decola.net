using Decolei.net.Data;
using Decolei.net.Interfaces;
using Decolei.net.Models;
using Microsoft.EntityFrameworkCore;

namespace Decolei.net.Repository
{
    public class PacoteRepository : IPacoteRepository
    {
        private readonly DecoleiDbContext _context;

        public PacoteRepository(DecoleiDbContext context)
        {
            _context = context;
        }

        // Função auxiliar para evitar repetição de código
        private IQueryable<PacoteViagem> GetPacotesComIncludes()
        {
            return _context.PacotesViagem
                .Include(p => p.Reservas) // Inclui a lista de reservas do pacote
                    .ThenInclude(r => r.Viajantes) // Para cada reserva, inclui os viajantes
                .Include(p => p.Avaliacoes) // Inclui as avaliações do pacote
                    .ThenInclude(a => a.Usuario); // Para cada avaliação, inclui o usuário que fez a avaliação
        }

        public async Task<IEnumerable<PacoteViagem>> GetByFiltersAsync(string? destino, decimal? precoMin, decimal? precoMax, DateTime? dataInicio, DateTime? dataFim)
        {
            var query = GetPacotesComIncludes(); // Usa a função auxiliar

            if (!string.IsNullOrEmpty(destino))
            {
                query = query.Where(p => p.Destino.Contains(destino));
            }
            if (precoMin.HasValue)
            {
                query = query.Where(p => p.Valor >= precoMin.Value);
            }
            if (precoMax.HasValue)
            {
                query = query.Where(p => p.Valor <= precoMax.Value);
            }
            if (dataInicio.HasValue)
            {
                query = query.Where(p => p.DataInicio >= dataInicio.Value);
            }
            if (dataFim.HasValue)
            {
                query = query.Where(p => p.DataFim <= dataFim.Value);
            }
            return await query.ToListAsync();
        }

        public async Task<PacoteViagem?> ObterPorIdAsync(int id)
        {
            return await GetPacotesComIncludes().FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AdicionarAsync(PacoteViagem pacote)
        {
            _context.PacotesViagem.Add(pacote);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarAsync(PacoteViagem pacote)
        {
            _context.Entry(pacote).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task RemoverAsync(PacoteViagem pacote)
        {
            _context.PacotesViagem.Remove(pacote);
            await _context.SaveChangesAsync();
        }

        // Se a sua interface IPacoteRepository tiver o método ListarTodosAsync, adicione-o também.
        public async Task<IEnumerable<PacoteViagem>> ListarTodosAsync()
        {
            return await GetPacotesComIncludes().ToListAsync();
        }
    }
}