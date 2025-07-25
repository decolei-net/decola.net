using Decolei.net.Data;
using Decolei.net.Interfaces;
using Decolei.net.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Decolei.net.Repository
{
    namespace Decolei.net.Repository
    {
        public class ReservaRepository : IReservaRepository
        {
            private readonly DecoleiDbContext _context;

            public ReservaRepository(DecoleiDbContext context)
            {
                _context = context;
            }

            // NOVO MÉTODO
            public async Task<IEnumerable<Reserva>> ObterPorUsuarioIdAsync(int usuarioId)
            {
                return await _context.Reservas
                    .Where(r => r.Usuario_Id == usuarioId)
                    .Include(r => r.Usuario)
                    .Include(r => r.PacoteViagem)
                    .OrderByDescending(r => r.Data)
                    .ToListAsync();
            }

            public async Task<IEnumerable<Reserva>> ObterTodasAsync()
            {
                return await _context.Reservas
                    .Include(r => r.Usuario)
                    .Include(r => r.PacoteViagem)
                    .OrderByDescending(r => r.Data)
                    .ToListAsync();
            }

            public async Task<Reserva> ObterPorIdAsync(int id)
            {
                return await _context.Reservas
                    .Include(r => r.Viajantes)
                    .Include(r => r.Pagamentos)
                    .Include(r => r.Usuario)
                    .Include(r => r.PacoteViagem)
                    .FirstOrDefaultAsync(r => r.Id == id);
            }

            public async Task AdicionarAsync(Reserva reserva)
            {
                _context.Reservas.Add(reserva);
                await _context.SaveChangesAsync();
            }

            public async Task AtualizarAsync(Reserva reserva)
            {
                _context.Entry(reserva).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
        }
    }
}