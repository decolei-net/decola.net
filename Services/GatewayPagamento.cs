using System;
using System.Globalization;
using Decolei.net.Enums; // importando o enum MetodoPagamento

namespace Decolei.net.Services
{
    // Classe que simula um gateway de pagamento (como Mercado Pago, PagSeguro, etc...)
    public class GatewayPagamento
    {
        // Nome completo do cliente que está pagando
        public string NomeCompleto { get; set; }

        public string Cpf { get; set; }

        // Método de pagamento usado (é o enum: Pix, Boleto, Credito, Debito)
        public MetodoPagamento Metodo { get; set; }

        public decimal ValorTotal { get; set; }
        public int Parcelas { get; set; }
        public string NumeroCartaoMascarado { get; set; }

        public string IdTransacao { get; private set; }

        // Resultado do pagamento: "APROVADO", "PENDENTE", "RECUSADO"
        public string Status { get; private set; }

        // Construtor da classe → sempre que criamos um GatewayPagamento, já gera um ID de transação automático
        public GatewayPagamento()
        {
            // cria um ID de transação único usando Guid; guid é um identificador global único
            IdTransacao = Guid.NewGuid().ToString();
        }

        public void ProcessarPagamento()
        {
            switch (Metodo)
            {
                case MetodoPagamento.Pix:
                    Status = "APROVADO";
                    break;

                case MetodoPagamento.Boleto:
                    Status = "PENDENTE";
                    break;

                case MetodoPagamento.Credito:
                case MetodoPagamento.Debito:
                    // Simulação: número de cartão deve ter pelo menos 12 dígitos válidos
                    if (string.IsNullOrWhiteSpace(NumeroCartaoMascarado) || NumeroCartaoMascarado.Length < 12 || NumeroCartaoMascarado == "string")
                    {
                        Status = "RECUSADO";
                    }
                    else
                    {
                        Status = "APROVADO";
                    }
                    break;

                default:
                    Status = "RECUSADO";
                    break;
            }
        }
    }
}
