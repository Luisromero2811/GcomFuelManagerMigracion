using AutoMapper;
using FluentValidation;
using GComFuelManager.Client.Helpers;
using GComFuelManager.Server.Helpers;
using GComFuelManager.Shared.DTOs.CRM;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Server.Controllers.CRM
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CRMCatalogoController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IValidator<CRMCatalogoValorPostDTO> validator;

        public CRMCatalogoController(ApplicationDbContext context, IMapper mapper, IValidator<CRMCatalogoValorPostDTO> validator)
        {
            this.context = context;
            this.mapper = mapper;
            this.validator = validator;
        }

        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] CRMCatalogoDTO cRM)
        {
            var catalogos = context.CRMCatalogos.Where(x => x.Activo).AsQueryable();

            if (!string.IsNullOrEmpty(cRM.Nombre) && !string.IsNullOrWhiteSpace(cRM.Nombre))
                catalogos = catalogos.Where(x => x.Nombre.ToLower().Contains(x.Nombre.ToLower()));

            await HttpContext.InsertarParametrosPaginacion(catalogos, cRM.Registros_por_pagina, cRM.Pagina);
            cRM.Pagina = HttpContext.ObtenerPagina();

            catalogos = catalogos.Skip((cRM.Pagina - 1) * cRM.Registros_por_pagina).Take(cRM.Registros_por_pagina);

            var catalogodto = catalogos.Select(x => mapper.Map<CRMCatalogoDTO>(x));

            return Ok(catalogodto);

        }

        [HttpGet("valor")]
        public async Task<ActionResult> GetValor([FromQuery] CRMCatalogoValorDTO cRM)
        {
            var catalogos = context.CRMCatalogoValores.Where(x => x.Activo).AsQueryable();

            if (!string.IsNullOrEmpty(cRM.Valor) && !string.IsNullOrWhiteSpace(cRM.Valor))
                catalogos = catalogos.Where(x => x.Valor.ToLower().Contains(x.Valor.ToLower()));

            if (!cRM.CatalogoId.IsZero())
                catalogos = catalogos.Where(x => x.CatalogoId == cRM.CatalogoId);

            await HttpContext.InsertarParametrosPaginacion(catalogos, cRM.Registros_por_pagina, cRM.Pagina);
            cRM.Pagina = HttpContext.ObtenerPagina();

            catalogos = catalogos.Skip((cRM.Pagina - 1) * cRM.Registros_por_pagina).Take(cRM.Registros_por_pagina);

            var catalogodto = catalogos.Select(x => mapper.Map<CRMCatalogoValorDTO>(x));

            return Ok(catalogodto);
        }

        [HttpGet("valor/{Id:int}")]
        public async Task<ActionResult> GetValorById([FromRoute] int Id)
        {
            var valor = await context.CRMCatalogoValores.AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id);
            if (valor is null) return NotFound();

            var valordto = mapper.Map<CRMCatalogoValorPostDTO>(valor);

            return Ok(valordto);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CRMCatalogoValorPostDTO cRM)
        {
            try
            {
                var validate = validator.Validate(cRM);
                if (!validate.IsValid) return BadRequest(validate.Errors);

                var valordto = mapper.Map<CRMCatalogoValor>(cRM);

                if (!cRM.Id.IsZero())
                {
                    var valordb = await context.CRMCatalogoValores.AsNoTracking().FirstOrDefaultAsync(x => x.Id == cRM.Id);
                    if (valordb is null) { return NotFound(); }
                    var valor = mapper.Map(valordto, valordb);

                    context.Update(valor);
                }
                else
                {
                    await context.AddAsync(valordto);
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
        public async Task<ActionResult> Post([FromRoute] int Id)
        {
            try
            {
                var valor = await context.CRMCatalogoValores.FindAsync(Id);
                if (valor is null) return NotFound();

                valor.Activo = false;
                context.Update(valor);
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
