using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
    }
}
