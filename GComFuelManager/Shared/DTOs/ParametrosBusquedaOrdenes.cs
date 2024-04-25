using System;
using GComFuelManager.Shared.Modelos;
using OfficeOpenXml.Attributes;

namespace GComFuelManager.Shared.DTOs
{
	public class ParametrosBusquedaOrdenes
	{
        public string estado { get; set; } = string.Empty;
        public string producto { get; set; } = string.Empty;
        public string destino { get; set; } = string.Empty;
        public string unidad { get; set; } = string.Empty;
        public string chofer { get; set; } = string.Empty;
        public string transportista { get; set; } = string.Empty;
        public int pagina { get; set; } = 1;
        public int tamanopagina { get; set; } = 10;
        public DateTime DateInicio { get; set; } = DateTime.Today.Date;
        public DateTime DateFin { get; set; } = DateTime.Now;
        [EpplusIgnore] public Tad? Tad { get; set; }
    }
}

