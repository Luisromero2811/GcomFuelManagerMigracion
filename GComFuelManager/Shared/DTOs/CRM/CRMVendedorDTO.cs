﻿using GComFuelManager.Shared.Filtro;

namespace GComFuelManager.Shared.DTOs.CRM
{
    public class CRMVendedorDTO : Parametros_Busqueda_Gen
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string? NombreDivision { get; set; } = string.Empty;
        public string Tel_Movil { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Fullname { get => $"{Nombre} {Apellidos}"; }
        public int EquipoId { get; set; }
        public int DivisionId { get; set; }
    }
}
