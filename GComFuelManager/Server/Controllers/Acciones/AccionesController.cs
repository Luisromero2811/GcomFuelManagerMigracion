using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GComFuelManager.Server.Controllers.Acciones
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccionesController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public AccionesController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            try
            {
                List<Accion> accions = new List<Accion>();
                
                accions = context.Accion.Where(x => x.Estatus == true).ToList();

                return Ok(accions);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] AccionCorreo accion)
        {
            try
            {
                if (accion.Cod is null)
                    context.Add(accion);
                else
                    context.Update(accion);

                await context.SaveChangesAsync();

                return Ok(accion);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
