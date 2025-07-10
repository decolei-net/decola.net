using System.ComponentModel.DataAnnotations;

namespace Decolei.net.DTOs
{
    public class LoginUsuarioDto
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? Senha { get; set; }
    }
}
