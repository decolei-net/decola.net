using System.ComponentModel.DataAnnotations;

namespace Decolei.net.DTOs
{
    public class UpdatePacoteViagemDto
    {
        [StringLength(100, ErrorMessage = "O titulo deve ter no máximo 100 caracteres")]
        public string? Titulo { get; set; }

        [StringLength(100, ErrorMessage = "O titulo deve ter no máximo 500 caracteres")]
        public string? Descricao { get; set; }

        [StringLength(255, ErrorMessage = "A URL da imagem deve ter no máximo 255 caracteres.")]
        public string? ImagemURL { get; set; }

        [StringLength(255, ErrorMessage = "A URL do vídeo deve ter no máximo 255 caracteres.")]
        public string? VideoURL { get; set; }

        [StringLength(100, ErrorMessage = "O destino deve ter no máximo 100 caracteres.")]
        public string? Destino { get; set; }

        [Range(0.01, 99999999.99, ErrorMessage = "O valor deve ser maior que zero.")]
        public decimal? Valor { get; set; }

        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
    }
}
