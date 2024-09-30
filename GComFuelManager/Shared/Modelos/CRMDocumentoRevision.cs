using System.ComponentModel.DataAnnotations.Schema;

namespace GComFuelManager.Shared.Modelos
{
    public class CRMDocumentoRevision
    {
        public int DocumentoId { get; set; }
        public int RevisionId { get; set; }

        [NotMapped]
        public CRMDocumento? Documento { get; set; } = null!;
        [NotMapped]
        public CRMDocumento? Revision { get; set; } = null!;
    }
}
