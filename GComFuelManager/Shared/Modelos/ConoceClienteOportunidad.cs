using System;
using System.ComponentModel.DataAnnotations.Schema;
using GComFuelManager.Shared.Filtro;

namespace GComFuelManager.Shared.Modelos
{
    public class ConoceClienteOportunidad : Parametros_Busqueda_Gen
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
        public string? Longitud { get; set; } = string.Empty;
        //Suministro
        public int Suministro { get; set; }
        //Pago
        public int Pago { get; set; }
        public int MetodoPago { get; set; } 
        public int FormaPago { get; set; }
        public int CFDI { get; set; }
        //Ejecutivo
        public string? Ejecutivo { get; set; } = string.Empty;
        //Personal de contacto
        public string? NombrePersonalContacto { get; set; } = string.Empty;
        public int TelefonoPersonalContacto { get; set; }
        public string? EmailPersonalContacto { get; set; } = string.Empty;
        //Encargado
        public string? EncargadoPagoNombre { get; set; } = string.Empty;
        public int EncargadoPagoTelefono { get; set; }
        public string? EncargadoPagoEmail { get; set; } = string.Empty;
        //Recepcion
        public string? RecepcionFacturasNombre { get; set; } = string.Empty;
        public int RecepcionFacturasNumero { get; set; } 
        public string? RecepcionFacturasEmail { get; set; } = string.Empty;
        //Documentos
        public int InfoEtica1 { get; set; }
        public int InfoEtica2 { get; set; } 
        public int InfoEtica3 { get; set; } 
        public int InfoEtica4 { get; set; } 
        public int InfoEtica5 { get; set; } 
        public int InfoEtica6 { get; set; } 
        public int InfoEtica7 { get; set; }
        //Fecha y firma
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public string? Firma { get; set; } = string.Empty;

        public string? Comentarios { get; set; } = string.Empty;

        public int? OportunidadId { get; set; }
        [NotMapped]
        public CRMOportunidad? CRMOportunidad { get; set; } = null!;

        public CRMCatalogoValor TiposCliente { get; set; } = null!;
        public CRMCatalogoValor Giros { get; set; } = null!;
        public CRMCatalogoValor TiposEntrega { get; set; } = null!;
        public CRMCatalogoValor Suministros { get; set; } = null!;
        public CRMCatalogoValor Pagos { get; set; } = null!;
        public CRMCatalogoValor MetodosPago { get; set; } = null!;
        public CRMCatalogoValor FormasPago { get; set; } = null!;
        public CRMCatalogoValor CFDIS { get; set; } = null!;
        public CRMCatalogoValor InfosEtica1 { get; set; } = null!;
        public CRMCatalogoValor InfosEtica2 { get; set; } = null!;
        public CRMCatalogoValor InfosEtica3 { get; set; } = null!;
        public CRMCatalogoValor InfosEtica4 { get; set; } = null!;
        public CRMCatalogoValor InfosEtica5 { get; set; } = null!;
        public CRMCatalogoValor InfosEtica6 { get; set; } = null!;
        public CRMCatalogoValor InfosEtica7 { get; set; } = null!;

        public List<CRMDocumento> Documentos { get; set; } = new();
        public List<CRMConoceClienteDocumentos> CRMConoceClienteDocumentos { get; set; } = new();
    }
}
//Números teléfonicos en string
