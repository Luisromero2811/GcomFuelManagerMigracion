using GComFuelManager.Shared.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TerminalController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public TerminalController(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<ActionResult> Get()
        {
            try
            {
                var terminales = await context.Tad
                .Where(x => x.Activo == true)
                //.Select(x => new CodDenDTO { Cod = x.Cod, Den = x.Den! })
                .ToListAsync();
                return Ok(terminales);
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }
    }
}
