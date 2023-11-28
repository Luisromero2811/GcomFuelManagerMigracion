using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq;
using System.Security.Claims;

namespace GComFuelManager.Server.Controllers.Cierres
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, Administrador Sistema, Direccion, Gerencia, Ejecutivo de Cuenta Comercial, Comprador, Programador, Ejecutivo de Cuenta Operativo, Lectura de Cierre de Orden, Cierre Pedidos, Consulta Precios, Cliente Lectura")]
    public class CierreController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly VerifyUserId verifyUser;

        public CierreController(ApplicationDbContext context, UserManager<IdentityUsuario> userManager, VerifyUserId verifyUser)
        {
            this.context = context;
            UserManager = userManager;
            this.verifyUser = verifyUser;
        }

        public UserManager<IdentityUsuario> UserManager { get; }

        private async Task SaveErrors(Exception e, string accion)
        {
            context.Add(new Errors()
            {
                Error = JsonConvert.SerializeObject(new Error()
                {
                    Inner = JsonConvert.SerializeObject(e.InnerException),
                    Message = JsonConvert.SerializeObject(e.Message),
                    StackTrace = JsonConvert.SerializeObject(e.StackTrace)
                }),
                Accion = accion
            });
            await context.SaveChangesAsync();
        }

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

        [HttpGet("get/{folio}")]
        public async Task<ActionResult> GetCierreByFolio([FromRoute] string folio)
        {
            try
            {
                Porcentaje porcentaje = new Porcentaje();
                var por = context.Porcentaje.FirstOrDefault(x => x.Accion == "cierre");
                if (por != null)
                    porcentaje = por;

                var cierres = await context.OrdenCierre.Where(x => x.Folio == folio)
                    .Include(x => x.Destino)
                    .Include(x => x.Cliente)
                    .Include(x => x.Producto)
                    .Include(x => x.Grupo)
                    .Include(x => x.OrdenPedidos)
                    .ThenInclude(x => x.OrdenEmbarque)
                    .ToListAsync();

                cierres.ForEach(x => x.GetTieneVolumenDisponible(porcentaje));

                return Ok(cierres);
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
                List<OrdenCierre> cierresVolumen = new List<OrdenCierre>();
                var ordenes = context.OrdenCierre.Where(x => x.Folio == folio && x.Estatus == true).ToList();

                if (ordenes.Count > 0)
                {

                    var pedidos = context.OrdenPedido.Where(x => x.Folio == folio && x.CodPed != null && x.OrdenEmbarque != null).ToList();
                    foreach (var item1 in pedidos)
                    {
                        var pedido = context.OrdenCierre.Where(x => x.CodPed == item1.CodPed)
                            .Include(x => x.OrdenEmbarque)
                            .Include(x => x.Cliente)
                            .Include(x => x.Producto)
                            .Include(x => x.Destino)
                            .Include(x => x.OrdenEmbarque)
                            .ThenInclude(x => x.Tad)
                            .Include(x => x.OrdenEmbarque)
                            .ThenInclude(x => x.Tonel)
                            .Include(x => x.OrdenEmbarque)
                            .ThenInclude(x => x.Estado)
                            .Include(x => x.OrdenEmbarque)
                            .ThenInclude(x => x.Orden)
                            .ThenInclude(x => x.Estado)
                            .DefaultIfEmpty()
                            .FirstOrDefault();

                        if (pedido != null)
                            cierresVolumen.Add(pedido);
                    }

                    return Ok(cierresVolumen);
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
                orden.OrdenEmbarque = null!;
                orden.Cliente = null!;
                orden.Producto = null!;
                orden.Destino = null!;
                if (!orden.isGroup)
                {
                    var cliente = context.Cliente.FirstOrDefault(x => x.Cod == orden.CodCte);

                    orden.TipoVenta = cliente.Tipven;

                    if (!string.IsNullOrEmpty(cliente.Tipven))
                        orden.ModeloVenta = cliente.Tipven.ToLower() == "rack" ? "Rack" : "Delivery";
                    else
                        orden.ModeloVenta = string.Empty;
                }
                orden.FchVencimiento = orden.FchCierre?.AddDays(6);
                context.Add(orden);

                var id = await verifyUser.GetId(HttpContext, UserManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                await context.SaveChangesAsync(id, 1);

                var NewOrden = await context.OrdenCierre.Where(x => x.Cod == orden.Cod)
                    .Include(x => x.Destino)
                    .Include(x => x.Producto)
                    .Include(x => x.Grupo)
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x.Estado)
                    .Include(x => x.Cliente)
                    .Include(x => x.Destino)
                    .Include(x => x.OrdenPedidos)
                    .ThenInclude(x => x.OrdenEmbarque)
                    .FirstOrDefaultAsync();

                if (orden.PrecioOverDate)
                {
                    CierrePrecioDespuesFecha cierreprecio = new CierrePrecioDespuesFecha()
                    {
                        CodCie = orden.Cod,
                        CodCte = orden.CodCte,
                        CodPrd = orden.CodPrd,
                        Precio = orden.Precio
                    };
                    context.Add(cierreprecio);
                    await context.SaveChangesAsync();
                }

                if (NewOrden is not null)
                    NewOrden.SetVolumen();

                return Ok(NewOrden);
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

                var newOrden = context.OrdenCierre.Where(x => x.Cod == orden.Cod)
                    .Include(x => x.OrdenEmbarque)
                    .Include(x => x.Destino)
                    .Include(x => x.Producto)
                    .Include(x => x.Cliente)
                    .FirstOrDefault();

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
                    return NotFound();

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
                        .Include(x => x.OrdenEmbarque)
                        .ThenInclude(x => x.Tonel)
                        .Include(x => x.OrdenEmbarque)
                        .ThenInclude(x => x.Estado)
                        .Include(x => x.OrdenEmbarque)
                        .ThenInclude(x => x.Tad)
                        .Include(x => x.OrdenEmbarque)
                        .ThenInclude(x => x.Orden)
                        .ToListAsync();

                }
                else
                {
                    //Filtrado mediante el folio del cierre
                    if (!string.IsNullOrEmpty(filtroDTO.Folio))
                    {
                        cierres = await context.OrdenCierre.Where(x => x.Folio == filtroDTO.Folio && x.Estatus == true)
                            .Include(x => x.Cliente)
                            .Include(x => x.Producto)
                            .Include(x => x.Destino)
                            .Include(x => x.ContactoN)
                            .Include(x => x.OrdenEmbarque)
                            .ThenInclude(x => x.Tonel)
                            .Include(x => x.OrdenEmbarque)
                            .ThenInclude(x => x.Estado)
                            .Include(x => x.OrdenEmbarque)
                            .ThenInclude(x => x.Tad)
                            .Include(x => x.OrdenEmbarque)
                            .ThenInclude(x => x.Orden)
                            .Include(x => x.OrdenPedidos)
                            .ThenInclude(x => x.OrdenEmbarque)
                            .ToListAsync();
                    }
                    else
                        return BadRequest("Debe escribir un folio valido.");
                }

                return Ok(cierres);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("filtrar/activo")]
        public async Task<ActionResult> PostFilterActivos([FromBody] CierreFiltroDTO filtroDTO)
        {
            try
            {
                List<OrdenCierre> cierres = new List<OrdenCierre>();

                if (!filtroDTO.forFolio)
                {
                    cierres = await context.OrdenCierre.Where(x => x.CodCte == filtroDTO.codCte
                    && x.FchCierre >= filtroDTO.FchInicio && x.FchCierre <= filtroDTO.FchFin && x.Estatus == true && x.Activa == true)
                        .Include(x => x.Cliente)
                        .Include(x => x.Producto)
                        .Include(x => x.Destino)
                        .Include(x => x.ContactoN)
                        .Include(x => x.OrdenEmbarque)
                        .ThenInclude(x => x.Tonel)
                        .Include(x => x.OrdenEmbarque)
                        .ThenInclude(x => x.Estado)
                        .Include(x => x.OrdenEmbarque)
                        .ThenInclude(x => x.Tad)
                        .ToListAsync();

                }
                else
                {
                    if (!string.IsNullOrEmpty(filtroDTO.Folio))
                    {
                        cierres = await context.OrdenCierre.Where(x => x.Folio == filtroDTO.Folio && x.Estatus == true && x.Activa == true)
                            .Include(x => x.Cliente)
                            .Include(x => x.Producto)
                            .Include(x => x.Destino)
                            .Include(x => x.ContactoN)
                            .Include(x => x.OrdenEmbarque)
                            .ThenInclude(x => x.Tonel)
                            .Include(x => x.OrdenEmbarque)
                            .ThenInclude(x => x.Estado)
                            .Include(x => x.OrdenEmbarque)
                            .ThenInclude(x => x.Tad)
                            .Include(x => x.OrdenEmbarque)
                            .ThenInclude(x => x.Orden)
                            .ToListAsync();
                    }
                    else
                        return BadRequest("Debe escribir un folio valido.");
                }
                //foreach (var item in cierres)
                //{
                //    if (item.OrdenEmbarque is not null)
                //    {
                //        if (item.OrdenEmbarque!.Folio != 0 && item.OrdenEmbarque!.Folio != null)
                //        {
                //            var o = await context.Orden.Where(y => y.Ref!.Contains(item.OrdenEmbarque!.Folio.ToString()!)).Include(x => x.Estado).FirstOrDefaultAsync();
                //            if (o != null)
                //                cierres.FirstOrDefault(x => x.Cod == item.Cod)!.OrdenEmbarque!.Orden = o;
                //            else
                //                cierres.FirstOrDefault(x => x.Cod == item.Cod)!.OrdenEmbarque!.Orden = null!;
                //        }
                //    }
                //}
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

        [HttpPost("asignacion/filtrar")]
        public async Task<ActionResult> GetDate([FromBody] FechasF fechas)
        {
            try
            {
                var ordens = await context.OrdenCierre
                    .Where(x => x.OrdenEmbarque!.Fchcar >= fechas.DateInicio && x.OrdenEmbarque!.Fchcar <= fechas.DateFin && x.OrdenEmbarque!.Codest == 3 && x.OrdenEmbarque!.FchOrd != null
                    && x.OrdenEmbarque!.Bolguidid == null)
                    .Include(x => x.OrdenEmbarque!.Chofer)
                    .Include(x => x.OrdenEmbarque!.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.OrdenEmbarque!.Estado)
                    .Include(x => x.OrdenEmbarque!.OrdenCompra)
                    .Include(x => x.OrdenEmbarque!.Tad)
                    .Include(x => x.OrdenEmbarque!.Producto)
                    .Include(x => x.OrdenEmbarque!.Tonel)
                    .ThenInclude(x => x.Transportista)
                    .OrderBy(x => x.OrdenEmbarque!.Fchpet)
                    .Take(1000)
                    .ToListAsync();

                return Ok(ordens);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("vencimiento")]
        public async Task<ActionResult> PostVencimieto([FromBody] CierreFiltroDTO fechas)
        {
            try
            {
                List<OrdenCierre> ordens = new List<OrdenCierre>();

                ordens = context.OrdenCierre.Where(x => x.Folio == fechas.Folio).ToList();

                if (ordens is null)
                    return BadRequest();

                ordens.ForEach(x =>
                {
                    x.FchVencimiento = fechas.FchFin;
                    if (x.FchVencimiento >= DateTime.Today)
                        x.Activa = true;
                });

                context.UpdateRange(ordens);

                var id = await verifyUser.GetId(HttpContext, UserManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                await context.SaveChangesAsync(id, 5);

                var ordenes = await context.OrdenCierre.Where(x => x.Folio == fechas.Folio && x.Estatus == true)
                    .Include(x => x.Producto)
                    .Include(x => x.Destino)
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x!.Destino)
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x!.Producto)
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x.Tad)
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x.Tonel)
                    .ToListAsync();

                return Ok(ordens);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("margen")]
        public async Task<ActionResult> PostMargen([FromBody] CierreFiltroDTO fechas)
        {
            try
            {
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("volumen")]
        public async Task<ActionResult<VolumenDisponibleDTO>> GetListadoVolumen([FromBody] CierreFiltroDTO filtro)
        {
            try
            {
                VolumenDisponibleDTO volumen = new VolumenDisponibleDTO();

                if (!filtro.forFolio)
                {
                    var cierres = await context.OrdenCierre.Where(x => x.FchCierre >= filtro.FchInicio && x.FchCierre <= filtro.FchFin
                    && x.Estatus == true && x.CodCte == filtro.codCte)
                        .Include(x => x.Producto).Include(x => x.OrdenEmbarque).ThenInclude(x => x.Tonel).Include(x => x.OrdenEmbarque).ThenInclude(x => x.Orden).ToListAsync();

                    if (cierres is null)
                        return new VolumenDisponibleDTO();

                    cierres = cierres.DistinctBy(x => x.Cod).ToList();

                    foreach (var item in cierres.DistinctBy(x => x.Cod))
                    {
                        if (context.OrdenPedido.Any(x => x.Folio.Equals(item.Folio)))
                        {

                            var VolumenDisponible = item.Volumen;

                            var listConsumido = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Tonel != null && x.OrdenEmbarque.Codprd == item.CodPrd
                            && x.OrdenEmbarque.Codest == 22
                            && x.OrdenEmbarque.Folio != null
                            && x.OrdenEmbarque.Bolguidid != null)
                            .Include(x => x.OrdenEmbarque)
                            .ThenInclude(x => x.Tonel).ToList();

                            var VolumenCongelado = listConsumido.Sum(item => item.OrdenEmbarque!.Compartment == 1 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom!.ToString())
                                            : item.OrdenEmbarque!.Compartment == 2 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom2!.ToString())
                                            : item.OrdenEmbarque!.Compartment == 3 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom3!.ToString())
                                            : item.OrdenEmbarque!.Compartment == 4 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom4!.ToString())
                                            : item.OrdenEmbarque!.Vol);

                            var VolumenConsumido = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Orden != null && x.OrdenEmbarque.Folio != null
                                && x.OrdenEmbarque.Orden.Codest != 14
                                && x.OrdenEmbarque.Codest != 14
                            && x.OrdenEmbarque.Codprd == item.CodPrd
                            && x.OrdenEmbarque.Orden.BatchId != null)
                                .Include(x => x.OrdenEmbarque)
                                .ThenInclude(x => x.Orden)
                                .Sum(x => x.OrdenEmbarque!.Orden!.Vol);

                            var VolumenProgramado = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Folio == null
                                && x.OrdenEmbarque.Codprd == item.CodPrd
                                && x.OrdenEmbarque.Bolguidid == null && x.OrdenEmbarque.Codest == 3
                                && x.OrdenEmbarque.FchOrd != null)
                                .Include(x => x.OrdenEmbarque)
                                    .Sum(x => x.OrdenEmbarque!.Vol);

                            var VolumenSolicitado = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Folio == null
                            && x.OrdenEmbarque.Codprd == item.CodPrd
                            && x.OrdenEmbarque.Bolguidid == null && x.OrdenEmbarque.Codest == 9
                            && x.OrdenEmbarque.FchOrd == null)
                                .Include(x => x.OrdenEmbarque)
                                .Sum(x => x.OrdenEmbarque!.Vol);

                            var VolumenTotalDisponible = VolumenDisponible - (VolumenConsumido + VolumenCongelado + VolumenProgramado);

                            ProductoVolumen productoVolumen = new ProductoVolumen();

                            productoVolumen.Nombre = item.Producto?.Den;
                            productoVolumen.Disponible = VolumenTotalDisponible;
                            productoVolumen.Congelado = VolumenCongelado;
                            productoVolumen.Consumido = VolumenConsumido;
                            productoVolumen.Total = VolumenDisponible;
                            productoVolumen.Solicitud = VolumenSolicitado;
                            productoVolumen.Programado = VolumenProgramado;

                            if (volumen.Productos.Any(x => x.Nombre.Equals(item.Producto.Den)))
                            {

                                volumen.Productos.FirstOrDefault(x => x.Nombre!.Equals(item.Producto!.Den))!.Disponible += productoVolumen.Disponible;
                                volumen.Productos.FirstOrDefault(x => x.Nombre!.Equals(item.Producto!.Den))!.Congelado += productoVolumen.Congelado;
                                volumen.Productos.FirstOrDefault(x => x.Nombre!.Equals(item.Producto!.Den))!.Consumido += productoVolumen.Consumido;
                                volumen.Productos.FirstOrDefault(x => x.Nombre!.Equals(item.Producto!.Den))!.Total += productoVolumen.Total;
                                volumen.Productos.FirstOrDefault(x => x.Nombre!.Equals(item.Producto!.Den))!.Programado += productoVolumen.Programado;
                                volumen.Productos.FirstOrDefault(x => x.Nombre!.Equals(item.Producto!.Den))!.Solicitud += productoVolumen.Solicitud;
                            }
                            else
                                volumen.Productos.Add(productoVolumen);
                        }
                        else
                        {

                            var VolumenDisponible = item.Volumen;

                            var VolumenCongelado = item.OrdenEmbarque?.Folio is not null && item.OrdenEmbarque?.Orden is null ? item?.OrdenEmbarque?.Compartment == 1
                                && item.OrdenEmbarque?.Codest == 22
                                && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom?.ToString())
                                            : item?.OrdenEmbarque?.Compartment == 2 && item.OrdenEmbarque?.Tonel is not null
                                            && item.OrdenEmbarque?.Codest == 22 ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom2?.ToString())
                                            : item?.OrdenEmbarque?.Compartment == 3 && item.OrdenEmbarque?.Tonel is not null
                                            && item.OrdenEmbarque?.Codest == 22 ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom3?.ToString())
                                            : item?.OrdenEmbarque?.Compartment == 4 && item.OrdenEmbarque?.Tonel is not null
                                            && item.OrdenEmbarque?.Codest == 22 ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom4?.ToString())
                                            : item?.OrdenEmbarque?.Vol : 0;

                            var VolumenConsumido = item.OrdenEmbarque?.Folio is not null && item.OrdenEmbarque?.Orden?.BatchId is not null
                            && item.OrdenEmbarque?.Orden?.Codest != 14 && item.OrdenEmbarque?.Codest != 14
                            ? item.OrdenEmbarque?.Orden?.Vol : 0;

                            var VolumenProgramado = item?.OrdenEmbarque?.Codest == 3 && item.OrdenEmbarque.FchOrd != null &&
                                item.OrdenEmbarque.Folio == null && item.OrdenEmbarque.Bolguidid == null ? item.OrdenEmbarque.Vol : 0;

                            var VolumenSolicitado = item?.OrdenEmbarque?.Codest == 9 && item.OrdenEmbarque.FchOrd == null &&
                                item.OrdenEmbarque.Folio == null && item.OrdenEmbarque.Bolguidid == null ? item.OrdenEmbarque.Vol : 0;

                            var VolumenTotalDisponible = VolumenDisponible - (VolumenConsumido + VolumenCongelado + VolumenProgramado);

                            ProductoVolumen productoVolumen = new ProductoVolumen();
                            productoVolumen.Nombre = item.Producto!.Den;
                            productoVolumen.Disponible = VolumenTotalDisponible;
                            productoVolumen.Congelado = VolumenCongelado;
                            productoVolumen.Consumido = VolumenConsumido;
                            productoVolumen.Total = VolumenDisponible;
                            productoVolumen.Solicitud = VolumenSolicitado;
                            productoVolumen.Programado = VolumenProgramado;

                            if (volumen.Productos.Any(x => x.Nombre.Equals(item.Producto.Den)))
                            {
                                volumen.Productos.FirstOrDefault(x => x.Nombre!.Equals(item.Producto!.Den))!.Disponible += productoVolumen.Disponible;
                                volumen.Productos.FirstOrDefault(x => x.Nombre!.Equals(item.Producto!.Den))!.Congelado += productoVolumen.Congelado;
                                volumen.Productos.FirstOrDefault(x => x.Nombre!.Equals(item.Producto!.Den))!.Consumido += productoVolumen.Consumido;
                                volumen.Productos.FirstOrDefault(x => x.Nombre!.Equals(item.Producto!.Den))!.Total += productoVolumen.Total;
                                volumen.Productos.FirstOrDefault(x => x.Nombre!.Equals(item.Producto!.Den))!.Programado += productoVolumen.Programado;
                                volumen.Productos.FirstOrDefault(x => x.Nombre!.Equals(item.Producto!.Den))!.Solicitud += productoVolumen.Solicitud;
                            }
                            else
                                volumen.Productos.Add(productoVolumen);

                        }
                    }
                    return Ok(volumen);
                }
                else
                {
                    if (context.OrdenPedido.Any(x => !string.IsNullOrEmpty(x.Folio) && x.Folio == filtro.Folio))
                    {
                        var cierres = await context.OrdenCierre.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio == filtro.Folio).Include(x => x.Producto).ToListAsync();
                        if (cierres is null)
                            return BadRequest("No existe el cierre.");

                        cierres = cierres.DistinctBy(x => x.Cod).ToList();

                        foreach (var item in cierres.DistinctBy(x => x.Cod))
                        {
                            var VolumenDisponible = item.Volumen;

                            var listConsumido = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Tonel != null && x.OrdenEmbarque.Codprd == item.CodPrd
                            && x.OrdenEmbarque.Codest == 22
                            && x.OrdenEmbarque.Folio != null
                            && x.OrdenEmbarque.Bolguidid != null)
                            .Include(x => x.OrdenEmbarque)
                            .ThenInclude(x => x.Tonel).ToList();

                            var VolumenCongelado = listConsumido.Sum(item => item.OrdenEmbarque!.Compartment == 1 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom!.ToString())
                                            : item.OrdenEmbarque!.Compartment == 2 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom2!.ToString())
                                            : item.OrdenEmbarque!.Compartment == 3 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom3!.ToString())
                                            : item.OrdenEmbarque!.Compartment == 4 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom4!.ToString())
                                            : item.OrdenEmbarque!.Vol);

                            var VolumenConsumido = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Orden != null && x.OrdenEmbarque.Folio != null
                                && x.OrdenEmbarque.Orden.Codest != 14
                                && x.OrdenEmbarque.Codest != 14
                            && x.OrdenEmbarque.Codprd == item.CodPrd
                            && x.OrdenEmbarque.Orden.BatchId != null)
                                .Include(x => x.OrdenEmbarque)
                                .ThenInclude(x => x.Orden)
                                .Sum(x => x.OrdenEmbarque!.Orden!.Vol);

                            var VolumenProgramado = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Folio == null
                                && x.OrdenEmbarque.Codprd == item.CodPrd
                                && x.OrdenEmbarque.Bolguidid == null && x.OrdenEmbarque.Codest == 3
                                && x.OrdenEmbarque.FchOrd != null)
                                .Include(x => x.OrdenEmbarque)
                                    .Sum(x => x.OrdenEmbarque!.Vol);

                            var VolumenSolicitado = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Folio == null
                            && x.OrdenEmbarque.Codprd == item.CodPrd
                            && x.OrdenEmbarque.Bolguidid == null && x.OrdenEmbarque.Codest == 9
                            && x.OrdenEmbarque.FchOrd == null)
                                .Include(x => x.OrdenEmbarque)
                                .Sum(x => x.OrdenEmbarque!.Vol);

                            var VolumenTotalDisponible = VolumenDisponible - (VolumenConsumido + VolumenCongelado + VolumenProgramado);

                            ProductoVolumen productoVolumen = new ProductoVolumen();

                            productoVolumen.Nombre = item.Producto?.Den;
                            productoVolumen.Disponible = VolumenTotalDisponible;
                            productoVolumen.Congelado = VolumenCongelado;
                            productoVolumen.Consumido = VolumenConsumido;
                            productoVolumen.Total = VolumenDisponible;
                            productoVolumen.Solicitud = VolumenSolicitado;
                            productoVolumen.Programado = VolumenProgramado;

                            volumen.Productos.Add(productoVolumen);
                        }
                        return Ok(volumen);

                    }
                    else
                    {
                        var ordenes = context.OrdenCierre.Where(x => x.Folio == filtro.Folio)
                            .Include(x => x.Producto)
                            .Include(x => x.OrdenEmbarque)
                            .ThenInclude(x => x!.Orden)
                            .Include(x => x.OrdenEmbarque)
                            .ThenInclude(x => x!.Tonel).ToList();

                        foreach (var item in ordenes.DistinctBy(x => x.Producto?.Den))
                        {
                            var VolumenDisponible = ordenes.Where(x => x.CodPrd == item.CodPrd && x.Estatus is true).Sum(x => x.Volumen);

                            var VolumenCongelado = ordenes.Where(x => x.CodPrd == item.CodPrd
                            && x.Estatus is true && item.OrdenEmbarque?.Codest == 22
                            && x?.OrdenEmbarque?.Folio is not null
                            && x?.OrdenEmbarque?.Orden is null).Sum(item =>
                            item?.OrdenEmbarque?.Compartment == 1 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom?.ToString() ?? "0")
                                            : item?.OrdenEmbarque?.Compartment == 2 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom2?.ToString() ?? "0")
                                            : item?.OrdenEmbarque?.Compartment == 3 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom3?.ToString() ?? "0")
                                            : item?.OrdenEmbarque?.Compartment == 4 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom4?.ToString() ?? "0")
                                            : item?.OrdenEmbarque?.Vol);

                            var VolumenConsumido = ordenes.Where(x => x.CodPrd == item.CodPrd
                            && x.OrdenEmbarque?.Orden?.Codest != 14 && item.OrdenEmbarque?.Codest != 14
                            && x.Estatus is true
                            && x?.OrdenEmbarque?.Folio is not null
                            && x?.OrdenEmbarque?.Orden?.BatchId is not null).Sum(x => x.OrdenEmbarque?.Orden?.Vol);

                            var VolumenProgramado = ordenes.Where(x => x.OrdenEmbarque?.Folio is null
                            && x.OrdenEmbarque?.Codprd == item.CodPrd
                            && x.OrdenEmbarque?.Bolguidid is null && x.OrdenEmbarque?.Codest == 3
                            && x.OrdenEmbarque.FchOrd is not null)
                                .Sum(x => x.OrdenEmbarque?.Vol);

                            var VolumenSolicitado = ordenes.Where(x => x.OrdenEmbarque?.Folio is null
                            && x.OrdenEmbarque?.Codprd == item.CodPrd
                            && x.OrdenEmbarque?.Bolguidid is null && x.OrdenEmbarque?.Codest == 9
                            && x.OrdenEmbarque.FchOrd is null)
                                .Sum(x => x.OrdenEmbarque?.Vol);

                            var VolumenTotalDisponible = VolumenDisponible - (VolumenConsumido + VolumenCongelado + VolumenProgramado);

                            ProductoVolumen productoVolumen = new ProductoVolumen();
                            productoVolumen.Nombre = item.Producto?.Den;
                            productoVolumen.Disponible = VolumenTotalDisponible;
                            productoVolumen.Congelado = VolumenCongelado;
                            productoVolumen.Consumido = VolumenConsumido;
                            productoVolumen.Total = VolumenDisponible;
                            productoVolumen.Solicitud = VolumenSolicitado;
                            productoVolumen.Programado = VolumenProgramado;

                            volumen.Productos.Add(productoVolumen);
                        }
                        return Ok(volumen);
                    }
                }
                return Ok(volumen);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
                throw;
            }
        }

        [HttpPost("volumen/mes")]
        public async Task<ActionResult<VolumenDisponibleDTO>> GetListadoVolumenMes([FromBody] CierreFiltroDTO filtro)
        {
            try
            {
                VolumenDisponibleDTO volumen = new VolumenDisponibleDTO();

                var cierres = await context.OrdenCierre.Where(x => x.FchCierre >= filtro.FchInicio && x.FchCierre <= filtro.FchFin
                && x.Estatus == true && x.CodCte == filtro.codCte)
                    .Include(x => x.Producto).Include(x => x.OrdenEmbarque).ThenInclude(x => x.Tonel).Include(x => x.OrdenEmbarque).ThenInclude(x => x.Orden).ToListAsync();

                if (cierres is null)
                    return new VolumenDisponibleDTO();

                foreach (var item in cierres.DistinctBy(x => x.Cod))
                {
                    var VolumenDisponible = item.CodPed == 0 ? item.Volumen : 0;

                    var VolumenCongelado = item?.OrdenEmbarque?.Folio is not null && item.OrdenEmbarque?.Orden is null
                        && item.OrdenEmbarque?.Codest == 22 ?
                                      item?.OrdenEmbarque?.Compartment == 1 && item.OrdenEmbarque?.Tonel is not null
                                    && item.OrdenEmbarque?.Codest == 22 ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom?.ToString() ?? "0")
                                    : item?.OrdenEmbarque?.Compartment == 2 && item.OrdenEmbarque?.Tonel is not null
                                    && item.OrdenEmbarque?.Codest == 22 ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom2?.ToString() ?? "0")
                                    : item?.OrdenEmbarque?.Compartment == 3 && item.OrdenEmbarque?.Tonel is not null
                                    && item.OrdenEmbarque?.Codest == 22 ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom3?.ToString() ?? "0")
                                    : item?.OrdenEmbarque?.Compartment == 4 && item.OrdenEmbarque?.Tonel is not null
                                    && item.OrdenEmbarque?.Codest == 22 ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom4?.ToString() ?? "0")
                                    : item?.OrdenEmbarque?.Vol : 0;

                    var VolumenConsumido = item?.OrdenEmbarque?.Folio is not null && item.OrdenEmbarque?.Orden?.BatchId is not null
                    && item.OrdenEmbarque?.Codest != 14
                    && item.OrdenEmbarque?.Orden?.Codest != 14
                    ? item.OrdenEmbarque?.Orden?.Vol : 0;

                    var VolumenProgramado = item?.OrdenEmbarque?.Codest == 3 && item.OrdenEmbarque.FchOrd != null &&
                        item.OrdenEmbarque.Folio == null && item.OrdenEmbarque.Bolguidid == null ? item.OrdenEmbarque.Vol : 0;

                    var VolumenSolicitado = item?.OrdenEmbarque?.Codest == 9 && item.OrdenEmbarque.FchOrd == null &&
                        item.OrdenEmbarque.Folio == null && item.OrdenEmbarque.Bolguidid == null ? item.OrdenEmbarque.Vol : 0;

                    var VolumenTotalDisponible = VolumenDisponible - (VolumenConsumido + VolumenCongelado + VolumenProgramado);

                    ProductoVolumen productoVolumen = new ProductoVolumen();
                    productoVolumen.Nombre = item?.Producto!.Den;
                    productoVolumen.Disponible = VolumenTotalDisponible;
                    productoVolumen.Congelado = VolumenCongelado;
                    productoVolumen.Consumido = VolumenConsumido;
                    productoVolumen.Total = VolumenDisponible;
                    productoVolumen.Solicitud = VolumenSolicitado;
                    productoVolumen.Programado = VolumenProgramado;

                    if (volumen.Productos.Any(x => !string.IsNullOrEmpty(x.Nombre) && item.Producto != null && x.Nombre.Equals(item.Producto.Den)))
                    {
                        volumen.Productos.FirstOrDefault(x => x.Nombre!.Equals(item.Producto!.Den))!.Disponible += productoVolumen.Disponible;
                        volumen.Productos.FirstOrDefault(x => x.Nombre!.Equals(item.Producto!.Den))!.Congelado += productoVolumen.Congelado;
                        volumen.Productos.FirstOrDefault(x => x.Nombre!.Equals(item.Producto!.Den))!.Consumido += productoVolumen.Consumido;
                        volumen.Productos.FirstOrDefault(x => x.Nombre!.Equals(item.Producto!.Den))!.Total += productoVolumen.Total;
                        volumen.Productos.FirstOrDefault(x => x.Nombre!.Equals(item.Producto!.Den))!.Programado += productoVolumen.Programado;
                        volumen.Productos.FirstOrDefault(x => x.Nombre!.Equals(item.Producto!.Den))!.Solicitud += productoVolumen.Solicitud;
                    }
                    else
                        volumen.Productos.Add(productoVolumen);
                }
                return Ok(volumen);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
                throw;
            }
        }

        [HttpPost("volumen/activo")]
        public async Task<ActionResult<VolumenDisponibleDTO>> GetListadoVolumenActiva([FromBody] CierreFiltroDTO filtro)
        {
            try
            {
                VolumenDisponibleDTO volumen = new VolumenDisponibleDTO();

                if (!filtro.forFolio)
                {
                    var cierres = await context.OrdenCierre.Where(x => x.FchCierre >= filtro.FchInicio && x.FchCierre <= filtro.FchFin
                    && x.Estatus == true && x.CodCte == filtro.codCte)
                        .Include(x => x.Producto).Include(x => x.OrdenEmbarque).ThenInclude(x => x.Tonel).Include(x => x.OrdenEmbarque).ThenInclude(x => x.Orden).ToListAsync();

                    if (cierres is null)
                        return new VolumenDisponibleDTO();

                    foreach (var item in cierres.DistinctBy(x => x.Cod))
                    {
                        if (context.OrdenPedido.Any(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio)))
                        {

                            var VolumenDisponible = item.Volumen;

                            var listConsumido = context.OrdenPedido.Where(x => x.OrdenEmbarque != null && x.OrdenEmbarque.Tonel != null && !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Tonel != null && x.OrdenEmbarque.Codprd == item.CodPrd
                            && x.OrdenEmbarque.Codest == 22
                            && x.OrdenEmbarque.Folio != null
                            && x.OrdenEmbarque.Bolguidid != null)
                            .Include(x => x.OrdenEmbarque)
                            .ThenInclude(x => x.Tonel).ToList();

                            var VolumenCongelado = listConsumido.Sum(item => item.OrdenEmbarque!.Compartment == 1 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom!.ToString())
                                            : item.OrdenEmbarque!.Compartment == 2 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom2!.ToString())
                                            : item.OrdenEmbarque!.Compartment == 3 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom3!.ToString())
                                            : item.OrdenEmbarque!.Compartment == 4 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom4!.ToString())
                                            : item.OrdenEmbarque!.Vol);

                            var VolumenConsumido = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Orden != null && x.OrdenEmbarque.Folio != null
                                && x.OrdenEmbarque.Orden.Codest != 14
                                && x.OrdenEmbarque.Codest != 14
                            && x.OrdenEmbarque.Codprd == item.CodPrd
                            && x.OrdenEmbarque.Orden.BatchId != null)
                                .Include(x => x.OrdenEmbarque)
                                .ThenInclude(x => x.Orden)
                                .Sum(x => x.OrdenEmbarque!.Orden!.Vol);

                            var VolumenProgramado = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Folio == null
                                && x.OrdenEmbarque.Codprd == item.CodPrd
                                && x.OrdenEmbarque.Bolguidid == null && x.OrdenEmbarque.Codest == 3
                                && x.OrdenEmbarque.FchOrd != null)
                                .Include(x => x.OrdenEmbarque)
                                    .Sum(x => x.OrdenEmbarque!.Vol);

                            var VolumenSolicitado = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Folio == null
                            && x.OrdenEmbarque.Codprd == item.CodPrd
                            && x.OrdenEmbarque.Bolguidid == null && x.OrdenEmbarque.Codest == 9
                            && x.OrdenEmbarque.FchOrd == null)
                                .Include(x => x.OrdenEmbarque)
                                .Sum(x => x.OrdenEmbarque!.Vol);

                            var VolumenTotalDisponible = VolumenDisponible - (VolumenConsumido + VolumenCongelado + VolumenProgramado);

                            ProductoVolumen productoVolumen = new ProductoVolumen();

                            productoVolumen.Nombre = item.Producto?.Den;
                            productoVolumen.Disponible = VolumenTotalDisponible;
                            productoVolumen.Congelado = VolumenCongelado;
                            productoVolumen.Consumido = VolumenConsumido;
                            productoVolumen.Total = VolumenDisponible;
                            productoVolumen.Solicitud = VolumenSolicitado;
                            productoVolumen.Programado = VolumenProgramado;

                            if (volumen.Productos.Any(x => !string.IsNullOrEmpty(x.Nombre) && item.Producto != null && x.Nombre.Equals(item.Producto.Den)))
                            {

                                volumen.Productos.FirstOrDefault(x => x.Nombre!.Equals(item.Producto!.Den))!.Disponible += productoVolumen.Disponible;
                                volumen.Productos.FirstOrDefault(x => x.Nombre!.Equals(item.Producto!.Den))!.Congelado += productoVolumen.Congelado;
                                volumen.Productos.FirstOrDefault(x => x.Nombre!.Equals(item.Producto!.Den))!.Consumido += productoVolumen.Consumido;
                                volumen.Productos.FirstOrDefault(x => x.Nombre!.Equals(item.Producto!.Den))!.Total += productoVolumen.Total;
                                volumen.Productos.FirstOrDefault(x => x.Nombre!.Equals(item.Producto!.Den))!.Programado += productoVolumen.Programado;
                                volumen.Productos.FirstOrDefault(x => x.Nombre!.Equals(item.Producto!.Den))!.Solicitud += productoVolumen.Solicitud;
                            }
                            else
                                volumen.Productos.Add(productoVolumen);
                        }
                        else
                        {

                            var VolumenDisponible = item.Volumen;

                            var listConsumido = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Tonel != null && x.OrdenEmbarque.Codprd == item.CodPrd
                            && x.OrdenEmbarque.Codest == 22
                            && x.OrdenEmbarque.Folio != null
                            && x.OrdenEmbarque.Bolguidid != null)
                            .Include(x => x.OrdenEmbarque)
                            .ThenInclude(x => x.Tonel).ToList();

                            var VolumenCongelado = listConsumido.Sum(item => item.OrdenEmbarque!.Compartment == 1 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom!.ToString())
                                            : item.OrdenEmbarque!.Compartment == 2 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom2!.ToString())
                                            : item.OrdenEmbarque!.Compartment == 3 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom3!.ToString())
                                            : item.OrdenEmbarque!.Compartment == 4 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom4!.ToString())
                                            : item.OrdenEmbarque!.Vol);

                            var VolumenConsumido = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Orden != null && x.OrdenEmbarque.Folio != null
                                && x.OrdenEmbarque.Orden.Codest != 14
                                && x.OrdenEmbarque.Codest != 14
                            && x.OrdenEmbarque.Codprd == item.CodPrd
                            && x.OrdenEmbarque.Orden.BatchId != null)
                                .Include(x => x.OrdenEmbarque)
                                .ThenInclude(x => x.Orden)
                                .Sum(x => x.OrdenEmbarque!.Orden!.Vol);

                            var VolumenProgramado = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Folio == null
                                && x.OrdenEmbarque.Codprd == item.CodPrd
                                && x.OrdenEmbarque.Bolguidid == null && x.OrdenEmbarque.Codest == 3
                                && x.OrdenEmbarque.FchOrd != null)
                                .Include(x => x.OrdenEmbarque)
                                    .Sum(x => x.OrdenEmbarque!.Vol);

                            var VolumenSolicitado = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Folio == null
                            && x.OrdenEmbarque.Codprd == item.CodPrd
                            && x.OrdenEmbarque.Bolguidid == null && x.OrdenEmbarque.Codest == 9
                            && x.OrdenEmbarque.FchOrd == null)
                                .Include(x => x.OrdenEmbarque)
                                .Sum(x => x.OrdenEmbarque!.Vol);

                            var VolumenTotalDisponible = VolumenDisponible - (VolumenConsumido + VolumenCongelado + VolumenProgramado);

                            ProductoVolumen productoVolumen = new ProductoVolumen();
                            productoVolumen.Nombre = item.Producto!.Den;
                            productoVolumen.Disponible = VolumenTotalDisponible;
                            productoVolumen.Congelado = VolumenCongelado;
                            productoVolumen.Consumido = VolumenConsumido;
                            productoVolumen.Total = VolumenDisponible;
                            productoVolumen.Solicitud = VolumenSolicitado;
                            productoVolumen.Programado = VolumenProgramado;

                            if (volumen.Productos.Any(x => !string.IsNullOrEmpty(x.Nombre) && x.Nombre.Equals(item.Producto.Den)))
                            {
                                volumen.Productos.FirstOrDefault(x => x.Nombre!.Equals(item.Producto!.Den))!.Disponible += productoVolumen.Disponible;
                                volumen.Productos.FirstOrDefault(x => x.Nombre!.Equals(item.Producto!.Den))!.Congelado += productoVolumen.Congelado;
                                volumen.Productos.FirstOrDefault(x => x.Nombre!.Equals(item.Producto!.Den))!.Consumido += productoVolumen.Consumido;
                                volumen.Productos.FirstOrDefault(x => x.Nombre!.Equals(item.Producto!.Den))!.Total += productoVolumen.Total;
                                volumen.Productos.FirstOrDefault(x => x.Nombre!.Equals(item.Producto!.Den))!.Programado += productoVolumen.Programado;
                                volumen.Productos.FirstOrDefault(x => x.Nombre!.Equals(item.Producto!.Den))!.Solicitud += productoVolumen.Solicitud;
                            }
                            else
                                volumen.Productos.Add(productoVolumen);

                        }
                    }
                    return Ok(volumen);
                }
                else
                {
                    if (context.OrdenPedido.Any(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(filtro.Folio)))
                    {
                        var cierres = await context.OrdenCierre.Where(x => x.Folio!.Equals(filtro.Folio) && x.Activa == true).Include(x => x.Producto).ToListAsync();
                        if (cierres is null)
                            return BadRequest("No existe el cierre.");

                        foreach (var item in cierres)
                        {
                            var VolumenDisponible = item.Volumen;

                            var listConsumido = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Tonel != null && x.OrdenEmbarque.Codprd == item.CodPrd
                            && x.OrdenEmbarque.Codest == 22
                            && x.OrdenEmbarque.Folio != null
                            && x.OrdenEmbarque.Bolguidid != null)
                            .Include(x => x.OrdenEmbarque)
                            .ThenInclude(x => x.Tonel).ToList();

                            var VolumenCongelado = listConsumido.Sum(item => item.OrdenEmbarque!.Compartment == 1 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom!.ToString())
                                            : item.OrdenEmbarque!.Compartment == 2 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom2!.ToString())
                                            : item.OrdenEmbarque!.Compartment == 3 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom3!.ToString())
                                            : item.OrdenEmbarque!.Compartment == 4 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom4!.ToString())
                                            : item.OrdenEmbarque!.Vol);

                            var VolumenConsumido = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Orden != null && x.OrdenEmbarque.Folio != null
                                && x.OrdenEmbarque.Orden.Codest != 14
                                && x.OrdenEmbarque.Codest != 14
                            && x.OrdenEmbarque.Codprd == item.CodPrd
                            && x.OrdenEmbarque.Orden.BatchId != null)
                                .Include(x => x.OrdenEmbarque)
                                .ThenInclude(x => x.Orden)
                                .Sum(x => x.OrdenEmbarque!.Orden!.Vol);

                            var VolumenProgramado = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Folio == null
                                && x.OrdenEmbarque.Codprd == item.CodPrd
                                && x.OrdenEmbarque.Bolguidid == null && x.OrdenEmbarque.Codest == 3
                                && x.OrdenEmbarque.FchOrd != null)
                                .Include(x => x.OrdenEmbarque)
                                    .Sum(x => x.OrdenEmbarque!.Vol);

                            var VolumenSolicitado = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Folio == null
                            && x.OrdenEmbarque.Codprd == item.CodPrd
                            && x.OrdenEmbarque.Bolguidid == null && x.OrdenEmbarque.Codest == 9
                            && x.OrdenEmbarque.FchOrd == null)
                                .Include(x => x.OrdenEmbarque)
                                .Sum(x => x.OrdenEmbarque!.Vol);

                            var VolumenTotalDisponible = VolumenDisponible - (VolumenConsumido + VolumenCongelado + VolumenProgramado);

                            ProductoVolumen productoVolumen = new ProductoVolumen();

                            productoVolumen.Nombre = item.Producto?.Den;
                            productoVolumen.Disponible = VolumenTotalDisponible;
                            productoVolumen.Congelado = VolumenCongelado;
                            productoVolumen.Consumido = VolumenConsumido;
                            productoVolumen.Total = VolumenDisponible;
                            productoVolumen.Solicitud = VolumenSolicitado;
                            productoVolumen.Programado = VolumenProgramado;

                            volumen.Productos.Add(productoVolumen);
                        }
                        return Ok(volumen);

                    }
                    else
                    {
                        var ordenes = context.OrdenCierre.Where(x => x.Folio == filtro.Folio)
                            .Include(x => x.Producto)
                            .Include(x => x.OrdenEmbarque)
                            .ThenInclude(x => x!.Orden)
                            .Include(x => x.OrdenEmbarque)
                            .ThenInclude(x => x!.Tonel).ToList();

                        ordenes = ordenes.DistinctBy(x => x.Cod).ToList();

                        foreach (var item in ordenes.DistinctBy(x => x.Producto?.Den))
                        {
                            var VolumenDisponible = ordenes.Where(x => x.CodPrd == item.CodPrd && x.Estatus is true).Sum(x => x.Volumen);

                            var listConsumido = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Tonel != null && x.OrdenEmbarque.Codprd == item.CodPrd
                            && x.OrdenEmbarque.Codest == 22
                            && x.OrdenEmbarque.Folio != null
                            && x.OrdenEmbarque.Bolguidid != null)
                            .Include(x => x.OrdenEmbarque)
                            .ThenInclude(x => x.Tonel).ToList();

                            var VolumenCongelado = listConsumido.Sum(item => item.OrdenEmbarque!.Compartment == 1 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom!.ToString())
                                            : item.OrdenEmbarque!.Compartment == 2 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom2!.ToString())
                                            : item.OrdenEmbarque!.Compartment == 3 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom3!.ToString())
                                            : item.OrdenEmbarque!.Compartment == 4 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom4!.ToString())
                                            : item.OrdenEmbarque!.Vol);

                            var VolumenConsumido = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Orden != null && x.OrdenEmbarque.Folio != null
                                && x.OrdenEmbarque.Orden.Codest != 14
                                && x.OrdenEmbarque.Codest != 14
                            && x.OrdenEmbarque.Codprd == item.CodPrd
                            && x.OrdenEmbarque.Orden.BatchId != null)
                                .Include(x => x.OrdenEmbarque)
                                .ThenInclude(x => x.Orden)
                                .Sum(x => x.OrdenEmbarque!.Orden!.Vol);

                            var VolumenProgramado = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Folio == null
                                && x.OrdenEmbarque.Codprd == item.CodPrd
                                && x.OrdenEmbarque.Bolguidid == null && x.OrdenEmbarque.Codest == 3
                                && x.OrdenEmbarque.FchOrd != null)
                                .Include(x => x.OrdenEmbarque)
                                    .Sum(x => x.OrdenEmbarque!.Vol);

                            var VolumenSolicitado = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Folio == null
                            && x.OrdenEmbarque.Codprd == item.CodPrd
                            && x.OrdenEmbarque.Bolguidid == null && x.OrdenEmbarque.Codest == 9
                            && x.OrdenEmbarque.FchOrd == null)
                                .Include(x => x.OrdenEmbarque)
                                .Sum(x => x.OrdenEmbarque!.Vol);

                            var VolumenTotalDisponible = VolumenDisponible - (VolumenConsumido + VolumenCongelado + VolumenProgramado);

                            ProductoVolumen productoVolumen = new ProductoVolumen();
                            productoVolumen.Nombre = item.Producto?.Den;
                            productoVolumen.Disponible = VolumenTotalDisponible;
                            productoVolumen.Congelado = VolumenCongelado;
                            productoVolumen.Consumido = VolumenConsumido;
                            productoVolumen.Total = VolumenDisponible;
                            productoVolumen.Solicitud = VolumenSolicitado;
                            productoVolumen.Programado = VolumenProgramado;

                            volumen.Productos.Add(productoVolumen);
                        }
                        return Ok(volumen);
                    }
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
                throw;
            }
        }

        [HttpPost("activo")]
        public async Task<ActionResult> CheckCierreActivo([FromBody] OrdenCierre orden)
        {
            try
            {
                var producto = context.Producto.Find(orden.CodPrd);
                var ordens = await context.OrdenCierre.Where(x => x.CodCte == orden.CodCte && x.Activa == true && x.Estatus == true).OrderByDescending(x => x.FchCierre).Take(100)
                    .Include(x => x.Producto)
                    .ToListAsync();

                foreach (var item in ordens)
                {
                    if (item.Activa is true && producto?.Cod == item.CodPrd && item.CodPed == 0)
                    {
                        return BadRequest($"El producto {item.Producto?.Den} aun cuenta con un cierre activo. Folio: {item.Folio}");
                    }

                    if (item.Activa is true && producto?.Cod == item.CodPrd)
                    {
                        return BadRequest($"El producto {item.Producto?.Den} aun cuenta con ordenes disponibles. Folio: {item.Folio}");
                    }
                }

                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("cerrar/pedido/{folio}")]
        public async Task<ActionResult> ClosePedido([FromRoute] string folio)
        {
            try
            {
                if (string.IsNullOrEmpty(folio))
                    return BadRequest("Folio vacio o no valido");

                var ordens = context.OrdenCierre.Where(x => x.Folio.Equals(folio)).ToList();

                foreach (var item in ordens)
                {
                    await CloseOrden(item.Cod);
                }

                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("cerrar/orden/{cod:int}")]
        public async Task<ActionResult> CloseOrden([FromRoute] int cod)
        {
            try
            {
                if (cod == 0)
                    return BadRequest("Orden no valida");

                var orden = context.OrdenCierre.Find(cod);

                if (orden is null)
                    return BadRequest("No se encontro la orden");

                orden.Activa = false;
                orden.Estatus = false;

                context.Update(orden);

                var id = await verifyUser.GetId(HttpContext, UserManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                await context.SaveChangesAsync(id, 16);

                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("folios")]
        public async Task<ActionResult> GetFoliosValidos()
        {
            try
            {
                List<string?> folios = new List<string?>();

                folios = context.OrdenCierre.Where(x => x.FchCierre >= DateTime.Today.AddDays(-10) && x.FchCierre <= DateTime.Today.AddDays(1)
                && !string.IsNullOrEmpty(x.Folio) && x.Activa == true)
                    .Select(x => x.Folio)
                    .Distinct()
                    .ToList();

                return Ok(folios);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("folios/detalle")]
        public async Task<ActionResult> GetFoliosValidosActivos([FromBody] CierreFiltroDTO filtro)
        {
            try
            {
                List<FolioDetalleDTO> folios = new List<FolioDetalleDTO>();
                //Filtro de órdenes mediante la obtención del folio
                if (!filtro.forFolio)
                {
                    folios = await context.OrdenCierre.OrderBy(x => x.FchCierre).Where(
                x => x.FchCierre >= DateTime.Today.AddDays(-10) && x.FchCierre <= DateTime.Today.AddDays(1)
                && !string.IsNullOrEmpty(x.Folio)
                && x.Activa == true
                && x.CodPed == 0
                && x.Estatus == true ||
                x.FchCierre >= DateTime.Today.AddDays(-10) && x.FchCierre <= DateTime.Today.AddDays(1)
                && !string.IsNullOrEmpty(x.Folio)
                && x.Activa == true
                && x.Folio.StartsWith("OP")
                && x.Estatus == true)
                    .Include(x => x.Cliente)
                    .Include(x => x.Destino)
                    .Include(x => x.Producto)
                    .Include(x => x.Grupo)
                    .Select(x => new FolioDetalleDTO()
                    {
                        Folio = x.Folio,
                        Grupo = x.Grupo,
                        Cliente = x.Cliente,
                        Destino = x.Destino,
                        Producto = x.Producto,
                        FchCierre = x.FchCierre,
                        Comentarios = x.Observaciones
                    })
                    .OrderByDescending(x => x.FchCierre)
                    .ToListAsync();
                }
                //Filtro de órdenes obteniendo el cliente con su grupo empresarial
                else
                {
                    if (filtro.codCte != null && filtro.codGru != null)
                        folios = await context.OrdenCierre.OrderBy(x => x.FchCierre).Where(x => x.FchCierre >= filtro.FchInicio && x.FchCierre <= filtro.FchFin
                    && !string.IsNullOrEmpty(x.Folio) && x.Activa == true && x.CodPed == 0 && x.Estatus == true && x.CodGru == filtro.codGru && x.CodCte == filtro.codCte ||
                      //x.FchCierre >= DateTime.Today.AddDays(-10) && x.FchCierre <= DateTime.Today.AddDays(1)
                      x.FchCierre >= filtro.FchInicio && x.FchCierre <= filtro.FchFin
                    && !string.IsNullOrEmpty(x.Folio)
                    && x.Activa == true
                    && x.Folio.StartsWith("OP")
                    && x.Estatus == true && x.CodGru == filtro.codGru && x.CodCte == filtro.codCte)
                        .Include(x => x.Cliente)
                        .Include(x => x.Destino)
                        .Include(x => x.Producto)
                         .Include(x => x.Grupo)
                        .Select(x => new FolioDetalleDTO()
                        {
                            Folio = x.Folio,
                            Grupo = x.Grupo,
                            Cliente = x.Cliente,
                            Destino = x.Destino,
                            Producto = x.Producto,
                            FchCierre = x.FchCierre,
                            Comentarios = x.Observaciones

                        })
                    .OrderByDescending(x => x.FchCierre)
                        .ToListAsync();

                    //Filtro de órdenes solamente obteniendo el grupo
                    else if (filtro.codGru != null)
                    {
                        folios = await context.OrdenCierre.OrderBy(x => x.FchCierre).Where(x => x.FchCierre >= filtro.FchInicio && x.FchCierre <= filtro.FchFin
                    && !string.IsNullOrEmpty(x.Folio) && x.Activa == true && x.CodPed == 0 && x.Estatus == true && x.CodGru == filtro.codGru ||
                    x.FchCierre >= DateTime.Today.AddDays(-10) && x.FchCierre <= DateTime.Today.AddDays(1)
                    && !string.IsNullOrEmpty(x.Folio)
                    && x.Activa == true
                    && x.Folio.StartsWith("OP")
                    && x.Estatus == true && x.CodGru == filtro.codGru)
                    .Include(x => x.Cliente)
                    .Include(x => x.Destino)
                    .Include(x => x.Producto)
                     .Include(x => x.Grupo)
                    .Select(x => new FolioDetalleDTO()
                    {
                        Folio = x.Folio,
                        Grupo = x.Grupo,
                        Cliente = x.Cliente,
                        Destino = x.Destino,
                        Producto = x.Producto,
                        FchCierre = x.FchCierre,
                        Comentarios = x.Observaciones
                    })
                    .OrderByDescending(x => x.FchCierre)
                    .ToListAsync();
                    }
                    //Filtro de órdenes por rango de fechas
                    //  x.FchCierre >= DateTime.Today.AddDays(-10) && x.FchCierre <= DateTime.Today.AddDays(1)
                    else
                    {
                        folios = await context.OrdenCierre.OrderBy(x => x.FchCierre).Where(x => x.FchCierre >= filtro.FchInicio && x.FchCierre <= filtro.FchFin
                    && !string.IsNullOrEmpty(x.Folio) && x.Activa == true && x.CodPed == 0 && x.Estatus == true
                    ||
                    x.FchCierre >= filtro.FchInicio && x.FchCierre <= filtro.FchFin
                    && !string.IsNullOrEmpty(x.Folio)
                    && x.Activa == true
                    && x.Folio.StartsWith("OP")
                    && x.Estatus == true)
                    .Include(x => x.Cliente)
                    .Include(x => x.Destino)
                    .Include(x => x.Producto)
                     .Include(x => x.Grupo)
                    .Select(x => new FolioDetalleDTO()
                    {
                        Folio = x.Folio,
                        Grupo = x.Grupo,
                        Cliente = x.Cliente,
                        Destino = x.Destino,
                        Producto = x.Producto,
                        FchCierre = x.FchCierre,
                        Comentarios = x.Observaciones
                    })
                    .OrderByDescending(x => x.FchCierre)
                    .ToListAsync();
                    }

                }
                return Ok(folios);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("folios/detalle/status")]
        public async Task<ActionResult> GetFoliosValidosActivosStatus([FromBody] CierreFiltroDTO filtro)
        {
            try
            {
                List<FolioDetalleDTO> folios = new List<FolioDetalleDTO>();
                //Cuando se sabe el folio de la orden
                if (!filtro.forFolio)
                    folios = await context.OrdenCierre.Where(
                x => x.FchCierre >= DateTime.Today.AddDays(-10) && x.FchCierre <= DateTime.Today.AddDays(1)
                && !string.IsNullOrEmpty(x.Folio)
                && x.Activa == true
                && x.CodPed == 0
                && x.Estatus == true ||
                x.FchCierre >= DateTime.Today.AddDays(-10) && x.FchCierre <= DateTime.Today.AddDays(1)
                && !string.IsNullOrEmpty(x.Folio)
                && x.Activa == true
                && x.Folio.StartsWith("OP")
                && x.Estatus == true)
                    .Include(x => x.Cliente)
                    .Include(x => x.Destino)
                    .Include(x => x.Producto)
                     .Include(x => x.Grupo)
                    .Select(x => new FolioDetalleDTO()
                    {
                        Folio = x.Folio,
                        Grupo = x.Grupo,
                        Cliente = x.Cliente,
                        Destino = x.Destino,
                        Producto = x.Producto,
                        FchCierre = x.FchCierre,
                        Comentarios = x.Observaciones
                    })
                    .OrderByDescending(x => x.FchCierre)
                    .ToListAsync();
                else
                {
                    //Cuando se filtra por grupo empresarial y cliente
                    if (filtro.codCte != null && filtro.codGru != null)
                        folios = await context.OrdenCierre.Where(x => x.FchCierre >= filtro.FchInicio && x.FchCierre <= filtro.FchFin
                    && !string.IsNullOrEmpty(x.Folio) && x.CodPed == 0 && x.Estatus == true && x.CodGru == filtro.codGru && x.CodCte == filtro.codCte ||
                      //x.FchCierre >= DateTime.Today.AddDays(-10) && x.FchCierre <= DateTime.Today.AddDays(1)
                      x.FchCierre >= filtro.FchInicio && x.FchCierre <= filtro.FchFin
                    && !string.IsNullOrEmpty(x.Folio)
                    && x.Folio.StartsWith("OP")
                    && x.Estatus == true && x.CodGru == filtro.codGru && x.CodCte == filtro.codCte)
                        .Include(x => x.Cliente)
                         .Include(x => x.Grupo)
                        .Include(x => x.Destino)
                        .Include(x => x.Producto)
                        .Include(x => x.OrdenEmbarque)
                        .ThenInclude(x => x.Orden)
                        .ThenInclude(x => x.Estado)
                        .Select(x => new FolioDetalleDTO()
                        {
                            Folio = x.Folio,
                            Grupo = x.Grupo,
                            Cliente = x.Cliente,
                            Destino = x.Destino,
                            Producto = x.Producto,
                            FchCierre = x.FchCierre,
                            Comentarios = x.Observaciones,
                            Estado = x.OrdenEmbarque.Estado.den,
                            Activa = x.Activa
                        })
                    .OrderByDescending(x => x.FchCierre)
                        .ToListAsync();
                    //Cuando se filtra solo por su grupo 
                    else if (filtro.codGru != null)
                        folios = await context.OrdenCierre.Where(x => x.FchCierre >= filtro.FchInicio && x.FchCierre <= filtro.FchFin
                    && !string.IsNullOrEmpty(x.Folio) && x.CodPed == 0 && x.Estatus == true && x.CodGru == filtro.codGru ||
                    x.FchCierre >= DateTime.Today.AddDays(-10) && x.FchCierre <= DateTime.Today.AddDays(1)
                    && !string.IsNullOrEmpty(x.Folio)
                    && x.Folio.StartsWith("OP")
                    && x.Estatus == true && x.CodGru == filtro.codGru)
                    .Include(x => x.Cliente)
                     .Include(x => x.Grupo)
                    .Include(x => x.Destino)
                    .Include(x => x.Producto)
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x.Orden)
                    .ThenInclude(x => x.Estado)
                    .Select(x => new FolioDetalleDTO()
                    {
                        Folio = x.Folio,
                        Grupo = x.Grupo,
                        Cliente = x.Cliente,
                        Destino = x.Destino,
                        Producto = x.Producto,
                        FchCierre = x.FchCierre,
                        Comentarios = x.Observaciones,
                        Estado = x.OrdenEmbarque.Estado.den,
                        Activa = x.Activa
                    })
                    .OrderByDescending(x => x.FchCierre)
                    .ToListAsync();
                    else
                        //Solamente las fechas
                        folios = await context.OrdenCierre.Where(x => x.FchCierre >= filtro.FchInicio && x.FchCierre <= filtro.FchFin
                    && !string.IsNullOrEmpty(x.Folio) && x.CodPed == 0 && x.Estatus == true ||
                    //x.FchCierre >= DateTime.Today.AddDays(-10) && x.FchCierre <= DateTime.Today.AddDays(1)
                    x.FchCierre >= filtro.FchInicio && x.FchCierre <= filtro.FchFin
                    && !string.IsNullOrEmpty(x.Folio)
                    && x.Folio.StartsWith("OP")
                    && x.Estatus == true)
                    .Include(x => x.Cliente)
                     .Include(x => x.Grupo)
                    .Include(x => x.Destino)
                    .Include(x => x.Producto)
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x.Orden)
                    .ThenInclude(x => x.Estado)
                    .Select(x => new FolioDetalleDTO()
                    {
                        Folio = x.Folio,
                        Grupo = x.Grupo,
                        Cliente = x.Cliente,
                        Destino = x.Destino,
                        Producto = x.Producto,
                        FchCierre = x.FchCierre,
                        Comentarios = x.Observaciones,
                        Estado = x.OrdenEmbarque.Estado.den,
                        Activa = x.Activa
                    })
                    .OrderByDescending(x => x.FchCierre)
                    .ToListAsync();
                }
                return Ok(folios);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        //Filtro para cierre de pedidos
        [HttpPost("cierrefolio/detalle")]
        public async Task<ActionResult> GetFoliosCierres([FromBody] CierreDiarioDTO filtro)
        {
            try
            {
                List<FolioCierreDTO> folios = new List<FolioCierreDTO>();
                if (filtro.codCte != null && filtro.codGru != null)
                {
                    folios = await context.OrdenCierre.OrderBy(x => x.FchCierre)
                        .Where(x => x.FchCierre >= filtro.FchInicio && x.FchCierre <= filtro.FchFin && x.Folio.StartsWith("P")
                        && x.CodGru == filtro.codGru && x.CodCte == filtro.codCte)
                        .Include(x => x.Cliente)
                        .Include(x => x.Destino)
                        .Include(x => x.Producto)
                        .Include(x => x.Grupo)
                        .Include(x => x.OrdenEmbarque)
                        .ThenInclude(x => x.Orden)
                        .ThenInclude(x => x.Estado)
                        .Select(x => new FolioCierreDTO()
                        {
                            Folio = x.Folio,
                            cliente = x.Cliente,
                            destino = x.Destino,
                            Producto = x.Producto,
                            FchCierre = x.FchCierre,
                            Grupo = x.Grupo,
                            Estado = x.OrdenEmbarque.Estado.den,
                            Activa = x.Activa,
                            Precio = x.Precio,
                            Volumen = x.Volumen,
                            Observaciones = x.Observaciones
                        })
                        .OrderByDescending(x => x.FchCierre)
                        .ToListAsync();
                }
                else
                {
                    folios = await context.OrdenCierre.OrderBy(x => x.FchCierre)
                       .Where(x => x.FchCierre >= filtro.FchInicio && x.FchCierre <= filtro.FchFin && x.Folio.StartsWith("P"))
                       .Include(x => x.Cliente)
                       .Include(x => x.Destino)
                       .Include(x => x.Producto)
                       .Include(x => x.Grupo)
                       .Include(x => x.OrdenEmbarque)
                       .ThenInclude(x => x.Orden)
                       .ThenInclude(x => x.Estado)
                       .Select(x => new FolioCierreDTO()
                       {
                           Folio = x.Folio,
                           cliente = x.Cliente,
                           destino = x.Destino,
                           Producto = x.Producto,
                           FchCierre = x.FchCierre,
                           Grupo = x.Grupo,
                           Estado = x.OrdenEmbarque.Estado.den,
                           Activa = x.Activa,
                           Precio = x.Precio,
                           Volumen = x.Volumen,
                           Observaciones = x.Observaciones
                       })
                       .OrderByDescending(x => x.FchCierre)
                       .ToListAsync();
                }
                return Ok(folios);

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        //Filtro para cierres grupales
        [HttpPost("cierregrupo/detalle")]
        public async Task<ActionResult> GetCierresGrupales([FromBody] CierreDiarioDTO filtro)
        {
            try
            {
                List<FolioCierreDTO> folios = new List<FolioCierreDTO>();
                if (filtro.codGru != null)
                {
                    folios = await context.OrdenCierre.OrderBy(x => x.FchCierre)
                        .Where(x => x.FchCierre >= filtro.FchInicio && x.FchCierre <= filtro.FchFin && x.Folio.StartsWith("G")
                          && x.CodGru == filtro.codGru)
                        .Include(x => x.Cliente)
                        .Include(x => x.Destino)
                        .Include(x => x.Producto)
                        .Include(x => x.Grupo)
                        .Include(x => x.OrdenEmbarque)
                        .ThenInclude(x => x.Orden)
                        .ThenInclude(x => x.Estado)
                        .Select(x => new FolioCierreDTO()
                        {
                            Folio = x.Folio,
                            cliente = x.Cliente,
                            destino = x.Destino,
                            Producto = x.Producto,
                            FchCierre = x.FchCierre,
                            Grupo = x.Grupo,
                            Estado = x.OrdenEmbarque.Estado.den,
                            Activa = x.Activa,
                            Precio = x.Precio,
                            Volumen = x.Volumen,
                            Observaciones = x.Observaciones
                        })
                        .OrderByDescending(x => x.FchCierre)
                        .ToListAsync();
                }
                else
                {
                    folios = await context.OrdenCierre.OrderBy(x => x.FchCierre)
                       .Where(x => x.FchCierre >= filtro.FchInicio && x.FchCierre <= filtro.FchFin && x.Folio.StartsWith("G"))
                       .Include(x => x.Cliente)
                       .Include(x => x.Destino)
                       .Include(x => x.Producto)
                        .Include(x => x.Grupo)
                       .Include(x => x.OrdenEmbarque)
                       .ThenInclude(x => x.Orden)
                       .ThenInclude(x => x.Estado)
                       .Select(x => new FolioCierreDTO()
                       {
                           Folio = x.Folio,
                           cliente = x.Cliente,
                           destino = x.Destino,
                           Producto = x.Producto,
                           FchCierre = x.FchCierre,
                           Grupo = x.Grupo,
                           Estado = x.OrdenEmbarque.Estado.den,
                           Activa = x.Activa,
                           Precio = x.Precio,
                           Volumen = x.Volumen,
                           Observaciones = x.Observaciones
                       })
                       .OrderByDescending(x => x.FchCierre)
                       .ToListAsync();
                }
                return Ok(folios);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("folios/{cliente}")]
        public async Task<ActionResult> GetFoliosValidosFiltro([FromBody] CierreFiltroDTO filtroDTO, [FromRoute] int cliente)
        {
            try
            {
                List<string?> folios = new List<string?>();
                if (cliente != 0)
                    folios = context.OrdenCierre.Where(x => x.FchCierre >= filtroDTO.FchInicio && x.FchCierre <= filtroDTO.FchFin
                    && !string.IsNullOrEmpty(x.Folio) && x.Activa == true && x.CodCte == cliente)
                        .Select(x => x.Folio)
                        .Distinct()
                        .ToList();
                else
                    folios = context.OrdenCierre.Where(x => x.FchCierre >= filtroDTO.FchInicio && x.FchCierre <= filtroDTO.FchFin
                && !string.IsNullOrEmpty(x.Folio) && x.Activa == true)
                    .Select(x => x.Folio)
                    .Distinct()
                    .ToList();

                return Ok(folios);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("disponibles")]
        public async Task<ActionResult> GetCierresDisponiles([FromBody] CierreFiltroDTO filtroDTO)
        {
            try
            {
                if (filtroDTO is null)
                    return BadRequest();

                List<OrdenCierre> cierres = new List<OrdenCierre>();

                var cierresDis = await context.OrdenCierre.Where(x => x.FchCierre >= filtroDTO.FchInicio && x.FchCierre <= filtroDTO.FchFin
                && x.Activa == true && x.CodPed == 0)
                    .Include(x => x.Producto)
                    .Include(x => x.Destino)
                    .Include(x => x.Cliente)
                    .Include(x => x.Grupo)
                    .ToListAsync();

                if (cierresDis is null)
                    return Ok(cierres);

                Porcentaje porcentaje = new Porcentaje();
                var por = context.Porcentaje.FirstOrDefault(x => x.Accion == "cierre");
                if (por != null)
                    porcentaje = por;

                foreach (var item in cierresDis)
                {
                    if (context.OrdenPedido.Any(x => x.Folio.Equals(item.Folio)))
                    {

                        var VolumenDisponible = item.Volumen;

                        var listConsumido = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Tonel != null && x.OrdenEmbarque.Codprd == item.CodPrd
                            && x.OrdenEmbarque.Codest == 22
                            && x.OrdenEmbarque.Folio != null
                            && x.OrdenEmbarque.Bolguidid != null)
                            .Include(x => x.OrdenEmbarque)
                            .ThenInclude(x => x.Tonel).ToList();

                        var VolumenCongelado = listConsumido.Sum(item => item.OrdenEmbarque!.Compartment == 1 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom!.ToString())
                                        : item.OrdenEmbarque!.Compartment == 2 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom2!.ToString())
                                        : item.OrdenEmbarque!.Compartment == 3 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom3!.ToString())
                                        : item.OrdenEmbarque!.Compartment == 4 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom4!.ToString())
                                        : item.OrdenEmbarque!.Vol);

                        var countCongelado = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Codprd == item.CodPrd
                        && x.OrdenEmbarque.Codest == 22
                        && x.OrdenEmbarque.Folio != null)
                            .Include(x => x.OrdenEmbarque)
                            .Count();

                        var VolumenConsumido = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Orden != null && x.OrdenEmbarque.Folio != null
                            && x.OrdenEmbarque.Orden.Codest != 14
                            && x.OrdenEmbarque.Codest != 14
                        && x.OrdenEmbarque.Codprd == item.CodPrd
                        && x.OrdenEmbarque.Orden.BatchId != null)
                            .Include(x => x.OrdenEmbarque)
                            .ThenInclude(x => x.Orden)
                            .Sum(x => x.OrdenEmbarque!.Orden!.Vol);

                        var countConsumido = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Orden != null && x.OrdenEmbarque.Folio != null
                            && x.OrdenEmbarque.Orden.Codest != 14
                            && x.OrdenEmbarque.Codest != 14
                            && x.OrdenEmbarque.Codprd == item.CodPrd
                            && x.OrdenEmbarque.Orden.BatchId != null)
                            .Include(x => x.OrdenEmbarque)
                            .ThenInclude(x => x.Orden)
                            .Count();

                        var VolumenProgramado = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Folio == null
                            && x.OrdenEmbarque.Codprd == item.CodPrd
                            && x.OrdenEmbarque.Bolguidid == null && x.OrdenEmbarque.Codest == 3
                            && x.OrdenEmbarque.FchOrd != null)
                            .Include(x => x.OrdenEmbarque)
                                .Sum(x => x.OrdenEmbarque!.Vol);

                        var CountVolumenProgramado = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Folio == null
                            && x.OrdenEmbarque.Codprd == item.CodPrd
                            && x.OrdenEmbarque.Bolguidid == null && x.OrdenEmbarque.Codest == 3
                            && x.OrdenEmbarque.FchOrd != null)
                            .Include(x => x.OrdenEmbarque).Count();

                        var VolumenSolicitado = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Folio == null
                        && x.OrdenEmbarque.Codprd == item.CodPrd
                        && x.OrdenEmbarque.Bolguidid == null && x.OrdenEmbarque.Codest == 9
                        && x.OrdenEmbarque.FchOrd == null)
                            .Include(x => x.OrdenEmbarque)
                            .Sum(x => x.OrdenEmbarque!.Vol);

                        var VolumenTotalDisponible = VolumenDisponible - (VolumenConsumido + VolumenCongelado + VolumenProgramado);
                        double? PromedioCargas = 0;
                        var sumVolumen = VolumenConsumido + VolumenCongelado + VolumenProgramado;
                        var sumCount = countCongelado + countConsumido + CountVolumenProgramado;

                        if (sumVolumen != 0 && sumCount != 0)
                            PromedioCargas = sumVolumen / sumCount;

                        ProductoVolumen productoVolumen = new ProductoVolumen();

                        productoVolumen.Nombre = item.Producto?.Den;
                        productoVolumen.Disponible = VolumenTotalDisponible;
                        productoVolumen.Congelado = VolumenCongelado;
                        productoVolumen.Consumido = VolumenConsumido;
                        productoVolumen.Total = VolumenDisponible;
                        productoVolumen.PromedioCarga = PromedioCargas;
                        productoVolumen.Solicitud = VolumenSolicitado;
                        productoVolumen.Programado = VolumenProgramado;

                        item.VolumenDisponible?.Productos?.Add(productoVolumen);

                    }
                    //if (item.VolumenDisponible?.Productos?.FirstOrDefault()?.PromedioCarga >=
                    //(item.VolumenDisponible?.Productos?.FirstOrDefault()?.Disponible * (porcentaje.Porcen/100)))
                    cierres.Add(item);
                }

                return Ok(cierres);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("cerrar/auto")]
        public async Task<ActionResult> CerrarCierresPorPorcentaje([FromBody] CierreFiltroDTO filtroDTO)
        {
            try
            {
                if (filtroDTO is null)
                    return BadRequest();

                var id = await verifyUser.GetId(HttpContext, UserManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                List<OrdenCierre> cierres = new List<OrdenCierre>();

                var cierresDis = await context.OrdenCierre.Where(x => x.FchCierre >= filtroDTO.FchInicio && x.FchCierre <= filtroDTO.FchFin
                && x.Activa == true && x.CodPed == 0)
                    .Include(x => x.Producto)
                    .ToListAsync();

                if (cierresDis is null)
                    return Ok(cierres);

                Porcentaje porcentaje = new Porcentaje();
                var por = context.Porcentaje.FirstOrDefault(x => x.Accion == "cierre");
                if (por != null)
                    porcentaje = por;

                foreach (var item in cierresDis)
                {
                    if (context.OrdenPedido.Any(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio)))
                    {

                        var VolumenDisponible = item.Volumen;

                        var listConsumido = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Tonel != null && x.OrdenEmbarque.Codprd == item.CodPrd
                            && x.OrdenEmbarque.Codest == 22
                            && x.OrdenEmbarque.Folio != null
                            && x.OrdenEmbarque.Bolguidid != null)
                            .Include(x => x.OrdenEmbarque)
                            .ThenInclude(x => x.Tonel).ToList();

                        var VolumenCongelado = listConsumido.Sum(item => item.OrdenEmbarque!.Compartment == 1 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom!.ToString())
                                        : item.OrdenEmbarque!.Compartment == 2 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom2!.ToString())
                                        : item.OrdenEmbarque!.Compartment == 3 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom3!.ToString())
                                        : item.OrdenEmbarque!.Compartment == 4 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom4!.ToString())
                                        : item.OrdenEmbarque!.Vol);

                        var countCongelado = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Codprd == item.CodPrd
                        && x.OrdenEmbarque.Codest == 22
                        && x.OrdenEmbarque.Folio != null)
                            .Include(x => x.OrdenEmbarque)
                            .Count();

                        var VolumenConsumido = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Orden != null && x.OrdenEmbarque.Folio != null
                            && x.OrdenEmbarque.Orden.Codest != 14
                            && x.OrdenEmbarque.Codest != 14
                        && x.OrdenEmbarque.Codprd == item.CodPrd
                        && x.OrdenEmbarque.Orden.BatchId != null)
                            .Include(x => x.OrdenEmbarque)
                            .ThenInclude(x => x.Orden)
                            .Sum(x => x.OrdenEmbarque!.Orden!.Vol);

                        var countConsumido = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Orden != null && x.OrdenEmbarque.Folio != null
                            && x.OrdenEmbarque.Orden.Codest != 14
                            && x.OrdenEmbarque.Codest != 14
                            && x.OrdenEmbarque.Codprd == item.CodPrd
                            && x.OrdenEmbarque.Orden.BatchId != null)
                            .Include(x => x.OrdenEmbarque)
                            .ThenInclude(x => x.Orden)
                            .Count();

                        var VolumenProgramado = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Folio == null
                            && x.OrdenEmbarque.Codprd == item.CodPrd
                            && x.OrdenEmbarque.Bolguidid == null && x.OrdenEmbarque.Codest == 3
                            && x.OrdenEmbarque.FchOrd != null)
                            .Include(x => x.OrdenEmbarque)
                                .Sum(x => x.OrdenEmbarque!.Vol);

                        var CountVolumenProgramado = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Folio == null
                            && x.OrdenEmbarque.Codprd == item.CodPrd
                            && x.OrdenEmbarque.Bolguidid == null && x.OrdenEmbarque.Codest == 3
                            && x.OrdenEmbarque.FchOrd != null)
                            .Include(x => x.OrdenEmbarque).Count();

                        var VolumenSolicitado = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Folio == null
                        && x.OrdenEmbarque.Codprd == item.CodPrd
                        && x.OrdenEmbarque.Bolguidid == null && x.OrdenEmbarque.Codest == 9
                        && x.OrdenEmbarque.FchOrd == null)
                            .Include(x => x.OrdenEmbarque)
                            .Sum(x => x.OrdenEmbarque!.Vol);

                        var VolumenTotalDisponible = VolumenDisponible - (VolumenConsumido + VolumenCongelado + VolumenProgramado);

                        double? PromedioCargas = 0;
                        var sumVolumen = VolumenConsumido + VolumenCongelado + VolumenProgramado;
                        var sumCount = countCongelado + countConsumido + CountVolumenProgramado;

                        if (sumVolumen != 0 && sumCount != 0)
                            PromedioCargas = sumVolumen / sumCount;

                        ProductoVolumen productoVolumen = new ProductoVolumen();

                        productoVolumen.Nombre = item.Producto?.Den;
                        productoVolumen.Disponible = VolumenTotalDisponible;
                        productoVolumen.Congelado = VolumenCongelado;
                        productoVolumen.Consumido = VolumenConsumido;
                        productoVolumen.Total = VolumenDisponible;
                        productoVolumen.PromedioCarga = PromedioCargas;
                        productoVolumen.Solicitud = VolumenSolicitado;
                        productoVolumen.Programado = VolumenProgramado;

                        item.VolumenDisponible?.Productos?.Add(productoVolumen);

                    }

                    if (item.VolumenDisponible?.Productos?.FirstOrDefault()?.PromedioCarga >=
                    (item.VolumenDisponible?.Productos?.FirstOrDefault()?.Disponible * (porcentaje.Porcen / 100)))
                    {
                        item.Activa = false;
                        cierres.Add(item);
                    }
                }
                context.UpdateRange(cierres);

                await context.SaveChangesAsync(id, 16);

                return Ok(cierres);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("filtrar/pendientes")]
        public async Task<ActionResult> GetCierresPendentes([FromBody] FechasF fechas)
        {
            try
            {
                List<OrdenCierre> cierres = new List<OrdenCierre>();
                cierres = await context.OrdenCierre.Where(x => x.FchCierre >= fechas.DateInicio && x.FchCierre <= fechas.DateFin && x.Confirmada == false
                && x.Activa == true && x.Estatus == true)
                    .Include(x => x.Cliente)
                    .Include(x => x.Destino)
                    .Include(x => x.Producto)
                    .Include(x => x.Grupo)
                    .OrderBy(x => x.FchCierre)
                    .Take(1000)
                    .ToListAsync();

                return Ok(cierres);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
                throw;
            }
        }

        [HttpPost("confirm")]
        public async Task<ActionResult> ConfirmCierresPendentes([FromBody] List<OrdenCierre> cierres)
        {
            try
            {

                cierres.ForEach(x =>
                {
                    x.Cliente = null!;
                    x.Destino = null!;
                    x.OrdenEmbarque = null!;
                    x.Grupo = null!;
                    x.Producto = null!;
                    x.VolumenDisponible = null!;
                    x.Confirmada = true;
                });

                context.UpdateRange(cierres);

                var id = await verifyUser.GetId(HttpContext, UserManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                await context.SaveChangesAsync(id, 32);

                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
                throw;
            }
        }

        [HttpGet("caducidad/verify")]
        public async Task<ActionResult> VerificarCaducidad([FromQuery] CierreFiltroDTO dTO)
        {
            try
            {
                if (string.IsNullOrEmpty(dTO.Folio))
                    return BadRequest("No se encontro un folio");

                var activo = false;
                var fchActiva = string.Empty;

                var cierre = context.OrdenCierre.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(dTO.Folio)).ToList();

                foreach (var item in cierre)
                    if (item?.FchVencimiento > DateTime.Today)
                        activo = true;
                    else
                        fchActiva = item?.FchVen;

                if (cierre is not null)
                {
                    if (activo)
                        return Ok(activo);
                    else
                        return BadRequest($"Este pedido ya no se encuentra vigente.Fecha de vecimiento: {fchActiva}");
                }
                else
                    return BadRequest($"No se encontro algun cierre perteneciente al folio {dTO.Folio}");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("caducidad/cerrar")]
        public async Task<ActionResult> CerrarCierreAutoCaducidad()
        {
            try
            {
                List<OrdenCierre> cierres = new List<OrdenCierre>();

                cierres = context.OrdenCierre.Where(x => !string.IsNullOrEmpty(x.Folio) && x.FchVencimiento < DateTime.Today && x.Activa == true && x.Folio.StartsWith("P")
                || !string.IsNullOrEmpty(x.Folio) && x.FchVencimiento < DateTime.Today && x.Activa == true && x.Folio.StartsWith("G"))
                    .OrderByDescending(x => x.FchVencimiento)
                    .ToList();

                foreach (var item in cierres)
                {
                    item.Activa = false;
                    context.Update(item);
                }

                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("caducidad/abrir")]
        public async Task<ActionResult> AbrirCierreAutoCaducidad()
        {
            try
            {
                List<OrdenCierre> cierres = new List<OrdenCierre>();

                cierres = context.OrdenCierre.Where(x => x.FchVencimiento > DateTime.Today && x.Activa == false).ToList();

                foreach (var item in cierres)
                {
                    item.Activa = true;
                    context.Update(item);
                }

                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("autocreate/orden")]
        public async Task<ActionResult> AutocrearOrdenes([FromBody] OrdenCierre cierre)
        {
            try
            {
                List<OrdenEmbarque> embarques = new List<OrdenEmbarque>();

                if (cierre is null)
                    return BadRequest("No se envio cierre para crear ordenes");

                var newCierre = context.OrdenCierre.Where(x => x.Cod == cierre.Cod)
                    .Include(x => x.Producto)
                    .Include(x => x.Destino)
                    .Include(x => x.Cliente)
                    .Include(x => x.OrdenPedidos)
                    .ThenInclude(x => x.OrdenEmbarque)
                    .FirstOrDefault();

                if (newCierre is null)
                    return BadRequest();

                newCierre.SetVolumen();

                if ((cierre.Volumen_Por_Unidad * cierre.Cantidad_Confirmada) > newCierre.GetVolumenDisponible())
                    return BadRequest($"No tiene suficiente volumen disponible. Disponible: {newCierre.GetVolumenDisponible()}. Solicitado: {cierre.Volumen_Por_Unidad * cierre.Cantidad_Confirmada}");

                if ((cierre.Volumen_Por_Unidad * cierre.Cantidad_Confirmada) > newCierre.GetVolumenDisponible())
                    return BadRequest($"No tiene suficiente volumen disponible. Disponible: {cierre.GetVolumenDisponible()}. Solicitado: {cierre.Volumen_Por_Unidad * cierre.Cantidad_Confirmada}");

                for (int i = 0; i < cierre.Cantidad_Confirmada; i++)
                {
                    //var litros = cierre.Volumen_Seleccionado >= 42000 ? cierre.Volumen_Seleccionado / 2 : 20000;

                    var bin = await context.OrdenEmbarque.Select(x => x.Bin).OrderBy(x => x).LastOrDefaultAsync();

                    var embarque = new OrdenEmbarque()
                    {
                        Codprd = cierre.CodPrd,
                        Coddes = cierre.CodDes,
                        Codtad = cierre.CodTad,
                        Pre = cierre.Precio,
                        Fchpet = DateTime.Now,
                        Codest = 9,
                        Vol = cierre.Volumen_Por_Unidad,
                        Fchcar = cierre.FchCar,
                        Bin = i == 0 ? ++bin : i % 2 == 0 ? ++bin : bin
                    };

                    context.Add(embarque);
                    await context.SaveChangesAsync();

                    var ordenpedido = new OrdenPedido()
                    {
                        CodPed = embarque.Cod,
                        CodCierre = newCierre.Cod,
                        Folio = newCierre.Folio,
                        OrdenEmbarque = null,
                        OrdenCierre = null
                    };

                    context.Add(ordenpedido);
                    await context.SaveChangesAsync();

                    var ordencierre = cierre.ShallowCopy();
                    ordencierre.Cod = 0;
                    ordencierre.Vendedor = string.Empty;
                    ordencierre.Observaciones = string.Empty;
                    ordencierre.CodPed = embarque.Cod;
                    ordencierre.Folio = $"O-{Guid.NewGuid().ToString().Split("-")[4]}";
                    ordencierre.Activa = true;
                    ordencierre.Estatus = true;
                    ordencierre.Cliente = null;
                    ordencierre.Destino = null;
                    ordencierre.OrdenEmbarque = null;
                    ordencierre.OrdenPedidos = null;
                    ordencierre.Producto = null;
                    ordencierre.Grupo = null;
                    ordencierre.Volumen = (int)embarque.Vol;

                    context.Add(ordencierre);
                    await context.SaveChangesAsync();
                    //embarques.Add(embarque);
                }

                //context.AddRange(embarques);
                //await context.SaveChangesAsync();

                return Ok(newCierre);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
