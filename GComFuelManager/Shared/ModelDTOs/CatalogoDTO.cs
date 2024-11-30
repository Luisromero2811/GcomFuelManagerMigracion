using GComFuelManager.Shared.Filtro;

namespace GComFuelManager.Shared.ModelDTOs
{
    public class CatalogoDTO : Parametros_Busqueda_Gen
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }
}
