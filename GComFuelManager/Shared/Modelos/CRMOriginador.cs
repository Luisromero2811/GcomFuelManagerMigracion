namespace GComFuelManager.Shared.Modelos
{
    public class CRMOriginador
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public int DivisionId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Departamento { get; set; } = string.Empty;
        public string Tel_Oficina { get; set; } = string.Empty;
        public string Tel_Movil { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public CRMDivision Division { get; set; } = null!;
        public List<CRMVendedorOriginador> VendedorOriginadores { get; set; } = new();
        public List<CRMVendedor> Vendedores { get; set; } = new();
    }
}
