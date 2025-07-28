using System.ComponentModel.DataAnnotations.Schema;
using Decolei.net.Models;

namespace Decolei.net.Models
{
    public class PacoteViagem
    {
        public int Id { get; set; }
        public string? Titulo { get; set; }
        public string? Descricao { get; set; }
        public string? ImagemURL { get; set; }
        public string? VideoURL { get; set; }
        public string? Destino { get; set; }
        public decimal Valor { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }

        // --- CORREÇÃO: VOLTANDO PARA INT ---
        // A chave estrangeira para o seu usuário do Identity é um int.
        public int UsuarioId { get; set; }

        // A propriedade de navegação aponta para a sua classe Usuario.
        public virtual Usuario? Usuario { get; set; }
        // --- FIM DA CORREÇÃO ---

        [NotMapped]
        public int QuantidadeVagas { get; set; } = 30;

        public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
        public virtual ICollection<Avaliacao> Avaliacoes { get; set; } = new List<Avaliacao>();
    }
}