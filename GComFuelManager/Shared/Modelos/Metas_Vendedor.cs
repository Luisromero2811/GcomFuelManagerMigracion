using OfficeOpenXml.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class Metas_Vendedor
    {
        [EpplusIgnore] public int Id { get; set; }
        [EpplusIgnore] public int VendedorId { get; set; }
        [NotMapped, DisplayName("Mes")]
        public string Nombre_Mes
        {
            get
            {
                return Mes.ToString("MMM");
            }
        }
        //[NotMapped, DisplayName("Vendedor")]
        //public string Nombre_Vendedor
        //{
        //    get
        //    {
        //        if (Vendedor is not null)
        //            if (!string.IsNullOrEmpty(Vendedor.Nombre))
        //                return Vendedor.Nombre;

        //        return string.Empty;
        //    }
        //}
        public double? Meta { get; set; } = 0;
        public double? Referencia { get; set; } = 0;
        [DisplayName("Real")]
        public double? Venta_Real { get; set; } = 0;
        [EpplusIgnore] public DateTime Mes { get; set; } = DateTime.Today;
        [EpplusIgnore] public bool Activa { get; set; } = true;

        [NotMapped, EpplusIgnore] public bool Editar_Meta { get; set; } = false;
        [DisplayName("Meta acumulada")]
        [NotMapped] public double? Meta_Acumulada { get; set; } = 0;
        [DisplayName("Resultado mes")]
        [NotMapped] public double? Resultado_Mes { get; set; } = 0;
        [DisplayName("Cumplimiento mes")]
        [NotMapped] public double? Cumplimiento_Mes { get; set; } = 0;
        [DisplayName("Resultado acumulado")]
        [NotMapped] public double? Resultado_Acumulado { get; set; } = 0;
        [DisplayName("Porciento cumplimiento")]
        [NotMapped] public double? Porciento_Cumplimiento { get; set; } = 0;

        [NotMapped, EpplusIgnore]
        public int Mes_De_Referencia
        {
            get
            {
                if ((Mes.Month - 1) == 0)
                    return 12;
                return (Mes.Month - 1);
            }
        }


        [NotMapped, EpplusIgnore] public Vendedor? Vendedor { get; set; } = null!;

        [NotMapped, EpplusIgnore] public int Ano_reporte { get; set; } = DateTime.Today.Year;
    }
}
