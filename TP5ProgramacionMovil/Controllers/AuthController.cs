using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TP5ProgramacionMovil.Data;
using TP5ProgramacionMovil.DTOs;
using TP5ProgramacionMovil.Models;

namespace TP5ProgramacionMovil.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataContext _context;

        public AuthController(DataContext context)
        {
            _context = context;
        }

        [HttpPost("Registro")]
        public async Task<IActionResult> Registro(RegistroRequestDto registroDto)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Username == registroDto.Username))
            {
                return BadRequest("El nombre de usuario ya está en uso.");
            }

            var nuevoUsuario = new Usuario
            {
                Nombre = registroDto.Nombre,
                Username = registroDto.Username,
                PasswordHash = registroDto.Password, // <-- Aquí está el cambio
                Rol = registroDto.Rol
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            return Ok(new { Mensaje = "Usuario registrado exitosamente." });
        }

        [HttpPost("Login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto loginDto)
        {
            // <-- Aquí también está el cambio
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Username == loginDto.Username && u.PasswordHash == loginDto.Password);

            if (usuario == null)
            {
                return Unauthorized("Usuario o contraseña incorrectos.");
            }

            var response = new AuthResponseDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Username = usuario.Username,
                Rol = usuario.Rol,
                Token = "jwt-token-generado-proximamente"
            };

            return Ok(response);
        }
    }
}