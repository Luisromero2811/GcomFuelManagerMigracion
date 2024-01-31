using OfficeOpenXml.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class Vendedor
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        [NotMapped] public List<Cliente>? Clientes { get; set; } = null!;
        [NotMapped] public List<Mes_Venta> Venta_Por_Meses { get; set; } = new();
        [NotMapped, EpplusIgnore] public DateTime Fecha_Registro { get; set; } = DateTime.Now;
    }

    public class Mes_Venta
    {
        public int Nro_Mes { get; set; } = 0;
        public string Nombre_Mes { get; set; } = string.Empty;
        public double Venta { get; set; } = 0;
        public double Litros_Vendidos { get; set; } = 0;
        public Meta_Venta Meta_Venta { get; set; } = Meta_Venta.None;
        public List<Mes_Venta_Producto> Mes_Venta_Productos { get; set; } = new();
        
    }

    public class Mes_Venta_Producto
    {
        public string Producto { get; set; } = string.Empty;
        public double Litros_Vendidos { get; set; } = 0;
        public double Venta { get; set; } = 0;
    }

    public enum Meta_Venta
    {
        None,
        Verde,
        Amarillo,
        Rojo
    }
}
