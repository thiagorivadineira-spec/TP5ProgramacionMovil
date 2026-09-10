using System.ComponentModel.DataAnnotations;

namespace TP5ProgramacionMovil.Models
{
    public class Venta
    {
        [Key]
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        // Claves foráneas
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;
    }
}
