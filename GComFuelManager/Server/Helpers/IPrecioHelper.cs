namespace GComFuelManager.Server.Helpers
{
    public interface IPrecioHelper
    {
        public Task<double> ObtenerPrecioPorIdOrdenAsync(long? id);
        public Task<double> ObtenerPrecioPorIdOrdenEmbarqueAsync(int? id);
        public double ObtenerPrecioPorIdOrden(long? id);
        public double ObtenerPrecioPorIdOrdenEmbarque(int? id);
    }
}
