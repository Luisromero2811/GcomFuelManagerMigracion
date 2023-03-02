using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Server.Controllers.AsignacionUnidadesController
{
    [Route("api/[controller]")]
    [ApiController]
    public class DestinoController : ControllerBase 
	{
        private readonly ApplicationDbContext context;

        public DestinoController(ApplicationDbContext context)
		{
            this.context = context;
        }
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            try
            {
                var destinos = await context.Destino.Where(x => x.Activo == true).ToListAsync();
                return Ok(destinos);
            }
            catch (Exception)
            {

                throw;
            }
        }
	}
}

