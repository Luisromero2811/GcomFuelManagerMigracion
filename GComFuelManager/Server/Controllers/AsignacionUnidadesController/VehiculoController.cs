using System;
using Microsoft.AspNetCore.Mvc;
using GComFuelManager.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Diagnostics;
using System.ServiceModel;
using ServiceReference6;

namespace GComFuelManager.Server.Controllers.AsignacionUnidadesController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class VehiculoController : ControllerBase
	{
        private readonly ApplicationDbContext context;

        public VehiculoController(ApplicationDbContext context)
		{
            this.context = context;
        }
        [HttpGet("{transportista:int}")]
        public async Task<ActionResult> Get(int transportista)
        {
            try
            {
                var vehiculos = await context.Tonel
                    .Where(x => Convert.ToInt32(x.Carid) == transportista && x.Activo == true)
                    //.Select(x => new CodDenDTO { Cod = x.Cod, Den =  $"{x.Tracto} {x.Placatracto} {x.Placa} {x.Capcom!} {x.Capcom2!} {x.Capcom3!} {x.Capcom4!} {x.Codsyn!}" })
                    .OrderBy(x => x.Placa)
                    .ToListAsync();
                return Ok(vehiculos);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Route("service")]
        [HttpGet]
        public async Task<ActionResult> GetVehiculo()
        {
            try
            {
                BusinessEntityServiceClient client = new BusinessEntityServiceClient(BusinessEntityServiceClient.EndpointConfiguration.BasicHttpBinding_BusinessEntityService);
                client.ClientCredentials.UserName.UserName = "energasws";
                client.ClientCredentials.UserName.Password = "Energas23!";
                client.Endpoint.Binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
                try
                {

                }
                catch (Exception e)
                {
                    return BadRequest();
                }
                finally
                {
                    client.Close();
                }
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

    }
}

//+ x.Tracto + x.Placatracto
//+ x.Placa + x.Capcom + " " + x.Capcom2 + " " + x.Capcom3 + " " + x.Capcom4 + " " + x.Codsyn