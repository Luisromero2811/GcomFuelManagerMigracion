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
using ServiceReference6;

namespace GComFuelManager.Server.Controllers.Cierres
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ClientesController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public ClientesController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            try
            {
                var clientes = context.Cliente.AsEnumerable().Select(x => new CodDenDTO { Cod = x.Cod, Den = x.Den! }).OrderBy(x => x.Den);
                return Ok(clientes);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("{cod:int}")]
        public async Task<ActionResult> GetByCod([FromRoute]int cod)
        {
            try
            {
                var clientes = context.Cliente.Find(cod);
                return Ok(clientes);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("all")]
        public async Task<ActionResult> GetAll()
        {
            try
            {
                var clientes = context.Cliente.AsEnumerable().OrderBy(x => x.Den);
                return Ok(clientes);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("grupo/{cod:int}")]
        public async Task<ActionResult> Get(int cod)
        {
            try
            {
                var clientes = context.Cliente.Where(x=>x.Codgru == cod).AsEnumerable().OrderBy(x => x.Den);
                return Ok(clientes);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("asignar/{cod:int}")]
        public async Task<ActionResult> PostAsignar([FromBody]CodDenDTO codden, [FromRoute] int cod)
        {
            try
            {
                var destino = await context.Destino.FirstOrDefaultAsync(x => x.Cod == codden.Cod);

                if (destino == null)
                {
                    return NotFound();
                }

                destino.Codcte = cod;
                context.Update(destino);
                await context.SaveChangesAsync();
                
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut]
        public async Task<ActionResult> PutCliente([FromBody] Cliente cliente)
        {
            try
            {
                if (cliente == null)
                {
                    return BadRequest();
                }
                
                cliente.Grupo = null;

                context.Update(cliente);
                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("folio/{cod:int}")]
        public async Task<ActionResult> GetFolio([FromRoute]int cod)
        {
            try
            {
                var cliente = await context.Cliente.FindAsync(cod);
                if (cliente == null)
                    return NotFound();

                cliente.Consecutivo = cliente.Consecutivo != null ? cliente.Consecutivo + 1 : 1;

                var folio = cliente.CodCte != null ? cliente.CodCte + Convert.ToString(cliente.Consecutivo) : string.Empty;

                cliente.Grupo = null!;

                context.Update(cliente);
                await context.SaveChangesAsync();

                return Ok(folio);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Route("service")]
        [HttpGet]
        public async Task<ActionResult> GetClienteService()
        {
            try
            {
                BusinessEntityServiceClient client = new BusinessEntityServiceClient(BusinessEntityServiceClient.EndpointConfiguration.BasicHttpBinding_BusinessEntityService);
                client.ClientCredentials.UserName.UserName = "energasws";
                client.ClientCredentials.UserName.Password = "Energas23!";
                client.Endpoint.Binding.ReceiveTimeout = TimeSpan.FromMinutes(10);

                try
                {
                    var svc = client.ChannelFactory.CreateChannel();

                    ServiceReference6.WsGetBusinessEntityAssociationsRequest getReq = new ServiceReference6.WsGetBusinessEntityAssociationsRequest();

                    getReq.IncludeChildObjects = new ServiceReference6.NBool();
                    getReq.IncludeChildObjects.Value = true;

                    getReq.BusinessEntityType = new ServiceReference6.NInt();
                    getReq.BusinessEntityType.Value = 4;

                    getReq.AssociatedBusinessEntityId = new ServiceReference6.Identifier();
                    getReq.AssociatedBusinessEntityId.Id = new ServiceReference6.NLong();
                    getReq.AssociatedBusinessEntityId.Id.Value = 51004;

                    getReq.AssociatedBusinessEntityType = new ServiceReference6.NInt();
                    getReq.AssociatedBusinessEntityType.Value = 1;

                    getReq.ActiveIndicator = new ServiceReference6.NEnumOfActiveIndicatorEnum();
                    getReq.ActiveIndicator.Value = ServiceReference6.ActiveIndicatorEnum.ACTIVE;

                    var respuesta = await svc.GetBusinessEntityAssociationsAsync(getReq);

                    Debug.WriteLine(JsonConvert.SerializeObject(respuesta.BusinessEntityAssociations));
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
