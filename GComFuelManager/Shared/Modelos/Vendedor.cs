using OfficeOpenXml.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Dynamic;
using System.Text.Json.Serialization;

namespace GComFuelManager.Shared.Modelos
{
    public class Vendedor
    {
        public int Id { get; set; }
        [StringLength(250, ErrorMessage = "{0} no debe de tener una longitud de mas de 250 caracteres")] public string Nombre { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        [NotMapped] public List<Cliente>? Clientes { get; set; } = null!;
        [NotMapped] public List<Mes_Venta> Venta_Por_Meses { get; set; } = new();
        [NotMapped, EpplusIgnore] public int Fecha_Registro { get; set; } = DateTime.Today.Year;
        [NotMapped, JsonIgnore] public List<Vendedor_Originador> Vendedor_Originador { get; set; } = new();
        [NotMapped] public List<Originador> Originadores { get; set; } = new();
        [NotMapped] public int Id_Originador { get; set; } = 0;
        [NotMapped] public string Nombre_Originador { get; set; } = string.Empty;
        [NotMapped] public bool Show_Originador { get; set; } = false;
        [NotMapped] public List<Metas_Vendedor> Metas_Vendedor { get; set; } = new();
        [NotMapped] public List<int> Meses_Venta { get; set; } = new();

    }

    public class Mes_Venta
    {
        public int Nro_Mes { get; set; } = 0;
        public string Nombre_Mes
        {
            get
            {
                if (Nro_Mes != 0)
                    return new DateTime(1, Nro_Mes, 1).ToString("MMM");
                return string.Empty;
            }
        }
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

    public class Reporte_Venta
    {
        public List<Vendedor> Vendedores { get; set; } = new();
        public List<int> Meses_Venta { get; set; } = new();
        public List<Mes_Venta> Totales { get; set; } = new();
    }

    public class Reporte_Completo_Vendedor_Desempeño
    {
        public List<ExpandoObject> Litros { get; set; } = new();
        public List<ExpandoObject> Venta { get; set; } = new();
        public List<Dictionary<string, object>> Diccionario_Litros { get; set; } = new();
        public List<Dictionary<string, object>> Diccionario_Ventas { get; set; } = new();
    }
}
