using AutoMapper;
using FluentValidation;
using GComFuelManager.Server.Helpers;
using GComFuelManager.Shared.DTOs.CRM;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;

namespace GComFuelManager.Server.Controllers.CRM
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, Administrador Sistema, CRM, CRM_Lider")]

    public class CRMOportunidadController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IValidator<CRMOportunidadPostDTO> validator;

        public CRMOportunidadController(ApplicationDbContext context, IMapper mapper, IValidator<CRMOportunidadPostDTO> validator)
        {
            this.context = context;
            this.mapper = mapper;
            this.validator = validator;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll([FromQuery] CRMOportunidadDTO dTO)
        {
            try
            {
                var oportunidades = context.CRMOportunidades.AsNoTracking().Where(x => x.Activo)
                    .Include(x => x.UnidadMedida)
                    .Include(x => x.Tipo)
                    .Include(x => x.CRMCliente)
                    .Include(x => x.EtapaVenta)
                    .Include(x => x.Origen)
                    .Include(x => x.Vendedor)
                    .Include(x => x.Contacto)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(dTO.Nombre_Opor) || !string.IsNullOrWhiteSpace(dTO.Nombre_Opor))
                    oportunidades = oportunidades.Where(x => x.Nombre_Opor.ToLower().Contains(dTO.Nombre_Opor.ToLower()));

                if (!string.IsNullOrEmpty(dTO.Vendedor) || !string.IsNullOrWhiteSpace(dTO.Vendedor))
                    oportunidades = oportunidades.Where(x => x.Vendedor != null && x.Vendedor.Nombre.ToLower().Contains(dTO.Vendedor.ToLower()));

                if (!string.IsNullOrEmpty(dTO.Cuenta) || !string.IsNullOrWhiteSpace(dTO.Cuenta))
                    oportunidades = oportunidades.Where(x => x.CRMCliente != null && x.CRMCliente.Nombre.ToLower().Contains(dTO.Cuenta.ToLower()));

                if (!string.IsNullOrEmpty(dTO.EtapaVenta) || !string.IsNullOrWhiteSpace(dTO.EtapaVenta))
                    oportunidades = oportunidades.Where(x => x.EtapaVenta != null && x.EtapaVenta.Valor.ToLower().Contains(dTO.EtapaVenta.ToLower()));

                await HttpContext.InsertarParametrosPaginacion(oportunidades, dTO.Registros_por_pagina, dTO.Pagina);

                if (HttpContext.Response.Headers.TryGetValue("pagina", out StringValues value))
                    if (!string.IsNullOrEmpty(value) || !string.IsNullOrWhiteSpace(value) && value != dTO.Pagina)
                        dTO.Pagina = int.Parse(value!);

                oportunidades = oportunidades.Skip((dTO.Pagina - 1) * dTO.Registros_por_pagina).Take(dTO.Registros_por_pagina);

                var dtoportunidades = oportunidades.Select(x => mapper.Map<CRMOportunidadDTO>(x));

                return Ok(dtoportunidades);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult> GetById([FromRoute] int id)
        {
            try
            {
                var oportunidad = await context.CRMOportunidades.AsNoTracking()
                    .Where(x => x.Id == id)
                    .Select(x => mapper.Map<CRMOportunidadPostDTO>(x))
                    .FirstOrDefaultAsync();
                if (oportunidad is null) { return NotFound(); }

                return Ok(oportunidad);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("{Id:int}/detalle")]
        public async Task<ActionResult> GetByIdDetail([FromRoute] int Id)
        {
            try
            {
                var contacto = await context.CRMOportunidades
                    .AsNoTracking()
                    .Where(x => x.Id == Id)
                    .Include(x => x.Vendedor)
                    .Include(x => x.Contacto)
                    .Include(x=>x.UnidadMedidaId)
                    .Include(x=>x.EtapaVenta)
                    .Include(x=>x.Periodo)
                    .Select(x => mapper.Map<CRMOportunidadDetalleDTO>(x)).FirstOrDefaultAsync();
                return Ok(contacto);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CRMOportunidadPostDTO dto)
        {
            try
            {
                var result = validator.Validate(dto);
                if (!result.IsValid) { return BadRequest(result.Errors); }

                var oportunidad = mapper.Map<CRMOportunidadPostDTO, CRMOportunidad>(dto);
                if (oportunidad.Id != 0)
                {
                    context.Update(oportunidad);
                }
                else
                {
                    await context.AddAsync(oportunidad);
                }
                await context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("{Id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int Id)
        {
            try
            {
                var oportunidad = await context.CRMOportunidades.FindAsync(Id);
                if (oportunidad is null) { return NotFound(); }

                oportunidad.Activo = false;

                context.Update(oportunidad);
                await context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/medida")]
        public async Task<ActionResult> ObtenerCatalogoMedida()
        {
            try
            {
                var catalogo = await context.Accion.FirstOrDefaultAsync(x => x.Nombre.Equals("Catalogo_Oportunidad_Medida"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para unidades de medida"); }

                var catalogo_fijo = await context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo.Cod)).ToListAsync();

                return Ok(catalogo_fijo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/tipo")]
        public async Task<ActionResult> ObtenerCatalogoTipo()
        {
            try
            {
                var catalogo = await context.Accion.FirstOrDefaultAsync(x => x.Nombre.Equals("Catalogo_Oportunidad_Tipo"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para tipo"); }

                var catalogo_fijo = await context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo.Cod)).ToListAsync();

                return Ok(catalogo_fijo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/etapa")]
        public async Task<ActionResult> ObtenerCatalogoEtapa()
        {
            try
            {
                var catalogo = await context.Accion.FirstOrDefaultAsync(x => x.Nombre.Equals("Catalogo_Oportunidad_Etapa"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para etapa de venta"); }

                var catalogo_fijo = await context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo.Cod)).ToListAsync();

                return Ok(catalogo_fijo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/periodo")]
        public async Task<ActionResult> ObtenerPeriodo()
        {
            try
            {
                var catalogo = await context.Accion.FirstOrDefaultAsync(x => x.Nombre.Equals("Catalogo_Oportunidad_Periodo"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para el periodo"); }

                var catalogo_fijo = await context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo.Cod)).ToListAsync();

                return Ok(catalogo_fijo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
