using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using GComFuelManager.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Newtonsoft.Json;
using System.Diagnostics;
using System.ServiceModel;
using ServiceReference6;
using GComFuelManager.Shared.Modelos;
using ServiceReference3;
using System.Text.Json;
using GComFuelManager.Server.Helpers;

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