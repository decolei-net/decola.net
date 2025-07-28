using System.ComponentModel.DataAnnotations;

namespace Decolei.net.DTOs
{
    public class AtualizarStatusPagamentoDto
    {
        [Required]
        // aqui o regular expression garante que o status só pode ser 'PENDENTE', 'APROVADO' ou 'RECUSADO' mantendo padrao com do banco de dados
        [RegularExpression("^(PENDENTE|APROVADO|RECUSADO)$", ErrorMessage = "Status deve ser 'PENDENTE', 'APROVADO' ou 'RECUSADO'.")]
        public string? Status { get; set; }

    }
}
