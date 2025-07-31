using System.ComponentModel.DataAnnotations;

namespace Decolei.net.DTOs
{
    public class UpdateReservaDto
    {
        [Required(ErrorMessage = "O status da reserva é obrigatório.")]
        [RegularExpression("^(PENDENTE|APROVADO|RECUSADO)$", ErrorMessage = "O status deve ser PENDENTE, APROVADO ou RECUSADO.")]
        public string? Status { get; set; }
    }
}