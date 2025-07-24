using System.ComponentModel.DataAnnotations;

namespace Decolei.net.DTOs
{
    public class RecuperarSenhaDto
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
