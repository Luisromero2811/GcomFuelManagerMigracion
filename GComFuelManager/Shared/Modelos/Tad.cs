using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;

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

        public override string ToString()
        {
            return Den ?? string.Empty;
        }
    }
}

