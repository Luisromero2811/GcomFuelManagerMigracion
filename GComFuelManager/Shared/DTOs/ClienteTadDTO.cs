using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using GComFuelManager.Shared.Modelos;
using Newtonsoft.Json;

namespace GComFuelManager.Shared.DTOs
{
	public class ClienteTadDTO
	{
        //Instancias
         public Cliente? Cliente { get; set; } = null!;
         public Tad? Terminal { get; set; } = null!;
        //Listas
        public List<Tad> Tads { get; set; } = new List<Tad>();
        public List<Cliente> Clientes { get; set; } = new List<Cliente>();
        //Strings
        public string NombreCliente { get; set; } = string.Empty!;
        public string NombreTerminal { get; set; } = string.Empty!;
        //Codigos
        public int CodCte { get; set; }
        public Int16 Id_Tad { get; set; }
    }
}

