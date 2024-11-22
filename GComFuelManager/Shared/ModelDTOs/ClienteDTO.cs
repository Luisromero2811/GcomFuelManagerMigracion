using OfficeOpenXml.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace GComFuelManager.Shared.ModelDTOs
{
    public class ClienteDTO
    {
        public int Cod { get; set; }
        public string Den { get; set; } = string.Empty;
        public short? Codgru { get; set; } = 0;
        public int Id_Vendedor { get; set; } = 0;
        public int Id_Originador { get; set; } = 0;
        public short? Id_Tad { get; set; } = 0;
        public bool Activo { get; set; } = true;
        public bool CodgruNotNull { get; set; } = false;
        public VendedorDTO Vendedor { get; set; } = new();
        public OriginadorDTO Originador { get; set; } = new();

        public bool IsEditing { get; set; } = false;
    }
}
