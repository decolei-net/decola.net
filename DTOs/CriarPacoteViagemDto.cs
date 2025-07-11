using System.ComponentModel.DataAnnotations;

namespace Decolei.net.DTOs
{
    public class CriarPacoteViagemDto
    {
        [Required(ErrorMessage = "O título é obrigatório.")]
        [StringLength(100, ErrorMessage = "O título deve ter no máximo 100 caracteres.")]
        public required string Titulo { get; set; }

        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres.")]
        public string? Descricao { get; set; }

        [StringLength(255, ErrorMessage = "A URL da imagem deve ter no máximo 255 caracteres.")]
        public string? ImagemURL { get; set; }

        [StringLength(255, ErrorMessage = "A URL do vídeo deve ter no máximo 255 caracteres.")]
        public string? VideoURL { get; set; }

        [Required(ErrorMessage = "O destino é obrigatório.")]
        [StringLength(100, ErrorMessage = "O destino deve ter no máximo 100 caracteres.")]
        public required string Destino { get; set; }

        [Required(ErrorMessage = "O valor é obrigatório.")]
        [Range(0.01, 99999999.99, ErrorMessage = "O valor deve ser maior que zero.")]
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "A data de início é obrigatória.")]
        public DateTime DataInicio { get; set; }

        [Required(ErrorMessage = "A data de fim é obrigatória.")]
        public DateTime DataFim { get; set; }
    }
}