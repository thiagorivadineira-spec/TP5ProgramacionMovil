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
        private readonly IWebHostEnvironment _env;

        // Extensiones y tipos MIME permitidos
        private readonly string[] _extensionesPermitidas =
            [".jpg", ".jpeg", ".png"];

        private readonly string[] _tiposMimePermitidos =
            ["image/jpeg", "image/png"];

        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

        public ProductosController(
            DataContext context,
            IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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
                    Imagenes = p.Imagenes.Select(i => new ImagenResponseDto
                    {
                        Id = i.Id,
                        NombreOriginal = i.NombreOriginal,
                        Url = "/" + i.RutaRelativa,
                        TipoContenido = i.TipoContenido,
                        TamanoBytes = i.TamanoBytes,
                        FechaCreacion = i.FechaCreacion
                    }).ToList(),
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
                    Imagenes = p.Imagenes.Select(i => new ImagenResponseDto
                    {
                        Id = i.Id,
                        NombreOriginal = i.NombreOriginal,
                        Url = "/" + i.RutaRelativa,
                        TipoContenido = i.TipoContenido,
                        TamanoBytes = i.TamanoBytes,
                        FechaCreacion = i.FechaCreacion
                    }).ToList(),
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
                Imagenes = new List<ImagenResponseDto>(),
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

        // POST: api/Productos/{id}/imagenes
        [HttpPost("{id}/imagenes")]
        public async Task<ActionResult<ImagenResponseDto>> SubirImagen(
            int id,
            IFormFile archivo)
        {
            // 1. Validar existencia del producto
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
            {
                return NotFound(new
                {
                    mensaje = $"El producto con ID {id} no existe."
                });
            }

            // 2. Validar que se haya enviado un archivo
            if (archivo == null || archivo.Length == 0)
            {
                return BadRequest(new
                {
                    mensaje = "Debe proporcionar un archivo de imagen válido."
                });
            }

            // 3. Validar tamaño máximo
            if (archivo.Length > MaxFileSize)
            {
                return BadRequest(new
                {
                    mensaje = "El archivo excede el tamaño máximo permitido de 5 MB."
                });
            }

            // 4. Validar extensión
            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();

            if (!_extensionesPermitidas.Contains(extension))
            {
                return BadRequest(new
                {
                    mensaje = "Formato no permitido. Solo se aceptan archivos PNG y JPG/JPEG."
                });
            }

            // 5. Validar tipo MIME
            if (!_tiposMimePermitidos.Contains(archivo.ContentType.ToLowerInvariant()))
            {
                return BadRequest(new
                {
                    mensaje = "Tipo MIME inválido para la imagen."
                });
            }

            // 6. Preparar carpeta wwwroot/uploads
            var webRoot = _env.WebRootPath
                ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            var uploadsDir = Path.Combine(webRoot, "uploads");

            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }

            // 7. Generar nombre único
            var nombreUnico = $"{Guid.NewGuid()}{extension}";

            var rutaFisicaCompleta =
                Path.Combine(uploadsDir, nombreUnico);

            // 8. Guardar archivo en disco
            using (var stream = new FileStream(
                rutaFisicaCompleta,
                FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            // 9. Registrar imagen en la base de datos
            var rutaRelativa = $"uploads/{nombreUnico}";

            var imagen = new Imagen
            {
                NombreOriginal = Path.GetFileName(archivo.FileName),
                NombreArchivo = nombreUnico,
                RutaRelativa = rutaRelativa,
                TipoContenido = archivo.ContentType,
                TamanoBytes = archivo.Length,
                ProductoId = id
            };

            _context.Imagenes.Add(imagen);

            await _context.SaveChangesAsync();

            // 10. Armar URL pública
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var dto = new ImagenResponseDto
            {
                Id = imagen.Id,
                NombreOriginal = imagen.NombreOriginal,
                Url = $"{baseUrl}/{imagen.RutaRelativa}",
                TipoContenido = imagen.TipoContenido,
                TamanoBytes = imagen.TamanoBytes,
                FechaCreacion = imagen.FechaCreacion
            };

            return Created($"/api/Productos/{id}", dto);
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