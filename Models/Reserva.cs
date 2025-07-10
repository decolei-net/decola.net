namespace Decolei.net.Models
{
    public class Reserva
    {
        public int Id { get; set; }
        public int Usuario_Id { get; set; }
        public int PacoteViagem_Id { get; set; }
        public DateTime? Data { get; set; }
        public decimal? ValorTotal { get; set; }
        public string? Status { get; set; }
        public string? Numero { get; set; }

        // Propriedades de navegação para as chaves estrangeiras
        public virtual Usuario Usuario { get; set; }
        public virtual PacoteViagem PacoteViagem { get; set; }
        public virtual ICollection<Viajante> Viajantes { get; set; } = new List<Viajante>();
        public virtual ICollection<Pagamento> Pagamentos { get; set; } = new List<Pagamento>();
    }
}
