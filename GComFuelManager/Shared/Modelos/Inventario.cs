using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class Inventario
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public int SitioId { get; set; }
        public int AlmacenId { get; set; }
        public int LocalidadId { get; set; }
        public int TipoMovimientoId { get; set; }
        public string Referencia { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public double Cantidad { get; set; }
        public int UnidadMedidaId { get; set; }
        public DateTime FechaCierre { get; set; } = DateTime.Today;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public bool Activo { get; set; } = true;

        public Producto Producto { get; set; } = new();
        public Catalogo_Fijo Sitio { get; set; } = new();
        public Catalogo_Fijo Almacen { get; set; } = new();
        public Catalogo_Fijo Localidad { get; set; } = new();
        public Catalogo_Fijo TipoMovimiento { get; set; } = new();
        public Catalogo_Fijo UnidadMedida { get; set; } = new();
    }
}
