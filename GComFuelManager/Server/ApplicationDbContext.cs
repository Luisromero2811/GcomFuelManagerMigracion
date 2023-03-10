using System;
using GComFuelManager.Shared.Modelos;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace GComFuelManager.Server
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Configuracion de la entidad a través de Modelbuilder.Entity
            //Con esta entidad podemos aplicar la llave compartida para generar un campo de muchos a muchos
            //Estacion
            modelBuilder.Entity<OrdenEmbarque>()
                    .HasOne(x => x.Destino)
                    .WithMany()
                    .HasForeignKey(x => x.Coddes);

            //Terminal
            modelBuilder.Entity<OrdenEmbarque>()
                .HasOne(x => x.Tad)
                .WithMany()
                .HasForeignKey(x => x.Codtad);
            //Producto
            modelBuilder.Entity<OrdenEmbarque>()
                .HasOne(x => x.Producto)
                .WithMany()
                .HasForeignKey(x => x.Codprd);
            //Cantidad
            modelBuilder.Entity<OrdenEmbarque>()
                .HasOne(x => x.Tonel)
                .WithMany()
                .HasForeignKey(x => x.Codton);

            //TransportistaOrden
            modelBuilder.Entity<OrdenEmbarque>()
                .HasOne(x => x.Chofer)
                .WithMany()
                .HasForeignKey(x => x.Codchf);
            //Tonel
            modelBuilder.Entity<Tonel>()
                .HasOne(x => x.Transportista)
                .WithMany()
                .HasPrincipalKey(x=>x.carrId)
                .HasForeignKey(x => x.Carid);
            //Orden compra
            modelBuilder.Entity<OrdenEmbarque>()
                .HasOne(x => x.OrdenCompra)
                .WithMany()
                .HasForeignKey(x => x.CodordCom);
            //cliente - destino
            modelBuilder.Entity<Destino>()
                .HasOne(x => x.Cliente)
                .WithMany()
                .HasForeignKey(x => x.Codcte);
            //Estado
            modelBuilder.Entity<OrdenEmbarque>()
                .HasOne(x=>x.Estado)
                .WithMany()
                .HasForeignKey(x=>x.Codest);
        }


        public DbSet<Actividad> Actividad { get; set; }
        public DbSet<BitacoraModificacion> BitacoraModificacion { get; set; }
        public DbSet<Chofer> Chofer { get; set; }
        public DbSet<Ciudad> Ciudad { get; set; }
        public DbSet<Cliente> Cliente { get; set; }
        public DbSet<Descarga> Descarga { get; set; }
        public DbSet<Destino> Destino { get; set; }
        public DbSet<Entidad> Entidad { get; set; }
        public DbSet<FormaPago> FormaPago { get; set; }
        public DbSet<Grupo> Grupo { get; set; }
        public DbSet<GrupoUsuario> GrupoUsuario { get; set; }
        public DbSet<Inventario> Inventario { get; set; }
        public DbSet<Log> Log { get; set; }
        public DbSet<OrdEmbDet> OrdEmbDet { get; set; }
        public DbSet<Orden> Orden { get; set; }
        public DbSet<OrdenCompra> OrdenCompra { get; set; }
        public DbSet<OrdenEmbarque> OrdenEmbarque { get; set; }
        public DbSet<Pipa> Pipa { get; set; }
        public DbSet<Privilegio> Privilegio { get; set; }
        public DbSet<PrivilegioUsuario> PrivilegioUsuario { get; set; }
        public DbSet<Producto> Producto { get; set; }
        public DbSet<Tad> Tad { get; set; }
        public DbSet<Tonel> Tonel { get; set; }
        public DbSet<Transportista> Transportista { get; set; }
        public DbSet<TransportistaGrupo> TransportistaGrupo { get; set; }
        public DbSet<users_descarga> Users_descarga { get; set; }
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Shared.Modelos.Version> Version { get; set; }



    }
}
