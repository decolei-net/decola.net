using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Decolei.net.DTOs
{
    public class CriarReservaDto
    {
        [Required(ErrorMessage = "O ID do pacote de viagem é obrigatório.")]
        public int PacoteViagemId { get; set; }

        // A lista de viajantes é opcional
        public List<ViajanteDto> Viajantes { get; set; } = new List<ViajanteDto>();
    }
}