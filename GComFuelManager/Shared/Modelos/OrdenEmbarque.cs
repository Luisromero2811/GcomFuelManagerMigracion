using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace GComFuelManager.Shared.Modelos
{
    public class OrdenEmbarque
    {
        [JsonProperty("cod"), Key] public int Cod { get; set; }
        [JsonProperty("fchOrd")] public DateTime? FchOrd { get; set; }
        [JsonProperty("fchPro")] public DateTime? FchPro { get; set; }
        [JsonProperty("codtad")] public Int16? Codtad { get; set; } = 1;
        [JsonProperty("codprd")] public byte? Codprd { get; set; }

        [JsonProperty("vol"), DisplayName("Volumen"), EpplusIgnore]
        public double? Vol { get; set; } = 0;
        [DisplayName("Volumen")]
        public string Volumenes { get { return string.Format(new System.Globalization.CultureInfo("en-US"), "{0:N2}", Vol); } }
        [JsonProperty("codchf")] public int? Codchf { get; set; }
        [JsonProperty("coddes")] public int? Coddes { get; set; }
        [JsonProperty("codest")] public byte? Codest { get; set; }
        [JsonProperty("fchpet")] public DateTime? Fchpet { get; set; }
        [JsonProperty("fchcar")] public DateTime? Fchcar { get; set; } = DateTime.Today;
        [JsonProperty("codton")] public int? Codton { get; set; }
        [JsonProperty("bin")] public int? Bin { get; set; }
        [JsonProperty("codusu")] public int? Codusu { get; set; }
        [JsonProperty("folio")] public int? Folio { get; set; }
        [JsonProperty("pre")] public double? Pre { get; set; } = 0;
        [JsonProperty("codordCom")] public int? CodordCom { get; set; }
        [JsonProperty("bolguidid")] public string? Bolguidid { get; set; }
        [JsonProperty("tp")] public bool? Tp { get; set; }
        [JsonProperty("CompartmentId")] public int? CompartmentId { get; set; }
        [JsonProperty("compartment")] public int? Compartment { get; set; } = 1;
        [JsonProperty("numTonel")] public int? NumTonel { get; set; }
        [EpplusIgnore, NotMapped] public Moneda? Moneda { get; set; } = null!;
        [EpplusIgnore] public int? ID_Moneda { get; set; } = 0;
        public double? Equibalencia { get; set; } = 1;
        [NotMapped] public Destino? Destino { get; set; } = null!;
        [NotMapped] public Tad? Tad { get; set; } = null!;
        [NotMapped] public Producto? Producto { get; set; } = null!;
        [NotMapped] public Tonel? Tonel { get; set; } = null!;
        [NotMapped] public Chofer? Chofer { get; set; } = null!;

        [NotMapped] public OrdenCompra? OrdenCompra { get; set; } = null!;
        [NotMapped] public Estado? Estado { get; set; } = null!;

        [NotMapped] public Cliente? Cliente { get; set; } = null!;
        [NotMapped] public Usuario? Usuario { get; set; } = null!;

        [NotMapped] public Orden? Orden { get; set; } = null!;

        [NotMapped] public Transportista? Transportista { get; set; } = null!;

        [NotMapped] public OrdenCierre? OrdenCierre { get; set; } = null!;
        [NotMapped] public OrdenPedido? OrdenPedido { get; set; } = null!;
        [NotMapped] public int? Compartimento { get; set; } = null!;
        public string? FolioSyn { get; set; } = string.Empty;
        public OrdenEmbarque ShallowCopy()
        {
            return (OrdenEmbarque)this.MemberwiseClone();
        }

        [NotMapped, EpplusIgnore] public int Ordenes_A_Crear { get; set; } = 1;
        [NotMapped, EpplusIgnore] public double Costo { get; set; } = 0;
        [NotMapped, EpplusIgnore] public double Utilidad { get; set; } = 0;
        [NotMapped, EpplusIgnore] public double? Utilidad_Sobre_Volumen { get; set; } = 0;

        public double Obtener_Utilidad_Coste()
        {
            Utilidad = ((Pre ?? 0) - Costo);
            return Utilidad;
        }
        public double? Obtener_Utilidad_Sobre_Volumen()
        {
            var vol = Compartment == 1 && Tonel != null ? double.Parse(Tonel!.Capcom!.ToString() ?? "0")
                                        : Compartment == 2 && Tonel != null ? double.Parse(Tonel!.Capcom2!.ToString() ?? "0")
                                        : Compartment == 3 && Tonel != null ? double.Parse(Tonel!.Capcom3!.ToString() ?? "0")
                                        : Compartment == 4 && Tonel != null ? double.Parse(Tonel!.Capcom4!.ToString() ?? "0")
                                        : Vol ?? 0;
            Utilidad_Sobre_Volumen = vol * Utilidad;
            return Utilidad_Sobre_Volumen;
        }
        public double Obtener_Volumen_De_Orden()
        {
            try
            {
                if (Orden is not null)
                    if(Orden.Vol is not null)
                    return (double)Orden.Vol;

                if (Tonel is not null)
                {
                    if (Compartment == 1) return double.Parse(Tonel.Capcom.ToString() ?? "0");
                    if (Compartment == 2) return double.Parse(Tonel.Capcom2.ToString() ?? "0");
                    if (Compartment == 3) return double.Parse(Tonel.Capcom3.ToString() ?? "0");
                    if (Compartment == 4) return double.Parse(Tonel.Capcom4.ToString() ?? "0");
                }

                if (Vol is not null)
                    return (double)Vol;

                return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }
        public string Obtener_Volumen_De_Orden_En_Formato()
        {
            try
            {
                return Obtener_Volumen_De_Orden().ToString("N", CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return "0";
            }
        }
        public string Obtener_Cliente_De_Orden { get { return OrdenCierre?.Cliente?.Den ?? "Sin cliente asignado"; } }
        public string Obtener_Destino_De_Orden { get { return Destino?.Den ?? "Sin cliente asignado"; } }
        public string Obtener_Producto_De_Orden { get { return Producto?.Den ?? "Sin cliente asignado"; } }
    }
}
