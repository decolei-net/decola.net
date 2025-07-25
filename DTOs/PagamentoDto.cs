using System.ComponentModel.DataAnnotations;

namespace Decolei.net.DTOs
{
    public class PagamentoDto
    {
        public int Id { get; set; }

        [Required]
        public int Reserva_Id { get; set; }

        [Required]
        public string? Forma { get; set; }

        public string? Status { get; set; } = "PENDENTE"; // Valor padrão "pendente"

        public string? ComprovanteURL { get; set; }

        public DateTime Data { get; set; } = DateTime.Now;
    }
}
