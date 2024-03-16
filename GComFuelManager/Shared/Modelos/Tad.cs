using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OfficeOpenXml.Attributes;
using System.Text.Json.Serialization;

namespace GComFuelManager.Shared.Modelos
{
    //5
    public class Tad
    {
        [Key]
        public Int16 Cod { get; set; }

        [StringLength(128)]
        public string? Den { get; set; } = string.Empty;

        public Int16? Nro { get; set; } = 0;

        public bool? Activo { get; set; } = true;
        [StringLength(5)]
        public string? Codigo { get; set; } = string.Empty;
        [StringLength(5)]
        public string? CodigoOrdenes { get; set; } = string.Empty;
        //public List<OrdenEmbarque> OrdenEmbarque { get; set; } = null!;
        [NotMapped, JsonIgnore] public List<Cliente> Clientes { get; set; } = new();
        [NotMapped, JsonIgnore] public List<Cliente_Tad> Cliente_Tads { get; set; } = new();
        [NotMapped, JsonIgnore] public List<Destino> Destinos { get; set; } = new();
        [NotMapped, JsonIgnore] public List<Destino_Tad> Destino_Tads { get; set; } = new();
        [NotMapped, JsonIgnore] public List<Transportista> Transportistas { get; set; } = new();
        [NotMapped, JsonIgnore] public List<Transportista_Tad> Transportista_Tads { get; set; } = new();
        [NotMapped, JsonIgnore] public List<Chofer> Choferes { get; set; } = new();
        [NotMapped, JsonIgnore] public List<Chofer_Tad> Chofer_Tads { get; set; } = new();
        [NotMapped, JsonIgnore] public List<Tonel> Unidades { get; set; } = new();
        [NotMapped, JsonIgnore] public List<Unidad_Tad> Unidad_Tads { get; set; } = new();
    }
}

