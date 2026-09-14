using System.ComponentModel.DataAnnotations;

namespace TP5ProgramacionMovil.DTOs
{
    public class VentaRequestDto
    {
        [Required]
        public int ClienteId { get; set; }

        [Required]
        public List<DetalleVentaRequestDto> Detalles { get; set; } = new List<DetalleVentaRequestDto>();
    }
}