using System.ComponentModel.DataAnnotations;

namespace TP5ProgramacionMovil.Models
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;
    }
}
