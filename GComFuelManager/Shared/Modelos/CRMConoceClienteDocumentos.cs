using System;
namespace GComFuelManager.Shared.Modelos
{
	public class CRMConoceClienteDocumentos
	{
        public int ConoceClienteId { get; set; }
        public int DocumentoId { get; set; }

        public ConoceClienteOportunidad? ConoceCliente { get; set; } = null!;
        public CRMDocumento? Documento { get; set; } = null!;
    }
}

