using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class Archivo
    {
        public int Id { get; set; }
        public string Directorio { get; set; } = string.Empty;
        public string URL { get; set; } = string.Empty;
        public int Id_Registro { get;set; }
        public short Id_Tad { get; set; }
        public Tipo_Archivo Tipo_Archivo { get; set; }

        public OrdenEmbarque? OrdenEmbarque { get; set; } = null!;
    }

    public enum Tipo_Archivo
    {
        NONE,
        PDF,
        XML,
        PDF_FACTURA,
        ARCHIVO_BOL,
        XML_FACTURA
    }
}
