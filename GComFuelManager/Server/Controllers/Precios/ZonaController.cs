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
    }
}
