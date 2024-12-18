using System;
namespace GComFuelManager.Shared.Modelos
{
	public class DocumentoTipoDocumento
	{
		public int DocumentoId { get; set; }
		public int TipoDocumentoId { get; set; }

		public CRMDocumento? CRMDocumento { get; set; } = null!;
		public TipoDocumento? TipoDocumento { get; set; } = null!;
	}
}

