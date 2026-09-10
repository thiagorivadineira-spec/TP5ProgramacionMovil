using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TP5ProgramacionMovil.Models
{
    public class Producto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        public int Stock { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        // Clave foránea
        public int CategoriaProductoId { get; set; }
        public CategoriaProducto CategoriaProducto { get; set; } = null!;
    }
}
