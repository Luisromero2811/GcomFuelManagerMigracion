using System;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
using System.ServiceModel;
using System.Drawing;


namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class PorcentajeController : ControllerBase
	{
        private readonly ApplicationDbContext context;

        public PorcentajeController(ApplicationDbContext context)
		{
            this.context = context;
        }

        [HttpPost]
        public async Task<ActionResult> SendPercent([FromBody] Porcentaje porcentaje)
        {
            try
            {
                if (porcentaje.Cod == null)
                {
                    context.Add(porcentaje);
                }
                else
                {
                    context.Update(porcentaje);
                }

                await context.SaveChangesAsync();

                return Ok(porcentaje);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

	}
}

