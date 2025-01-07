using AutoMapper;
using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using OnePlace.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Claims;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, Administrador Sistema, Direccion, Gerencia, Ejecutivo de Cuenta Comercial, Programador, Coordinador, Analista Credito, Contador, Auditor, Comprador, Ejecutivo de Cuenta Operativo")]
    public class PedidoController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly VerifyUserId verifyUser;
        private readonly UserManager<IdentityUsuario> userManager;
        private readonly User_Terminal terminal;
        private readonly IUsuarioHelper helper;
        private readonly IMapper mapper;

        public PedidoController(ApplicationDbContext context,
                                VerifyUserId verifyUser,
                                UserManager<IdentityUsuario> userManager,
                                User_Terminal _Terminal,
                                IUsuarioHelper helper,
                                IMapper mapper)
        {
            this.context = context;
            this.verifyUser = verifyUser;
            this.userManager = userManager;
            terminal = _Terminal;
            this.helper = helper;
            this.mapper = mapper;
        }

        [HttpGet("historial")]
        public async Task<ActionResult> Get([FromQuery] HistorialOrdenDTO historial)
        {
            try
            {
                var ordenes = context.OrdenEmbarque.Where(x => x.Fchpet >= historial.Fecha_Inicio && x.Fchpet <= historial.Fecha_Fin)
                    .Include(x => x.OrdenCierre)
                    .Include(x => x.Destino)
                    .Include(x => x.Chofer)
                    .Include(x => x.Tonel.Transportista)
                    .Include(x => x.Producto)
                    .Include(x => x.Orden.Estado)
                    .Include(x => x.Estado)
                    .Include(x => x.Tad)
                    .AsQueryable();

                if (historial.BOL.IsNull())
                    ordenes = ordenes.Where(x => x.Orden != null && x.Orden.BatchId.HasValue && x.Orden.BatchId.Value.ToString().StartsWith(historial.BOL));

                if (historial.Destino.IsNull())
                    ordenes = ordenes.Where(x => x.Destino != null && !x.Destino.Den.IsNullOrEmpty() && x.Destino.Den!.ToLower().StartsWith(historial.Destino.ToLower()));

                if (historial.Producto.IsNull())
                    ordenes = ordenes.Where(x => x.Producto != null && !x.Producto.Den.IsNullOrEmpty() && x.Producto.Den!.ToLower().StartsWith(historial.Destino.ToLower()));

                if (historial.Transportista.IsNull())
                    ordenes = ordenes.Where(x => x.Transportista != null && !x.Transportista.Den.IsNullOrEmpty() && x.Transportista.Den!.ToLower().StartsWith(historial.Transportista.ToLower()));

                if (historial.Tonel.IsNull())
                    ordenes = ordenes.Where(x => x.Tonel != null && !x.Tonel.Den.IsNullOrEmpty() && x.Tonel.Den!.ToLower().StartsWith(historial.Tonel.ToLower()));

                if (historial.Chofer.IsNull())
                    ordenes = ordenes.Where(x => x.Chofer != null && !x.Chofer.Den.IsNullOrEmpty() && x.Chofer.Den!.ToLower().StartsWith(historial.Chofer.ToLower()));

                await HttpContext.InsertarParametrosPaginacion(ordenes, historial.Registros_por_pagina, historial.Pagina);
                historial.Pagina = HttpContext.ObtenerPagina();

                var ordenesdto = ordenes
                    .Select(x => new HistorialOrdenDTO
                    {
                        Terminal = x.Tad != null ? x.Tad.ToString() : string.Empty,
                        FechaCarga = x.Fchcar ?? DateTime.MinValue,
                        Folio = x.FolioSyn ?? string.Empty,
                        BOL = x.Orden != null ? x.Orden.BatchId.ToString() : string.Empty,
                        Destino = x.Destino != null ? x.Destino.ToString() : string.Empty,
                        Producto = x.Producto != null ? x.Producto.ToString() : string.Empty,
                        Volumen = x.Vol ?? 0,
                        VolumenCargado = x.Orden != null ? x.Orden.Vol : 0,
                        Transportista = x.Tonel != null ? x.Tonel.Transportista != null ? x.Tonel.Transportista.ToString() : string.Empty : string.Empty,
                        Tonel = x.Tonel != null ? x.Tonel.ToString() : string.Empty,
                        Chofer = x.Chofer != null ? x.Chofer.ToString() : string.Empty,
                        Estado = x.Orden != null ? x.Orden.Estado.den ?? string.Empty : x.Estado.den ?? string.Empty
                    })
                    .Skip((historial.Pagina - 1) * historial.Registros_por_pagina)
                    .Take(historial.Registros_por_pagina);

                return Ok(ordenesdto);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        //Method para realizar (agregar) pedido
        //[HttpPost]
        public async Task<ActionResult> Post(OrdenEmbarque orden)
        {
            try
            {
                var user = await context.Usuario.FirstOrDefaultAsync(x => x.Usu == HttpContext.User.FindFirstValue(ClaimTypes.Name));
                if (user == null)
                    return NotFound();
                orden.Codusu = user!.Cod;
                orden.Destino = await context.Destino.FirstOrDefaultAsync(x => x.Cod == orden.Coddes);
                orden.Tad = await context.Tad.FirstOrDefaultAsync(x => x.Cod == orden.Codtad);
                orden.Producto = await context.Producto.FirstOrDefaultAsync(x => x.Cod == orden.Codprd);
                orden.Tonel = await context.Tonel.FirstOrDefaultAsync(x => x.Cod == orden.Codton);

                context.Add(orden);

                var id = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();
                await context.SaveChangesAsync(id, 2);
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
                    x.Destino = null!;
                    x.Estado = null!;
                    x.Tad = null!;
                    x.Chofer = null!;
                    x.Tonel = null!;
                    x.Producto = null;
                    x.OrdenCierre = null!;
                    x.Cliente = null!;
                    x.Codest = 3;
                    x.CodordCom = folio;
                    x.FchOrd = DateTime.Today.Date;
                    Debug.WriteLine(JsonConvert.SerializeObject(x));
                });
                context.UpdateRange(orden);

                var id = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                await context.SaveChangesAsync(id, 15);

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

        [HttpPost("update")]
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
                orden.Cliente = null!;
                orden.OrdenCierre = null!;

                context.Update(orden);
                await context.SaveChangesAsync();

                var ord = await context.OrdenEmbarque.Where(x => x.Cod == orden.Cod)
                    .Include(x => x.Producto)
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Tonel)
                    .ThenInclude(x => x.Transportista)
                    .Include(x => x.Tad)
                    .Include(x => x.Estado)
                    .Include(x => x.OrdenCompra)
                    .Include(x => x.Chofer)
                    .Include(x => x.OrdenCierre)
                    .FirstOrDefaultAsync();

                return Ok(ord);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPut("cierre/update/{cod:int}")]
        public async Task<ActionResult> PutPedidoCierre([FromBody] OrdenCierre orden, [FromRoute] int cod)
        {
            try
            {

                var o = await context.OrdenEmbarque.FirstOrDefaultAsync(x => x.Cod == cod);
                if (o == null)
                    return NotFound();

                o!.Codprd = orden.CodPrd;
                o!.Coddes = orden.CodDes;
                o!.Pre = orden.Precio;
                o!.Vol = orden.Volumen;

                context.Update(o);
                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost("historial/envios")]
        public async Task<ActionResult> GetDateHistorico([FromBody] FechasF fechas)
        {
            try
            {
                List<OrdenEmbarque> ordens = new List<OrdenEmbarque>();

                ordens = await context.OrdenEmbarque
                    .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin)
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

                ordens.OrderByDescending(x => x.Bin);

                return Ok(ordens);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("cierre/{folio?}")]
        public async Task<ActionResult> PostPedidoVolumen([FromBody] OrdenEmbarque orden, [FromRoute] string? folio)
        {
            try
            {

                if (string.IsNullOrEmpty(folio))
                {
                    await Post(orden);
                    return Ok(orden);
                }
                else
                {
                    var cierres = context.OrdenCierre.Where(x => x.Folio!.Equals(folio)).ToList();
                    if (cierres is null)
                        return BadRequest("No existe el cierre.");

                    var pedidos = context.OrdenPedido.Where(x => x.Folio!.Equals(folio)).Include(x => x.OrdenEmbarque).ThenInclude(x => x.Orden).ToList();

                    var user = await context.Usuario.FirstOrDefaultAsync(x => x.Usu == HttpContext.User.FindFirstValue(ClaimTypes.Name));
                    if (user == null)
                        return NotFound();

                    if (cierres.Any(x => x.CodPrd == orden.Codprd))
                    {

                        var VolumenDisponible = cierres.Where(x => x.CodPrd == orden.Codprd && x.Estatus is true).Sum(x => x.Volumen);

                        var VolumenCongelado = pedidos.Where(x => x.CodPed == orden.Codprd
                        && x.OrdenEmbarque.OrdenCierre.Estatus is true
                        && x?.OrdenEmbarque?.Folio is not null
                        && x?.OrdenEmbarque?.Orden is null).Sum(item =>
                        item?.OrdenEmbarque?.Compartment == 1 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom?.ToString())
                                        : item?.OrdenEmbarque?.Compartment == 2 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom2?.ToString())
                                        : item?.OrdenEmbarque?.Compartment == 3 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom3?.ToString())
                                        : item?.OrdenEmbarque?.Compartment == 4 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom4?.ToString())
                                        : item?.OrdenEmbarque?.Vol);

                        var VolumenConsumido = pedidos.Where(x => x.OrdenEmbarque?.Codprd == orden.Codprd
                        && x?.OrdenEmbarque?.OrdenCierre?.Estatus is true
                        && x?.OrdenEmbarque?.Folio is not null
                        && x?.OrdenEmbarque?.Orden?.BatchId is not null).Sum(x => x.OrdenEmbarque?.Orden?.Vol);

                        var VolumenTotalDisponible = VolumenDisponible - (VolumenConsumido + VolumenCongelado);

                        if (VolumenTotalDisponible < orden.Vol)
                        {
                            return BadRequest("No hay suficiente volumen disponible");
                        }
                    }

                    orden.Codusu = user!.Cod;
                    orden.Destino = await context.Destino.FirstOrDefaultAsync(x => x.Cod == orden.Coddes);
                    orden.Tad = await context.Tad.FirstOrDefaultAsync(x => x.Cod == orden.Codtad);
                    orden.Producto = await context.Producto.FirstOrDefaultAsync(x => x.Cod == orden.Codprd);
                    orden.Tonel = await context.Tonel.FirstOrDefaultAsync(x => x.Cod == orden.Codton);

                    context.Add(orden);

                    var id = await verifyUser.GetId(HttpContext, userManager);
                    if (string.IsNullOrEmpty(id))
                        return BadRequest();

                    await context.SaveChangesAsync(id, 2);
                    return Ok(orden);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("orden/add")]
        public async Task<ActionResult> PostRelation([FromBody] OrdenPedido ordenPedido)
        {
            try
            {
                ordenPedido.OrdenEmbarque = null!;
                context.Add(ordenPedido);
                await context.SaveChangesAsync();
                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("create/all")]
        public async Task<ActionResult> GetFolioToOrden([FromBody] OrdenCierre ordenCierre)
        {
            var id_terminal = terminal.Obtener_Terminal(context, HttpContext);
            if (id_terminal == 0)
                return BadRequest();

            try
            {
                if (!string.IsNullOrEmpty(ordenCierre.Folio_Perteneciente))
                {
                    var cierre = context.OrdenCierre.Where(x => x.Folio == ordenCierre.Folio_Perteneciente && x.Id_Tad == id_terminal).ToList();
                    if (cierre is not null)
                    {
                        if (cierre.Where(x => x.CodPrd == ordenCierre.CodPrd).Count() == 0)
                        {
                            return BadRequest("El producto seleccionado no se encuentra en el cierre");
                        }
                    }
                }

                var id = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                if (ordenCierre is null)
                    return BadRequest("No se encontro ninguna orden");

                string folio = string.Empty;

                //if (!string.IsNullOrEmpty(ordenCierre.Folio_Perteneciente))
                folio = context.OrdenCierre.FirstOrDefault(x => x.CodDes == ordenCierre.CodDes && x.CodCte == ordenCierre.CodCte && x.CodPrd == ordenCierre.CodPrd
                && x.CodPed != 0 && x.FchCierre == DateTime.Today && x.Estatus == true && x.Id_Tad == id_terminal)?.Folio ?? string.Empty;

                var user = await context.Usuario.FirstOrDefaultAsync(x => x.Usu == HttpContext.User.FindFirstValue(ClaimTypes.Name));
                if (user == null)
                    return NotFound();

                if (string.IsNullOrEmpty(folio))
                {
                    var consecutivo = context.Consecutivo.First(x => x.Nombre == "Folio" && x.Id_Tad == id_terminal);
                    if (consecutivo is null)
                    {
                        Consecutivo Nuevo_Consecutivo = new() { Numeracion = 1, Nombre = "Folio", Id_Tad = id_terminal };
                        context.Add(Nuevo_Consecutivo);
                        await context.SaveChangesAsync();
                        consecutivo = Nuevo_Consecutivo;
                    }
                    else
                    {
                        consecutivo.Numeracion++;
                        context.Update(consecutivo);
                        await context.SaveChangesAsync();
                    }

                    context.Update(consecutivo);
                    await context.SaveChangesAsync();

                    var cliente = context.Cliente.FirstOrDefault(x => x.Cod == ordenCierre.CodCte);

                    if (cliente is null)
                        return BadRequest("No se encontro el cliente");


                    if (!string.IsNullOrEmpty(ordenCierre.Folio_Perteneciente))
                        ordenCierre.Folio = $"O{DateTime.Now:yy}-{consecutivo.Numeracion:0000000}{(cliente is not null && !string.IsNullOrEmpty(cliente.CodCte) ? $"-{cliente.CodCte}" : "-DFT")}-{consecutivo.Obtener_Codigo_Terminal}";
                    else
                        ordenCierre.Folio = $"OP{DateTime.Now:yy}-{consecutivo.Numeracion:0000000}{(cliente is not null && !string.IsNullOrEmpty(cliente.CodCte) ? $"-{cliente.CodCte}" : "-DFT")}-{consecutivo.Obtener_Codigo_Terminal}";

                }
                else
                {
                    ordenCierre.Folio = folio;
                }

                var bin = context.OrdenEmbarque.Select(x => x.Bin).OrderBy(x => x).LastOrDefault();

                var bincount = context.OrdenEmbarque.Count(x => x.Bin == bin);

                var count = context.OrdenCierre.Count(x => x.Folio == folio && x.CodDes == ordenCierre.CodDes && x.CodCte == ordenCierre.CodCte
                && x.CodPrd == ordenCierre.CodPrd);

                ordenCierre.Id_Tad = id_terminal;

                OrdenEmbarque ordenEmbarque = new()
                {
                    Codest = 9,
                    Codtad = ordenCierre.CodTad,
                    Codprd = ordenCierre.CodPrd,
                    Pre = ordenCierre.Precio,
                    Vol = ordenCierre.Volumen,
                    Coddes = ordenCierre.CodDes,
                    Fchpet = DateTime.Now,
                    Fchcar = ordenCierre.FchCar,
                    Bin = count == 0 || bincount >= 2 ? ++bin : count % 2 == 0 ? ++bin : bin,
                    Codusu = user?.Cod,
                    Moneda = ordenCierre.Moneda,
                    Equibalencia = ordenCierre.Equibalencia
                };

                context.Add(ordenEmbarque);
                await context.SaveChangesAsync();

                ordenCierre.Producto = null!;
                ordenCierre.Destino = null!;
                ordenCierre.Grupo = null!;
                ordenCierre.OrdenEmbarque = null!;
                ordenCierre.OrdenPedidos = null!;
                ordenCierre.Cliente = null!;

                ordenCierre.CodPed = ordenEmbarque.Cod;
                ordenCierre.FchVencimiento = ordenCierre.FchCierre?.AddDays(5);

                context.Add(ordenCierre);

                await context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(ordenCierre.Folio_Perteneciente))
                {
                    var cierre = context.OrdenCierre.FirstOrDefault(x => x.Folio == ordenCierre.Folio_Perteneciente);

                    if (cierre is not null)
                    {
                        ordenCierre.TipoPago = cierre.TipoPago ?? string.Empty;
                        context.Update(ordenCierre);

                        OrdenPedido ordenPedido = new()
                        {
                            CodPed = ordenEmbarque.Cod,
                            CodCierre = cierre?.Cod ?? 0,
                            Folio = ordenCierre.Folio_Perteneciente,
                        };

                        context.Add(ordenPedido);
                        await context.SaveChangesAsync();
                    }
                }

                var newOrden = context.OrdenCierre.Where(x => x.Cod == ordenCierre.Cod && x.Id_Tad == id_terminal)
                    .Include(x => x.Producto)
                    .Include(x => x.Destino)
                    .Include(x => x.Cliente)
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x.Tad)
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x.Estado)
                    .Include(x => x.OrdenPedidos)
                    .FirstOrDefault();

                return Ok(newOrden);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("verificar/carga")]
        public async Task<ActionResult> VerifyVolumenAsignacion([FromBody] OrdenEmbarque orden)
        {
            try
            {
                if (orden == null)
                    return BadRequest();

                var folio = context.OrdenPedido.FirstOrDefault(x => x.CodPed == orden.Cod);

                var cierres = context.OrdenCierre.Where(x => x.Folio!.Equals(folio.Folio)).ToList();
                if (cierres is null)
                    return BadRequest("No existe el cierre.");

                var pedidos = context.OrdenPedido.Where(x => x.Folio!.Equals(folio.Folio))
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x.Orden)
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x.Tonel)
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x.OrdenCierre)
                    .ToList();


                if (cierres.Any(x => x.CodPrd == orden.Codprd))
                {



                    var VolumenDisponible = cierres.Where(x => x.CodPrd == orden.Codprd && x.Estatus is true).Sum(x => x.Volumen);

                    var VolumenCongelado = pedidos.Where(x => x.OrdenEmbarque.Codprd == orden.Codprd
                    && x.OrdenEmbarque.OrdenCierre.Estatus is true
                    && x?.OrdenEmbarque?.Folio is not null
                    && x?.OrdenEmbarque?.Orden?.BatchId is null).Sum(item =>
                    item?.OrdenEmbarque?.Compartment == 1 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom?.ToString())
                                    : item?.OrdenEmbarque?.Compartment == 2 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom2?.ToString())
                                    : item?.OrdenEmbarque?.Compartment == 3 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom3?.ToString())
                                    : item?.OrdenEmbarque?.Compartment == 4 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom4?.ToString())
                                    : item?.OrdenEmbarque?.Vol);

                    var VolumenConsumido = pedidos.Where(x => x.OrdenEmbarque?.Codprd == orden.Codprd
                    && x?.OrdenEmbarque?.OrdenCierre?.Estatus is true
                    && x?.OrdenEmbarque?.Folio is not null
                    && x?.OrdenEmbarque?.Orden?.BatchId is not null).Sum(x => x.OrdenEmbarque?.Orden?.Vol);

                    var VolumenTotalDisponible = VolumenDisponible - (VolumenConsumido + VolumenCongelado);

                    var tonel = context.Tonel.FirstOrDefault(x => x.Cod == orden.Codton);

                    if (tonel is null)
                        return BadRequest("No se encontro la unidad");

                    var volumen = orden?.Compartment == 1 ? double.Parse(tonel?.Capcom.ToString())
                            : orden?.Compartment == 2 ? double.Parse(tonel?.Capcom2.ToString())
                            : orden?.Compartment == 3 ? double.Parse(tonel?.Capcom3.ToString())
                            : double.Parse(tonel?.Capcom4.ToString());

                    if (VolumenTotalDisponible < volumen)
                    {
                        return BadRequest($"No hay suficiente volumen disponible. Volumen Disponible: {VolumenTotalDisponible}. Intento de carga: {volumen}");
                    }
                }

                await context.SaveChangesAsync();
                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

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

                var id = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                await context.SaveChangesAsync(id, 4);

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("folios/detalles")]
        public async Task<ActionResult> GetFoliosValidosPedidosActivos([FromBody] CierreFiltroDTO filtro)
        {
            try
            {
                var id_terminal = terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var user = await userManager.FindByNameAsync(HttpContext.User.FindFirstValue(ClaimTypes.Name)!);
                if (user == null)
                    return NotFound();
                var userSis = context.Usuario.FirstOrDefault(x => x.Usu == user.UserName);
                if (userSis == null)
                    return NotFound();

                List<FolioDetalleDTO> folios = new List<FolioDetalleDTO>();

                folios = await context.OrdenCierre.OrderBy(x => x.FchCierre).Where(x => x.FchCierre >= filtro.FchInicio && x.FchCierre <= filtro.FchFin && x.Id_Tad == id_terminal
                   && !string.IsNullOrEmpty(x.Folio) && x.Activa == true && x.Estatus == true && x.CodCte == userSis.CodCte && x.Folio.StartsWith("OP") ||
                   //x.FchCierre >= DateTime.Today.AddDays(-10) && x.FchCierre <= DateTime.Today.AddDays(1)
                   //&&
                   !string.IsNullOrEmpty(x.Folio)
                   && x.Activa == true
                   && x.Folio.StartsWith("OP")
                   && x.Estatus == true
                   && x.CodCte == userSis.CodCte
                   && x.Id_Tad == id_terminal)
                       .Include(x => x.Cliente)
                       .Include(x => x.Destino)
                       .Include(x => x.Producto)
                       .Select(x => new FolioDetalleDTO()
                       {
                           Folio = x.Folio,
                           Cliente = x.Cliente,
                           Destino = x.Destino,
                           Producto = x.Producto,
                           FchCierre = x.FchCierre,
                           Comentarios = x.Observaciones
                       })
                   .OrderByDescending(x => x.FchCierre)
                       .ToListAsync();
                return Ok(folios);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("folios/detalles/status")]
        public async Task<ActionResult> GetFoliosValidosPedidosActivo([FromBody] CierreFiltroDTO filtro)
        {
            try
            {
                var id_terminal = terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var user = await userManager.FindByNameAsync(HttpContext.User.FindFirstValue(ClaimTypes.Name)!);
                if (user == null)
                    return NotFound();
                var userSis = context.Usuario.FirstOrDefault(x => x.Usu == user.UserName);
                if (userSis == null)
                    return NotFound();
                //Pruebas
                List<FolioDetalleDTO> folios = new List<FolioDetalleDTO>();

                folios = await context.OrdenCierre.OrderBy(x => x.FchCierre).Where(x => x.FchCierre >= filtro.FchInicio && x.FchCierre <= filtro.FchFin && x.Id_Tad == id_terminal
                   && !string.IsNullOrEmpty(x.Folio) && x.Activa == true && x.Estatus == true && x.CodCte == userSis.CodCte && x.Folio.StartsWith("OP") ||
                   !string.IsNullOrEmpty(x.Folio)
                   && x.Activa == true
                   && x.Folio.StartsWith("OP")
                   && x.Estatus == true
                   && x.CodCte == userSis.CodCte
                   && x.Id_Tad == id_terminal)
                       .Include(x => x.Cliente)
                       .Include(x => x.Destino)
                       .Include(x => x.Producto)
                       .Select(x => new FolioDetalleDTO()
                       {
                           Folio = x.Folio,
                           Cliente = x.Cliente,
                           Destino = x.Destino,
                           Producto = x.Producto,
                           FchCierre = x.FchCierre,
                           Comentarios = x.Observaciones
                       })
                   .OrderByDescending(x => x.FchCierre)
                       .ToListAsync();
                return Ok(folios);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("detalle")]
        public async Task<ActionResult> GetFoliosOrdenes([FromQuery] ParametrosBusquedaOrdenes parametros)
        {
            try
            {
                var id_terminal = terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var user = await userManager.FindByNameAsync(HttpContext.User.FindFirstValue(ClaimTypes.Name)!);
                if (user == null)
                    return NotFound();
                var userSis = context.Usuario.FirstOrDefault(x => x.Usu == user.UserName);
                if (userSis == null)
                    return NotFound();
                //Pruebas
                // List<FolioDetalleDTO> folios = new List<FolioDetalleDTO>();

                var folios = context.OrdenCierre.OrderBy(x => x.FchCierre).Where(x => x.Id_Tad == id_terminal
                   && !string.IsNullOrEmpty(x.Folio) && x.Activa == true && x.Estatus == true && x.CodCte == userSis.CodCte && x.Folio.StartsWith("OP") ||
                   !string.IsNullOrEmpty(x.Folio)
                   && x.Activa == true
                   && x.Folio.StartsWith("OP")
                   && x.Estatus == true
                   && x.CodCte == userSis.CodCte
                   && x.Id_Tad == id_terminal)
                       .Include(x => x.Cliente)
                       .Include(x => x.Destino)
                       .Include(x => x.Producto)
                       .Include(x => x.OrdenEmbarque)
                       .ThenInclude(x => x.Estado)
                       .Include(x => x.OrdenEmbarque)
                       .ThenInclude(x => x.Orden)
                       .Include(x => x.OrdenEmbarque)
                       .ThenInclude(x => x.Chofer)
                       .Include(x => x.OrdenEmbarque)
                       .ThenInclude(x => x.Tonel)
                       .ThenInclude(x => x.Transportista)
                       .Select(x => new FolioDetalleDTO()
                       {
                           Folio = x.Folio,
                           Cliente = x.Cliente,
                           Destino = x.Destino,
                           Producto = x.Producto,
                           FchCierre = x.FchCierre,
                           Comentarios = x.Observaciones,
                           Estado = x.OrdenEmbarque.Estado,
                           OrdenEmbarque = x.OrdenEmbarque,
                       })
                   .OrderByDescending(x => x.FchCierre)
                       .AsQueryable();

                if (!string.IsNullOrEmpty(parametros.estado))
                    folios = folios.Where(x => x.OrdenEmbarque.Estado != null && !string.IsNullOrEmpty(x.OrdenEmbarque.Estado.den) && x.OrdenEmbarque.Estado.den.ToLower().Contains(parametros.estado.ToLower()));
                if (!string.IsNullOrEmpty(parametros.transportista))
                    folios = folios.Where(x => x.OrdenEmbarque.Tonel.Transportista != null && !string.IsNullOrEmpty(x.OrdenEmbarque.Tonel.Transportista.Den) && x.OrdenEmbarque.Tonel.Transportista.Den.ToLower().Contains(parametros.transportista.ToLower()));
                if (!string.IsNullOrEmpty(parametros.producto))
                    folios = folios.Where(x => x.Producto != null && !string.IsNullOrEmpty(x.Producto.Den) && x.Producto.Den.ToLower().Contains(parametros.producto.ToLower()));
                if (!string.IsNullOrEmpty(parametros.unidad))
                    folios = folios.Where(x => x.OrdenEmbarque.Tonel != null && !string.IsNullOrEmpty(x.OrdenEmbarque.Tonel.Tracto) && x.OrdenEmbarque.Tonel.Tracto.ToLower().Contains(parametros.unidad.ToLower()));
                if (!string.IsNullOrEmpty(parametros.chofer))
                    folios = folios.Where(x => x.OrdenEmbarque.Chofer != null && !string.IsNullOrEmpty(x.OrdenEmbarque.Chofer.Den) && x.OrdenEmbarque.Chofer.Den.ToLower().Contains(parametros.chofer.ToLower()));
                if (!string.IsNullOrEmpty(parametros.destino))
                    folios = folios.Where(x => x.Destino != null && !string.IsNullOrEmpty(x.Destino.Den) && x.Destino.Den.ToLower().Contains(parametros.destino.ToLower()));

                await HttpContext.InsertarParametrosPaginacion(folios, parametros.tamanopagina, parametros.pagina);

                if (HttpContext.Response.Headers.ContainsKey("pagina"))
                {
                    var pagina = HttpContext.Response.Headers["pagina"];
                    if (pagina != parametros.pagina && !string.IsNullOrEmpty(pagina))
                    {
                        parametros.pagina = int.Parse(pagina!);
                    }
                }

                folios = folios.Skip((parametros.pagina - 1) * parametros.tamanopagina).Take(parametros.tamanopagina);

                return Ok(folios);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("filtrohist")]
        public async Task<ActionResult> GetFoliosOrdenesFechas([FromQuery] ParametrosBusquedaOrdenes parametros)
        {
            try
            {
                var id_terminal = terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var user = await userManager.FindByNameAsync(HttpContext.User.FindFirstValue(ClaimTypes.Name)!);
                if (user == null)
                    return NotFound();
                var userSis = context.Usuario.FirstOrDefault(x => x.Usu == user.UserName);
                if (userSis == null)
                    return NotFound();
                //Pruebas
                // List<FolioDetalleDTO> folios = new List<FolioDetalleDTO>();

                var folios = context.OrdenCierre.OrderBy(x => x.FchCierre).Where(x => x.Id_Tad == id_terminal && x.FchCierre >= parametros.DateInicio && x.FchCierre <= parametros.DateFin
                   && !string.IsNullOrEmpty(x.Folio) && x.Activa == true && x.Estatus == true && x.CodCte == userSis.CodCte && x.Folio.StartsWith("OP") ||
                   x.FchCierre >= parametros.DateInicio
                   && x.FchCierre <= parametros.DateFin &&
                   !string.IsNullOrEmpty(x.Folio)
                   && x.Activa == true
                   && x.Folio.StartsWith("OP")
                   && x.Estatus == true
                   && x.CodCte == userSis.CodCte
                   && x.Id_Tad == id_terminal)
                       .Include(x => x.Cliente)
                       .Include(x => x.Destino)
                       .Include(x => x.Producto)
                       .Include(x => x.OrdenEmbarque)
                       .ThenInclude(x => x.Estado)
                       .Include(x => x.OrdenEmbarque)
                       .ThenInclude(x => x.Orden)
                       .Include(x => x.OrdenEmbarque)
                       .ThenInclude(x => x.Chofer)
                       .Include(x => x.OrdenEmbarque)
                       .ThenInclude(x => x.Tonel)
                       .ThenInclude(x => x.Transportista)
                       .Select(x => new FolioDetalleDTO()
                       {
                           Folio = x.Folio,
                           Cliente = x.Cliente,
                           Destino = x.Destino,
                           Producto = x.Producto,
                           FchCierre = x.FchCierre,
                           Comentarios = x.Observaciones,
                           Estado = x.OrdenEmbarque.Estado,
                           OrdenEmbarque = x.OrdenEmbarque,
                       })
                   .OrderByDescending(x => x.FchCierre)
                       .AsQueryable();

                if (!string.IsNullOrEmpty(parametros.estado))
                    folios = folios.Where(x => x.OrdenEmbarque.Estado != null && !string.IsNullOrEmpty(x.OrdenEmbarque.Estado.den) && x.OrdenEmbarque.Estado.den.ToLower().Contains(parametros.estado.ToLower()));
                if (!string.IsNullOrEmpty(parametros.transportista))
                    folios = folios.Where(x => x.OrdenEmbarque.Tonel.Transportista != null && !string.IsNullOrEmpty(x.OrdenEmbarque.Tonel.Transportista.Den) && x.OrdenEmbarque.Tonel.Transportista.Den.ToLower().Contains(parametros.transportista.ToLower()));
                if (!string.IsNullOrEmpty(parametros.producto))
                    folios = folios.Where(x => x.Producto != null && !string.IsNullOrEmpty(x.Producto.Den) && x.Producto.Den.ToLower().Contains(parametros.producto.ToLower()));
                if (!string.IsNullOrEmpty(parametros.destino))
                    folios = folios.Where(x => x.Destino != null && !string.IsNullOrEmpty(x.Destino.Den) && x.Destino.Den.ToLower().Contains(parametros.destino.ToLower()));

                await HttpContext.InsertarParametrosPaginacion(folios, parametros.tamanopagina, parametros.pagina);

                if (HttpContext.Response.Headers.ContainsKey("pagina"))
                {
                    var pagina = HttpContext.Response.Headers["pagina"];
                    if (pagina != parametros.pagina && !string.IsNullOrEmpty(pagina))
                    {
                        parametros.pagina = int.Parse(pagina!);
                    }
                }

                folios = folios.Skip((parametros.pagina - 1) * parametros.tamanopagina).Take(parametros.tamanopagina);

                return Ok(folios);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("Excel")]
        public async Task<ActionResult> Excel()
        {
            try
            {
                var id_terminal = terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var user = await userManager.FindByNameAsync(HttpContext.User.FindFirstValue(ClaimTypes.Name)!);
                if (user == null)
                    return NotFound();
                var userSis = context.Usuario.FirstOrDefault(x => x.Usu == user.UserName);
                if (userSis == null)
                    return NotFound();
                //Pruebas
                List<FolioDetalleDTO> folios = new List<FolioDetalleDTO>();

                folios = await context.OrdenCierre.OrderBy(x => x.FchCierre).Where(x =>
                  !string.IsNullOrEmpty(x.Folio)
                  && x.Activa == true
                  && x.Folio.StartsWith("OP")
                  && x.Estatus == true
                  && x.CodCte == userSis.CodCte
                  && x.Id_Tad == id_terminal)
                      .Include(x => x.Cliente)
                      .Include(x => x.Destino)
                      .Include(x => x.Producto)
                      .Include(x => x.OrdenEmbarque)
                      .ThenInclude(x => x.Estado)
                      .OrderByDescending(x => x.FchCierre)
                      .Select(x => new FolioDetalleDTO()
                      {
                          Folio = x.Folio,
                          BOL = x.OrdenEmbarque.Bol,
                          FechaCierre = x.Fch,
                          NombreDestino = x.Destino.Den,
                          NombreProducto = x.Producto.Den,
                          NombreEstado = x.OrdenEmbarque.Estado.den,
                      })

                      .ToListAsync();

                return Ok(folios);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
//x => x.Id_Tad == id_terminal
//                   && !string.IsNullOrEmpty(x.Folio) && x.Activa == true && x.Estatus == true && x.CodCte == userSis.CodCte && x.Folio.StartsWith("OP") ||