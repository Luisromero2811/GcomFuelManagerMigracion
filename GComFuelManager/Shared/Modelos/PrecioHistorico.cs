﻿using Newtonsoft.Json;
using OfficeOpenXml.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class PrecioHistorico
    {
        [Key, JsonProperty("cod"), EpplusIgnore] public byte Cod { get; set; }
        [JsonProperty("codZona"), EpplusIgnore] public int codZona { get; set; }
        [JsonProperty("codGru"), EpplusIgnore] public Int16 codGru { get; set; }
        [JsonProperty("codCte"), EpplusIgnore] public int codCte { get; set; }
        [JsonProperty("codDes"), EpplusIgnore] public int codDes { get; set; }
        [JsonProperty("codPrd"), EpplusIgnore] public byte codPrd { get; set; }
        [JsonProperty("pre")] public float pre { get; set; }
        [JsonProperty("fchActualizacion"), EpplusIgnore] public DateTime FchActualizacion { get; set; } = DateTime.Now;
        [JsonProperty("fchDia"), EpplusIgnore] public DateTime FchDia { get; set; } = DateTime.Now;
        [JsonProperty("fecha"), DisplayName("Fecha")]
        public string FechaActualizacion
        {
            get
            {
                return FchActualizacion.ToString("dd/MM/yyyy");
            }
        }
        [JsonProperty("fecha"), DisplayName("Fecha")]
        public string Fecha
        {
            get
            {
                return FchDia.ToString("dd/MM/yyyy");
            }
        }
        [NotMapped, DisplayName("Zona")] public string? NombreZona { get { return Zona?.Nombre; } }
        [NotMapped, DisplayName("Cliente")] public string? NombreCliente { get { return Cliente?.Den; } }
        [NotMapped, DisplayName("Producto")] public string? NombreProducto { get { return Producto?.Den; } }
        [NotMapped,DisplayName("Destino")] public string? NombreDestino { get { return Destino?.Den; } }

        [NotMapped, EpplusIgnore] public Zona? Zona { get; set; } = null!;
        [NotMapped, EpplusIgnore] public Cliente? Cliente { get; set; } = null!;
        [NotMapped, EpplusIgnore] public Producto? Producto { get; set; } = null!;
        [NotMapped, EpplusIgnore] public Destino? Destino { get; set; } = null!;
    }
}