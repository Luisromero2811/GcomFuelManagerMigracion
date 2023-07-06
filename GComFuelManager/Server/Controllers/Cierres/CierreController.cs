using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace GComFuelManager.Server.Controllers.Cierres
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, Administrador Sistema, Direccion, Gerencia, Ejecutivo de Cuenta Comercial, Comprador, Programador, Ejecutivo de Cuenta Operativo, Lectura de Cierre de Orden, Cierre Pedidos")]
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

        [HttpGet("{folio}"), AllowAnonymous]
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
                List<OrdenCierre> cierresVolumen = new List<OrdenCierre>();
                var ordenes = await context.OrdenCierre.Where(x => x.Folio == folio && x.Estatus == true)
                    .Include(x => x.OrdenEmbarque)
                    .Include(x => x.Cliente)
                        .Include(x => x.Producto)
                        .Include(x => x.Destino)
                        .Include(x => x.ContactoN)
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x.Tad)
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x.Tonel)
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x.Estado)
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x.Orden)
                    .ThenInclude(x => x.Estado)
                    .ToListAsync();

                if (ordenes.Count > 0)
                {


                    if (ordenes.FirstOrDefault()!.CodPed != 0)
                    {
                        foreach (var item in ordenes)
                        {
                            if (item.OrdenEmbarque is not null)
                            {
                                if (item.OrdenEmbarque!.Folio != 0 && item.OrdenEmbarque!.Folio != null)
                                {
                                    var o = await context.Orden.Where(y => y.Ref!.Contains(item.OrdenEmbarque!.Folio.ToString()!)).Include(x => x.Estado).FirstOrDefaultAsync();
                                    if (o != null)
                                        ordenes.FirstOrDefault(x => x.Cod == item.Cod)!.OrdenEmbarque!.Orden = o;
                                    else
                                        ordenes.FirstOrDefault(x => x.Cod == item.Cod)!.OrdenEmbarque!.Orden = null!;
                                }
                            }
                        }
                    }
                    else
                    {
                        var pedidos = context.OrdenPedido.Where(x => x.Folio == ordenes.FirstOrDefault()!.Folio).ToList();
                        foreach (var item1 in pedidos)
                        {
                            var pedido = await context.OrdenCierre.Where(x => x.CodPed == item1.CodPed)
                                .Include(x => x.OrdenEmbarque)
                                .Include(x => x.Cliente)
                                .Include(x => x.Producto)
                                .Include(x => x.Destino)
                                .Include(x => x.ContactoN)
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
                                .FirstOrDefaultAsync();

                            cierresVolumen.Add(pedido);
                        }

                        return Ok(cierresVolumen);
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
                orden.OrdenEmbarque = null;
                var user = await UserManager.FindByNameAsync(HttpContext.User.FindFirstValue(ClaimTypes.Name)!);
                if (user == null)
                    return NotFound();

                if (await UserManager.IsInRoleAsync(user, "Comprador"))
                {
                    var userSis = context.Usuario.FirstOrDefault(x => x.Usu == user.UserName);
                    if (userSis == null)
                        return NotFound();
                    orden.CodCte = userSis.CodCte;
                    orden.CodGru = userSis.CodGru;
                    orden.Vendedor = userSis.Den;
                }
                var cliente = context.Cliente.FirstOrDefault(x => x.Cod == orden.CodCte);

                orden.TipoVenta = cliente.Tipven;

                if (string.IsNullOrEmpty(cliente.Tipven))
                    orden.ModeloVenta = cliente.Tipven.ToLower() == "rack" ? "Rack" : "Delivery";
                else
                    orden.ModeloVenta = string.Empty;

                orden.FchVencimiento = orden.FchCierre?.AddDays(8);
                context.Add(orden);
                await context.SaveChangesAsync();

                orden.Destino = await context.Destino.FirstOrDefaultAsync(x => x.Cod == orden.CodDes);
                orden.Producto = await context.Producto.FirstOrDefaultAsync(x => x.Cod == orden.CodPrd);
                orden.Cliente = await context.Cliente.FirstOrDefaultAsync(x => x.Cod == orden.CodCte);
                orden.ContactoN = await context.Contacto.FirstOrDefaultAsync(x => x.Cod == orden.CodCon);
                var Embarque = await context.OrdenEmbarque.Where(x => x.Cod == orden.CodPed).Include(x => x.Tad).FirstOrDefaultAsync();
                orden.OrdenEmbarque = Embarque;

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
                var Embarque = await context.OrdenEmbarque.Where(x => x.Cod == orden.CodPed).Include(x => x.Tad).FirstOrDefaultAsync();
                orden.OrdenEmbarque = Embarque;

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

                if (string.IsNullOrEmpty(HttpContext.User.FindFirstValue(ClaimTypes.Name)))
                    return BadRequest();

                var usuario = await UserManager.FindByNameAsync(HttpContext.User.FindFirstValue(ClaimTypes.Name)!);
                if (usuario == null)
                    return NotFound();

                var isClient = await UserManager.IsInRoleAsync(usuario, "Cliente Lectura");

                if (isClient)
                    filtroDTO.codCte = context.Usuario.Find(usuario.UserCod)!.CodCte;

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
                        .ToListAsync();

                }
                else
                {
                    if (!string.IsNullOrEmpty(filtroDTO.Folio))
                    {
                        if (isClient)
                        {
                            cierres = await context.OrdenCierre.Where(x => x.Folio == filtroDTO.Folio && x.Estatus == true && x.Folio.Contains(context.Cliente.Find(filtroDTO.codCte)!.CodCte!))
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
                            .ToListAsync();
                        }
                    }
                    else
                        return BadRequest("Debe escribir un folio valido.");
                }
                foreach (var item in cierres)
                {
                    if (item.OrdenEmbarque is not null)
                    {
                        if (item.OrdenEmbarque!.Folio != 0 && item.OrdenEmbarque!.Folio != null)
                        {
                            var o = await context.Orden.Where(y => y.Ref!.Contains(item.OrdenEmbarque!.Folio.ToString()!)).Include(x => x.Estado).FirstOrDefaultAsync();
                            if (o != null)
                                cierres.FirstOrDefault(x => x.Cod == item.Cod)!.OrdenEmbarque!.Orden = o;
                            else
                                cierres.FirstOrDefault(x => x.Cod == item.Cod)!.OrdenEmbarque!.Orden = null!;
                        }
                    }
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
                });

                context.UpdateRange(ordens);

                await context.SaveChangesAsync();

                var ordenes = await context.OrdenCierre.Where(x => x.Folio == fechas.Folio && x.Estatus == true)
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

                var user = await UserManager.FindByNameAsync(HttpContext.User.FindFirstValue(ClaimTypes.Name)!);
                if (user == null)
                    return NotFound();

                if (await UserManager.IsInRoleAsync(user, "Cliente Lectura"))
                {
                    var userSis = context.Usuario.FirstOrDefault(x => x.Usu == user.UserName);
                    if (userSis == null)
                        return NotFound();

                    filtro.codCte = userSis.CodCte;
                }

                if (!filtro.forFolio)
                {
                    var cierres = await context.OrdenCierre.Where(x => x.FchCierre >= filtro.FchInicio && x.FchCierre <= filtro.FchFin
                    && x.Estatus == true && x.CodCte == filtro.codCte)
                        .Include(x => x.Producto).Include(x => x.OrdenEmbarque).ThenInclude(x => x.Tonel).Include(x => x.OrdenEmbarque).ThenInclude(x => x.Orden).ToListAsync();

                    if (cierres is null)
                        return new VolumenDisponibleDTO();

                    foreach (var item in cierres)
                    {
                        if (context.OrdenPedido.Any(x => x.Folio.Equals(item.Folio)))
                        {
                            var pedidos = await context.OrdenPedido.Where(x => x.Folio!.Equals(filtro.Folio))
                                .Include(x => x.OrdenEmbarque).ThenInclude(x => x.Tonel).Include(x => x.OrdenEmbarque).ThenInclude(x => x.Orden).ToListAsync();

                            var VolumenDisponible = item.Volumen;

                            var VolumenCongelado = pedidos.Where(x => x.OrdenEmbarque?.Codprd == item.CodPrd
                            && x.OrdenEmbarque?.Folio is not null
                            && x.OrdenEmbarque?.Orden is null)
                                .Sum(item => item?.OrdenEmbarque?.Compartment == 1 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom?.ToString())
                                            : item?.OrdenEmbarque?.Compartment == 2 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom2?.ToString())
                                            : item?.OrdenEmbarque?.Compartment == 3 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom3?.ToString())
                                            : item?.OrdenEmbarque?.Compartment == 4 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom4?.ToString())
                                            : item?.OrdenEmbarque?.Vol);

                            var VolumenConsumido = pedidos.Where(x => x.OrdenEmbarque?.Folio is not null
                            && x.OrdenEmbarque.Codprd == item.CodPrd
                            && x.OrdenEmbarque?.Orden?.BatchId is not null)
                                .Sum(x => x.OrdenEmbarque?.Orden?.Vol);

                            var VolumenTotalDisponible = VolumenDisponible - (VolumenConsumido + VolumenCongelado);

                            ProductoVolumen productoVolumen = new ProductoVolumen();

                            productoVolumen.Nombre = item.Producto?.Den;
                            productoVolumen.Disponible = VolumenTotalDisponible;
                            productoVolumen.Congelado = VolumenCongelado;
                            productoVolumen.Consumido = VolumenConsumido;
                            productoVolumen.Total = VolumenDisponible;

                            if (volumen.Productos.Any(x => x.Nombre.Equals(item.Producto.Den)))
                            {

                                volumen.Productos.FirstOrDefault(x => x.Nombre!.Equals(item.Producto!.Den))!.Disponible += productoVolumen.Disponible;
                                volumen.Productos.FirstOrDefault(x => x.Nombre!.Equals(item.Producto!.Den))!.Congelado += productoVolumen.Congelado;
                                volumen.Productos.FirstOrDefault(x => x.Nombre!.Equals(item.Producto!.Den))!.Consumido += productoVolumen.Consumido;
                                volumen.Productos.FirstOrDefault(x => x.Nombre!.Equals(item.Producto!.Den))!.Total += productoVolumen.Total;
                            }
                            else
                                volumen.Productos.Add(productoVolumen);
                        }
                        else
                        {

                            var VolumenDisponible = item.Volumen;

                            var VolumenCongelado = item.OrdenEmbarque?.Folio is not null && item.OrdenEmbarque?.Orden is null ? item?.OrdenEmbarque?.Compartment == 1 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom?.ToString())
                                            : item?.OrdenEmbarque?.Compartment == 2 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom2?.ToString())
                                            : item?.OrdenEmbarque?.Compartment == 3 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom3?.ToString())
                                            : item?.OrdenEmbarque?.Compartment == 4 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom4?.ToString())
                                            : item?.OrdenEmbarque?.Vol : 0;

                            var VolumenConsumido = item.OrdenEmbarque?.Folio is not null && item.OrdenEmbarque?.Orden?.BatchId is not null
                                ? item.OrdenEmbarque?.Orden?.Vol : 0;

                            var VolumenTotalDisponible = VolumenDisponible - (VolumenConsumido + VolumenCongelado);

                            ProductoVolumen productoVolumen = new ProductoVolumen();
                            productoVolumen.Nombre = item.Producto!.Den;
                            productoVolumen.Disponible = VolumenTotalDisponible;
                            productoVolumen.Congelado = VolumenCongelado;
                            productoVolumen.Consumido = VolumenConsumido;
                            productoVolumen.Total = VolumenDisponible;

                            if (volumen.Productos.Any(x => x.Nombre.Equals(item.Producto.Den)))
                            {
                                volumen.Productos.FirstOrDefault(x => x.Nombre.Equals(item.Producto.Den)).Disponible += productoVolumen.Disponible;
                                volumen.Productos.FirstOrDefault(x => x.Nombre.Equals(item.Producto.Den)).Congelado += productoVolumen.Congelado;
                                volumen.Productos.FirstOrDefault(x => x.Nombre.Equals(item.Producto.Den)).Consumido += productoVolumen.Consumido;
                                volumen.Productos.FirstOrDefault(x => x.Nombre.Equals(item.Producto.Den)).Total += productoVolumen.Total;
                            }
                            else
                                volumen.Productos.Add(productoVolumen);

                        }
                    }
                    return Ok(volumen);
                }
                else
                {
                    if (context.OrdenPedido.Any(x => x.Folio.Equals(filtro.Folio)))
                    {
                        var cierres = await context.OrdenCierre.Where(x => x.Folio!.Equals(filtro.Folio)).Include(x => x.Producto).ToListAsync();
                        if (cierres is null)
                            return BadRequest("No existe el cierre.");

                        var pedidos = await context.OrdenPedido.Where(x => x.Folio!.Equals(filtro.Folio))
                            .Include(x => x.OrdenEmbarque).ThenInclude(x => x.Orden).Include(x => x.OrdenEmbarque).ThenInclude(x => x.Tonel).ToListAsync();

                        foreach (var item in cierres)
                        {
                            var VolumenDisponible = item.Volumen;

                            var VolumenCongelado = pedidos.Where(x => x.OrdenEmbarque?.Codprd == item.CodPrd
                            && x.OrdenEmbarque?.Folio is not null
                            && x.OrdenEmbarque?.Orden is null)
                                .Sum(item => item?.OrdenEmbarque?.Compartment == 1 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom?.ToString())
                                            : item?.OrdenEmbarque?.Compartment == 2 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom2?.ToString())
                                            : item?.OrdenEmbarque?.Compartment == 3 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom3?.ToString())
                                            : item?.OrdenEmbarque?.Compartment == 4 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom4?.ToString())
                                            : item?.OrdenEmbarque?.Vol);

                            var VolumenConsumido = pedidos.Where(x => x.OrdenEmbarque?.Folio is not null
                            && x.OrdenEmbarque.Codprd == item.CodPrd
                            && x.OrdenEmbarque?.Orden?.BatchId is not null)
                                .Sum(x => x.OrdenEmbarque?.Orden?.Vol);

                            var VolumenTotalDisponible = VolumenDisponible - (VolumenConsumido + VolumenCongelado);

                            ProductoVolumen productoVolumen = new ProductoVolumen();

                            productoVolumen.Nombre = item.Producto?.Den;
                            productoVolumen.Disponible = VolumenTotalDisponible;
                            productoVolumen.Congelado = VolumenCongelado;
                            productoVolumen.Consumido = VolumenConsumido;
                            productoVolumen.Total = VolumenDisponible;

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
                            && x.Estatus is true
                            && x?.OrdenEmbarque?.Folio is not null
                            && x?.OrdenEmbarque?.Orden?.BatchId is null).Sum(item =>
                            item?.OrdenEmbarque?.Compartment == 1 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom?.ToString())
                                            : item?.OrdenEmbarque?.Compartment == 2 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom2?.ToString())
                                            : item?.OrdenEmbarque?.Compartment == 3 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom3?.ToString())
                                            : item?.OrdenEmbarque?.Compartment == 4 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom4?.ToString())
                                            : item?.OrdenEmbarque?.Vol);

                            var VolumenConsumido = ordenes.Where(x => x.CodPrd == item.CodPrd
                            && x.Estatus is true
                            && x?.OrdenEmbarque?.Folio is not null
                            && x?.OrdenEmbarque?.Orden?.BatchId is not null).Sum(x => x.OrdenEmbarque?.Orden?.Vol);

                            var VolumenTotalDisponible = VolumenDisponible - (VolumenConsumido + VolumenCongelado);

                            ProductoVolumen productoVolumen = new ProductoVolumen();
                            productoVolumen.Nombre = item.Producto?.Den;
                            productoVolumen.Disponible = VolumenTotalDisponible;
                            productoVolumen.Congelado = VolumenCongelado;
                            productoVolumen.Consumido = VolumenConsumido;
                            productoVolumen.Total = VolumenDisponible;

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

                context.Update(orden);
                await context.SaveChangesAsync();

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

        [HttpPost("folios/{cliente}")]
        public async Task<ActionResult> GetFoliosValidosFiltro([FromBody] CierreFiltroDTO filtroDTO, [FromRoute] int cliente)
        {
            try
            {
                List<string?> folios = new List<string?>();

                folios = context.OrdenCierre.Where(x => x.FchCierre >= filtroDTO.FchInicio && x.FchCierre <= filtroDTO.FchFin
                && !string.IsNullOrEmpty(x.Folio) && x.Activa == true && x.CodCte == cliente && x.CodPed == 0)
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
                        var pedidos = await context.OrdenPedido.Where(x => x.Folio!.Equals(item.Folio))
                            .Include(x => x.OrdenEmbarque).ThenInclude(x => x.Tonel).Include(x => x.OrdenEmbarque).ThenInclude(x => x.Orden).ToListAsync();

                        var VolumenDisponible = item.Volumen;

                        var VolumenCongelado = pedidos.Where(x => x.OrdenEmbarque?.Codprd == item.CodPrd
                        && x.OrdenEmbarque?.Folio is not null
                        && x.OrdenEmbarque?.Orden is null)
                            .Sum(item => item?.OrdenEmbarque?.Compartment == 1 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom?.ToString())
                                        : item?.OrdenEmbarque?.Compartment == 2 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom2?.ToString())
                                        : item?.OrdenEmbarque?.Compartment == 3 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom3?.ToString())
                                        : item?.OrdenEmbarque?.Compartment == 4 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom4?.ToString())
                                        : item?.OrdenEmbarque?.Vol);

                        var countCongelado = pedidos.Where(x => x.OrdenEmbarque?.Codprd == item.CodPrd
                        && x.OrdenEmbarque?.Folio is not null
                        && x.OrdenEmbarque?.Orden is null).Count();

                        var VolumenConsumido = pedidos.Where(x => x.OrdenEmbarque?.Folio is not null
                        && x.OrdenEmbarque.Codprd == item.CodPrd
                        && x.OrdenEmbarque?.Orden?.BatchId is not null)
                            .Sum(x => x.OrdenEmbarque?.Orden?.Vol);

                        var countConsumido = pedidos.Where(x => x.OrdenEmbarque?.Folio is not null
                        && x.OrdenEmbarque.Codprd == item.CodPrd
                        && x.OrdenEmbarque?.Orden?.BatchId is not null).Count();

                        var VolumenTotalDisponible = VolumenDisponible - (VolumenConsumido + VolumenCongelado);

                        var sumVolumen = (VolumenConsumido + VolumenCongelado) != 0 ? (VolumenConsumido + VolumenCongelado) : 1;
                        var sumCount = (countCongelado + countConsumido) != 0 ? (countCongelado + countConsumido) : 1;

                        var PromedioCargas = sumVolumen / sumCount;

                        ProductoVolumen productoVolumen = new ProductoVolumen();

                        productoVolumen.Nombre = item.Producto?.Den;
                        productoVolumen.Disponible = VolumenTotalDisponible;
                        productoVolumen.Congelado = VolumenCongelado;
                        productoVolumen.Consumido = VolumenConsumido;
                        productoVolumen.Total = VolumenDisponible;
                        productoVolumen.PromedioCarga = PromedioCargas;

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

        [HttpGet("bol/{bol}")]
        public async Task<ActionResult> GetPrecioByBol([FromRoute] int Bol)
        {
            try
            {
                var orden = context.Orden.Where(x => x.BatchId == Bol).Include(x => x.OrdenEmbarque).ThenInclude(x => x.OrdenCierre).FirstOrDefault();
                if (orden is null)
                    return BadRequest($"No se encontro el BOL. BOL: {Bol}");

                PrecioBol precio = new PrecioBol()
                {
                    Precio = orden.OrdenEmbarque?.OrdenCierre?.Precio
                };
                return Ok(precio);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        public class PrecioBol
        {
            public double? Precio { get; set; } = 0;
        }
    }
}
