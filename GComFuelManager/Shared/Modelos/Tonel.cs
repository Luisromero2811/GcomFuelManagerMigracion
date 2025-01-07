using Newtonsoft.Json;
using OfficeOpenXml.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class Tonel
    {
        [Key] public int Cod { get; set; }
        public string? Placa { get; set; } = string.Empty;
        public int? Codsyn { get; set; } = 0;
        public string? Carid { get; set; } = string.Empty;
        public int? Nrocom { get; set; } = 0;
        public int? Idcom { get; set; } = 0;
        public decimal? Capcom { get; set; } = decimal.Zero;
        public int? Nrocom2 { get; set; } = 0;
        public int? Idcom2 { get; set; } = 0;
        public decimal? Capcom2 { get; set; } = decimal.Zero;
        public int? Nrocom3 { get; set; } = 0;
        public int? Idcom3 { get; set; } = 0;
        public decimal? Capcom3 { get; set; } = decimal.Zero;
        public string? Tracto { get; set; } = string.Empty;
        public string? Placatracto { get; set; } = string.Empty;
        public bool? Activo { get; set; } = true;
        public bool? Gps { get; set; } = false;
        public int? Nrocom4 { get; set; } = 0;
        public int? Idcom4 { get; set; } = 0;
        public int? Capcom4 { get; set; } = 0;
        public Transportista? Transportista { get; set; } = null!;

        public string Den { get { return $"{Tracto} {Placatracto} {Placa} {Capcom!} {Capcom2!} {Capcom3!} {Capcom4!} {Codsyn!}"; } }

        public string Veh { get { return $"{Tracto} {Placa}"; } }

        public override string ToString()
        {
            return Veh;
        }
    }
}
