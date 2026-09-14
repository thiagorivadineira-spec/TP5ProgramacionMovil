using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TP5ProgramacionMovil.Data;
using TP5ProgramacionMovil.DTOs;
using TP5ProgramacionMovil.Models;

namespace TP5ProgramacionMovil.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasProductosController : ControllerBase
    {
        private readonly DataContext _context;

        public CategoriasProductosController(DataContext context)
        {
            _context = context;
        }

        // GET: api/CategoriasProductos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoriaResponseDto>>> GetCategorias()
        {
            var categorias = await _context.CategoriasProductos
                .AsNoTracking()
                .Select(c => new CategoriaResponseDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    Activo = c.Activo
                })
                .ToListAsync();

            return Ok(categorias);
        }

        // GET: api/CategoriasProductos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoriaResponseDto>> GetCategoria(int id)
        {
            var categoria = await _context.CategoriasProductos
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new CategoriaResponseDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    Activo = c.Activo
                })
                .FirstOrDefaultAsync();

            if (categoria == null)
            {
                return NotFound(new
                {
                    mensaje = "Categoría no encontrada."
                });
            }

            return Ok(categoria);
        }

        // POST: api/CategoriasProductos
        [HttpPost]
        public async Task<ActionResult<CategoriaResponseDto>> CrearCategoria(
            CategoriaRequestDto dto)
        {
            var categoria = new CategoriaProducto
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Activo = dto.Activo
            };

            _context.CategoriasProductos.Add(categoria);
            await _context.SaveChangesAsync();

            var response = new CategoriaResponseDto
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion,
                Activo = categoria.Activo
            };

            return CreatedAtAction(
                nameof(GetCategoria),
                new { id = categoria.Id },
                response
            );
        }

        // PUT: api/CategoriasProductos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarCategoria(
            int id,
            CategoriaRequestDto dto)
        {
            var categoria = await _context.CategoriasProductos.FindAsync(id);

            if (categoria == null)
            {
                return NotFound(new
                {
                    mensaje = "Categoría no encontrada."
                });
            }

            categoria.Nombre = dto.Nombre;
            categoria.Descripcion = dto.Descripcion;
            categoria.Activo = dto.Activo;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/CategoriasProductos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarCategoria(int id)
        {
            var categoria = await _context.CategoriasProductos.FindAsync(id);

            if (categoria == null)
            {
                return NotFound(new
                {
                    mensaje = "Categoría no encontrada."
                });
            }

            categoria.Activo = false;

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}