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
    public class CRMOriginadorController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUsuario> userManager;
        private readonly IMapper mapper;
        private readonly IValidator<CRMOriginadorPostDTO> validator;

        public CRMOriginadorController(ApplicationDbContext context,
            UserManager<IdentityUsuario> userManager,
            IMapper mapper,
            IValidator<CRMOriginadorPostDTO> validator)
        {
            this.context = context;
            this.userManager = userManager;
            this.mapper = mapper;
            this.validator = validator;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll([FromQuery] CRMOriginadorDTO dTO)
        {
            try
            {
                var originadores = context.CRMOriginadores.AsNoTracking().AsQueryable();

                if (!string.IsNullOrEmpty(dTO.Nombre) || !string.IsNullOrWhiteSpace(dTO.Nombre))
                    originadores = originadores.Where(v => v.Nombre.ToLower().Contains(dTO.Nombre.ToLower()) || v.Apellidos.ToLower().Contains(dTO.Nombre.ToLower()));

                if (!string.IsNullOrEmpty(dTO.Tel_Movil) || !string.IsNullOrWhiteSpace(dTO.Tel_Movil))
                    originadores = originadores.Where(v => !string.IsNullOrEmpty(v.Tel_Movil) && v.Tel_Movil.ToLower().Contains(dTO.Tel_Movil.ToLower()));

                if (!string.IsNullOrEmpty(dTO.Correo) || !string.IsNullOrWhiteSpace(dTO.Correo))
                    originadores = originadores.Where(v => !string.IsNullOrEmpty(v.Correo) && v.Correo.ToLower().Contains(dTO.Correo.ToLower()));

                if (dTO.Paginacion)
                {
                    await HttpContext.InsertarParametrosPaginacion(originadores, dTO.Registros_por_pagina, dTO.Pagina);
                    dTO.Pagina = HttpContext.ObtenerPagina();
                    originadores = originadores.Include(x => x.Division).Skip((dTO.Pagina - 1) * dTO.Registros_por_pagina).Take(dTO.Registros_por_pagina);
                }

                var originadoresdto = originadores.Select(x => mapper.Map<CRMOriginadorDTO>(x));
                return Ok(originadoresdto);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("listoriginadores")]
        public async Task<ActionResult> GetAllOriginadores()
        {
            try
            {
                var originadoresCRM = await context.CRMOriginadores
                    .Where(x => x.Activo == true)
                    .ToListAsync();
                return Ok(originadoresCRM);
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
                var originador = await context.CRMOriginadores.FindAsync(Id);
                if (originador is null) { return NotFound(); }

                //var originadordto = mapper.Map<CRMoriginadorDTO>(originador);

                return Ok(originador);
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
                var originador = await context.CRMOriginadores
                    .AsNoTracking()
                    .Include(x => x.Division)
                    .Include(x => x.Equipos)
                    .ThenInclude(x => x.Division)
                    .Where(x => x.Id == Id)
                    .Select(x => mapper.Map<CRMOriginador, CRMOriginadorDetalleDTO>(x))
                    .SingleOrDefaultAsync();
                if (originador is null) { return NotFound(); }

                return Ok(originador);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CRMOriginadorPostDTO dTO)
        {
            try
            {
                var validate = await validator.ValidateAsync(dTO);
                if (!validate.IsValid) { return BadRequest(validate.Errors); }

                var originadordto = mapper.Map<CRMOriginadorPostDTO, CRMOriginador>(dTO);

                if (originadordto.Id != 0)
                {

                    var originadordb = await context.CRMOriginadores.FindAsync(dTO.Id);
                    if (originadordb is null) { return NotFound(); }
                    var originador = mapper.Map(originadordto, originadordb);
                    context.Update(originador);
                }
                else
                    await context.AddAsync(originadordto);

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
                var originador = await context.CRMOriginadores.FindAsync(Id);
                if (originador is null) { return NotFound(); }
                originador.Activo = false;
                context.Update(originador);
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
