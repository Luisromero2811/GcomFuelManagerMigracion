namespace GComFuelManager.Shared.ModelDTOs
{
    public class VendedorDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Id_Originador { get; set; } = 0;
        public List<OriginadorDTO> Originadores { get; set; } = new();
    }
}
