using System.ComponentModel.DataAnnotations;

namespace TP5ProgramacionMovil.DTOs
{
    public class DetalleVentaRequestDto
    {
        [Required]
        public int ProductoId { get; set; }

        [Required]
        public int Cantidad { get; set; }
    }
}