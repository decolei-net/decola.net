namespace Decolei.net.Models
{
    public class PacoteViagem
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string? Descricao { get; set; }
        public string? ImagemURL { get; set; }
        public string? VideoURL { get; set; }
        public string? Destino { get; set; }
        public decimal? Valor { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }

        // --- INÍCIO DA MUDANÇA ---
        // Chave estrangeira para o usuário que criou o pacote
        public int UsuarioId { get; set; }

        // Propriedade de navegação para o usuário
        public virtual Usuario Usuario { get; set; }

        // --- FIM DA MUDANÇA ---
        // Propriedade de navegação para as Reservas e Avaliações
        public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
        public virtual ICollection<Avaliacao> Avaliacoes { get; set; } = new List<Avaliacao>();
    }
}
