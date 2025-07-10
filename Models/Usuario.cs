using Microsoft.AspNetCore.Identity;

namespace Decolei.net.Models
{
    public class Usuario : IdentityUser<int> // Herda do Identity usando INT como chave
    {
        // Id (já vem do IdentityUser<int>)
        // UserName (vamos mapear para Usuario_Nome)
        // Email (já vem do IdentityUser)
        // PasswordHash (vamos mapear para Usuario_Senha)
        // PhoneNumber (já vem do IdentityUser, vamos mapear para Usuario_Telefone)

        // Propriedades EXCLUSIVAS do nosso banco/tabela
        public string? Documento { get; set; }

        [Required(ErrorMessage = "O campo Perfil é obrigatório.")]
        [RegularExpression("^(admin|cliente)$", ErrorMessage = "Perfil deve ser 'admin' ou 'cliente'")]
        public string? Perfil { get; set; }

        public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
        public virtual ICollection<Avaliacao> Avaliacoes { get; set; } = new List<Avaliacao>();
    }
}
