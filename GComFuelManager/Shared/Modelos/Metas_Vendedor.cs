using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class Metas_Vendedor
    {
        public int Id { get; set; }
        public int VendedorId { get; set; }
        public double? Meta { get; set; } = 0;
        public double? Referencia { get; set; } = 0;
        public double? Venta_Real { get; set; } = 0;
        public DateTime Mes { get; set; } = DateTime.Today;
        public bool Activa { get; set; } = true;

        [NotMapped] public bool Editar_Meta { get; set; } = false;

        [NotMapped] public double? Meta_Acumulada { get; set; } = 0;
        [NotMapped] public double? Resultado_Mes { get; set; } = 0;
        [NotMapped] public double? Cumplimiento_Mes { get; set; } = 0;
        [NotMapped] public double? Resultado_Acumulado { get; set; } = 0;
        [NotMapped] public double? Porciento_Cumplimiento { get; set; } = 0;
        [NotMapped]
        public string Nombre_Mes
        {
            get
            {
                return Mes.ToString("MMM");
            }
        }

        [NotMapped]
        public int Mes_De_Referencia
        {
            get
            {
                if ((Mes.Month - 1) == 0)
                    return 12;
                return (Mes.Month - 1);
            }
        }


        [NotMapped] public Vendedor? Vendedor { get; set; } = null!;
    }
}
