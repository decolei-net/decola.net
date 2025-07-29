using Decolei.net.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Decolei.net.Interfaces
{
    public interface IReservaRepository
    {
        Task<IEnumerable<Reserva>> ObterTodasAsync();
        Task<IEnumerable<Reserva>> ObterPorUsuarioIdAsync(int usuarioId); // NOVO
        Task<Reserva> ObterPorIdAsync(int id);
        Task AdicionarAsync(Reserva reserva);
        Task AtualizarAsync(Reserva reserva);
        Task<IEnumerable<Reserva>> ObterPorPacoteIdAsync(int pacoteId);
    }
}