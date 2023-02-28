using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstacionController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public EstacionController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet("{cliente}")]
        public async Task<ActionResult> Get(int cliente)
        {
            try
            {
                var estaciones = await context.Destino.Where(x=>x.Codcte == cliente).ToListAsync();
                return Ok(estaciones);
            }
            catch (Exception e)
            {

                throw e;
            }
        }
    }
}
