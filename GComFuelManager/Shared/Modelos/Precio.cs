﻿using Newtonsoft.Json;
using OfficeOpenXml.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class Precio
    {
        [Key, JsonProperty("cod"), EpplusIgnore] public int? Cod { get; set; }
        [EpplusIgnore] public int? codZona { get; set; }
        [EpplusIgnore] public int? codDes { get; set; }
        [EpplusIgnore] public int? codCte { get; set; }
        [EpplusIgnore] public Int16? codGru { get; set; }
        [EpplusIgnore] public byte? codPrd { get; set; }
        [JsonProperty("pre")] public double Pre { get; set; } = 0;
        [EpplusIgnore] public DateTime FchActualizacion { get; set; } = DateTime.Now;
        [EpplusIgnore] public DateTime FchDia { get; set; } = DateTime.Today;
        [EpplusIgnore, NotMapped] public Moneda? Moneda { get; set; } = null!;
        [EpplusIgnore] public int? ID_Moneda { get; set; } = 0;
        public double? Equibalencia { get; set; } = 1;
        public int? ID_Usuario { get; set; } = 0;

        [DisplayName("Fecha de actualizacion")]
        public string FechaActualizacion
        {
            get
            {
                return FchActualizacion.ToString("dd/MM/yyyy");
            }
        }
        [DisplayName("Fecha")]
        public string Fecha
        {
            get
            {
                return FchDia.ToString("dd/MM/yyyy");
            }
        }
        [EpplusIgnore, NotMapped]
        public bool PrecioOverDate
        {
            get { return FchDia < DateTime.Today; }
        }
        [NotMapped, DisplayName("Zona")] public string? NombreZona { get { return Zona?.Nombre; } }
        [NotMapped, DisplayName("Destino")] public string? NombreDestino { get { return Destino?.Den; } }
        [NotMapped, DisplayName("Cliente")] public string? NombreCliente { get { return Cliente?.Den; } }
        [NotMapped, DisplayName("Grupo")] public string? NombreGrupo { get { return Grupo?.Den; } }
        [NotMapped, DisplayName("Producto")] public string? NombreProducto { get { return Producto?.Den; } }
        [NotMapped, DisplayName("Usuario")] public string? NombreUsuario { get { return Usuario?.Den; } }

        [EpplusIgnore] public bool Activo { get; set; } = true;

        [NotMapped, EpplusIgnore] public Zona? Zona { get; set; } = null!;
        [NotMapped, EpplusIgnore] public Cliente? Cliente { get; set; } = null!;
        [NotMapped, EpplusIgnore] public Producto? Producto { get; set; } = null!;
        [NotMapped, EpplusIgnore] public Destino? Destino { get; set; } = null!;
        [NotMapped, EpplusIgnore] public Grupo? Grupo { get; set; } = null!;
        [NotMapped, EpplusIgnore] public Usuario? Usuario { get; set; } = null!;
    }
}
