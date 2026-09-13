using System.ComponentModel.DataAnnotations;

namespace TP5ProgramacionMovil.DTOs
{
    public class CategoriaRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;
    }
}