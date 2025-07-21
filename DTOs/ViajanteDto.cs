using System.ComponentModel.DataAnnotations;

namespace Decolei.net.DTOs
{
    public class ViajanteDto
    {
        [Required(ErrorMessage = "O nome do viajante é obrigatório.")]
        [StringLength(100)]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O documento do viajante é obrigatório.")]
        [StringLength(50)]
        public string Documento { get; set; }
    }
}