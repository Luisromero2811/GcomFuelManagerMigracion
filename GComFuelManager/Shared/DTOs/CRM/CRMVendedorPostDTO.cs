namespace GComFuelManager.Shared.DTOs.CRM
{
    public class CRMVendedorPostDTO
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

        //public List<CRMOriginadorDTO> OriginadoresDTO { get; set; } = new();
        public List<CRMEquipoDTO> EquiposDTO { get; set; } = new();
    }
}
