namespace TP5ProgramacionMovil.DTOs
{
    public class CompraRequestDto
    {
        public int ProveedorId { get; set; }

        public int UsuarioId { get; set; }

        public List<DetalleCompraRequestDto> Detalles { get; set; } = new();
    }
}