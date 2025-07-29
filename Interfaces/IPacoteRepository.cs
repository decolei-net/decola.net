using Decolei.net.Models;

namespace Decolei.net.Interfaces
{
    public interface IPacoteRepository
    {
        Task<IEnumerable<PacoteViagem>> ListarTodosAsync();
        Task<PacoteViagem?> ObterPorIdAsync(int id);
        Task<IEnumerable<PacoteViagem>> GetByFiltersAsync(
            string? destino,
            decimal? precoMin,
            decimal? precoMax,
            DateTime? dataInicio,
            DateTime? dataFim);
        Task AdicionarAsync(PacoteViagem pacote);
        Task AtualizarAsync(PacoteViagem pacote);
        Task RemoverAsync(PacoteViagem pacote);
    }
}