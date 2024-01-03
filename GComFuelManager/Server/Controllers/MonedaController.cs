using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MonedaController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public MonedaController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public ActionResult Index()
        {
            try
            {
                List<Moneda> monedas = new List<Moneda>();

                monedas = context.Moneda.Where(x => x.Estatus).ToList();

                return Ok(monedas);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("completas")]
        public ActionResult Index_All()
        {
            try
            {
                List<Moneda> monedas = new List<Moneda>();

                monedas = context.Moneda.ToList();

                return Ok(monedas);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost()]
        public async Task<ActionResult> Guardar_Moneda([FromBody] Moneda moneda)
        {
            try
            {
                context.Add(moneda);
                await context.SaveChangesAsync();
                return Ok(moneda);
            }
            catch (ArgumentNullException)
            {
                return BadRequest("No se admiten valores vacios");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("{id_moneda}")]
        public async Task<ActionResult> Eliminar_Moneda([FromRoute] int id_moneda)
        {
            try
            {
                var moneda = context.Moneda.FirstOrDefault(x => x.Id == id_moneda);

                if (moneda is null)
                    return BadRequest("No se encontro la moneda");

                moneda.Estatus = !moneda.Estatus;

                context.Update(moneda);
                await context.SaveChangesAsync();

                return Ok(moneda);
            }
            catch (ArgumentNullException)
            {
                return BadRequest("No se admiten valores vacios");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
