using GComFuelManager.Shared.Filtro;

namespace GComFuelManager.Shared.DTOs.CRM
{
    public class CRMCatalogoDTO : Parametros_Busqueda_Gen
    {
        public int Id { get; set; }
        public string Clave { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }
}
