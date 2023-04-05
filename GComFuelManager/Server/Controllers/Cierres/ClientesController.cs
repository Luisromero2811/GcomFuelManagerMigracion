using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Server.Controllers.Cierres
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ClientesController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public ClientesController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            try
            {
                var clientes = context.Cliente.AsEnumerable().Select(x => new CodDenDTO { Cod = x.Cod, Den = x.Den! }).OrderBy(x => x.Den);
                return Ok(clientes);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("grupo/{cod:int}")]
        public async Task<ActionResult> Get(int cod)
        {
            try
            {
                var clientes = context.Cliente.Where(x=>x.Codgru == cod).AsEnumerable().OrderBy(x => x.Den);
                return Ok(clientes);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("asignar")]
        public async Task<ActionResult> PostAsignar([FromBody]ClienteGrupoAsignacionDTO asignacionDTO)
        {
            try
            {
                var cliente = await context.Cliente.FirstOrDefaultAsync(x => x.Cod == asignacionDTO.Cliente);

                if (cliente == null)
                {
                    return NotFound();
                }

                cliente.Grupo = null;

                cliente.Codgru = asignacionDTO.Grupo;

                context.Update(cliente);
                await context.SaveChangesAsync();
                
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
