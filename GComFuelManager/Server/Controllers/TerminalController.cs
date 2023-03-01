using GComFuelManager.Shared.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TerminalController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public TerminalController(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<ActionResult> Get()
        {
            var terminales = await context.Tad
                .Where(x => x.activo == true)
                .Select(x => new CodDenDTO { Cod = x.cod, Den = x.den! })
                .ToListAsync();
            return Ok(terminales);
        }
    }
}
