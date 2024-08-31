using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Server.Controllers.CRM
{
    [Route("api/[controller]")]
    [ApiController]
    public class CRMDivisionController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public CRMDivisionController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            try
            {
                var divisiones = await context.CRMDivisiones.Where(x => x.Activo).ToListAsync();
                return Ok(divisiones);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
