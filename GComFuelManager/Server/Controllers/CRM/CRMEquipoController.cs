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

    public class CRMEquipoController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IValidator<CRMEquipoPostDTO> validator;
        private readonly UserManager<IdentityUsuario> manager;

        public CRMEquipoController(ApplicationDbContext context, IMapper mapper, IValidator<CRMEquipoPostDTO> validator, UserManager<IdentityUsuario> manager)
        {
            this.context = context;
            this.mapper = mapper;
            this.validator = validator;
            this.manager = manager;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll([FromQuery] CRMEquipoDTO dTO)
        {
            try
            {
                if (HttpContext.User.Identity is null) { return NotFound(); }
                if (string.IsNullOrEmpty(HttpContext.User.Identity.Name) || string.IsNullOrWhiteSpace(HttpContext.User.Identity.Name)) { return NotFound(); }

                var user = await manager.FindByNameAsync(HttpContext.User.Identity.Name);
                if (user is null) { return NotFound(); }

                var equipos = new List<CRMEquipo>().AsQueryable();

                if (await manager.IsInRoleAsync(user, "Admin"))
                {
                    equipos = context.CRMEquipos.Where(x => x.Activo)
                    .AsNoTracking()
                    .Include(x => x.Originador)
                    .Include(x => x.Division)
                    .OrderBy(x => x.Nombre)
                    .AsQueryable();
                }
                else if (await manager.IsInRoleAsync(user, "CRM_LIDER"))
                {
                    var comercial = await context.CRMOriginadores.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == user.Id);
                    if (comercial is null) return NotFound();

                    equipos = context.CRMEquipos.AsNoTracking().Where(x => x.Activo && x.LiderId == comercial.Id)
                    .Include(x => x.Originador)
                    .Include(x => x.Division)
                    .OrderBy(x => x.Nombre)
                    .AsQueryable();
                }
                else
                {
                    var vendedor = await context.CRMVendedores.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == user.Id);
                    if (vendedor is null) return NotFound();
                    var relacion = await context.CRMEquipoVendedores.AsNoTracking()
                        .Where(x => x.Equipo != null && x.Equipo.Activo && x.VendedorId == vendedor.Id)
                        .Include(x => x.Equipo)
                        .Select(x => x.EquipoId).ToListAsync();
                    equipos = context.CRMEquipos.AsNoTracking().Where(x => x.Activo && relacion.Contains(x.Id))
                        .IgnoreAutoIncludes()
                        .Include(x => x.Originador)
                        .Include(x => x.Division)
                        .OrderBy(x => x.Nombre)
                        .AsQueryable();
                }

                if (!string.IsNullOrEmpty(dTO.Nombre) || !string.IsNullOrWhiteSpace(dTO.Nombre))
                    equipos = equipos.Where(v => v.Nombre.ToLower().Contains(dTO.Nombre.ToLower()));

                if (!string.IsNullOrEmpty(dTO.Lider) || !string.IsNullOrWhiteSpace(dTO.Lider))
                    equipos = equipos.Where(v => v.Originador.Nombre.ToLower().Contains(dTO.Lider.ToLower()) || v.Originador.Apellidos.ToLower().Contains(dTO.Lider.ToLower()));

                if (!string.IsNullOrEmpty(dTO.Division) || !string.IsNullOrWhiteSpace(dTO.Division))
                    equipos = equipos.Where(v => v.Division.Nombre.ToLower().Contains(dTO.Division.ToLower()));

                if (dTO.Paginacion)
                {
                    await HttpContext.InsertarParametrosPaginacion(equipos, dTO.Registros_por_pagina, dTO.Pagina);
                    dTO.Pagina = HttpContext.ObtenerPagina();
                    equipos = equipos.Skip((dTO.Pagina - 1) * dTO.Registros_por_pagina).Take(dTO.Registros_por_pagina);
                }

                var equiposdto = equipos.Select(x => mapper.Map<CRMEquipoDTO>(x));

                return Ok(equiposdto);
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
                var equipo = await context.CRMEquipos.Where(x => x.Id == Id)
                    .Include(x => x.Originador)
                    .Include(x => x.Division)
                    .SingleOrDefaultAsync();
                if (equipo is null) { return NotFound(); }

                var vendedoresdto = equipo.Vendedores.Select(x => mapper.Map<CRMVendedor, CRMVendedorDTO>(x)).ToList();

                var equipodto = mapper.Map<CRMEquipoPostDTO>(equipo);
                equipodto.VendedoresDTO = vendedoresdto;

                return Ok(equipodto);
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
                var equipo = await context.CRMEquipos
                    .AsNoTracking()
                    .Include(x => x.Division)
                    .Include(x => x.Originador)
                    .ThenInclude(x => x.Division)
                    .Include(x => x.Vendedores)
                    .ThenInclude(x => x.Division)
                    .Where(x => x.Id == Id)
                    .Select(x => mapper.Map<CRMEquipo, CRMEquipoDetalleDTO>(x))
                    .SingleOrDefaultAsync();
                if (equipo is null) { return NotFound(); }

                //var vendedordto = mapper.Map<CRMVendedorDTO>(vendedor);

                return Ok(equipo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CRMEquipoPostDTO dTO)
        {
            try
            {
                var validate = await validator.ValidateAsync(dTO);
                if (!validate.IsValid) { return BadRequest(validate.Errors); }

                var equipo = mapper.Map<CRMEquipoPostDTO, CRMEquipo>(dTO);
                //var originadores = dTO.OriginadoresDTO.Select(x => mapper.Map<CRMOriginadorDTO, CRMOriginador>(x)).ToList();

                //vendedor.Originadores = originadores;

                var lider = await context.CRMOriginadores.AsNoTracking().FirstOrDefaultAsync(x => x.Id == equipo.LiderId);
                if (lider is null) { return NotFound(); }
                if (string.IsNullOrEmpty(lider.UserId)) { return BadRequest("El comercial no cuenta con un usuario relacionado"); }
                var usercomercial = await manager.FindByIdAsync(lider.UserId);
                if (usercomercial is null) { return BadRequest("El comercial no cuenta con un usuario relacionado"); }

                if (equipo.Id != 0)
                {
                    var relations = dTO.VendedoresDTO.Select(x => new CRMEquipoVendedor { EquipoId = equipo.Id, VendedorId = x.Id }).ToList();
                    var relations_actual = await context.CRMEquipoVendedores.Where(x => x.EquipoId == equipo.Id).ToListAsync();

                    if (!relations_actual.SequenceEqual(relations))
                    {
                        context.RemoveRange(relations_actual);
                        await context.AddRangeAsync(relations);
                    }

                    var equipodb = await context.CRMEquipos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == equipo.Id);
                    if (equipodb is null) { return NotFound(); }
                    if (equipo.LiderId != equipodb.LiderId)
                    {
                        var nlider = await context.CRMOriginadores.AsNoTracking().FirstOrDefaultAsync(x => x.Id == equipo.LiderId);
                        if (nlider is null) { return NotFound(); }
                        if (string.IsNullOrEmpty(nlider.UserId)) { return BadRequest("El comercial no cuenta con un usuario relacionado"); }
                        var nusercomercial = await manager.FindByIdAsync(lider.UserId);
                        if (nusercomercial is null) { return BadRequest("El comercial no cuenta con un usuario relacionado"); }

                        if (!await context.CRMEquipos.AnyAsync(x => x.LiderId == lider.Id && x.Id != equipo.Id))
                            await manager.RemoveFromRoleAsync(usercomercial, "CRM_LIDER");

                        if (!await manager.IsInRoleAsync(nusercomercial, "CRM_LIDER"))
                            await manager.AddToRoleAsync(nusercomercial, "CRM_LIDER");
                    }

                    var e = mapper.Map(equipo, equipodb);
                    context.Update(e);
                }
                else
                {
                    var integrantes = dTO.VendedoresDTO.Select(x => new CRMEquipoVendedor { EquipoId = equipo.Id, VendedorId = x.Id }).ToList();
                    equipo.EquipoVendedores = integrantes;

                    if (!await manager.IsInRoleAsync(usercomercial, "CRM_LIDER"))
                        await manager.AddToRoleAsync(usercomercial, "CRM_LIDER");

                    await context.AddAsync(equipo);
                }
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
                var equipo = await context.CRMEquipos.FindAsync(Id);
                if (equipo is null) { return NotFound(); }
                equipo.Activo = false;
                context.Update(equipo);
                await context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("vendedores/equipo/{id:int}")]
        public async Task<ActionResult> GetIntegrantesByEquipoId([FromRoute] int id, [FromQuery] CRMVendedorDTO vendedor)
        {
            try
            {
                List<CRMPermisoDTO> permisos = new();

                var vendedoresDeEquipo = await context.CRMEquipoVendedores
                    .Where(x => x.EquipoId == id)
                    .ToListAsync();

                var allVendedores = await context.CRMVendedores
                    .Where(x => !string.IsNullOrEmpty(x.Nombre) && !string.IsNullOrEmpty(x.Apellidos) && x.Activo)
                    .Include(x => x.Division)
                    .OrderBy(x => x.Nombre)
                    .ToListAsync();

                var vendedoresEquipo = allVendedores.IntersectBy(vendedoresDeEquipo.Select(x => x.VendedorId), x => x.Id)
                    .Select(x => mapper.Map<CRMVendedor, CRMVendedorDTO>(x)).AsQueryable();

                if (!string.IsNullOrEmpty(vendedor.Nombre) && !string.IsNullOrWhiteSpace(vendedor.Nombre))
                    vendedoresEquipo = vendedoresEquipo.Where(x => x.Nombre.ToLower().Contains(vendedor.Nombre.ToLower()) || x.Apellidos.ToLower().Contains(vendedor.Nombre.ToLower()));

                return Ok(vendedoresEquipo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("vendedores/no/equipo/{id:int}")]
        public async Task<ActionResult> GetNoIntegrantesByEquipoId([FromRoute] int id, [FromQuery] CRMVendedorDTO vendedor)
        {
            try
            {
                if (HttpContext.User.Identity is null) { return NotFound(); }
                if (string.IsNullOrEmpty(HttpContext.User.Identity.Name) || string.IsNullOrWhiteSpace(HttpContext.User.Identity.Name)) { return NotFound(); }

                var user = await manager.FindByNameAsync(HttpContext.User.Identity.Name);
                if (user is null) { return NotFound(); }

                List<CRMVendedor> allVendedores = new();

                if (await manager.IsInRoleAsync(user, "Admin"))
                {
                    allVendedores = await context.CRMVendedores.AsNoTracking()
                        .Where(x => !string.IsNullOrEmpty(x.Nombre) && !string.IsNullOrEmpty(x.Apellidos) && x.Activo)
                        .Include(x => x.Division)
                        .OrderBy(x => x.Nombre)
                        .ToListAsync();
                }
                else if (await manager.IsInRoleAsync(user, "CRM_LIDER"))
                {
                    //List<int> divisiones = await context.CRMUsuarioDivisiones.Where(x => x.UsuarioId == user.Id).Select(x => x.DivisionId).ToListAsync();

                    //allVendedores = await context.CRMVendedores
                    //    .Where(x => !string.IsNullOrEmpty(x.Nombre) && !string.IsNullOrEmpty(x.Apellidos) && x.Activo && divisiones.Any(y => y == x.DivisionId))
                    //    .Include(x => x.Division)
                    //    .OrderBy(x => x.Nombre)
                    //    .ToListAsync();

                    allVendedores = await context.CRMVendedores.AsNoTracking()
                        .Where(x => !string.IsNullOrEmpty(x.Nombre) && !string.IsNullOrEmpty(x.Apellidos) && x.Activo)
                        .Include(x => x.Division)
                        .OrderBy(x => x.Nombre)
                        .ToListAsync();
                }

                var vendedoresDeEquipo = await context.CRMEquipoVendedores
                    .Where(x => x.EquipoId == id)
                    .ToListAsync();

                var vendedoresNoEquipo = allVendedores.ExceptBy(vendedoresDeEquipo.Select(x => x.VendedorId), x => x.Id)
                    .Select(x => mapper.Map<CRMVendedor, CRMVendedorDTO>(x)).AsQueryable();

                if (!string.IsNullOrEmpty(vendedor.Nombre) && !string.IsNullOrWhiteSpace(vendedor.Nombre))
                    vendedoresNoEquipo = vendedoresNoEquipo.Where(x => x.Nombre.ToLower().Contains(vendedor.Nombre.ToLower()) || x.Apellidos.ToLower().Contains(vendedor.Nombre.ToLower()));


                return Ok(vendedoresNoEquipo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
