using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
                    .Include(x => x.Destino)
                    .Include(x => x.Tad)
                    .Include(x => x.Producto)
                    .Include(x => x.Tonel)
                    .Take(10000)
                    .ToListAsync();
                return Ok(pedidos);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        //Realización de pedido
        [HttpPost("list")]
        public async Task<ActionResult> PostList(List<int> list)
        {
            try
            {
                List<OrdenEmbarque> ordenes = new List<OrdenEmbarque>();
                OrdenEmbarque? pedido = new OrdenEmbarque();

                foreach (var item in list)
                {
                    pedido = await context.OrdenEmbarque
                    .Where(x => x.Cod == item)
                    .Include(x => x.Destino)
                    .Include(x => x.Tad)
                    .Include(x => x.Producto)
                    .FirstOrDefaultAsync();
                    ordenes.Add(pedido);
                }
                return Ok(ordenes);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        //Borrar pedido
        [HttpDelete("{cod:int}/cancel")]
        public async Task<ActionResult> PutCancel([FromRoute] int cod)
        {
            try
            {
                OrdenEmbarque? pedido = await context.OrdenEmbarque.FirstOrDefaultAsync(x => x.Cod == cod);

                if (pedido is null)
                {
                    return NotFound();
                }

                pedido.Codest = 14;
                context.Update(pedido);
                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        //Method para obtener pedidos mediante un rango de fechas
        [HttpPost("filtrar")]
        public async Task<ActionResult> GetDate([FromBody] FechasF fechas)
        {
            try
            {
                var v = 1;
                List<OrdenEmbarque> ordens = new List<OrdenEmbarque>();
                var ordenTpAsig = await context.OrdenEmbarque
                    .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Codest == 3 && x.Tp == true && x.FchOrd != null
                    && x.Bolguidid == null && x.Tonel.Transportista.activo == true)
                    .Include(x => x.Chofer)
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Estado)
                    .Include(x => x.OrdenCompra)
                    .Include(x => x.Tad)
                    .Include(x => x.Producto)
                    .Include(x => x.Tonel)
                    .ThenInclude(x => x.Transportista)
                    .OrderBy(x => x.Fchpet)
                    .Take(10000)
                    .ToListAsync();

                ordens.AddRange(ordenTpAsig);

                var ordenTpNoAsign = await context.OrdenEmbarque
                    .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Codest == 3 && x.Tp == true && x.FchOrd != null
                    && x.Codton == null && x.CompartmentId == null && x.Compartment == null)
                    .Include(x => x.Chofer)
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Estado)
                    .Include(x => x.OrdenCompra)
                    .Include(x => x.Tad)
                    .Include(x => x.Producto)
                    .Include(x => x.Tonel)
                    .ThenInclude(x => x.Transportista)
                    .OrderBy(x => x.Fchpet)
                    .Take(10000)
                    .ToListAsync();

                ordens.AddRange(ordenTpNoAsign);

                if (v == 1)
                {
                    var ordenNoTpNoEnv = await context.OrdenEmbarque
                    .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Codest == 3 && (x.Tp == null || x.Tp == false)
                    && x.FchOrd != null && x.Bolguidid == null)
                    .Include(x => x.Chofer)
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Estado)
                    .Include(x => x.OrdenCompra)
                    .Include(x => x.Tad)
                    .Include(x => x.Producto)
                    .Include(x => x.Tonel)
                    .ThenInclude(x => x.Transportista)
                    .OrderBy(x => x.Fchpet)
                    .Take(10000)
                    .ToListAsync();

                    ordens.AddRange(ordenNoTpNoEnv);

                    //var ordenNoTpNoAsig = await context.OrdenEmbarque
                    //.Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Codest == 3 && (x.Tp == null || x.Tp == false)
                    //&& x.FchOrd != null && x.Codton == null && x.CompartmentId == null && x.Compartment == null)
                    //.Include(x => x.Chofer)
                    //.Include(x => x.Destino)
                    //.ThenInclude(x => x.Cliente)
                    //.Include(x => x.Estado)
                    //.Include(x => x.OrdenCompra)
                    //.Include(x => x.Tad)
                    //.Include(x => x.Producto)
                    //.Include(x => x.Tonel)
                    //.ThenInclude(x => x.Transportista)
                    //.OrderBy(x => x.Fchpet)
                    //.Take(10000)
                    //.ToListAsync();

                    //ordens.AddRange(ordenNoTpNoAsig);
                }

                ordens.OrderBy(x => x.Bin);

                return Ok(ordens);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        //Method para obtener pedidos mediante rango de fechas y checbox seleccionado
        //REALIZAR TRES CONDICIONES, UNA POR CADA RADIOBUTTON, QUE SEA EN TRUE SE HARÁ EL MISMO FILTRO POR FECHAS EN WHERE AÑADIENDO LOS CAMPOS QUE UTILIZAN CADA CLAUSULA
        [HttpPost("filtro")]
        public async Task<ActionResult> GetDateRadio([FromBody] FechasF fechas)
        {
            try
            {
                if (fechas.Estado == 1)
                {
                    //Traerme al bolguid is not null, codest =3 y transportista activo en 1
                    var pedidosDate = await context.OrdenEmbarque
                    .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Bolguidid != null && x.FchOrd != null && x.Codest == 3 && x.Tonel!.Transportista.activo == true)
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Tad)
                    .Include(x => x.Producto)
                    .Include(x => x.Tonel)
                    .ThenInclude(x => x.Transportista)
                    .Include(x => x.Chofer)
                    .Include(x => x.Estado)
                    //.Select(x => new OrdenesDTO() { Referencia = x.Folio })
                    .Select(o => new Orden()
                    {
                        Cod = o.Cod,
                        Ref = "ENER-" + o.Folio.ToString(),
                        //Ref = o.ref
                        Fchcar = o.Fchcar,
                        Estado = o.Estado,
                        Destino = o.Destino,
                        Producto = o.Producto,
                        Vol2 = o.Vol,
                        Vol = null!,
                        Bolguiid = null!,
                        BatchId = null!,
                        Tonel = o.Tonel,
                        Chofer = o.Chofer
                    })
                    .OrderBy(x => x.Fchcar)
                    .Take(10000)
                    .ToListAsync();
                    return Ok(pedidosDate);
                }
                else if (fechas.Estado == 2)
                {
                    //Traerme al transportista activo en 1 y codest = 26
                    var pedidosDate = await context.Orden
                    .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Tonel!.Transportista.activo == true && x.Codest == 20)
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Estado)
                    .Include(x => x.Producto)
                    .Include(x => x.Chofer)
                    .Include(x => x.Tonel)
                    .ThenInclude(x => x.Transportista)
                    .OrderBy(x => x.Fchcar)
                    .Take(10000)
                    .ToListAsync();
                    return Ok(pedidosDate);
                }
                else if (fechas.Estado == 3)
                {
                    //Traerme al transportista activo en 1
                    var pedidosDate = await context.Orden
                    .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Tonel!.Transportista.activo == true && x.Codest == 26)
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Producto)
                    .Include(x => x.Tonel)
                    .ThenInclude(x => x.Transportista)
                    .Include(x => x.Estado)
                    .Include(x => x.Chofer)
                    .OrderBy(x => x.Fchcar)
                    .Take(10000)
                    .ToListAsync();
                    return Ok(pedidosDate);
                }
                else
                {
                    return BadRequest();
                }

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("trafico")]
        public async Task<ActionResult> GetTraffic([FromBody] FechasF fechas)
        {
            try
            {

                var pedidosDate = await context.Orden
                  .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Tonel!.Transportista.activo == true && x.Codprd2 != 10157)
                  .Include(x => x.Destino)
                  .ThenInclude(x => x.Cliente)
                  .Include(x => x.Producto)
                  .Include(x => x.Tonel)
                  .ThenInclude(x => x.Transportista)
                  .Include(x => x.Estado)
                  .Include(x => x.Chofer)
                  .OrderBy(x => x.Fchcar)
                  .Take(10000)
                  .ToListAsync();
                return Ok(pedidosDate);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
           
        }
        [HttpPost("Historial")]
        public async Task<ActionResult> GetHistorial([FromBody] FechasF fechas)
        {

            try
            {
                List<Orden> Ordenes = new List<Orden>();
                    var pedidosDate = await context.OrdenEmbarque
                    .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Bolguidid != null && x.FchOrd != null && x.Codest == 3 && x.Tonel!.Transportista.activo == true)
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Tad)
                    .Include(x => x.Producto)
                    .Include(x => x.Tonel)
                    .ThenInclude(x => x.Transportista)
                    .Include(x => x.Chofer)
                    .Include(x => x.Estado)
                    .Select(o => new Orden()
                    {
                        Cod = o.Cod,
                        Ref = "ENER-" + o.Folio.ToString(),
                        Fchcar = o.Fchcar,
                        Estado = o.Estado,
                        Destino = o.Destino,
                        Producto = o.Producto,
                        Vol2 = o.Vol,
                        Vol = null!,
                        Bolguiid = null!,
                        BatchId = null!,
                        Tonel = o.Tonel,
                        Chofer = o.Chofer
                    })
                    .OrderBy(x => x.Fchcar)
                    .Take(10000)
                    .ToListAsync();
                Ordenes.AddRange(pedidosDate);
                var pedidosDate2 = await context.Orden
                .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Tonel!.Transportista.activo == true && x.Codest == 20)
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Estado)
                    .Include(x => x.Producto)
                    .Include(x => x.Chofer)
                    .Include(x => x.Tonel)
                    .ThenInclude(x => x.Transportista)
                    .OrderBy(x => x.Fchcar)
                    .Take(10000)
                    .ToListAsync();
                Ordenes.AddRange(pedidosDate2);

                var pedidosDate3 = await context.Orden
                .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Tonel!.Transportista.activo == true && x.Codest == 26)
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Producto)
                    .Include(x => x.Tonel)
                    .ThenInclude(x => x.Transportista)
                    .Include(x => x.Estado)
                    .Include(x => x.Chofer)
                    .OrderBy(x => x.Fchcar)
                    .Take(10000)
                    .ToListAsync();
                Ordenes.AddRange(pedidosDate3);
                    return Ok(Ordenes);

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        //Method para realizar (agregar) pedido
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
        //Confirmar pedido
        [HttpPost("confirm")]
        public async Task<ActionResult<OrdenCompra>> PostConfirm(List<OrdenEmbarque> orden)
        {
            try
            {
                OrdenCompra newFolio = new OrdenCompra();
                var folio = await context.OrdenCompra.Select(x => x.cod).OrderBy(x => x).LastOrDefaultAsync();
                if (folio != 0)
                {
                    ++folio;
                    newFolio = new OrdenCompra() { den = $"ENER_{DateTime.Now:yyyy-MM-dd}_{folio}" };
                    context.Add(newFolio);
                }
                orden.ForEach(x =>
                {
                    x.Destino = null;
                    x.Tad = null!;
                    x.Chofer = null!;
                    x.Tonel = null!;
                    x.Producto = null;
                    x.Codest = 3;
                    x.CodordCom = folio;
                    x.FchOrd = DateTime.Today.Date;
                    context.Update(x);
                });
                await context.SaveChangesAsync();
                return Ok(newFolio);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost("check/chofer")]
        public async Task<ActionResult> PostConfirmChofer([FromBody] CheckChofer checkChofer)
        {
            try
            {
                var chofer = await context.OrdenEmbarque.FirstOrDefaultAsync(x => x.Codton == checkChofer.Tonel
                && x.Codchf == checkChofer.Chofer && x.CompartmentId == checkChofer.Compartimento && x.Fchcar == checkChofer.FechaCarga
                && x.Bolguidid == null);
                if (chofer == null)
                {
                    return Ok(0);
                }
                return Ok(chofer.Cod);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("desasignar/{code:int}")]
        public async Task<ActionResult> PutAsignacion(int code)
        {
            try
            {
                var orden = await context.OrdenEmbarque.FirstOrDefaultAsync(x => x.Cod == code);

                orden.Chofer = null;
                orden.Tonel = null;

                orden.Codchf = null;
                orden.Codton = null;
                orden.Compartment = null;
                orden.CompartmentId = null;

                context.Update(orden);
                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut]
        public async Task<ActionResult> PutPedido([FromBody] OrdenEmbarque orden)
        {
            try
            {
                orden.Producto = null;
                orden.Chofer = null;
                orden.Destino = null;
                orden.Tonel = null;
                orden.Tad = null;
                orden.OrdenCompra = null;
                orden.Estado = null;

                context.Update(orden);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPut("tp")]
        public async Task<ActionResult> PutTP([FromBody] OrdenesTP ordenesTP)
        {
            try
            {
                var orden = await context.OrdenEmbarque.FirstOrDefaultAsync(x => x.Cod == ordenesTP.Cod);
                orden.Tp = ordenesTP.TP;
                context.Update(orden);

                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
