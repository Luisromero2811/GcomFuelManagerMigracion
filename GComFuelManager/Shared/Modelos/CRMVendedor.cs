using System.ComponentModel.DataAnnotations.Schema;

namespace GComFuelManager.Shared.Modelos
{
    public class CRMVendedor
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public int DivisionId { get; set; }
        public string? Titulo { get; set; } = string.Empty;
        public string? Departamento { get; set; } = string.Empty;
        public string? Tel_Oficina { get; set; } = string.Empty;
        public string? Tel_Movil { get; set; } = string.Empty;
        public string? Correo { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public string? UserId { get; set; } = string.Empty;
        public CRMDivision Division { get; set; } = null!;
        public List<CRMVendedorOriginador> VendedorOriginadores { get; set; } = new();
        public List<CRMOriginador> Originadores { get; set; } = new();
        public List<CRMEquipoVendedor> EquipoVendedores { get; set; } = new();
        public List<CRMEquipo> Equipos { get; set; } = new();

        [NotMapped]
        public string FullName { get { return $"{Nombre} {Apellidos}"; } }
    }
}
