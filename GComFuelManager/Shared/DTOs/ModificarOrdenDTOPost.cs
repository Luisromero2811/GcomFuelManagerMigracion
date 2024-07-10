using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.DTOs
{
    public class ModificarOrdenDTOPost
    {
        public int? ID { get; set; }
        public long? Id_Orden { get; set; }
        public int? Id_OrdenEmbarque { get; set; }
        public bool? Facturar { get; set; } = true;
        public int? Bol { get; set; }
        public DateTime? Fecha_Carga { get; set; } = DateTime.Today;
        public DateTime? Fecha_LLegada { get; set; } = DateTime.Today;
        public double? Litros { get; set; }
        public string? Pedimento { get; set; } = string.Empty;
        public string? Sellos { get; set; } = string.Empty;
        public string? NOrden { get; set; } = string.Empty;
        public string? Factura { get; set;} = string.Empty;
        public string Eta
        {
            get
            {
                TimeSpan time;
                double hours = 0;
                double min = 0;
                if (Fecha_Carga is not null && Fecha_LLegada is not null)
                {
                    time = TimeSpan.FromHours(Fecha_LLegada.Value.Subtract((DateTime)Fecha_Carga).TotalHours);
                    if (time.Days >= 1)
                    {
                        hours = time.Days * 24;
                        hours += time.Hours;

                        return $"{hours:00}:{min:00}";
                    }
                    else
                    {

                        return time.ToString($"hh\\:mm");
                    }
                }
                return string.Empty;
            }
        }
        public long? Id_Bol { get; set; }
        public int Id_Cliente { get; set; }
        public int Id_Destino { get; set; }
        public short? Id_Grupo { get; set; }
        public int Id_Tonel { get; set; }
        public int Id_Transportista { get; set; }
        public int Id_Chofer { get; set; }
        public byte Id_Producto { get; set; }
    }
}
