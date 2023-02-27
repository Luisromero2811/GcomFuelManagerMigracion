using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClienteController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public ClienteController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet("{grupo:int}")]
        public async Task<ActionResult> Get(int grupo)
        {
            try
            {
                var grupos = await context.Cliente.Where(x => x.Codgru == grupo).ToListAsync();
                return Ok(grupos);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
