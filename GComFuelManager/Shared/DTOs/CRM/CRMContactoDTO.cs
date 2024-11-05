using GComFuelManager.Shared.Filtro;

namespace GComFuelManager.Shared.DTOs.CRM
{
    public class CRMContactoDTO : Parametros_Busqueda_Gen
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Cuenta { get; set; } = string.Empty;
        public int? CuentaId { get; set; } = 0;
        public string Tel_Oficina { get; set; } = string.Empty;
        public string Tel_Movil { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Estatus { get; set; } = string.Empty;
        public string? Estado { get; set; } = string.Empty;
        //public int Estatus { get; set; }
        public string Vendedor { get; set; } = string.Empty;
        public int VendedorId { get; set; }
        public bool Activo { get; set; }
        public string Division { get; set; } = string.Empty;
        public string FullName
        {
            get
            {
                return $"{Nombre} {Apellidos}";
            }
        }
    }
}
