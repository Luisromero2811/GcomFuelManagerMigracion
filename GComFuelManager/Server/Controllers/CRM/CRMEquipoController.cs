using AutoMapper;
using FluentValidation;
using GComFuelManager.Server.Helpers;
using GComFuelManager.Shared.DTOs.CRM;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Server.Controllers.CRM
{
    [Route("api/[controller]")]
    [ApiController]
    public class CRMEquipoController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IValidator<CRMEquipoPostDTO> validator;

        public CRMEquipoController(ApplicationDbContext context, IMapper mapper, IValidator<CRMEquipoPostDTO> validator)
        {
            this.context = context;
            this.mapper = mapper;
            this.validator = validator;

        }

        [HttpGet]
        public async Task<ActionResult> GetAll([FromQuery] CRMEquipoDTO dTO)
        {
            try
            {
                var equipos = context.CRMEquipos
                    .Include(x => x.Originador)
                    .Include(x => x.Division)
                    .AsNoTracking()
                    .AsQueryable();

                if (!string.IsNullOrEmpty(dTO.Nombre) || !string.IsNullOrWhiteSpace(dTO.Nombre))
                    equipos = equipos.Where(v => v.Nombre.ToLower().Contains(dTO.Nombre.ToLower()));

                if (!string.IsNullOrEmpty(dTO.Lider) || !string.IsNullOrWhiteSpace(dTO.Lider))
                    equipos = equipos.Where(v => v.Originador.Nombre.ToLower().Contains(dTO.Lider.ToLower()) || v.Originador.Apellidos.ToLower().Contains(dTO.Lider.ToLower()));

                if (!string.IsNullOrEmpty(dTO.Division) || !string.IsNullOrWhiteSpace(dTO.Division))
                    equipos = equipos.Where(v => v.Division.Nombre.ToLower().Contains(dTO.Division.ToLower()));

                await HttpContext.InsertarParametrosPaginacion(equipos, dTO.Registros_por_pagina, dTO.Pagina);

                dTO.Pagina = HttpContext.ObtenerPagina();

                equipos = equipos.Skip((dTO.Pagina - 1) * dTO.Registros_por_pagina).Take(dTO.Registros_por_pagina);
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
                equipodto.Vendedores = vendedoresdto;

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
                    .Include(x => x.Vendedores)
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

                if (equipo.Id != 0)
                {
                    var relations = dTO.Vendedores.Select(x => new CRMEquipoVendedor { EquipoId = equipo.Id, VendedorId = x.Id }).ToList();
                    var relations_actual = await context.CRMEquipoVendedores.Where(x => x.EquipoId == equipo.Id).ToListAsync();

                    if (!relations_actual.SequenceEqual(relations))
                    {
                        context.RemoveRange(relations_actual);
                        await context.AddRangeAsync(relations);
                    }

                    context.Update(equipo);
                }
                else
                    await context.AddAsync(equipo);

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
    }
}
