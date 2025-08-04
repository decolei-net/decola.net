using System.ComponentModel.DataAnnotations;

namespace Decolei.net.DTOs
{
    public class AlterarSenhaDto
    {
        [Required(ErrorMessage = "A senha atual é obrigatória.")]
        public string? SenhaAtual { get; set; }

        [Required(ErrorMessage = "A nova senha é obrigatória.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A nova senha deve ter entre 6 e 100 caracteres.")]
        [DataType(DataType.Password)]
        public string? NovaSenha { get; set; }

        [DataType(DataType.Password)]
        [Compare("NovaSenha", ErrorMessage = "A nova senha e a confirmação de senha não coincidem.")]
        public string? ConfirmarNovaSenha { get; set; }
    }
}