using GComFuelManager.Shared.Filtro;
using GComFuelManager.Shared.Modelos;

namespace GComFuelManager.Shared.DTOs.CRM
{
    public class CRMOportunidadDTO : Parametros_Busqueda_Gen
    {
        public int Id { get; set; }
        public string Nombre_Opor { get; set; } = string.Empty;
        public double ValorOportunidad { get; set; }
        //public string Contacto { get; set; } = string.Empty;
        public string Vendedor { get; set; } = string.Empty;
        public string Cuenta { get; set; } = string.Empty;
        public string EtapaVenta { get; set; } = string.Empty;
        public decimal Probabilidad { get; set; }
        public bool Activo { get; set; } = true;
        public string Contacto { get; set; } = string.Empty;
        public string Medida { get; set; } = string.Empty;
        public int ContactoId { get; set; }
        public int VendedorId { get; set; }
        public string Equipo { get; set; } = string.Empty;
        public int EquipoId { get; set; }
        public string Division { get; set; } = string.Empty; 
    }
}
