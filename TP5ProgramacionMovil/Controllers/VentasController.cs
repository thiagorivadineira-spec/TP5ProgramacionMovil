using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TP5ProgramacionMovil.Data;
using TP5ProgramacionMovil.DTOs;
using TP5ProgramacionMovil.Models;

namespace TP5ProgramacionMovil.Controllers
{
    [Authorize] // Requiere autenticación para acceder a este controlador
    [Route("api/[controller]")]
    [ApiController]
    public class VentasController : ControllerBase
    {
        private readonly DataContext _context;

        public VentasController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Ventas?pagina=1&tamanoPagina=10&buscar=cliente&ordenarPor=fecha_desc
        [HttpGet]
        public async Task<ActionResult<RespuestaPaginadaDto<VentaResponseDto>>> GetVentas(
            [FromQuery] ParametrosPaginacionDto parametros)
        {
            var query = _context.Ventas
                .AsNoTracking()
                .AsQueryable();

            // Búsqueda opcional por nombre del cliente
            if (!string.IsNullOrWhiteSpace(parametros.Buscar))
            {
                var termino = parametros.Buscar.Trim().ToLower();

                query = query.Where(v =>
                    v.Cliente.Nombre.ToLower().Contains(termino));
            }

            // Cantidad total antes de paginar
            var totalRegistros = await query.CountAsync();

            // Ordenamiento
            query = parametros.OrdenarPor?.ToLower() switch
            {
                "fecha" => query.OrderBy(v => v.Fecha),
                "fecha_desc" => query.OrderByDescending(v => v.Fecha),
                "total_asc" => query.OrderBy(v => v.Total),
                "total_desc" => query.OrderByDescending(v => v.Total),
                _ => query.OrderBy(v => v.Id)
            };

            // Paginación
            var ventas = await query
                .Skip((parametros.Pagina - 1) * parametros.TamanoPagina)
                .Take(parametros.TamanoPagina)
                .Select(v => new VentaResponseDto
                {
                    Id = v.Id,
                    Fecha = v.Fecha,
                    Total = v.Total,
                    NombreCliente = v.Cliente.Nombre,

                    // Temporal hasta integrar completamente JWT
                    NombreUsuario = "Admin",

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

            var respuesta = new RespuestaPaginadaDto<VentaResponseDto>(
                ventas,
                totalRegistros,
                parametros.Pagina,
                parametros.TamanoPagina
            );

            return Ok(respuesta);
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