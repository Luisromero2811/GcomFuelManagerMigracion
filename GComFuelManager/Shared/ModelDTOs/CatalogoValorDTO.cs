using GComFuelManager.Shared.Filtro;

namespace GComFuelManager.Shared.ModelDTOs
{
    public class CatalogoValorDTO : Parametros_Busqueda_Gen
    {
        public int Id { get; set; }
        public string Valor { get; set; } = string.Empty;
        public string Abreviacion { get; set; } = string.Empty;
        public int CatalogoId { get; set; }

        public override string ToString()
        {
            return Valor;
        }
    }
}
