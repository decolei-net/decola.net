namespace Decolei.net.DTOs
{
    public class AvaliacaoRequest
    {
        public int Reserva_Id { get; set; }
        public int Usuario_Id { get; set; }
        public int PacoteViagem_Id { get; set; }
        public int Nota { get; set; }
        public string? Comentario { get; set; }
    }
}
