using Decolei.net.Enums;
using System.ComponentModel.DataAnnotations;

namespace Decolei.net.DTOs
{
    // DTO que representa os dados que o front-end envia ao pagar
    public class PagamentoEntradaDTO
    {
        [Required]
        public int ReservaId { get; set; }

        [Required]
        public string? NomeCompleto { get; set; }

        [Required]
        public string? Cpf { get; set; }

        [Required]
        public MetodoPagamento Metodo { get; set; }

        [Required]
        public decimal Valor { get; set; }

        // Apenas necessário se for crédito
        public int Parcelas { get; set; }

        public string? NumeroCartao { get; set; }

        [Required]
        public string? Email { get; set; }
    }
}
