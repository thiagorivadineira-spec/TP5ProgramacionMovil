using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TP5ProgramacionMovil.Data;
using TP5ProgramacionMovil.DTOs;
using TP5ProgramacionMovil.Models;

namespace TP5ProgramacionMovil.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComprasController : ControllerBase
    {
        private readonly DataContext _context;

        public ComprasController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Compras
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CompraResponseDto>>> GetCompras()
        {
            var compras = await _context.Compras
                .AsNoTracking()
                .Include(c => c.Proveedor)
                .Include(c => c.Usuario)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Producto)
                .Select(c => new CompraResponseDto
                {
                    Id = c.Id,
                    Fecha = c.Fecha,
                    Total = c.Total,

                    ProveedorId = c.ProveedorId,
                    ProveedorNombre = c.Proveedor.RazonSocial,

                    UsuarioId = c.UsuarioId,
                    UsuarioNombre = c.Usuario.Nombre,

                    Detalles = c.Detalles.Select(d => new DetalleCompraResponseDto
                    {
                        Id = d.Id,
                        ProductoId = d.ProductoId,
                        ProductoNombre = d.Producto.Nombre,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Cantidad * d.PrecioUnitario
                    }).ToList()
                })
                .ToListAsync();

            return Ok(compras);
        }

        // GET: api/Compras/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CompraResponseDto>> GetCompra(int id)
        {
            var compra = await _context.Compras
                .AsNoTracking()
                .Include(c => c.Proveedor)
                .Include(c => c.Usuario)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(c => c.Id == id)
                .Select(c => new CompraResponseDto
                {
                    Id = c.Id,
                    Fecha = c.Fecha,
                    Total = c.Total,

                    ProveedorId = c.ProveedorId,
                    ProveedorNombre = c.Proveedor.RazonSocial,

                    UsuarioId = c.UsuarioId,
                    UsuarioNombre = c.Usuario.Nombre,

                    Detalles = c.Detalles.Select(d => new DetalleCompraResponseDto
                    {
                        Id = d.Id,
                        ProductoId = d.ProductoId,
                        ProductoNombre = d.Producto.Nombre,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Cantidad * d.PrecioUnitario
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (compra == null)
            {
                return NotFound(new
                {
                    mensaje = "Compra no encontrada."
                });
            }

            return Ok(compra);
        }

        // POST: api/Compras
        [HttpPost]
        public async Task<ActionResult<CompraResponseDto>> CrearCompra(
            CompraRequestDto dto)
        {
            if (dto.Detalles == null || dto.Detalles.Count == 0)
            {
                return BadRequest(new
                {
                    mensaje = "La compra debe contener al menos un producto."
                });
            }

            var proveedor = await _context.Proveedores
                .FirstOrDefaultAsync(p => p.Id == dto.ProveedorId);

            if (proveedor == null)
            {
                return BadRequest(new
                {
                    mensaje = "El proveedor indicado no existe."
                });
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == dto.UsuarioId);

            if (usuario == null)
            {
                return BadRequest(new
                {
                    mensaje = "El usuario indicado no existe."
                });
            }

            // Transacción: compra + detalles + stock deben guardarse juntos
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var compra = new Compra
                {
                    ProveedorId = dto.ProveedorId,
                    UsuarioId = dto.UsuarioId,
                    Fecha = DateTime.Now,
                    Total = 0
                };

                _context.Compras.Add(compra);
                await _context.SaveChangesAsync();

                decimal total = 0;

                foreach (var detalleDto in dto.Detalles)
                {
                    if (detalleDto.Cantidad <= 0)
                    {
                        return BadRequest(new
                        {
                            mensaje = "La cantidad debe ser mayor a cero."
                        });
                    }

                    if (detalleDto.PrecioUnitario <= 0)
                    {
                        return BadRequest(new
                        {
                            mensaje = "El precio unitario debe ser mayor a cero."
                        });
                    }

                    var producto = await _context.Productos
                        .FirstOrDefaultAsync(p => p.Id == detalleDto.ProductoId);

                    if (producto == null)
                    {
                        return BadRequest(new
                        {
                            mensaje = $"El producto con ID {detalleDto.ProductoId} no existe."
                        });
                    }

                    var detalle = new DetalleCompra
                    {
                        CompraId = compra.Id,
                        ProductoId = producto.Id,
                        Cantidad = detalleDto.Cantidad,
                        PrecioUnitario = detalleDto.PrecioUnitario
                    };

                    _context.DetallesCompras.Add(detalle);

                    // Aumentar stock
                    producto.StockActual += detalleDto.Cantidad;

                    total += detalleDto.Cantidad * detalleDto.PrecioUnitario;
                }

                compra.Total = total;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(
                    nameof(GetCompra),
                    new { id = compra.Id },
                    new
                    {
                        mensaje = "Compra registrada correctamente.",
                        compraId = compra.Id,
                        total = compra.Total
                    }
                );
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}