using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TP5ProgramacionMovil.Data;
using TP5ProgramacionMovil.DTOs;
using TP5ProgramacionMovil.Models;

namespace TP5ProgramacionMovil.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly DataContext _context;

        public ProductosController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Productos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductoResponseDto>>> GetProductos()
        {
            var productos = await _context.Productos
                .AsNoTracking()
                .Include(p => p.CategoriaProducto)
                .Select(p => new ProductoResponseDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    PrecioVenta = p.PrecioVenta,
                    StockActual = p.StockActual,
                    PuntoReposicion = p.PuntoReposicion,
                    StockMaximo = p.StockMaximo,
                    ImagenUrl = p.ImagenUrl,
                    Activo = p.Activo,
                    CategoriaProductoId = p.CategoriaProductoId,
                    CategoriaNombre = p.CategoriaProducto.Nombre
                })
                .ToListAsync();

            return Ok(productos);
        }

        // GET: api/Productos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductoResponseDto>> GetProducto(int id)
        {
            var producto = await _context.Productos
                .AsNoTracking()
                .Include(p => p.CategoriaProducto)
                .Where(p => p.Id == id)
                .Select(p => new ProductoResponseDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    PrecioVenta = p.PrecioVenta,
                    StockActual = p.StockActual,
                    PuntoReposicion = p.PuntoReposicion,
                    StockMaximo = p.StockMaximo,
                    ImagenUrl = p.ImagenUrl,
                    Activo = p.Activo,
                    CategoriaProductoId = p.CategoriaProductoId,
                    CategoriaNombre = p.CategoriaProducto.Nombre
                })
                .FirstOrDefaultAsync();

            if (producto == null)
            {
                return NotFound(new
                {
                    mensaje = "Producto no encontrado."
                });
            }

            return Ok(producto);
        }

        // POST: api/Productos
        [HttpPost]
        public async Task<ActionResult<ProductoResponseDto>> CrearProducto(
            ProductoRequestDto dto)
        {
            var categoriaExiste = await _context.CategoriasProductos
                .AnyAsync(c => c.Id == dto.CategoriaProductoId);

            if (!categoriaExiste)
            {
                return BadRequest(new
                {
                    mensaje = "La categoría indicada no existe."
                });
            }

            var producto = new Producto
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                PrecioVenta = dto.PrecioVenta,
                StockActual = dto.StockActual,
                PuntoReposicion = dto.PuntoReposicion,
                StockMaximo = dto.StockMaximo,
                Activo = dto.Activo,
                CategoriaProductoId = dto.CategoriaProductoId
            };

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            var categoria = await _context.CategoriasProductos
                .FindAsync(producto.CategoriaProductoId);

            var response = new ProductoResponseDto
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                PrecioVenta = producto.PrecioVenta,
                StockActual = producto.StockActual,
                PuntoReposicion = producto.PuntoReposicion,
                StockMaximo = producto.StockMaximo,
                ImagenUrl = producto.ImagenUrl,
                Activo = producto.Activo,
                CategoriaProductoId = producto.CategoriaProductoId,
                CategoriaNombre = categoria?.Nombre
            };

            return CreatedAtAction(
                nameof(GetProducto),
                new { id = producto.Id },
                response
            );
        }

        // PUT: api/Productos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarProducto(
            int id,
            ProductoRequestDto dto)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
            {
                return NotFound(new
                {
                    mensaje = "Producto no encontrado."
                });
            }

            var categoriaExiste = await _context.CategoriasProductos
                .AnyAsync(c => c.Id == dto.CategoriaProductoId);

            if (!categoriaExiste)
            {
                return BadRequest(new
                {
                    mensaje = "La categoría indicada no existe."
                });
            }

            producto.Nombre = dto.Nombre;
            producto.Descripcion = dto.Descripcion;
            producto.PrecioVenta = dto.PrecioVenta;
            producto.StockActual = dto.StockActual;
            producto.PuntoReposicion = dto.PuntoReposicion;
            producto.StockMaximo = dto.StockMaximo;
            producto.Activo = dto.Activo;
            producto.CategoriaProductoId = dto.CategoriaProductoId;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Productos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
            {
                return NotFound(new
                {
                    mensaje = "Producto no encontrado."
                });
            }

            // Baja lógica
            producto.Activo = false;

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}