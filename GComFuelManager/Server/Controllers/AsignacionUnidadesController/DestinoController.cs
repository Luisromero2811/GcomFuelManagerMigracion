using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using GComFuelManager.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace GComFuelManager.Server.Controllers.AsignacionUnidadesController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
                var destinos = await context.Destino
                    .Where(x => x.Activo == true)
                    .Select(x => new CodDenDTO { Cod = x.Cod, Den = x.Den })
                    .ToListAsync();
                return Ok(destinos);
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }
    }
}

