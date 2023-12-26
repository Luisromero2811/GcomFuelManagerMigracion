using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.DTOs
{
    public class VolumenDisponibleDTO
    {
        public int VolumenTotal { get; set; } = 0;
        public List<ProductoVolumen> Productos { get; set; } = new List<ProductoVolumen>();
    }

    public class ProductoVolumen
    {
        public byte? ID_Producto { get; set; } = 0;
        public string? Nombre { get; set; } = string.Empty;
        public double? Total { get; set; } = 0;
        [DisplayName("Total")]
        public string Totales { get { return string.Format(new System.Globalization.CultureInfo("en-US"), "{0:N2}", Total); } }
        public double? Reservado { get; set; } = 0;
        [DisplayName("Reservado")]
        public string Reservados { get { return string.Format(new System.Globalization.CultureInfo("en-US"), "{0:N2}", Reservado); } }
        public double? Disponible { get; set; } = 0;
        [DisplayName("Disponible")]
        public string Disponibles { get { return string.Format(new System.Globalization.CultureInfo("en-US"), "{0:N2}", Disponible); } }
        public double? Congelado { get; set; } = 0;
        [DisplayName("Congelado")]
        public string Congelados { get { return string.Format(new System.Globalization.CultureInfo("en-US"), "{0:N2}", Congelado); } }
        public double? Consumido { get; set; } = 0;
        [DisplayName("Consumidos")]
        public string Consumidos { get { return string.Format(new System.Globalization.CultureInfo("en-US"), "{0:N2}", Consumido); } }
        public double? Remanente { get; set; } = 0;
        [DisplayName("Remanentes")]
        public string Remanentes { get { return string.Format(new System.Globalization.CultureInfo("en-US"), "{0:N2}", Remanente); } }
        public double? PromedioCarga { get; set; } = 0;
        [DisplayName("PromediosCarga")]
        public string Promedios { get { return string.Format(new System.Globalization.CultureInfo("en-US"), "{0:N2}", PromedioCarga); } }
        public double? Programado { get; set; } = 0;
        [DisplayName("Programados")]
        public string Programados { get { return string.Format(new System.Globalization.CultureInfo("en-US"), "{0:N2}", Programado); } }
        public double? Solicitud { get; set; } = 0;
        [DisplayName("Solicitudes")]
        public string Solicitudes { get { return string.Format(new System.Globalization.CultureInfo("en-US"), "{0:N2}", Solicitud); } }
    }
}
