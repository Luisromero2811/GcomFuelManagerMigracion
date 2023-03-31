using System;
using Microsoft.AspNetCore.Mvc;
using GComFuelManager.Shared.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Server.Controllers.AsignacionUnidadesController
{
    [Route("api/[controller]")]
    [ApiController]
	public class ChoferController : ControllerBase 
	{
        private readonly ApplicationDbContext context;

        public ChoferController(ApplicationDbContext context)
		{
            this.context = context;
        }
        [HttpGet("{transportista:int}")]
        public async Task<ActionResult> Get(int transportista)
        {
            try
            {
                var transportistas = await context.Chofer
                    .Where(x => x.Codtransport == transportista && x.Activo == true)
                    .Select(x => new CodDenDTO { Cod = x.Cod, Den = x.Den! })
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

