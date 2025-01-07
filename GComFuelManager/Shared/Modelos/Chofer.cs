using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;


namespace GComFuelManager.Shared.Modelos
{
    public class Chofer
    {
        [Key]
        public int Cod { get; set; }
        public string? Den { get; set; } = string.Empty;
        public int? Codtransport { get; set; } = 0;
        public string? Dricod { get; set; } = string.Empty;
        public string? Shortden { get; set; } = string.Empty;
        public bool? Activo { get; set; } = true;
        public string? RFC { get; set; } = string.Empty;

        [NotMapped] public Transportista? Transportista { get; set; } = null!;
        [NotMapped] public Tonel? Tonel { get; set; } = null!;

        [NotMapped]
        public string Fullname
        {
            get
            {
                if (!string.IsNullOrEmpty(Den) && !string.IsNullOrEmpty(Shortden) && Den.ToLower().Equals(Shortden.ToLower()))
                    return Shortden;
                else
                    return $"{Den} {Shortden}";
            }
        }

        public override string ToString()
        {
            return Fullname;
        }
    }
}

