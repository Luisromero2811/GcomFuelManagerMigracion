﻿namespace GComFuelManager.Shared.Modelos
{
    public class CRMEquipo
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int LiderId { get; set; }
        public int DivisionId { get; set; }
        public bool Activo { get; set; } = true;
        public CRMOriginador Originador { get; set; } = new();
        public List<CRMEquipoVendedor> EquipoVendedores { get; set; } = new();
        public List<CRMVendedor> Vendedores { get; set; } = new();
        public CRMDivision Division { get; set; } = new();
    }
}
