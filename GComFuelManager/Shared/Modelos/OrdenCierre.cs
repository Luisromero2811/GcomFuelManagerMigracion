using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class OrdenCierre
    {
        [Key]
        public int Cod { get; set; }
        public DateTime FchCierre { get; set; } = DateTime.Now;
        public string Folio { get; set; } = string.Empty;
        public string Contacto { get; set; } = string.Empty;
        public string Correo { get; set;} = string.Empty;
        public int Prd { get; set; }
        public Producto Producto { get; set; } = null!;
        public int Cte { get; set; }
        public Cliente Cliente { get; set; } = null!;
        public double Precio { get; set; }
        public double Tempratura { get; set; }
        public string Vendedor { get; set; } = string.Empty;
        public int Des { get; set; }
        public Destino Destino { get; set; } = null!;
        public int Volumen { get; set; }
        public string Observaciones { get; set; } = string.Empty;
        public bool Estatus { get; set; } = true;
    }
}
