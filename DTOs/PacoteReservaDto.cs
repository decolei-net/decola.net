namespace Decolei.net.DTOs
{
    public class PacoteReservaDto
    {
        public int Id { get; set; }
        public string? Titulo { get; set; }
        public string? Destino { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }

    }
}