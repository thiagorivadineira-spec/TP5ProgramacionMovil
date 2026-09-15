using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TP5ProgramacionMovil.Data;
using TP5ProgramacionMovil.DTOs;
using TP5ProgramacionMovil.Models;

namespace TP5ProgramacionMovil.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly DataContext _context;

        public ClientesController(DataContext context)
        {
            _context = context;
        }

        // 1. GET: api/Clientes (Obtener todos los clientes activos)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClienteResponseDto>>> GetClientes()
        {
            var clientes = await _context.Clientes
                .Where(c => c.Activo) // Solo traemos los activos (buena práctica)
                .Select(c => new ClienteResponseDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Documento = c.Documento,
                    Telefono = c.Telefono,
                    Email = c.Email,
                    Direccion = c.Direccion,
                    Activo = c.Activo
                })
                .ToListAsync();

            return Ok(clientes);
        }

        // 2. GET: api/Clientes/5 (Obtener un cliente por ID)
        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteResponseDto>> GetCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
            {
                return NotFound("Cliente no encontrado.");
            }

            var responseDto = new ClienteResponseDto
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Documento = cliente.Documento,
                Telefono = cliente.Telefono,
                Email = cliente.Email,
                Direccion = cliente.Direccion,
                Activo = cliente.Activo
            };

            return Ok(responseDto);
        }

        // 3. POST: api/Clientes (Crear cliente nuevo)
        [HttpPost]
        public async Task<ActionResult<ClienteResponseDto>> PostCliente(ClienteRequestDto clienteDto)
        {
            var nuevoCliente = new Cliente
            {
                Nombre = clienteDto.Nombre,
                Documento = clienteDto.Documento,
                Telefono = clienteDto.Telefono,
                Email = clienteDto.Email,
                Direccion = clienteDto.Direccion,
                Activo = true
            };

            _context.Clientes.Add(nuevoCliente);
            await _context.SaveChangesAsync();

            var responseDto = new ClienteResponseDto
            {
                Id = nuevoCliente.Id,
                Nombre = nuevoCliente.Nombre,
                Documento = nuevoCliente.Documento,
                Telefono = nuevoCliente.Telefono,
                Email = nuevoCliente.Email,
                Direccion = nuevoCliente.Direccion,
                Activo = nuevoCliente.Activo
            };

            return CreatedAtAction(nameof(GetCliente), new { id = nuevoCliente.Id }, responseDto);
        }

        // 4. PUT: api/Clientes/5 (Editar un cliente)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCliente(int id, ClienteRequestDto clienteDto)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
            {
                return NotFound("Cliente no encontrado.");
            }

            // Actualizamos los datos del modelo con lo que viene del DTO
            cliente.Nombre = clienteDto.Nombre;
            cliente.Documento = clienteDto.Documento;
            cliente.Telefono = clienteDto.Telefono;
            cliente.Email = clienteDto.Email;
            cliente.Direccion = clienteDto.Direccion;

            await _context.SaveChangesAsync();

            return NoContent(); // 204 No Content es el estándar para un PUT exitoso
        }

        // 5. DELETE: api/Clientes/5 (Borrado lógico)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return NotFound("Cliente no encontrado.");
            }

            // Borrado lógico: no lo eliminamos de la BD, solo lo desactivamos
            cliente.Activo = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}