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

    public class Vendedor_Reporte_Desemeño
    {
        public string Vendedor { get; set; } = string.Empty;
        public double Ene { get; set; } = 0;
        public double Feb { get; set; } = 0;
        public double Mar { get; set; } = 0;
        public double Abr { get; set; } = 0;
        public double May { get; set; } = 0;
        public double Jun { get; set; } = 0;
        public double Jul { get; set; } = 0;
        public double Ago { get; set; } = 0;
        public double Sep { get; set; } = 0;
        public double Oct { get; set; } = 0;
        public double Nov { get; set; } = 0;
        public double Dic { get; set; } = 0;

    }

    public class Reporte_Venta
    {
        public List<Vendedor> Vendedores { get; set; } = new();
        public List<int> Meses_Venta { get; set; } = new();
    }

    public class Reporte_Completo_Vendedor_Desempeño
    {
        //public string Letra_Inicio { get; set; } = string.Empty;
        [EpplusIgnore] public string Letra_Fin { get; set; } = string.Empty;
        [EpplusIgnore] public List<string> Meses { get; set; } = new();
        //public List<Vendedor_Reporte_Desemeño> Litros { get; set; } = new();
        //public List<Vendedor_Reporte_Desemeño> Venta { get; set; } = new();
        public List<ExpandoObject> Litros { get; set; } = new();
        public List<ExpandoObject> Venta { get; set; } = new();
        public List<Dictionary<string,object>> Diccionario_Litros { get; set; } = new();
        public List<Dictionary<string, object>> Diccionario_Ventas { get; set; } = new();
    }

    public class Mes_Letra_Excel
    {
        public string Mes { get; set; } = string.Empty;
        public string Letra { get; set; } = string.Empty;
    }
}
