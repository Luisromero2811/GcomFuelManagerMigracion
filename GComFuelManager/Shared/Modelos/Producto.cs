using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
namespace GComFuelManager.Shared.Modelos
{
	public class Producto
	{
        //5
        [JsonProperty("cod"), Key]
		public byte Cod { get; set; }//FK

		[JsonProperty("den"), MaxLength(50)]
		public string? Den { get; set; } = string.Empty;

		[JsonProperty("codsyn"), MaxLength(10)]
		public string? Codsyn { get; set; } = string.Empty;

		[JsonProperty("activo")]
		public bool? Activo { get; set; } = true;

        public short? Id_Tad { get; set; } = 0;
       
        [NotMapped]
        public Tad? Terminal { get; set; } = null!;

        public string Nombre_Producto { get { return !string.IsNullOrEmpty(Den) ? Den : string.Empty; } }
     
        public string Obtener_Terminal
        {
            get
            {
                if (Terminal is not null)
                    if (!string.IsNullOrEmpty(Terminal.Den) || !string.IsNullOrWhiteSpace(Terminal.Den))
                        return Terminal.Den;

                return string.Empty;
            }
        }

        //public List<OrdenEmbarque> OrdenEmbarque { get; set; } = null!;
    }
}

