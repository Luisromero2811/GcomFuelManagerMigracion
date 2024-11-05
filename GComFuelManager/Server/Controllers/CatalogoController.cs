using AutoMapper;
using FluentValidation;
using GComFuelManager.Server.Helpers;
using GComFuelManager.Shared.Extensiones;
using GComFuelManager.Shared.ModelDTOs;
using GComFuelManager.Shared.Modelos;
using Humanizer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CatalogoController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly User_Terminal terminal;
        private readonly IMapper mapper;
        private readonly IValidator<CatalogoValorPostDTO> validator;

        public CatalogoController(ApplicationDbContext context, User_Terminal terminal, IMapper mapper, IValidator<CatalogoValorPostDTO> validator)
        {
            this.context = context;
            this.terminal = terminal;
            this.mapper = mapper;
            this.validator = validator;
        }

        // GET: api/<CatalogoController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CatalogoDTO>>> Get([FromQuery] CatalogoDTO dto)
        {
            try
            {
                var catalogos = context.Catalogos
                    .AsNoTracking()
                    .Where(x => x.Activo)
                    .Select(x => mapper.Map<CatalogoDTO>(x))
                    .AsQueryable();

                if (!string.IsNullOrEmpty(dto.Nombre) && !string.IsNullOrWhiteSpace(dto.Nombre))
                    catalogos = catalogos.Where(x => x.Nombre.ToLower().Contains(dto.Nombre.ToLower()));

                await HttpContext.InsertarParametrosPaginacion(catalogos, dto.Registros_por_pagina, dto.Pagina);
                dto.Pagina = HttpContext.ObtenerPagina();

                catalogos = catalogos.Skip((dto.Pagina - 1) * dto.Registros_por_pagina).Take(dto.Registros_por_pagina);

                return Ok(catalogos);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        // GET api/<CatalogoController>/5
        [HttpGet("valor")]
        public async Task<ActionResult<IEnumerable<CatalogoValorDTO>>> Get([FromQuery] CatalogoValorDTO dTO)
        {
            try
            {
                var id_terminal = terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var valores = context.CatalogoValores
                    .AsNoTracking()
                    .Where(x => x.Activo && x.TadId.Equals(id_terminal))
                    .OrderBy(x => x.Abreviacion)
                    .ThenBy(x => x.Valor)
                    .AsQueryable();

                if (!dTO.CatalogoId.IsZero())
                    valores = valores.Where(x => x.CatalogoId.Equals(dTO.CatalogoId));

                if (!string.IsNullOrEmpty(dTO.Valor) && !string.IsNullOrWhiteSpace(dTO.Valor))
                    valores = valores.Where(x => x.Valor.ToLower().Contains(dTO.Valor.ToLower()));

                await HttpContext.InsertarParametrosPaginacion(valores, dTO.Registros_por_pagina, dTO.Pagina);
                dTO.Pagina = HttpContext.ObtenerPagina();

                valores = valores.Skip((dTO.Pagina - 1) * dTO.Registros_por_pagina).Take(dTO.Registros_por_pagina);

                return Ok(valores.Select(x => mapper.Map<CatalogoValorDTO>(x)));
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpGet("valor/{Id}")]
        public async Task<ActionResult<IEnumerable<CatalogoValorDTO>>> GetById([FromRoute] int Id)
        {
            try
            {
                var id_terminal = terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var valor = await context.CatalogoValores
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id.Equals(Id));

                return Ok(mapper.Map<CatalogoValorPostDTO>(valor));
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        // POST api/<CatalogoController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CatalogoValorPostDTO postDTO)
        {
            try
            {
                var id_terminal = terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var result = validator.Validate(postDTO);
                if (!result.IsValid) { return BadRequest(result.Errors.Select(x => x.ErrorMessage)); }

                var valor = mapper.Map<CatalogoValor>(postDTO);

                valor.TadId = id_terminal;

                if (!valor.Id.IsZero())
                {
                    var valordb = await context.CatalogoValores.AsNoTracking().FirstOrDefaultAsync(x => x.Id.Equals(valor.Id));
                    if (valordb is null) { return NotFound(); }

                    if (!valordb.EsEditable) { return BadRequest("Este valor no se puede editar"); }

                    valor = mapper.Map(valor, valordb);

                    context.Update(valor);
                }
                else
                {
                    valor.Code = valor.GetHashCode();
                    await context.AddAsync(valor);
                }

                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        // DELETE api/<CatalogoController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var valor = await context.CatalogoValores.FindAsync(id);
                if (valor is null) { return NotFound(); }

                valor.Activo = false;

                context.Update(valor);

                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

    }
}
