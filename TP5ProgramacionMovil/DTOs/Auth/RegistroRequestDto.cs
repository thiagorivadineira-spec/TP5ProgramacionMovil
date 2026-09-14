using System.ComponentModel.DataAnnotations;

namespace TP5ProgramacionMovil.DTOs
{
    public class RegistroRequestDto
    {
        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Rol { get; set; } = string.Empty;
    }
}