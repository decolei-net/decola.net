// decolei-net/decola.net/decola.net-c4696dfb948c967c213b787dce91e7fdcc55014d/Models/PacoteViagem.cs

using System.ComponentModel.DataAnnotations.Schema;
using Decolei.net.Models;

namespace Decolei.net.Models
{
    public class PacoteViagem
    {
        public int Id { get; set; }
        public string? Titulo { get; set; }
        public string? Descricao { get; set; }
        // REMOVIDO: public string? ImagemURL { get; set; }
        public string? VideoURL { get; set; }
        public string? Destino { get; set; }
        public decimal Valor { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public int UsuarioId { get; set; }
        public virtual Usuario? Usuario { get; set; }

        [NotMapped]
        public int QuantidadeVagas { get; set; } = 30;

        // ADICIONADO: Coleção de imagens
        public virtual ICollection<Imagem> Imagens { get; set; } = new List<Imagem>();

        public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
        public virtual ICollection<Avaliacao> Avaliacoes { get; set; } = new List<Avaliacao>();
    }
}