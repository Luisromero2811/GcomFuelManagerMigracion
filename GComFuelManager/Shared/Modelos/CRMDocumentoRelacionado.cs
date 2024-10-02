using System.ComponentModel.DataAnnotations.Schema;

namespace GComFuelManager.Shared.Modelos
{
    public class CRMDocumentoRelacionado
    {
        public int DocumentoId { get; set; }
        public int DocumentoRelacionadoId { get; set; }

        [NotMapped]
        public CRMDocumento? Documento { get; set; } = null!;
        [NotMapped]
        public CRMDocumento? DocumentoRelacionado { get; set; } = null!;

    }
}
