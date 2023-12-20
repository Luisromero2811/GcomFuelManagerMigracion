﻿using GComFuelManager.Shared.Modelos;

namespace GComFuelManager.Shared.DTOs
{
    public class Folio_Activo_Vigente
    {
        public string Folio { get; set; } = string.Empty;
        public double Volumen_Disponible { get; set; } = 0;
        public Cliente? Cliente { get; set; } = null;
        public Destino? Destino { get; set; } = null;
        public Grupo? Grupo { get; set; } = null;
        public Producto? Producto { get; set; } = null;
        public DateTime Fecha_Vigencia { get; set; } = DateTime.Now;
        public DateTime Fecha_Cierre { get; set; } = DateTime.Now;
        public int ID_Cierre { get; set; }
        public byte? ID_Producto { get; set; } = 0;
        public string? Nombre_Grupo { get { return Grupo is not null ? Grupo.Den : "Sin grupo"; } }
        public string? Nombre_Cliente { get { return Cliente is not null ? Cliente.Den : "Sin cliente"; } }
        public string? Nombre_Destino { get { return Destino is not null ? Destino.Den : "Sin destino"; } }
        public string? Nombre_Producto { get { return Producto is not null ? Producto.Den : "Sin producto"; } }
        public string? Grupo_Filtrado { get; set; } = string.Empty;
        public string? Cliente_Filtrado { get; set; } = string.Empty;
        public string? Destino_Filtrado { get; set; } = string.Empty;
        public string? Producto_Filtrado { get; set; } = string.Empty;
        public VolumenDisponibleDTO? VolumenDisponibleDTO { get; set; } = null;
        public string? Comentarios { get; set; } = string.Empty;
    }
}