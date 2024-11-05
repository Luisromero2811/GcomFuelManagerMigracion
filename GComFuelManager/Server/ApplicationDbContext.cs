using System;
using GComFuelManager.Shared.Modelos;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.DTOs;
using Microsoft.AspNetCore.Identity;

namespace GComFuelManager.Server
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUsuario>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public virtual async Task<int> SaveChangesAsync(string? id, int? accion)
        {
            if (!string.IsNullOrEmpty(id) && accion != 0)
                OnBeforeSaveChanges(id, accion);
            var Results = await base.SaveChangesAsync();
            return Results;
        }

        private void OnBeforeSaveChanges(string? id, int? accion)
        {
            ChangeTracker.DetectChanges();
            var AccEntries = new List<ActividadDTO>();
            foreach (var item in ChangeTracker.Entries())
            {
                if (item.Entity is ActividadRegistrada || item.State == EntityState.Detached || item.State == EntityState.Unchanged)
                    continue;

                var actividad = new ActividadDTO(item);

                actividad.TableName = item.Entity.GetType().Name; //obtiene el nombre de la tabla
                actividad.UserId = id;
                actividad.AuditType = accion;

                AccEntries.Add(actividad);

                foreach (var prop in item.Properties)
                {
                    string propName = prop.Metadata.Name;

                    //si la propiedad actual es una clave principal, agréguela al diccionario de claves principales y sáltela.
                    if (prop.Metadata.IsPrimaryKey())
                    {
                        actividad.KeyValues[propName] = prop.CurrentValue!;
                    }

                    //en el switch detectamos el estado de la entidad (Agregado, Eliminado o Modificado)
                    //y por cada caso agregamos nuevos datos a cada campo de la tabla auditoria
                    switch (item.State)
                    {
                        case EntityState.Deleted:
                            actividad.OldValues[propName] = prop.OriginalValue!;
                            break;
                        case EntityState.Modified:
                            if (prop.IsModified)
                            {
                                actividad.ChangedColumns.Add(propName);
                                actividad.OldValues[propName] = prop.OriginalValue!;
                                actividad.NewValues[propName] = prop.CurrentValue!;
                            }
                            break;
                        case EntityState.Added:
                            actividad.NewValues[propName] = prop.CurrentValue!;
                            break;
                        default:
                            break;
                    }
                }
            }
            //convertimos todas las Entradas de Auditoría a Auditorías y guardamos los cambios en el metodo original: var result = await base.SaveChangesAsync();
            foreach (var item in AccEntries)
                ActividadRegistrada.Add(item.ToAudit());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Actividad a Asignado a CRMVendedor
            modelBuilder.Entity<CRMActividades>()
                .HasOne(x => x.vendedor)
                .WithMany(x => x.Actividades)
                .HasForeignKey(x => x.Asignado)
                .OnDelete(DeleteBehavior.Restrict);

            //Actividad Relacionada Con CRMContacto
            modelBuilder.Entity<CRMActividades>()
                .HasOne(x => x.contacto)
                .WithMany()
                .HasForeignKey(x => x.Contacto_Rel)
                .OnDelete(DeleteBehavior.Restrict);

            //Actividad a Catalogo Fijo de Asunto
            modelBuilder.Entity<CRMActividades>()
                .HasOne(x => x.asuntos)
                .WithMany()
                .HasForeignKey(x => x.Asunto)
                .OnDelete(DeleteBehavior.Restrict);

            //Actividad a Catalogo Fijo de Estado
            modelBuilder.Entity<CRMActividades>()
                .HasOne(x => x.Estados)
                .WithMany()
                .HasForeignKey(x => x.Estatus)
                .OnDelete(DeleteBehavior.Restrict);


            //Actividad a Catalogo Fijo de prioridades
            modelBuilder.Entity<CRMActividades>()
                .HasOne(x => x.prioridades)
                .WithMany()
                .HasForeignKey(x => x.Prioridad)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMContacto>()
                .HasOne(x => x.Estatus)
                .WithMany()
                .HasForeignKey(x => x.EstatusId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMContacto>()
                .HasOne(x => x.Vendedor)
                .WithMany()
                .HasForeignKey(x => x.VendedorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMContacto>()
                .HasOne(x => x.Origen)
                .WithMany()
                .HasForeignKey(x => x.OrigenId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMVendedor>()
                .HasMany(x => x.Contactos)
                .WithOne(x => x.Vendedor)
                .HasForeignKey(x => x.VendedorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMContacto>()
                .HasOne(x => x.Cliente)
                .WithMany()
                .HasForeignKey(x => x.CuentaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMOportunidad>()
                .HasOne(x => x.Vendedor)
                .WithMany(x => x.Oportunidades)
                .HasForeignKey(x => x.VendedorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMOportunidad>()
                .HasOne(x => x.CRMCliente)
                .WithMany()
                .HasForeignKey(x => x.CuentaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMOportunidad>()
                .HasOne(x => x.EtapaVenta)
                .WithMany()
                .HasForeignKey(x => x.EtapaVentaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMOportunidad>()
                .HasOne(x => x.Tipo)
                .WithMany()
                .HasForeignKey(x => x.TipoId);

            modelBuilder.Entity<CRMOportunidad>()
                .HasOne(x => x.UnidadMedida)
                .WithMany()
                .HasForeignKey(x => x.UnidadMedidaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMOportunidad>()
                .HasOne(x => x.Contacto)
                .WithMany()
                .HasForeignKey(x => x.ContactoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMOportunidad>()
                .HasOne(x => x.Periodo)
                .WithMany()
                .HasForeignKey(x => x.PeriodoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMOportunidad>()
                .HasOne(x => x.OrigenProducto)
                .WithMany()
                .HasForeignKey(x => x.OrigenPrductoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMOportunidad>()
                .HasOne(x => x.TipoProducto)
                .WithMany()
                .HasForeignKey(x => x.TipoProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMOportunidad>()
                .HasOne(x => x.ModeloVenta)
                .WithMany()
                .HasForeignKey(x => x.ModeloVentaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMOportunidad>()
                .HasOne(x => x.Volumen)
                .WithMany()
                .HasForeignKey(x => x.VolumenId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMOportunidad>()
                .HasOne(x => x.FormaPago)
                .WithMany()
                .HasForeignKey(x => x.FormaPagoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMOportunidad>()
                .HasOne(x => x.DiasCredito)
                .WithMany()
                .HasForeignKey(x => x.DiasPagoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMContacto>()
                .HasOne(x => x.Division)
                .WithMany()
                .HasForeignKey(x => x.DivisionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMOriginador>()
                .HasOne(x => x.Division)
                .WithMany()
                .HasForeignKey(x => x.DivisionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMVendedorOriginador>().HasKey(x => new { x.VendedorId, x.OriginadorId });

            modelBuilder.Entity<CRMVendedor>()
                .HasMany(x => x.Originadores)
                .WithMany(x => x.Vendedores)
                .UsingEntity<CRMVendedorOriginador>(
                l => l.HasOne(x => x.Originador).WithMany(x => x.VendedorOriginadores).HasForeignKey(x => x.OriginadorId).OnDelete(DeleteBehavior.Restrict),
                r => r.HasOne(x => x.Vendedor).WithMany(x => x.VendedorOriginadores).HasForeignKey(x => x.VendedorId).OnDelete(DeleteBehavior.Restrict)
                );

            modelBuilder.Entity<CRMRolPermiso>().HasKey(x => new { x.RolId, x.PermisoId });
            modelBuilder.Entity<CRMRolUsuario>().HasKey(x => new { x.RolId, x.UserId });

            //modelBuilder.Entity<CRMRol>()
            //    .HasOne(x => x.Division)
            //    .WithMany()
            //    .HasForeignKey(x => x.DivisionId)
            //    .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<CRMEquipoOriginadores>().HasKey(x => new { x.EquipoId, x.OriginadorId });
            modelBuilder.Entity<CRMEquipo>()
                .HasMany(x => x.Originadores)
                .WithMany(x => x.Equipos)
                .UsingEntity<CRMEquipoOriginadores>(
                l => l.HasOne(x => x.Originador).WithMany(x => x.EquipoOriginadores).HasForeignKey(x => x.OriginadorId).OnDelete(DeleteBehavior.Restrict),
                s => s.HasOne(x => x.Equipo).WithMany(x => x.EquipoOriginadores).HasForeignKey(x => x.EquipoId).OnDelete(DeleteBehavior.Restrict));

            modelBuilder.Entity<CRMEquipoVendedor>().HasKey(x => new { x.EquipoId, x.VendedorId });
            modelBuilder.Entity<CRMEquipo>()
                .HasMany(x => x.Vendedores)
                .WithMany(x => x.Equipos)
                .UsingEntity<CRMEquipoVendedor>(
                l => l.HasOne(x => x.Vendedor).WithMany(x => x.EquipoVendedores).HasForeignKey(x => x.VendedorId).OnDelete(DeleteBehavior.Restrict),
                r => r.HasOne(x => x.Equipo).WithMany(x => x.EquipoVendedores).HasForeignKey(x => x.EquipoId).OnDelete(DeleteBehavior.Restrict));

            modelBuilder.Entity<CRMUsuarioDivision>().HasKey(x => new { x.UsuarioId, x.DivisionId });

            modelBuilder.Entity<CRMVendedor>()
                .HasMany(x => x.Oportunidades)
                .WithOne(x => x.Vendedor)
                .HasForeignKey(x => x.VendedorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMOportunidad>()
                .HasOne(x => x.Equipo)
                .WithMany(x => x.Oportunidades)
                .HasForeignKey(x => x.EquipoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMCliente>()
                .HasOne(x => x.Contacto)
                .WithMany()
                .HasForeignKey(x => x.ContactoPrincipalId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMActividades>()
                .HasOne(x => x.Equipo)
                .WithMany(x => x.Actividades)
                .HasForeignKey(x => x.EquipoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMOportunidadDocumento>().HasKey(x => new { x.OportunidadId, x.DocumentoId });
            modelBuilder.Entity<CRMOportunidad>()
                .HasMany(x => x.Documentos)
                .WithMany(x => x.Oportunidades)
                .UsingEntity<CRMOportunidadDocumento>(
                l => l.HasOne(x => x.Documento).WithMany(x => x.OportunidadDocumentos).HasForeignKey(x => x.DocumentoId).OnDelete(DeleteBehavior.Restrict),
                r => r.HasOne(x => x.Oportunidad).WithMany(x => x.OportunidadDocumentos).HasForeignKey(x => x.OportunidadId).OnDelete(DeleteBehavior.Restrict));

            modelBuilder.Entity<CRMActividadDocumento>().HasKey(x => new { x.ActividadId, x.DocumentoId });
            modelBuilder.Entity<CRMActividades>()
                .HasMany(x => x.Documentos)
                .WithMany(x => x.Actividades)
                .UsingEntity<CRMActividadDocumento>(
                l => l.HasOne(x => x.Documento).WithMany(x => x.ActividadDocumentos).HasForeignKey(x => x.DocumentoId).OnDelete(DeleteBehavior.Restrict),
                r => r.HasOne(x => x.Actividad).WithMany(x => x.ActividadDocumentos).HasForeignKey(x => x.ActividadId).OnDelete(DeleteBehavior.Restrict));

            modelBuilder.Entity<CRMDocumentoRelacionado>().HasKey(x => new { x.DocumentoId, x.DocumentoRelacionadoId });
            modelBuilder.Entity<CRMDocumentoRevision>().HasKey(x => new { x.DocumentoId, x.RevisionId });

            modelBuilder.Entity<CRMCatalogo>()
                .HasMany(x => x.Valores)
                .WithOne(x => x.Catalogo)
                .HasForeignKey(x => x.CatalogoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMGrupoRol>().HasKey(x => new { x.RolId, x.GrupoId });
            modelBuilder.Entity<CRMGrupo>()
                .HasMany(x => x.GrupoRols)
                .WithOne(x => x.Grupo)
                .HasForeignKey(x => x.GrupoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMOportunidad>()
                .HasMany(x => x.HistorialEstados)
                .WithOne(x => x.Oportunidad)
                .HasForeignKey(x => x.OportunidadId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CRMOportunidadEstadoHistorial>()
                .HasOne(x => x.EtapaVenta)
                .WithMany()
                .HasForeignKey(x => x.EtapaVentaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
        public DbSet<Log> Log { get; set; }
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<ActividadRegistrada> ActividadRegistrada { get; set; }
        public DbSet<Errors> Errors { get; set; }
        public DbSet<CRMContacto> CRMContactos { get; set; }
        public DbSet<CRMCliente> CRMClientes { get; set; }
        public DbSet<CRMActividades> CRMActividades { get; set; }
        public DbSet<CRMOportunidad> CRMOportunidades { get; set; }
        public DbSet<CRMDivision> CRMDivisiones { get; set; }
        public DbSet<CRMVendedor> CRMVendedores { get; set; }
        public DbSet<CRMOriginador> CRMOriginadores { get; set; }
        public DbSet<CRMVendedorOriginador> CRMVendedorOriginadores { get; set; }
        public DbSet<CRMRol> CRMRoles { get; set; }
        public DbSet<CRMRolPermiso> CRMRolPermisos { get; set; }
        public DbSet<CRMRolUsuario> CRMRolUsuarios { get; set; }
        public DbSet<CRMEquipo> CRMEquipos { get; set; }
        public DbSet<CRMEquipoVendedor> CRMEquipoVendedores { get; set; }
        public DbSet<CRMEquipoOriginadores> CRMEquipoOriginadores { get; set; }
        public DbSet<CRMUsuarioDivision> CRMUsuarioDivisiones { get; set; }
        public DbSet<CRMDocumento> CRMDocumentos { get; set; }
        public DbSet<CRMOportunidadDocumento> CRMOportunidadDocumentos { get; set; }
        public DbSet<CRMActividadDocumento> CRMActividadDocumentos { get; set; }
        public DbSet<CRMDocumentoRelacionado> CRMDocumentoRelacionados { get; set; }
        public DbSet<CRMDocumentoRevision> CRMDocumentoRevisiones { get; set; }
        public DbSet<CRMCatalogo> CRMCatalogos { get; set; }
        public DbSet<CRMCatalogoValor> CRMCatalogoValores { get; set; }
        public DbSet<CRMGrupo> CRMGrupos { get; set; }
        public DbSet<CRMGrupoRol> CRMGrupoRoles { get; set; }
        public DbSet<CRMOportunidadEstadoHistorial> CRMOportunidadEstadoHistoriales { get; set; }
    }
}