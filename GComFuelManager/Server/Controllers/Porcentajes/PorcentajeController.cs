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

        [HttpGet]
        [Route("{accion}")]
        public async Task<ActionResult> Get([FromRoute] string accion)
        {
            try
            {
                Porcentaje? porcentaje = new Porcentaje();

                if (string.IsNullOrEmpty(accion))
                    return BadRequest();

                var result = context.Porcentaje.FirstOrDefault(x => x.Accion == accion);
                
                if (result != null)
                    porcentaje = result;

                return Ok(porcentaje);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("{accion}")]
        public async Task<ActionResult> SendPercent([FromBody] Porcentaje porcentaje, [FromRoute] string accion)
        {
            try
            {
                if (string.IsNullOrEmpty(accion))
                    return BadRequest();

                if (porcentaje.Cod != null)
                    context.Update(porcentaje);
                else
                {
                    porcentaje.Accion = accion;
                    context.Add(porcentaje);
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

