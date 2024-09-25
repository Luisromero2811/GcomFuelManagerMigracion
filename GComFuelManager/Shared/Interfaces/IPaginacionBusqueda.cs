namespace GComFuelManager.Shared.Interfaces
{
    public interface IPaginacionBusqueda
    {
        public int Pagina { get; set; }
        public int Pagina_ACtual { get; set; }
        public int Total_paginas { get; set; }
        public int Registros_por_pagina { get; set; }
        public int Total_registros { get; set; }
        public DateTime Fecha_Inicio { get; set; }
        public DateTime Fecha_Fin { get; set; }
        public bool Excel { get; set; }
        public bool Paginacion { get; set; }
    }
}
