using AutoMapper;
using FluentValidation;
using GComFuelManager.Shared.DTOs.CRM;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Server.Controllers.CRM
{
    [Route("api/[controller]")]
    [ApiController]
    public class CRMContactosController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IValidator<CRMContactoPostDTO> validator;
        private readonly IMapper mapper;

        public CRMContactosController(ApplicationDbContext context, IValidator<CRMContactoPostDTO> validator, IMapper mapper)
        {
            this.context = context;
            this.validator = validator;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] CRMContacto contacto)
        {
            try
            {
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CRMContactoPostDTO contactodto)
        {
            try
            {
                var result = await validator.ValidateAsync(contactodto);
                if (!result.IsValid) { return BadRequest(result.Errors); }
                var contacto = mapper.Map<CRMContactoPostDTO, CRMContacto>(contactodto);
                await context.AddAsync(contacto);
                await context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/status")]
        public async Task<ActionResult> ObtenerCatalogoStatus()
        {
            try
            {
                var catalogo = await context.Accion.FirstOrDefaultAsync(x => x.Nombre.Equals("Catalogo_Contacto_Status"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para conjuntos"); }

                var catalogo_fijo = await context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo.Cod)).ToListAsync();

                return Ok(catalogo_fijo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/origen")]
        public async Task<ActionResult> ObtenerCatalogoOrigen()
        {
            try
            {
                var catalogo = await context.Accion.FirstOrDefaultAsync(x => x.Nombre.Equals("Catalogo_Contacto_Origen"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para conjuntos"); }

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
