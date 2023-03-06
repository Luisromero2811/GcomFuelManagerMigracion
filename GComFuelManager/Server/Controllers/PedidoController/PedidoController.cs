using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Globalization;
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
                    .Include(x=>x.Destino)
                    .Include(x=>x.Tad)
                    .Include(x=>x.Producto)
                    .Include(x=>x.Tonel)
                    .Take(10000)
                    .ToListAsync();
                return Ok(pedidos);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        [HttpPost("filtrar")]
        public async Task<ActionResult> GetDate([FromBody] FechasF fechas)
        {
            try
            {
                var pedidosDate = await context.OrdenEmbarque
                    .Where(x => x.Fchpet >= fechas.DateInicio && x.Fchpet <= fechas.DateFin)
                    .Include(x => x.Destino)
                    .Include(x => x.Tad)
                    .Include(x => x.Producto)
                    .Include(x => x.Tonel)
                    .OrderBy(x=>x.Fchpet)
                    .Take(10000)
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
                    orden.Bin = bin + 1;
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

        [HttpPut("confirm")]
        public async Task<ActionResult> PutPedido(List<OrdenEmbarque> orden)
        {
            try
            {
                OrdenCompra newFolio = new OrdenCompra();
                var folio = await context.OrdenCompra.Select(x=>x.cod).OrderBy(x=>x).LastOrDefaultAsync();
                if (folio != 0)
                {
                     newFolio = new() { cod = ++folio, den = $"ENER_{DateTime.Now:yyyy-MM-dd}_{folio}" };
                    context.Add(newFolio);
                }
                orden.ForEach(x =>
                {
                    x.Codest = 3;
                    x.CodordCom = folio;
                    x.FchOrd = DateTime.Now.Date;

                });
                context.AddRange(orden);
                await context.SaveChangesAsync();
                return Ok(newFolio.den);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
