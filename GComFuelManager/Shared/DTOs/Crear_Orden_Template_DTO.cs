using GComFuelManager.Shared.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.DTOs
{
    public class Crear_Orden_Template_DTO
    {
        public short? ID_Grupo { get; set; } = 0;
        public List<Grupo> Grupos { get; set; } = new List<Grupo>();
        public int? ID_Cliente { get; set; } = 0;
        public List<Cliente> Clientes { get; set; } = new List<Cliente>();
        public int? ID_Destino { get; set; } = 0;
        public List<Destino> Destinos { get; set; } = new List<Destino>();
        public byte? ID_Producto { get; set; } = 0;
        public Producto Producto { get; set; } = new Producto();
        public List<Producto> Productos { get; set; } = new List<Producto>();
        public OrdenCierre OrdenCierre { get; set; } = new OrdenCierre();
        public List<OrdenCierre> OrdenCierres { get; set; } = new List<OrdenCierre>();
        public Precio Precio { get; set; } = new Precio();
        public List<Precio> Precios { get; set; } = new List<Precio>();
        public bool Puede_Seleccionar_Cliete_Destino { get; set; } = false;
        public bool Es_Orden_Copiada { get; set; } = false;
    }
}
