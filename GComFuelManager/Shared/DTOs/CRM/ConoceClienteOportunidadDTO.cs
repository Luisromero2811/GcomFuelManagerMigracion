using System;
using GComFuelManager.Shared.Modelos;
using System.ComponentModel.DataAnnotations.Schema;

namespace GComFuelManager.Shared.DTOs.CRM
{
	public class ConoceClienteOportunidadDTO
	{
        public int Id { get; set; }
        public string? RazonSocial { get; set; } = string.Empty;
        public int TipoCliente { get; set; }
        public int Giro { get; set; }
        public string? CRE { get; set; } = string.Empty;
        public string? RFC { get; set; } = string.Empty;
        public string? DomicilioFiscal { get; set; } = string.Empty;
        public int TipoEntrega { get; set; }
        //Domicilio
        public string? Calle { get; set; } = string.Empty;
        public string? Numero { get; set; } = string.Empty;
        public string? Colonia { get; set; } = string.Empty;
        public int CP { get; set; }
        public string? Municipio { get; set; } = string.Empty;
        public string? EntidadFederal { get; set; } = string.Empty;
        public string? Coordenadas { get; set; } = string.Empty;
        //Suministro
        public int Suministro { get; set; }
        //Pago
        public int Pago { get; set; }
        public int MetodoPago { get; set; }
        public int FormaPago { get; set; }
        public int CFDI { get; set; }
        //Ejecutivo
        public string? Ejecutivo { get; set; } = string.Empty;
        //Personal
        public string? NombrePersonalContacto { get; set; } = string.Empty;
        public int TelefonoPersonalContacto { get; set; }
        public string? EmailPersonalContacto { get; set; } = string.Empty;
        //Encargado
        public string? EncargadoPagoNombre { get; set; } = string.Empty;
        public int EncargadoPagoTelefono { get; set; }
        public string? EncargadoPagoEmail { get; set; } = string.Empty;
        //Recepción
        public string? RecepcionFacturasNombre { get; set; } = string.Empty;
        public int RecepcionFacturasNumero { get; set; }
        public string? RecepcionFacturasEmail { get; set; } = string.Empty;

        public int InfoEtica1 { get; set; }
        public int InfoEtica2 { get; set; }
        public int InfoEtica3 { get; set; }
        public int InfoEtica4 { get; set; }
        public int InfoEtica5 { get; set; }
        public int InfoEtica6 { get; set; }
        public int InfoEtica7 { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public string? Firma { get; set; } = string.Empty;

        public string? Comentarios { get; set; } = string.Empty;

        //Documento
        public string NombreDocumento { get; set; } = string.Empty;
        public DateTime FechaCaducidad { get; set; } = DateTime.Today;

        // --Identificadores de documentos para Conoce tu cliente--
        public int? DocumentoIdEtica1 { get; set; }
        public int? DocumentoIdEtica2 { get; set; }
        public int? DocumentoIdEtica3 { get; set; }

        public int? DocumentoIdAdicional1 { get; set; }
        public int? DocumentoIdAdicional2 { get; set; }
        public int? DocumentoIdAdicional3 { get; set; }
        public int? DocumentoIdAdicional4 { get; set; }
        public int? DocumentoIdAdicional5 { get; set; }
        public int? DocumentoIdAdicional6 { get; set; }
        public int? DocumentoIdAdicional7 { get; set; }

        public string? IdentificadorDocumentoIdEtica1 { get; set; } = string.Empty;
        public string? IdentificadorDocumentoIdEtica2 { get; set; } = string.Empty;
        public string? IdentificadorDocumentoIdEtica3 { get; set; } = string.Empty;

        public string? IdentificadorDocumentoIdAdicional1 { get; set; } = string.Empty;
        public string? IdentificadorDocumentoIdAdicional2 { get; set; } = string.Empty;
        public string? IdentificadorDocumentoIdAdicional3 { get; set; } = string.Empty;
        public string? IdentificadorDocumentoIdAdicional4 { get; set; } = string.Empty;
        public string? IdentificadorDocumentoIdAdicional5 { get; set; } = string.Empty;
        public string? IdentificadorDocumentoIdAdicional6 { get; set; } = string.Empty;
        public string? IdentificadorDocumentoIdAdicional7 { get; set; } = string.Empty;

        //Instancias
        public List<int> TiposDocumentoIds { get; set; } = new();
        public List<CRMDocumentoDTO> Documentos { get; set; } = new();
        public CRMDocumentoDTO? DocumentoReciente { get; set; }

        public int? OportunidadId { get; set; }
        [NotMapped]
        public CRMOportunidad? CRMOportunidad { get; set; } = null!;

    }
}

