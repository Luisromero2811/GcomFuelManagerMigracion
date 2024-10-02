using AutoMapper;
using FluentValidation;
using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.DTOs.CRM;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Server.Controllers.CRM
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class CRMRolController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IValidator<CRMRolPostDTO> validator;
        private readonly UserManager<IdentityUsuario> userManager;
        private readonly RoleManager<IdentityRol> roleManager;

        public CRMRolController(ApplicationDbContext context, IMapper mapper, IValidator<CRMRolPostDTO> validator, UserManager<IdentityUsuario> userManager, RoleManager<IdentityRol> roleManager)
        {
            this.context = context;
            this.mapper = mapper;
            this.validator = validator;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll([FromQuery] CRMRolDTO dTO)
        {
            try
            {
                var roles = context.CRMRoles.Where(x => x.Activo).AsNoTracking().AsQueryable();

                if (!string.IsNullOrEmpty(dTO.Nombre) || !string.IsNullOrWhiteSpace(dTO.Nombre))
                    roles = roles.Where(v => v.Nombre.ToLower().Contains(dTO.Nombre.ToLower()));

                await HttpContext.InsertarParametrosPaginacion(roles, dTO.Registros_por_pagina, dTO.Pagina);

                dTO.Pagina = HttpContext.ObtenerPagina();

                roles = roles.Skip((dTO.Pagina - 1) * dTO.Registros_por_pagina).Take(dTO.Registros_por_pagina);
                var rolesdto = roles.Select(x => mapper.Map<CRMRolDTO>(x));

                return Ok(rolesdto);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("listroles")]
        public async Task<ActionResult> GetAllRoles()
        {
            try
            {
                var rolesCRM = await context.CRMRoles
                    .Where(x => x.Activo == true)
                    .ToListAsync();
                return Ok(rolesCRM);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("{Id:int}")]
        public async Task<ActionResult> GetById([FromRoute] int Id)
        {
            try
            {
                var rol = await context.CRMRoles
                    .AsNoTracking()
                    .Where(x => x.Id == Id)
                    .Select(x => mapper.Map<CRMRol, CRMRolPostDTO>(x))
                    .SingleOrDefaultAsync();
                if (rol is null) { return NotFound(); }

                //var vendedordto = mapper.Map<CRMVendedorDTO>(vendedor);

                return Ok(rol);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("{Id:int}/detalle")]
        public async Task<ActionResult> GetByIdDetalle([FromRoute] int Id)
        {
            try
            {
                var rol = await context.CRMRoles
                    .AsNoTracking()
                    .Where(x => x.Id == Id)
                    .Select(x => mapper.Map<CRMRol, CRMRolDetalleDTO>(x))
                    .SingleOrDefaultAsync();
                if (rol is null) { return NotFound(); }

                var permisosrol = await context.CRMRolPermisos.AsNoTracking().Where(x => x.RolId == rol.Id).ToListAsync();
                var allpermisos = await context.Roles.AsNoTracking().Where(x => x.Show && !string.IsNullOrEmpty(x.Name)).AsNoTracking().ToListAsync();

                var permisos = allpermisos.IntersectBy(permisosrol.Select(x => x.PermisoId), x => x.Id)
                    .Select(x => new CRMPermisoDTO
                    {
                        Id = x.Id,
                        Nombre = x.Name!
                    })
                    .OrderBy(x => x.Nombre)
                    .ToList();

                rol.Permisos = permisos;

                return Ok(rol);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CRMRolPostDTO dTO)
        {
            try
            {
                var validate = await validator.ValidateAsync(dTO);
                if (!validate.IsValid) { return BadRequest(validate.Errors); }

                var rol = mapper.Map<CRMRolPostDTO, CRMRol>(dTO);

                if (await context.CRMRoles.AnyAsync(x => x.Nombre.ToLower().Equals(rol.Nombre.ToLower())))
                    return BadRequest("Ya existe un rol con ese nombre");

                if (rol.Id != 0)
                {
                    var relations = dTO.Permisos.Select(x => new CRMRolPermiso { RolId = rol.Id, PermisoId = x.Id }).ToList();
                    var relations_actual = await context.CRMRolPermisos.Where(x => x.RolId == rol.Id).ToListAsync();

                    if (!relations_actual.SequenceEqual(relations))
                    {
                        context.RemoveRange(relations_actual);
                        await context.AddRangeAsync(relations);

                        var nombrePermisos = await context.Roles.AsNoTracking()
                            .Where(x => relations_actual.Select(x => x.PermisoId).Contains(x.Id) && !string.IsNullOrEmpty(x.Name) && !string.IsNullOrWhiteSpace(x.Name))
                            .Select(x => (string)x.Name!)
                            .ToListAsync();
                        var nombrePermisosNuevos = await context.Roles.AsNoTracking()
                            .Where(x => relations.Select(x => x.PermisoId).Contains(x.Id) && !string.IsNullOrEmpty(x.Name) && !string.IsNullOrWhiteSpace(x.Name))
                            .Select(x => (string)x.Name!)
                            .ToListAsync();

                        var userEnRoles = await context.CRMRolUsuarios.AsNoTracking().Where(x => x.RolId == rol.Id).Select(x => x.UserId).ToListAsync();
                        foreach (var userEnRol in userEnRoles)
                        {

                            var identityUser = await userManager.FindByIdAsync(userEnRol);
                            if (identityUser is not null)
                            {
                                await userManager.RemoveFromRolesAsync(identityUser, nombrePermisos);
                                await userManager.AddToRolesAsync(identityUser, nombrePermisosNuevos);
                            }
                        }
                    }

                    context.Update(rol);
                    await context.SaveChangesAsync();
                }
                else
                {
                    await context.AddAsync(rol);
                    await context.SaveChangesAsync();

                    var relations = dTO.Permisos.Select(x => new CRMRolPermiso { RolId = rol.Id, PermisoId = x.Id }).ToList();
                    await context.AddRangeAsync(relations);
                    await context.SaveChangesAsync();
                }


                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("{Id:int}")]
        public async Task<ActionResult> Delete([FromRoute] int Id)
        {
            try
            {
                var rol = await context.CRMRoles.FindAsync(Id);
                if (rol is null) { return NotFound(); }
                rol.Activo = false;
                context.Update(rol);
                //ids del permiso relacionado al rol a desactivar
                var relacion_permisos = await context.CRMRolPermisos.Where(x => x.RolId.Equals(rol.Id)).Select(x => x.PermisoId).ToListAsync();
                //permisos del rol a desactivar
                var roles = await context.Roles.Where(x => relacion_permisos.Contains(x.Id)).ToListAsync();

                //ids de usuarios relacionados al rol a desactivar
                var relacion_usuario = await context.CRMRolUsuarios.Where(x => x.RolId.Equals(rol.Id)).Select(x => x.UserId).ToListAsync();
                //roles de usuarios
                var relacion_roles_usuario = await context.CRMRolUsuarios.Where(x => relacion_usuario.Contains(x.UserId)).ToListAsync();

                foreach (var userid in relacion_usuario)
                {
                    //ids de roles de usuarios
                    var rolesuser = relacion_roles_usuario.Where(x => x.UserId == userid && x.RolId != rol.Id).Select(x => x.RolId).ToList();
                    //ids de permisos de roles del usuario
                    var permisosderoldeusuario = await context.CRMRolPermisos.Where(x => rolesuser.Contains(x.RolId)).Select(x => x.PermisoId).ToListAsync();
                    //permisos de roles
                    var permisosasociadosarolesdeusuario = await context.Roles.Where(x => permisosderoldeusuario.Contains(x.Id)).Select(x => x.Name).ToListAsync();
                    //permisos que no estan asociados a otros roles que mantiene el usuario
                    var rolesnopertenecientes = roles.ExceptBy(permisosasociadosarolesdeusuario, x => x.Name)
                        .Where(x => !string.IsNullOrEmpty(x.Name) && !string.IsNullOrWhiteSpace(x.Name))
                        .Select(x => x.Name!);

                    var user = await userManager.FindByIdAsync(userid);

                    if (rolesnopertenecientes is not null)
                        if (user is not null)
                            await userManager.RemoveFromRolesAsync(user, rolesnopertenecientes);
                }

                await context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
