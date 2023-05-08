using System;
using Microsoft.AspNetCore.Mvc;
using GComFuelManager.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Diagnostics;
using System.ServiceModel;
using ServiceReference1;

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
               
                client.ClientCredentials.UserName.UserName = "energasws";
                client.ClientCredentials.UserName.Password = "Energas23!";
                client.Endpoint.Binding.ReceiveTimeout = TimeSpan.FromMinutes(10);

                try
                {
                    var svc = client.ChannelFactory.CreateChannel();
                    
                    ServiceReference1.WsGetVehiclesRequest getReq = new ServiceReference1.WsGetVehiclesRequest();

                    //Pendientes

                    getReq.IncludeChildObjects = new ServiceReference1.NBool();
                    getReq.IncludeChildObjects.Value = true;

                    getReq.SystemId = new ServiceReference1.Identifier();
                    getReq.SystemId.Id = new ServiceReference1.NLong();
                    getReq.SystemId.Id.Value = 100;

                    getReq.VehicleType = new ServiceReference1.NInt();
                    getReq.VehicleType.Value = 4;

                    getReq.ActiveInd = new ServiceReference1.NEnumOfActiveIndicatorEnum();
                    getReq.ActiveInd.Value = ServiceReference1.ActiveIndicatorEnum.ACTIVE;

                    var respuesta = await svc.GetVehicles(getReq);

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

//BusinessEntityService.WsGetBusinessEntityAssociationsRequest be = new BusinessEntityService.WsGetBusinessEntityAssociationsRequest();