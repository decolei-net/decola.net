namespace Decolei.net.Models
{
    public class PacoteViagem
    {
        public int Id { get; set; }
        public string? Titulo { get; set; }
        public string? Descricao { get; set; }
        public string? ImagemURL { get; set; }
        public string? VideoURL { get; set; }
        public string? Destino { get; set; }
        public decimal? Valor { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }

        // Propriedade de navegação para as Reservas e Avaliações
        public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
        public virtual ICollection<Avaliacao> Avaliacoes { get; set; } = new List<Avaliacao>();
    }
}
