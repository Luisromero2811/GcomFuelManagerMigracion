using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;

namespace GComFuelManager.Server.Controllers.Cierres
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CierreController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public CierreController(ApplicationDbContext context, UserManager<IdentityUsuario> userManager)
        {
            this.context = context;
            UserManager = userManager;
        }

        public UserManager<IdentityUsuario> UserManager { get; }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            try
            {
                var ordenes = context.OrdenCierre.AsEnumerable();
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("{folio}")]
        public async Task<ActionResult> GetByFolio([FromRoute] string folio)
        {
            try
            {
                var ordenes = await context.OrdenCierre.Where(x => x.Folio == folio && x.Estatus == true)
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x!.Destino)
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x!.Producto)
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x.Tad)
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x.Tonel)
                    .ToListAsync();
                return Ok(ordenes);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("{folio}/detalle")]
        public async Task<ActionResult> GetDetailByFolio([FromRoute] string folio)
        {
            try
            {
                var ordenes = await context.OrdenCierre.Where(x => x.Folio == folio && x.Estatus == true)
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x!.Destino)
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x!.Producto)
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x.Tad)
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x.Tonel)
                    .ToListAsync();

                foreach (var item in ordenes)
                {
                    if (item.OrdenEmbarque!.Folio != 0 && item.OrdenEmbarque!.Folio != null)
                    {
                        var o = await context.Orden.Where(y => y.Ref!.Contains(item.OrdenEmbarque!.Folio.ToString()!)).Include(x => x.Estado).FirstOrDefaultAsync();
                        if (o != null)
                            ordenes.FirstOrDefault(x=>x.Cod == item.Cod)!.OrdenEmbarque!.Orden = o;
                        else
                            ordenes.FirstOrDefault(x=>x.Cod == item.Cod)!.OrdenEmbarque!.Orden = null!;
                    }
                }
                return Ok(ordenes);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] OrdenCierre orden)
        {
            try
            {
                orden.Destino = await context.Destino.FirstOrDefaultAsync(x => x.Cod == orden.CodDes);
                orden.Producto = await context.Producto.FirstOrDefaultAsync(x => x.Cod == orden.CodPrd);
                orden.Cliente = await context.Cliente.FirstOrDefaultAsync(x => x.Cod == orden.CodCte);
                orden.ContactoN = await context.Contacto.FirstOrDefaultAsync(x => x.Cod == orden.CodCon);

                context.Add(orden);
                await context.SaveChangesAsync();

                return Ok(orden);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("update")]
        public async Task<ActionResult> PutOrden([FromBody] OrdenCierre orden)
        {
            try
            {
                orden.Producto = null;
                orden.Destino = null;
                orden.Cliente = null;
                orden.ContactoN = null!;
                orden.OrdenEmbarque = null!;

                context.Update(orden);
                await context.SaveChangesAsync();

                orden.Destino = await context.Destino.FirstOrDefaultAsync(x => x.Cod == orden.CodDes);
                orden.Producto = await context.Producto.FirstOrDefaultAsync(x => x.Cod == orden.CodPrd);
                orden.Cliente = await context.Cliente.FirstOrDefaultAsync(x => x.Cod == orden.CodCte);
                orden.ContactoN = await context.Contacto.FirstOrDefaultAsync(x => x.Cod == orden.CodCon);

                return Ok(orden);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("{cod:int}/cancel")]
        public async Task<ActionResult> CancelCierre([FromRoute] int cod)
        {
            try
            {
                var orden = await context.OrdenCierre.FirstOrDefaultAsync(x => x.Cod == cod);

                if (orden == null)
                {
                    return NotFound();
                }

                orden.Estatus = false;

                context.Update(orden);
                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("list")]
        public async Task<ActionResult> PostList(List<int> list)
        {
            try
            {
                List<OrdenCierre> ordenes = new List<OrdenCierre>();
                OrdenCierre? pedido = new OrdenCierre();

                foreach (var item in list)
                {
                    pedido = await context.OrdenCierre
                    .Where(x => x.Cod == item)
                    .Include(x => x.Destino)
                    .Include(x => x.Producto)
                    .Include(x => x.Cliente)
                    .Include(x => x.ContactoN)
                    .FirstOrDefaultAsync();
                    ordenes.Add(pedido!);
                }
                return Ok(ordenes);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("filtrar")]
        public async Task<ActionResult> PostFilter([FromBody] CierreFiltroDTO filtroDTO)
        {
            try
            {
                List<OrdenCierre> cierres = new List<OrdenCierre>();
                if (!filtroDTO.forFolio)
                {
                    cierres = await context.OrdenCierre.Where(x => x.CodCte == filtroDTO.codCte
                    && x.FchCierre >= filtroDTO.FchInicio && x.FchCierre <= filtroDTO.FchFin && x.Estatus == true)
                        .Include(x => x.Cliente)
                        .Include(x => x.Producto)
                        .Include(x => x.Destino)
                        .Include(x => x.ContactoN)
                        .ToListAsync();
                }
                else
                {
                    cierres = await context.OrdenCierre.Where(x => x.Folio == filtroDTO.Folio && x.Estatus == true)
                        .Include(x => x.Cliente)
                        .Include(x => x.Producto)
                        .Include(x => x.Destino)
                        .Include(x => x.ContactoN)
                        .ToListAsync();
                }

                return Ok(cierres);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("years")]
        public async Task<ActionResult> GetYears()
        {
            try
            {
                var years = context.OrdenCierre.Select(x => x.FchCierre!.Value.Year).Distinct().ToList();
                return Ok(years);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
