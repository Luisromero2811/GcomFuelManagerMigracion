using AutoMapper;
using GComFuelManager.Shared.DTOs.CRM;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Server.Controllers.CRM
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class CRMPermisoController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public CRMPermisoController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult> GetPermisosByRolId([FromRoute] int id)
        {
            try
            {
                List<CRMPermisoDTO> permisos = new();

                var permisosDeRol = await context.CRMRolPermisos.Where(x => x.RolId == id).ToListAsync();

                var allPermisos = await context.Roles.Where(x => !string.IsNullOrEmpty(x.Name) && x.Show).OrderBy(x => x.Name).ToListAsync();

                var permisosRol = allPermisos.IntersectBy(permisosDeRol.Select(x => x.PermisoId), x => x.Id)
                    .Select(x => new CRMPermisoDTO
                    {
                        Id = x.Id,
                        Nombre = x.Name!
                    });

                var idspermisos = permisosRol.Select(x => x.Id);

                var grupo = await context.CRMGrupos
                    .Where(x => x.Activo)
                    .Include(x => x.GrupoRols)
                    .ToListAsync();

                var gruporoles = grupo.Select(x => new CRMGrupoPermisoDTO
                {
                    Grupo = x.Nombre,
                    Permisos = x.GrupoRols.IntersectBy(idspermisos, y => y.RolId).Select(y => new CRMPermisoDTO
                    {
                        Id = y.RolId,
                        Nombre = permisosRol.First(x => x.Id == y.RolId).Nombre
                    })
                    .ToList()
                });

                return Ok(gruporoles.Where(x => x.Permisos.Any()));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("no/{id:int}")]
        public async Task<ActionResult> GetNoPermisosByRolId([FromRoute] int id)
        {
            try
            {

                var allPermisos = await context.Roles.Where(x => !string.IsNullOrEmpty(x.Name) && x.Show).OrderBy(x => x.Name).ToListAsync();

                var permisosDeRol = await context.CRMRolPermisos.Where(x => x.RolId == id).ToListAsync();

                var permisosNoRol = allPermisos.ExceptBy(permisosDeRol.Select(x => x.PermisoId), x => x.Id)
                    .Select(x => new CRMPermisoDTO
                    {
                        Id = x.Id,
                        Nombre = x.Name!
                    });

                var idspermisos = permisosNoRol.Select(x => x.Id);

                var grupo = await context.CRMGrupos
                    .Where(x => x.Activo)
                    .Include(x => x.GrupoRols)
                    .ToListAsync();

                var gruporoles = grupo.Select(x => new CRMGrupoPermisoDTO
                {
                    Grupo = x.Nombre,
                    Permisos = x.GrupoRols.IntersectBy(idspermisos, y => y.RolId).Select(y => new CRMPermisoDTO
                    {
                        Id = y.RolId,
                        Nombre = permisosNoRol.First(x => x.Id == y.RolId).Nombre
                    })
                    .ToList()
                });

                return Ok(gruporoles.Where(x => x.Permisos.Any()));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
