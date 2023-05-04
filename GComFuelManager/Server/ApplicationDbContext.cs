﻿using System;
using GComFuelManager.Shared.Modelos;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using GComFuelManager.Server.Identity;

namespace GComFuelManager.Server
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUsuario>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Configuracion de la entidad a través de Modelbuilder.Entity
            //Con esta entidad podemos aplicar la llave compartida para generar un campo de muchos a muchos

            //Relaciones en su mayoria de tabla OrdenEmbarques
            //Estacion
            modelBuilder.Entity<OrdenEmbarque>()
                    .HasOne(x => x.Destino)
                    .WithMany()
                    .HasForeignKey(x => x.Coddes);
            modelBuilder.Entity<Destino>()
                .HasOne(x => x.Cliente)
                .WithMany()
                .HasForeignKey(x => x.Codcte);

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


            //TransportistaOrdenEmbarque
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
            //Relaciones Tabla de OrdEmbDet
            modelBuilder.Entity<OrdEmbDet>()
                .HasOne(x => x.Orden)
                .WithMany()
                .HasPrincipalKey(x => x.BatchId)
                .HasForeignKey(x => x.Bol);
            //Relaciones Tabla orden
            //Orden-OrdEmbDet
            modelBuilder.Entity<Orden>()
                .HasOne(x => x.OrdEmbDet)
                .WithMany()
                .HasPrincipalKey(x => x.Bol)
                .HasForeignKey(x => x.BatchId);
            //Orden-Estado 
            modelBuilder.Entity<Orden>()
                .HasOne(x => x.Estado)
                .WithMany()
                .HasForeignKey(x => x.Codest);
            //Orden-Estacion(Destino)
            modelBuilder.Entity<Orden>()
                .HasOne(x => x.Destino)
                .WithMany()
                .HasForeignKey(x => x.Coddes);
            //Orden-Producto
            modelBuilder.Entity<Orden>()
                .HasOne(x => x.Producto)
                .WithMany()
                .HasForeignKey(x => x.Codprd);
            //Orden-Tonel
            modelBuilder.Entity<Orden>()
                .HasOne(x => x.Tonel)
                .WithMany()
                .HasForeignKey(x => x.Coduni);
            //Orden-Chofer
            modelBuilder.Entity<Orden>()
                .HasOne(x => x.Chofer)
                .WithMany()
                .HasForeignKey(x => x.Codchf);
            //OrdenCierre-Destino
            modelBuilder.Entity<OrdenCierre>()
                .HasOne(x => x.Destino)
                .WithMany()
                .HasForeignKey(x => x.CodDes);
            //OrdenCierre-Cliente
            modelBuilder.Entity<OrdenCierre>()
                .HasOne(x => x.Cliente)
                .WithMany()
                .HasForeignKey(x => x.CodCte);
            //OrdenCierre-Producto
            modelBuilder.Entity<OrdenCierre>()
                .HasOne(x => x.Producto)
                .WithMany()
                .HasForeignKey(x => x.CodPrd);

            //Contacto - Cliente
            modelBuilder.Entity<Contacto>()
                .HasOne(x => x.Cliente)
                .WithMany()
                .HasForeignKey(x => x.CodCte);

            //Contacto - Cliente
            modelBuilder.Entity<OrdenCierre>()
                .HasOne(x => x.ContactoN)
                .WithMany()
                .HasForeignKey(x => x.CodCon);

            //OrdenEmbarque - Cierre
            modelBuilder.Entity<OrdenCierre>()
                .HasOne(x => x.OrdenEmbarque)
                .WithMany()
                .HasForeignKey(x => x.CodPed);
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
        public DbSet<OrdenCierre> OrdenCierre { get; set; }
        public DbSet<Contacto> Contacto { get; set; }
    }
}
