using System;
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

namespace GComFuelManager.Server.Controllers.AsignacionUnidadesController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class TransportistaController : ControllerBase 
	{
        private readonly ApplicationDbContext context;

        public TransportistaController(ApplicationDbContext context)
		{
            this.context = context;
        }
        
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            try
            {
                var transportistas = await context.Transportista.Where(x => x.activo == true && x.gru != null)
                    //.Select(x => new CodDenDTO { Cod = Convert.ToInt32(x.busentid), Den = x.den!})
                    .OrderBy(x => x.den)
                    .ToListAsync();
                return Ok(transportistas);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Route("service")]
        [HttpGet]
        public async Task<ActionResult> GetTransportistasService()
        {
            try
            {
                //ChannelFactory<ServiceReference6.BusinessEntityServiceChannel> svcTruck = new ChannelFactory<ServiceReference6.BusinessEntityServiceChannel>("BasicHttpBinding_BusinessEntityService");
                //svcTruck.Credentials.UserName.UserName = "energasws";
                //svcTruck.Credentials.UserName.Password = "Energas23!";
                
                BusinessEntityServiceClient client = new BusinessEntityServiceClient(BusinessEntityServiceClient.EndpointConfiguration.BasicHttpBinding_BusinessEntityService);
                client.ClientCredentials.UserName.UserName = "energasws";
                client.ClientCredentials.UserName.Password = "Energas23!";
                client.Endpoint.Binding.ReceiveTimeout = TimeSpan.FromMinutes(10);

                try
                {
                    //ServiceReference6.BusinessEntityServiceChannel svc = svcTruck.CreateChannel();
                    var svc = client.ChannelFactory.CreateChannel();

                    ServiceReference6.WsGetBusinessEntityAssociationsRequest getReq = new ServiceReference6.WsGetBusinessEntityAssociationsRequest();

                    getReq.IncludeChildObjects = new ServiceReference6.NBool();
                    getReq.IncludeChildObjects.Value = true;

                    getReq.BusinessEntityType = new ServiceReference6.NInt();
                    getReq.BusinessEntityType.Value = 8;//truck carrier

                    getReq.AssociatedBusinessEntityId = new ServiceReference6.Identifier();
                    getReq.AssociatedBusinessEntityId.Id = new ServiceReference6.NLong();
                    getReq.AssociatedBusinessEntityId.Id.Value = 51004;//energas

                    getReq.AssociatedBusinessEntityType = new ServiceReference6.NInt();
                    getReq.AssociatedBusinessEntityType.Value = 1;

                    var respuesta = await svc.GetBusinessEntityAssociationsAsync(getReq);

                    Debug.WriteLine(JsonConvert.SerializeObject(respuesta.BusinessEntityAssociations));

                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }
                finally
                {
                    //svcTruck.Close();
                    client.Close();
                }

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }

}