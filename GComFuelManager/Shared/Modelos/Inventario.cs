namespace GComFuelManager.Shared.Modelos
{
    public class Inventario
    {
        public int Id { get; set; }
        public byte ProductoId { get; set; }
        public int SitioId { get; set; }
        public int AlmacenId { get; set; }
        public int LocalidadId { get; set; }
        public int TipoMovimientoId { get; set; }
        public string Referencia { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public double Cantidad { get; set; }
        public double CantidadLTS { get; set; }
        public int UnidadMedidaId { get; set; }
        public DateTime? FechaCierre { get; set; } = null;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public short TadId { get; set; }
        public int? CierreId { get; set; } = 0;
        public bool Activo { get; set; } = true;

        public Producto Producto { get; set; } = null!;
        public CatalogoValor Sitio { get; set; } = null!;
        public CatalogoValor Almacen { get; set; } = null!;
        public CatalogoValor Localidad { get; set; } = null!;
        public CatalogoValor TipoMovimiento { get; set; } = null!;
        public Catalogo_Fijo UnidadMedida { get; set; } = null!;
        public Tad Terminal { get; set; } = null!;
        public InventarioCierre Cierre { get; set; } = null!;
    }
}
