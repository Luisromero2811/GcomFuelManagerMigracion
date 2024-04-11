using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GComFuelManager.Shared.DTOs;
using Newtonsoft.Json;
using OfficeOpenXml.Attributes;

namespace GComFuelManager.Shared.Modelos
{
	public class GrupoTransportista
	{
        [Key, JsonProperty("cod"), EpplusIgnore] public int cod { get; set; }
        //   public int Cod { get; set; } = 0;

        [MaxLength(255)]
        [DisplayName("Nombre del Grupo Transportista")] public string? den { get; set; } = string.Empty;
        [EpplusIgnore] public short? Id_Tad { get; set; }
        [EpplusIgnore] public bool Activo { get; set; } = true;

        [NotMapped, EpplusIgnore] public Tad? Tad { get; set; }
        [NotMapped, EpplusIgnore] public List<Tad> Terminales { get; set; } = new();
        [NotMapped, EpplusIgnore] public List<GrupoTransportista_Tad> GrupoTransportista_Tads { get; set; } = new();
    }
}

