using Newtonsoft.Json;
using OfficeOpenXml.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class PrecioHistorico
    {
        [Key, JsonProperty("cod"), EpplusIgnore] public int? Cod { get; set; } = 0;
        [JsonProperty("codZona"), EpplusIgnore] public int? codZona { get; set; } = 0;
        [JsonProperty("codGru"), EpplusIgnore] public Int16? codGru { get; set; } = 0;
        [JsonProperty("codCte"), EpplusIgnore] public int? codCte { get; set; } = 0;
        [JsonProperty("codDes"), EpplusIgnore] public int? codDes { get; set; } = 0;
        [JsonProperty("codPrd"), EpplusIgnore] public byte? codPrd { get; set; } = 0;
        [JsonProperty("pre")] public double? pre { get; set; } = 0;
        [JsonProperty("fchActualizacion"), EpplusIgnore] public DateTime FchActualizacion { get; set; } = DateTime.Now;
        [JsonProperty("fchDia"), EpplusIgnore] public DateTime FchDia { get; set; } = DateTime.Now;
        [EpplusIgnore, NotMapped] public Moneda? Moneda { get; set; } = null!;
        [EpplusIgnore] public int? ID_Moneda { get; set; } = 0;
        public double? Equibalencia { get; set; } = 1;
        public int? ID_Usuario { get; set; } = 0;

        [JsonProperty("fechaActualizacion"), DisplayName("Fecha de actualizacion")]
        public string FechaActualizacion
        {
            get
            {
                return FchActualizacion.ToString("dd/MM/yyyy");
            }
        }
        [JsonProperty("fecha"), DisplayName("Fecha")]
        public string Fecha
        {
            get
            {
                return FchDia.ToString("dd/MM/yyyy");
            }
        }
        [NotMapped, DisplayName("Zona")] public string? NombreZona { get { return Zona?.Nombre; } }
        [NotMapped, DisplayName("Cliente")] public string? NombreCliente { get { return Cliente?.Den; } }
        [NotMapped, DisplayName("Producto")] public string? NombreProducto { get { return Producto?.Den; } }
        [NotMapped,DisplayName("Destino")] public string? NombreDestino { get { return Destino?.Den; } }
        [NotMapped, DisplayName("Usuario")] public string? NombreUsuario { get { return Usuario?.Den; } }

        [NotMapped, EpplusIgnore] public Zona? Zona { get; set; } = null!;
        [NotMapped, EpplusIgnore] public Cliente? Cliente { get; set; } = null!;
        [NotMapped, EpplusIgnore] public Producto? Producto { get; set; } = null!;
        [NotMapped, EpplusIgnore] public Destino? Destino { get; set; } = null!;
        [NotMapped, EpplusIgnore] public Usuario? Usuario { get; set; } = null!;
    }
}
