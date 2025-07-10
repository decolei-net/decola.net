using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Decolei.net.Models
{
    [Table("Pagamento")]
    public class Pagamento
    {
        [Key]
        [Column("Pagamento_Id")]
        public int Id { get; set; }

        [Column("Reserva_Id")]
        public int Reserva_Id { get; set; }

        [StringLength(20)]
        [Column("Pagamento_Forma")]
        public string? Forma { get; set; } // string? indica que pode ser nulo

        [StringLength(20)]
        [Column("Pagamento_Status")]
        public string? Status { get; set; }

        [StringLength(255)]
        [Column("Pagamento_ComprovanteURL")]
        public string? ComprovanteURL { get; set; }

        [Column("Pagamento_Data")]
        public DateTime? Data { get; set; }

        // Propriedade de Navegação para a Reserva
        [ForeignKey("Reserva_Id")]
        public virtual Reserva Reserva { get; set; } = null!;
    }
}
