namespace GComFuelManager.Shared.ModelDTOs
{
    public class TerminalPostDTO
    {
        public short Cod { get; set; }
        public string? Den { get; set; } = string.Empty;
        public short? Nro { get; set; } = 0;
        public bool? Activo { get; set; } = true;
        public string? Codigo { get; set; } = string.Empty;
        public string? CodigoOrdenes { get; set; } = string.Empty;
        public int? Tipo_Vale { get; set; } = 0;
        public int? TipoTerminalId { get; set; } = 0;
    }
}
