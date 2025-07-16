using Decolei.net.Data;
using Decolei.net.Interfaces;
using Decolei.net.Models;
using Microsoft.EntityFrameworkCore;

namespace Decolei.net.Repositories
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