using System.ComponentModel.DataAnnotations;

namespace Decolei.net.DTOs
{
    public class RegistroUsuarioDto
    {
        [Required]
        [StringLength(100)]
        public string? Nome { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string? Senha { get; set; }

        [Required]
        public string? Documento { get; set; }

        public string? Telefone { get; set; }

        // O perfil será 'CLIENTE' por padrão no cadastro
    }
}
