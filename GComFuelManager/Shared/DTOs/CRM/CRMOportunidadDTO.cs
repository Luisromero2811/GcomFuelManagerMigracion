using GComFuelManager.Shared.Filtro;

namespace GComFuelManager.Shared.DTOs.CRM
{
    public class CRMOportunidadDTO : Parametros_Busqueda_Gen
    {
        public int Id { get; set; }
        public string Nombre_Opor { get; set; } = string.Empty;
        public string Vendedor { get; set; } = string.Empty;
        public string Cuenta { get; set; } = string.Empty;
        public string EtapaVenta { get; set; } = string.Empty;
        public decimal Probabilidad { get; set; }
        public bool Activo { get; set; } = true;
    }
}
