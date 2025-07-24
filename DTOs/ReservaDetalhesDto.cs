using System.Collections.Generic;
using System;

namespace Decolei.net.DTOs
{
    public class ReservaDetalhesDto
    {
        public int Id { get; set; }
        public DateTime? Data { get; set; }
        public decimal? ValorTotal { get; set; }
        public string? Status { get; set; }
        public string? Numero { get; set; }

        // Propriedades que antes causavam o ciclo, agora usando DTOs
        public PacoteReservaDto? PacoteViagem { get; set; }
        public UsuarioReservaDto? Usuario { get; set; }
        public ICollection<ViajanteDto> Viajantes { get; set; } = new List<ViajanteDto>();
    }
}