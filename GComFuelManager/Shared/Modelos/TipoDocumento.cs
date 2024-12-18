using System;
namespace GComFuelManager.Shared.Modelos
{
	public class TipoDocumento
	{
		public int Id { get; set; }
		public string? Nombre { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;

        public List<CRMDocumento> Documentos { get; set; } = null!;
		public List<DocumentoTipoDocumento> DocumentoTipoDocumentos { get; set; } = null!;
	}
}

