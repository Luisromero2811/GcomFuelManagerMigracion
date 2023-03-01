using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CantidadController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public CantidadController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            try
            {
                var cantidades = await context.Tonel
                    .Where(x => x.Capcom != null && x.Capcom > 10000 && x.Capcom < 268349)
                    .Select(x => x.Capcom)
                    .Distinct()
                    .ToListAsync();
                return Ok(cantidades);
            }
            catch (Exception e)
            {

                throw e;
            }
        }
    }
}
