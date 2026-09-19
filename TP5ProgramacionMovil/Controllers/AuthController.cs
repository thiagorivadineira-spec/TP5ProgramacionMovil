using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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
        private readonly IConfiguration _config; // <-- Necesario para leer la clave secreta

        public AuthController(DataContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("Registro")]
        public async Task<IActionResult> Registro(RegistroRequestDto registroDto)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Username == registroDto.Username))
                return BadRequest("El nombre de usuario ya está en uso.");

            var nuevoUsuario = new Usuario
            {
                Nombre = registroDto.Nombre,
                Username = registroDto.Username,
                PasswordHash = registroDto.Password,
                Rol = registroDto.Rol,
                Activo = true
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();
            return Ok(new { Mensaje = "Usuario registrado exitosamente." });
        }

        [HttpPost("Login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto loginDto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Username == loginDto.Username && u.PasswordHash == loginDto.Password);

            if (usuario == null)
                return Unauthorized("Usuario o contraseña incorrectos.");

            // --- FABRICACIÓN DEL TOKEN JWT ---
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Los "Claims" son los datos que viajan dentro del token
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Username),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2), // El token dura 2 horas
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            // ---------------------------------

            var response = new AuthResponseDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Username = usuario.Username,
                Rol = usuario.Rol,
                Token = tokenString //se envia el token real
            };

            return Ok(response);
        }
    }
}