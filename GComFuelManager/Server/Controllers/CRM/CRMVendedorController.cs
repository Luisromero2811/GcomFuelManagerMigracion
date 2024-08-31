using AutoMapper;
using FluentValidation;
using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.DTOs.CRM;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Server.Controllers.CRM
{
    [Route("api/[controller]")]
    [ApiController]
    public class CRMVendedorController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUsuario> userManager;
        private readonly IMapper mapper;
        private readonly IValidator<CRMVendedorPostDTO> validator;

        public CRMVendedorController(ApplicationDbContext context,
            UserManager<IdentityUsuario> userManager,
            IMapper mapper,
            IValidator<CRMVendedorPostDTO> validator)
        {
            this.context = context;
            this.userManager = userManager;
            this.mapper = mapper;
            this.validator = validator;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll([FromQuery] CRMVendedorDTO dTO)
        {
            try
            {
                var vendedores = context.CRMVendedores.AsNoTracking().AsQueryable();

                if (!string.IsNullOrEmpty(dTO.Nombre) || !string.IsNullOrWhiteSpace(dTO.Nombre))
                    vendedores = vendedores.Where(v => v.Nombre.ToLower().Contains(dTO.Nombre.ToLower()) || v.Apellidos.ToLower().Contains(dTO.Nombre));

                if (!string.IsNullOrEmpty(dTO.Tel_Movil) || !string.IsNullOrWhiteSpace(dTO.Tel_Movil))
                    vendedores = vendedores.Where(v => !string.IsNullOrEmpty(v.Tel_Movil) && v.Tel_Movil.ToLower().Contains(dTO.Tel_Movil.ToLower()));

                if (!string.IsNullOrEmpty(dTO.Correo) || !string.IsNullOrWhiteSpace(dTO.Correo))
                    vendedores = vendedores.Where(v => !string.IsNullOrEmpty(v.Correo) && v.Correo.ToLower().Contains(dTO.Correo.ToLower()));

                await HttpContext.InsertarParametrosPaginacion(vendedores, dTO.Registros_por_pagina, dTO.Pagina);

                dTO.Pagina = HttpContext.ObtenerPagina();

                vendedores = vendedores.Include(x => x.Division).Skip((dTO.Pagina - 1) * dTO.Registros_por_pagina).Take(dTO.Registros_por_pagina);
                var vendedoresdto = vendedores.Select(x => mapper.Map<CRMVendedorDTO>(x));

                return Ok(vendedoresdto);
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
                var vendedor = await context.CRMVendedores.Where(x => x.Id == Id)
                    .Include(x => x.Originadores)
                    .ThenInclude(x => x.Division)
                    .Include(x => x.Division)
                    .SingleOrDefaultAsync();
                if (vendedor is null) { return NotFound(); }

                var originadoresdto = vendedor.Originadores.Select(x => mapper.Map<CRMOriginador, CRMOriginadorDTO>(x)).ToList();

                var vendedordto = mapper.Map<CRMVendedorPostDTO>(vendedor);
                vendedordto.OriginadoresDTO = originadoresdto;

                return Ok(vendedordto);
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
                var vendedor = await context.CRMVendedores
                    .AsNoTracking()
                    .Include(x => x.Division)
                    .Include(x => x.Originadores)
                    .Where(x => x.Id == Id)
                    .Select(x => mapper.Map<CRMVendedor, CRMVendedorDetalleDTO>(x))
                    .SingleOrDefaultAsync();
                if (vendedor is null) { return NotFound(); }

                //var vendedordto = mapper.Map<CRMVendedorDTO>(vendedor);

                return Ok(vendedor);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CRMVendedorPostDTO dTO)
        {
            try
            {
                var validate = await validator.ValidateAsync(dTO);
                if (!validate.IsValid) { return BadRequest(validate.Errors); }

                var vendedor = mapper.Map<CRMVendedorPostDTO, CRMVendedor>(dTO);
                var originadores = dTO.OriginadoresDTO.Select(x => mapper.Map<CRMOriginadorDTO, CRMOriginador>(x)).ToList();

                vendedor.Originadores = originadores;

                if (vendedor.Id != 0)
                    context.Update(vendedor);
                else
                    await context.AddAsync(vendedor);

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
                var vendedor = await context.CRMVendedores.FindAsync(Id);
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

        [HttpDelete("relation")]
        public async Task<ActionResult> DeleteRelationVendedorOriginador([FromQuery] CRMVendedorOriginador dto)
        {
            try
            {
                if (dto is null) { return NotFound(); }
                context.Remove(dto);
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
