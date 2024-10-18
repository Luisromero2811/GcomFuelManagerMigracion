using System.ComponentModel.DataAnnotations;

namespace GComFuelManager.Shared.ModelDTOs
{
    public class TerminalDTO
    {
        public short Cod { get; set; }
        public string? Den { get; set; } = string.Empty;
        public string? Codigo { get; set; } = string.Empty;
        public string? CodigoOrdenes { get; set; } = string.Empty;
        public string? TipoTerminal { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
    }
}
