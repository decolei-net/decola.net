namespace Decolei.net.Models
{
    public class Viajante
    {
        public int Id { get; set; }
        public int Reserva_Id { get; set; }
        public string Nome { get; set; }
        public string Documento { get; set; }

        // Propriedade de Navegação
        public virtual Reserva Reserva { get; set; }
    }
}