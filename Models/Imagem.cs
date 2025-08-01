// decolei-net/decola.net/decola.net-c4696dfb948c967c213b787dce91e7fdcc55014d/Models/Imagem.cs

using System.ComponentModel.DataAnnotations.Schema;

namespace Decolei.net.Models
{
    public class Imagem
    {
        public int Id { get; set; }

        // Caminho relativo da imagem, ex: "uploads/pacotes/imagem-1.jpg"
        public string Url { get; set; }

        // Chave estrangeira para o PacoteViagem
        public int PacoteViagemId { get; set; }

        // Propriedade de Navegação
        [ForeignKey("PacoteViagemId")]
        public virtual PacoteViagem PacoteViagem { get; set; }
    }
}