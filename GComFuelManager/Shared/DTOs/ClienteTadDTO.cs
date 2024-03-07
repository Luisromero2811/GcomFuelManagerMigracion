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
        public List<ClienteTadDTO> Tads { get; set; } = new List<ClienteTadDTO>();
        public List<ClienteTadDTO> Clientes { get; set; } = new List<ClienteTadDTO>();
        //Strings
        public string NombreCliente { get; set; } = string.Empty!;
        public string NombreTerminal { get; set; } = string.Empty!;
        //Codigos
        public int CodCte { get; set; }
        public Int16 Cod { get; set; }
        public Int16 Id_Tad { get; set; } = 0;
    }
}

