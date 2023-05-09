using System;
using Microsoft.AspNetCore.Mvc;
using GComFuelManager.Shared.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Diagnostics;
using System.ServiceModel;
using ServiceReference6;
using GComFuelManager.Shared.Modelos;
using Newtonsoft.Json;

namespace GComFuelManager.Server.Controllers.AsignacionUnidadesController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
       
        [Route("service")]
        [HttpGet]
        public async Task<ActionResult> GetChofer()
        {
            Transportista tr = new Transportista();
            try
            {
                BusinessEntityServiceClient client = new BusinessEntityServiceClient(BusinessEntityServiceClient.EndpointConfiguration.BasicHttpBinding_BusinessEntityService);
                client.ClientCredentials.UserName.UserName = "energasws";
                client.ClientCredentials.UserName.Password = "Energas23!";
                client.Endpoint.Binding.ReceiveTimeout = TimeSpan.FromMinutes(10);

                try
                {
                    var svc = client.ChannelFactory.CreateChannel();

                    //ServiceReference6.WsGetBusinessEntityAssociationsRequest getReq = new WsGetBusinessEntityAssociationsRequest();

                    WsGetBusinessEntityAssociationsRequest getReq = new WsGetBusinessEntityAssociationsRequest();

                    getReq.IncludeChildObjects = new ServiceReference6.NBool();
                    getReq.IncludeChildObjects.Value = false;

                    getReq.BusinessEntityType = new ServiceReference6.NInt();
                    getReq.BusinessEntityType.Value = 8192;

                    getReq.AssociatedBusinessEntityType = new ServiceReference6.NInt();
                    getReq.AssociatedBusinessEntityType.Value = 8;

                    getReq.ActiveIndicator = new ServiceReference6.NEnumOfActiveIndicatorEnum();
                    getReq.ActiveIndicator.Value = ServiceReference6.ActiveIndicatorEnum.ACTIVE;

                    getReq.AssociatedBusinessEntityId = new ServiceReference6.Identifier();
                    getReq.AssociatedBusinessEntityId.Id = new ServiceReference6.NLong();
                    getReq.AssociatedBusinessEntityId.Id.Value = tr.Busentid.LongCount();

                    var respuesta = await svc.GetBusinessEntityAssociationsAsync(getReq);

                    Debug.WriteLine(JsonConvert.SerializeObject(respuesta));

                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }
                finally
                {
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

