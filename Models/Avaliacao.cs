using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Decolei.net.Models
{
    [Table("Avaliacao")]
    public class Avaliacao
    {
        [Key]
        [Column("Avaliacao_Id")]
        public int Id { get; set; }

        [Column("Usuario_Id")]
        public int Usuario_Id { get; set; }

        [Column("PacoteViagem_Id")]
        public int PacoteViagem_Id { get; set; }

        [Column("Avaliacao_Nota")]
        public int? Nota { get; set; }

        [StringLength(500)]
        [Column("Avaliacao_Comentario")]
        public string? Comentario { get; set; }

        [Column("Avaliacao_Data")]
        public DateTime? Data { get; set; }

        [Column("Avaliacao_Aprovada")]
        public bool? Aprovada { get; set; }

        // Propriedades de Navegação
        [ForeignKey("Usuario_Id")]
        public virtual Usuario Usuario { get; set; } = null!;

        [ForeignKey("PacoteViagem_Id")]
        public virtual PacoteViagem PacoteViagem { get; set; } = null!;
    }
}
