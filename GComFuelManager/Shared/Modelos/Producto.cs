using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
namespace GComFuelManager.Shared.Modelos
{
	public class Producto
	{
        [ Key]
		public byte Cod { get; set; }
		public string? Den { get; set; } = string.Empty;
		public string? Codsyn { get; set; } = string.Empty;
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
        public override string ToString()
        {
            return Den ?? string.Empty;
        }
    }
}

