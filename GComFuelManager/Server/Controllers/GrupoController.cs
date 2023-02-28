using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GrupoController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public GrupoController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            try
            {
                var grupos = await context.Grupo.ToListAsync();
                return Ok(grupos);
            }
            catch (Exception e)
            {

                throw e;
            }
        }
    }
}
