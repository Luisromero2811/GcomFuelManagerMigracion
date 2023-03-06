using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Linq;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidoController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public PedidoController(ApplicationDbContext context)
        {
            this.context = context;
        }
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            try
            {
                var pedidos = await context.OrdenEmbarque
                    .Where(x => x.Codusu == 1)
                    .Include(x => x.Destino)
                    .ToListAsync();
                return Ok(pedidos);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        [HttpGet("pedido/{Date1}/{Date2}")]

        [HttpGet]

        public async Task<ActionResult> GetDate(DateTime Date1, DateTime Date2)
        {
            //{Date1}/{Date2}
            try
            {
                var pedidosDate = await context.OrdenEmbarque
                    .Where(x => Date1 >= x.Fchpet &&  Date2 <= x.Fchpet)
                    .ToListAsync();
                    return Ok(pedidosDate);
            }
            catch(Exception e)
            {

                return BadRequest(e.Message);
            }
        }


        [HttpPost]
        public async Task<ActionResult> Post(OrdenEmbarque orden)
        {
            try
            {
                var bin = await context.OrdenEmbarque.Select(x => x.Bin).OrderBy(x => x).LastOrDefaultAsync();
                if (bin.HasValue)
                {
                    orden.Bin = bin;
                }
                else
                {
                    return BadRequest();
                }

                orden.Codusu = 1;
                orden.Destino = await context.Destino.FirstOrDefaultAsync(x => x.Cod == orden.Coddes);
                orden.Tad = await context.Tad.FirstOrDefaultAsync(x => x.Cod == orden.Codtad);
                orden.Producto = await context.Producto.FirstOrDefaultAsync(x => x.Cod == orden.Codprd);
                orden.Tonel = await context.Tonel.FirstOrDefaultAsync(x => x.Cod == orden.Codton);
                
                context.Add(orden);
                await context.SaveChangesAsync();
                return Ok(orden);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
