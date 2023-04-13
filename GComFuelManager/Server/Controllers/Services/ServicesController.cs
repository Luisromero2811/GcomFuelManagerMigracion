using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceReference4;
using System.ServiceModel;

namespace GComFuelManager.Server.Controllers.Services
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly BusinessEntityService businessEntity;

        public ServicesController(ApplicationDbContext context, BusinessEntityService businessEntity)
        {
            this.context = context;
            this.businessEntity = businessEntity;
        }

        [Route("transportistas")]
        [HttpGet]
        public async Task<ActionResult> GetTransportistas()
        {
            try
            {
                WsGetBusinessEntityAssociationsRequest be = new WsGetBusinessEntityAssociationsRequest();

                be.IncludeChildObjects = new ServiceReference4.NBool();

                var response = businessEntity.GetBusinessEntityAssociationsAsync(be);

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
