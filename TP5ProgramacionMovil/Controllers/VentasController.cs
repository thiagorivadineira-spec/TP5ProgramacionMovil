using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TP5ProgramacionMovil.Data;
using TP5ProgramacionMovil.DTOs;
using TP5ProgramacionMovil.Models;

namespace TP5ProgramacionMovil.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VentasController : ControllerBase
    {
        private readonly DataContext _context;

        public VentasController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VentaResponseDto>>> GetVentas()
        {
            var ventas = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .Select(v => new VentaResponseDto
                {
                    Id = v.Id,
                    Fecha = v.Fecha,
                    Total = v.Total,
                    NombreCliente = v.Cliente.Nombre,
                    NombreUsuario = "Admin", // Esto se cambiará cuando integren JWT
                    Detalles = v.Detalles.Select(d => new DetalleVentaResponseDto
                    {
                        ProductoId = d.ProductoId,
                        NombreProducto = d.Producto.Nombre,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Subtotal = d.Cantidad * d.PrecioUnitario
                    }).ToList()
                })
                .ToListAsync();

            return Ok(ventas);
        }

        [HttpPost]
        public async Task<ActionResult> PostVenta(VentaRequestDto ventaDto)
        {
            var cliente = await _context.Clientes.FindAsync(ventaDto.ClienteId);
            if (cliente == null || !cliente.Activo) return BadRequest("Cliente no válido o inactivo.");

            var nuevaVenta = new Venta
            {
                ClienteId = ventaDto.ClienteId,
                Fecha = DateTime.Now,
                Total = 0,
                Detalles = new List<DetalleVenta>()
            };

            foreach (var detalleDto in ventaDto.Detalles)
            {
                var producto = await _context.Productos.FindAsync(detalleDto.ProductoId);
                if (producto == null) return BadRequest($"El producto ID {detalleDto.ProductoId} no existe.");
                if (producto.StockActual < detalleDto.Cantidad) return BadRequest($"Stock insuficiente para {producto.Nombre}.");

                var nuevoDetalle = new DetalleVenta
                {
                    ProductoId = producto.Id,
                    Cantidad = detalleDto.Cantidad,
                    PrecioUnitario = producto.PrecioVenta
                };

                nuevaVenta.Total += (nuevoDetalle.Cantidad * nuevoDetalle.PrecioUnitario);
                nuevaVenta.Detalles.Add(nuevoDetalle);

                producto.StockActual -= nuevoDetalle.Cantidad; // Descuento de stock
            }

            _context.Ventas.Add(nuevaVenta);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { Mensaje = "Venta registrada con éxito", VentaId = nuevaVenta.Id, Total = nuevaVenta.Total });
        }
    }
}