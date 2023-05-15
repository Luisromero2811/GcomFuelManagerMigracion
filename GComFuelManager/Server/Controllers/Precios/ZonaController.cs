using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Server.Controllers.Precios
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZonaController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public ZonaController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            try
            {
                var zona = context.Zona.ToList();
                return Ok(zona);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("all")]
        public async Task<ActionResult> GetAll()
        {
            try
            {
                var zona = await context.ZonaCliente
                    .Include(x => x.Zona)
                    .Include(x => x.Cliente)
                    .Include(x => x.Destino)
                    .ToListAsync();
                return Ok(zona);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Zona zona)
        {
            try
            {
                if (zona == null)
                    return BadRequest();

                context.Add(zona);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("asignacion")]
        public async Task<ActionResult> PostZonaCliente([FromBody] ZonaCliente zonaCliente)
        {
            try
            {
                zonaCliente.Zona = null!;
                zonaCliente.Cliente = null!;
                zonaCliente.Destino = null!;

                if (zonaCliente == null)
                    return BadRequest();

                var relation = context.ZonaCliente.FirstOrDefault(x => x.Cod == zonaCliente.Cod);

                if (relation != null)
                    if (context.ZonaCliente.Any(x => x.CteCod == zonaCliente.CteCod && x.DesCod == zonaCliente.DesCod && x.ZonaCod == zonaCliente.ZonaCod))
                        return BadRequest("Ya existe esa relacion");

                if (relation == null)
                    context.Add(zonaCliente);
                else
                {
                    relation.DesCod = zonaCliente.DesCod;
                    relation.CteCod = zonaCliente.CteCod;
                    relation.ZonaCod = zonaCliente.ZonaCod;
                    context.Update(relation);
                }

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
