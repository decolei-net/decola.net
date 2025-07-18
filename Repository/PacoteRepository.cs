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

        public async Task AdicionarAsync(PacoteViagem pacote)
        {
            _context.PacotesViagem.Add(pacote);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<PacoteViagem>> ListarTodosAsync()
        {
            return await _context.PacotesViagem.ToListAsync();
        }

        public async Task<PacoteViagem> ObterPorIdAsync(int id)
        {
            return await _context.PacotesViagem.FindAsync(id);
        }

        // Novo método completo com filtros usando os nomes de propriedade do seu modelo C#
        public async Task<IEnumerable<PacoteViagem>> GetByFiltersAsync(
            string? destino,
            decimal? precoMin,
            decimal? precoMax,
            DateTime? dataInicio,
            DateTime? dataFim)
        {
            // Começamos com uma query "aberta" que representa a tabela inteira.
            var query = _context.PacotesViagem.AsQueryable();

            // O seu DbContext já sabe como traduzir "p.Destino" para "PacoteViagem_Destino" no SQL.
            if (!string.IsNullOrWhiteSpace(destino))
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

            // Somente aqui, no final, a query SQL é realmente construída e enviada ao banco.
            return await query.ToListAsync();
        }
        public async Task AtualizarAsync(PacoteViagem pacote)
        {
            _context.PacotesViagem.Update(pacote); // marca o objeto como modificado
            await _context.SaveChangesAsync(); // salvando alterações no banco
        }

        public async Task RemoverAsync(PacoteViagem pacote)
        {
            _context.PacotesViagem.Remove(pacote); // marca o objeto para remoção
            await _context.SaveChangesAsync(); // executa delete no banco
        }

    }
}