using GComFuelManager.Shared.DTOs;
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

        [HttpGet("{cliente:int}")]
        public async Task<ActionResult> Get(int cliente)
        {
            try
            {
                var estaciones = await context.Destino
                    .Where(x => x.Codcte == cliente)
                    .Select(x => new CodDenDTO { Cod = x.Cod, Den = x.Den! })
                    .OrderBy(x => x.Den)
                    .ToListAsync();
                return Ok(estaciones);
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }
    }
}
