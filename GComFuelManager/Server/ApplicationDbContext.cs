using System;
using GComFuelManager.Shared.Modelos;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.DTOs;

namespace GComFuelManager.Server
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUsuario, IdentityRol, string>
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

            modelBuilder.Entity<Contacto>()
                .HasOne(x => x.Cliente)
                .WithMany()
                .HasForeignKey(x => x.CodCte);

            modelBuilder.Entity<Cliente>()
                .HasOne(x => x.Vendedor)
                .WithMany()
                .HasForeignKey(x => x.Id_Vendedor);

            modelBuilder.Entity<Cliente>()
                .HasOne(x => x.Originador)
                .WithMany()
                .HasForeignKey(x => x.Id_Originador);

            modelBuilder.Entity<Vendedor>()
                .HasMany(x => x.Clientes)
                .WithOne(x => x.Vendedor)
                .HasForeignKey(x => x.Id_Vendedor)
                .HasPrincipalKey(x => x.Id);

            modelBuilder.Entity<Originador>()
                .HasMany(x => x.Clientes)
                .WithOne(x => x.Originador)
                .HasForeignKey(x => x.Id_Originador)
                .HasPrincipalKey(x => x.Id);

            modelBuilder.Entity<Vendedor_Originador>().HasKey(vo => new { vo.VendedorId, vo.OriginadorId });

            modelBuilder.Entity<Vendedor>()
                .HasMany(x => x.Originadores)
                .WithMany(x => x.Vendedores)
                .UsingEntity<Vendedor_Originador>(
                    l => l.HasOne(x => x.Originador).WithMany(x => x.Vendedor_Originador).OnDelete(DeleteBehavior.Restrict),
                    r => r.HasOne(x => x.Vendedor).WithMany(x => x.Vendedor_Originador).OnDelete(DeleteBehavior.Restrict)
                );

            modelBuilder.Entity<Vendedor>()
                .HasMany(x => x.Metas_Vendedor)
                .WithOne(x => x.Vendedor)
                .HasForeignKey(x => x.VendedorId);

            modelBuilder.Entity<Cliente_Tad>().HasKey(ct => new { ct.Id_Cliente, ct.Id_Terminal });
            modelBuilder.Entity<Usuario_Tad>().HasKey(ut => new { ut.Id_Usuario, ut.Id_Terminal });

            modelBuilder.Entity<Cliente>()
                .HasMany(x => x.Terminales)
                .WithMany(x => x.Clientes)
                .UsingEntity<Cliente_Tad>(
                    l => l.HasOne(x => x.Terminal).WithMany(x => x.Cliente_Tads).HasForeignKey(x => x.Id_Terminal).OnDelete(DeleteBehavior.Restrict),
                    r => r.HasOne(x => x.Cliente).WithMany(x => x.Cliente_Tads).HasForeignKey(x => x.Id_Cliente).OnDelete(DeleteBehavior.Restrict)
                );

            //Actividad a Asignado a CRMVendedor
            modelBuilder.Entity<CRMActividades>()
                .HasOne(x => x.vendedor)
                .WithMany()
                .HasForeignKey(x => x.Asignado);
            //Actividad Relacionada Con CRMContacto
            modelBuilder.Entity<CRMActividades>()
                .HasOne(x => x.contacto)
                .WithMany()
                .HasForeignKey(x => x.Contacto_Rel);
            //Actividad a Catalogo Fijo de Asunto
            modelBuilder.Entity<CRMActividades>()
                .HasOne(x => x.asuntos)
                .WithMany()
                .HasForeignKey(x => x.Asunto);
            //Actividad a Catalogo Fijo de Estado
            modelBuilder.Entity<CRMActividades>()
                .HasOne(x => x.Estados)
                .WithMany()
                .HasForeignKey(x => x.Estatus);
            //Actividad a Catalogo Fijo de prioridades
            modelBuilder.Entity<CRMActividades>()
                .HasOne(x => x.prioridades)
                .WithMany()
                .HasForeignKey(x => x.Prioridad);

            modelBuilder.Entity<CRMContacto>()
                .HasOne(x => x.Estatus)
                .WithMany()
                .HasForeignKey(x => x.EstatusId);

            modelBuilder.Entity<CRMContacto>()
                .HasOne(x => x.Vendedor)
                .WithMany()
                .HasForeignKey(x => x.VendedorId);

            modelBuilder.Entity<CRMContacto>()
                .HasOne(x => x.Origen)
                .WithMany()
                .HasForeignKey(x => x.OrigenId);

            modelBuilder.Entity<CRMVendedor>()
                .HasMany(x => x.Contactos)
                .WithOne(x => x.Vendedor)
                .HasForeignKey(x => x.VendedorId);

            modelBuilder.Entity<CRMContacto>()
                .HasOne(x => x.Cliente)
                .WithMany()
                .HasForeignKey(x => x.CuentaId);

            modelBuilder.Entity<CRMOportunidad>()
                .HasOne(x => x.Origen)
                .WithMany()
                .HasForeignKey(x => x.OrigenId);

            modelBuilder.Entity<CRMOportunidad>()
                .HasOne(x => x.Vendedor)
                .WithMany()
                .HasForeignKey(x => x.VendedorId);

            modelBuilder.Entity<CRMOportunidad>()
                .HasOne(x => x.CRMCliente)
                .WithMany()
                .HasForeignKey(x => x.CuentaId);

            modelBuilder.Entity<CRMOportunidad>()
                .HasOne(x => x.EtapaVenta)
                .WithMany()
                .HasForeignKey(x => x.EtapaVentaId);

            modelBuilder.Entity<CRMOportunidad>()
                .HasOne(x => x.Tipo)
                .WithMany()
                .HasForeignKey(x => x.TipoId);

            modelBuilder.Entity<CRMOportunidad>()
                .HasOne(x => x.UnidadMedida)
                .WithMany()
                .HasForeignKey(x => x.UnidadMedidaId);

            modelBuilder.Entity<CRMOportunidad>()
                .HasOne(x => x.Contacto)
                .WithMany()
                .HasForeignKey(x => x.ContactoId);

            modelBuilder.Entity<CRMOportunidad>()
                .HasOne(x => x.Periodo)
                .WithMany()
                .HasForeignKey(x => x.PeriodoId);

            modelBuilder.Entity<CRMContacto>()
                .HasOne(x => x.Division)
                .WithMany()
                .HasForeignKey(x => x.DivisionId);

            modelBuilder.Entity<CRMOriginador>()
                .HasOne(x => x.Division)
                .WithMany()
                .HasForeignKey(x => x.DivisionId);

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

            modelBuilder.Entity<CRMRol>()
                .HasOne(x => x.Division)
                .WithMany()
                .HasForeignKey(x => x.DivisionId);

            modelBuilder.Entity<CRMEquipo>()
                .HasOne(x => x.Originador)
                .WithMany(x => x.Equipos)
                .HasForeignKey(x => x.LiderId);

            modelBuilder.Entity<CRMEquipoVendedor>().HasKey(x => new { x.EquipoId, x.VendedorId });
            modelBuilder.Entity<CRMEquipo>()
                .HasMany(x => x.Vendedores)
                .WithMany(x => x.Equipos)
                .UsingEntity<CRMEquipoVendedor>(
                l => l.HasOne(x => x.Vendedor).WithMany(x => x.EquipoVendedores).HasForeignKey(x => x.VendedorId).OnDelete(DeleteBehavior.Restrict),
                r => r.HasOne(x => x.Equipo).WithMany(x => x.EquipoVendedores).HasForeignKey(x => x.EquipoId).OnDelete(DeleteBehavior.Restrict));

            modelBuilder.Entity<CRMUsuarioDivision>().HasKey(x => new { x.UsuarioId, x.DivisionId });

        }
        public DbSet<Accion> Accion { get; set; }
        public DbSet<Cliente> Cliente { get; set; }
        public DbSet<GrupoUsuario> GrupoUsuario { get; set; }
        public DbSet<Log> Log { get; set; }
        public DbSet<Tad> Tad { get; set; }
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Contacto> Contacto { get; set; }
        public DbSet<ActividadRegistrada> ActividadRegistrada { get; set; }
        public DbSet<Errors> Errors { get; set; }
        public DbSet<Vendedor> Vendedores { get; set; }
        public DbSet<Originador> Originadores { get; set; }
        public DbSet<Vendedor_Originador> Vendedor_Originador { get; set; }
        public DbSet<Metas_Vendedor> Metas_Vendedor { get; set; }
        public DbSet<Cliente_Tad> Cliente_Tad { get; set; }
        public DbSet<Usuario_Tad> Usuario_Tad { get; set; }
        public DbSet<Catalogo_Fijo> Catalogo_Fijo { get; set; }
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
        public DbSet<CRMUsuarioDivision> CRMUsuarioDivisiones { get; set; }
    }
}