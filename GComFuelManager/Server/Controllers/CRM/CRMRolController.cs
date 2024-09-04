using AutoMapper;
using FluentValidation;
using GComFuelManager.Server.Helpers;
using GComFuelManager.Shared.DTOs.CRM;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GComFuelManager.Server.Controllers.CRM
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class CRMRolController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IValidator<CRMRolPostDTO> validator;

        public CRMRolController(ApplicationDbContext context, IMapper mapper, IValidator<CRMRolPostDTO> validator)
        {
            this.context = context;
            this.mapper = mapper;
            this.validator = validator;
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

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CRMRolPostDTO dTO)
        {
            try
            {
                var validate = await validator.ValidateAsync(dTO);
                if (!validate.IsValid) { return BadRequest(validate.Errors); }

                var rol = mapper.Map<CRMRolPostDTO, CRMRol>(dTO);
                //var originadores = dTO.OriginadoresDTO.Select(x => mapper.Map<CRMOriginadorDTO, CRMOriginador>(x)).ToList();

                //vendedor.Originadores = originadores;

                if (rol.Id != 0)
                {
                    var relations = dTO.Permisos.Select(x => new CRMRolPermiso { RolId = rol.Id, PermisoId = x.Id }).ToList();
                    var relations_actual = await context.CRMRolPermisos.Where(x => x.RolId == rol.Id).ToListAsync();

                    if (!relations_actual.SequenceEqual(relations))
                    {
                        context.RemoveRange(relations_actual);
                        await context.AddRangeAsync(relations);
                    }

                    context.Update(rol);
                }
                else
                    await context.AddAsync(rol);

                await context.SaveChangesAsync();

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
                var vendedor = await context.CRMRoles.FindAsync(Id);
                if (vendedor is null) { return NotFound(); }
                vendedor.Activo = false;
                context.Update(vendedor);
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
