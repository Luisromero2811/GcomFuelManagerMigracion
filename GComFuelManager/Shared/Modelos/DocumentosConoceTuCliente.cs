﻿using System;
namespace GComFuelManager.Shared.Modelos
{
	public class DocumentosConoceTuCliente
	{
        public int Id { get; set; }
        public string NombreArchivo { get; set; } = string.Empty;
        public string NombreDocumento { get; set; } = string.Empty;
        public string TipoDocumento { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime FechaCaducidad { get; set; } = DateTime.Today;
        public string Descripcion { get; set; } = string.Empty;
        //public int DocumentoRelacionado { get; set; }
        public bool Activo { get; set; } = true;
        public string Version { get; set; } = string.Empty;
        //public int RevisionRelacionada { get; set; }
        public string VersionCreadaPor { get; set; } = string.Empty;
        public string Directorio { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;


    }
}

