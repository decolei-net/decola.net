using System.ComponentModel.DataAnnotations;

namespace Decolei.net.DTOs
{
    public class UpdateReservaDto
    {
        [Required(ErrorMessage = "O status da reserva é obrigatório.")]
        [RegularExpression("^(PENDENTE|CONFIRMADA|CANCELADA)$", ErrorMessage = "O status deve ser PENDENTE, CONFIRMADA ou CANCELADA.")]
        public string Status { get; set; }
    }
}