using System.ComponentModel.DataAnnotations;

namespace Decolei.net.DTOs
{
    public class RedefinirSenhaDto
    {
        [Required]
        public string? Email { get; set; }

        [Required]
        public string? Token { get; set; }

        [Required]
        [MinLength(6)]
        public string? NovaSenha { get; set; }
    }
}
