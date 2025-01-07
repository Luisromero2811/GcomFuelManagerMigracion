using GComFuelManager.Server.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Server.Controllers.AsignacionUnidadesController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TransportistaController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly RequestToFile toFile;

        public TransportistaController(ApplicationDbContext context, RequestToFile toFile)
        {
            this.context = context;
            this.toFile = toFile;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            try
            {
                var transportistas = await context.Transportista.Where(x => x.Activo == true && x.Gru != null)
                    .OrderBy(x => x.Den)
                    .ToListAsync();
                return Ok(transportistas);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }

}