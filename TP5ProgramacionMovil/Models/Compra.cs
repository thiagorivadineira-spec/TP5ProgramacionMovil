using System.ComponentModel.DataAnnotations;

namespace TP5ProgramacionMovil.Models
{
    public class Compra
    {
        [Key]
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        // Claves foráneas
        public int ProveedorId { get; set; }
        public Proveedor Proveedor { get; set; } = null!;

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;
    }
}
