using Microsoft.EntityFrameworkCore;
using TP5ProgramacionMovil.Models;

namespace TP5ProgramacionMovil.Data
{
    public class DataContext : DbContext
    {
        // Constructor necesario
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        // DbSets (Las tablas que se crearán en SQL Server)
        public DbSet<CategoriaProducto> CategoriasProductos { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Compra> Compras { get; set; }
        public DbSet<DetalleCompra> DetallesCompras { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetallesVentas { get; set; }
    }
}
