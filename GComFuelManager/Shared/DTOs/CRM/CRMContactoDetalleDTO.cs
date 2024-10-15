﻿using GComFuelManager.Shared.Modelos;

namespace GComFuelManager.Shared.DTOs.CRM
{
    public class CRMContactoDetalleDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Departamento { get; set; } = string.Empty;
        public int? DivisionId { get; set; }
        public int? CuentaId { get; set; }
        public string Tel_Oficina { get; set; } = string.Empty;
        public string Tel_Movil { get; set; } = string.Empty;
        public string SitioWeb { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Calle { get; set; } = string.Empty;
        public string Colonia { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string CP { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
        public int EstatusId { get; set; }
        public string Estatus_Desc { get; set; } = string.Empty;
        public int OrigenId { get; set; }
        public string Recomen { get; set; } = string.Empty;
        public int? VendedorId { get; set; }
        public DateTime Fecha_Creacion { get; set; } = DateTime.Now;

        public CRMCatalogoValor? Estatus { get; set; } = null!;
        public CRMCatalogoValor? Origen { get; set; } = null!;
        public CRMVendedorDTO? Vendedor { get; set; } = null!;
        public CRMCliente? Cliente { get; set; } = null!;
        public CRMDivision? Division { get; set; } = null!;
    }
}