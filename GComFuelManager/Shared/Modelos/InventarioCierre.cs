﻿using Microsoft.AspNetCore.Identity;

namespace GComFuelManager.Shared.Modelos
{
    public class InventarioCierre
    {
        public int Id { get; set; }
        public byte ProductoId { get; set; }
        public int UnidadMedidaId { get; set; }
        public int SitioId { get; set; }
        public int AlmacenId { get; set; }
        public int LocalidadId { get; set; }
        public double Fisico { get; set; }
        public double Reservado { get; set; }
        public double ReservadoDisponible { get; set; }
        public double Disponible { get; set; }
        public double PedidoTotal { get; set; }
        public double OrdenReservado { get; set; }
        public double EnOrden { get; set; }
        public double Cargado { get; set; }
        public double TotalDisponible { get; set; }
        public double TotalDisponibleFull { get; set; }
        public DateTime? FechaCierre { get; set; } = DateTime.Now;
        public DateTime? FechaInicio { get; set; } = DateTime.Now;
        public short TadId { get; set; }
        public bool Abierto { get; set; } = true;
        public bool Activo { get; set; } = true;
        public int? UsuarioInicioId { get; set; }
        public int? UsuarioCierreId { get; set; }

        public List<Inventario> Inventarios { get; set; } = new();
        public Tad Terminal { get; set; } = null!;
        public Producto Producto { get; set; } = null!;
        public Catalogo_Fijo UnidadMedida { get; set; } = null!;
        public CatalogoValor Sitio { get; set; } = null!;
        public CatalogoValor Almacen { get; set; } = null!;
        public CatalogoValor Localidad { get; set; } = null!;
        public Usuario? UsuarioInicio { get; set; } = null!;
        public Usuario? UsuarioCierre { get; set; } = null!;
    }
}
