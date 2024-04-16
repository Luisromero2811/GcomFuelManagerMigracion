﻿using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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

        public PedidoController(ApplicationDbContext context, VerifyUserId verifyUser, UserManager<IdentityUsuario> userManager)
        {
            this.context = context;
            this.verifyUser = verifyUser;
            this.userManager = userManager;
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
        
        


        //Method para obtener pedidos mediante un rango de fechas
        [HttpPost("filtrar")]
        public async Task<ActionResult> GetDate([FromBody] FechasF fechas)
        {
            try
            {
                List<OrdenEmbarque> ordens = new List<OrdenEmbarque>();

                ordens = await context.OrdenEmbarque
                    .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Codest == 3 && x.FchOrd != null
                    && x.Bolguidid == null)
                    .Include(x => x.Chofer)
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Estado)
                    .Include(x => x.OrdenCompra)
                    .Include(x => x.Tad)
                    .Include(x => x.Producto)
                    .Include(x => x.Tonel)
                    .ThenInclude(x => x.Transportista)
                    .Include(x=>x.OrdenCierre)
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

        //Method para obtener pedidos mediante rango de fechas y checbox seleccionado
        //REALIZAR TRES CONDICIONES, UNA POR CADA RADIOBUTTON, QUE SEA EN TRUE SE HARÁ EL MISMO FILTRO POR FECHAS EN WHERE AÑADIENDO LOS CAMPOS QUE UTILIZAN CADA CLAUSULA
        [HttpPost("filtro")]
        public async Task<ActionResult> GetDateRadio([FromBody] FechasF fechas)
        {
            try
            {
                if (fechas.Estado == 1)
                {
                    //Traerme al bolguid is not null, codest =3 y transportista activo en 1 --Ordenes Sin Cargar--
                    var pedidosDate = await context.OrdenEmbarque
                    .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Bolguidid != null && x.FchOrd != null && x.Codest == 3 && x.Tonel!.Transportista.Activo == true)
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
                    //Traerme al transportista activo en 1 y codest = 26 --Ordenes Cargadas--
                    var pedidosDate = await context.Orden
                    .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Tonel!.Transportista.Activo == true && x.Codest == 20)
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
                    //Traerme al transportista activo en 1 --Ordenes en trayecto-- 
                    var pedidosDate = await context.Orden
                    .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Tonel!.Transportista.Activo == true && x.Codest == 26)
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
                else if (fechas.Estado == 4)
                {
                    //Ordenes canceladas
                    var pedidosDate = await context.Orden
                        .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Codest == 14)
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

        [Route("binNumber")]
        [HttpGet]
        public async Task<ActionResult> GetLastBin()
        {
            try
            {
                var bin = await context.OrdenEmbarque.Select(x => x.Bin).OrderBy(x => x).LastOrDefaultAsync();
                return Ok(bin);
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
                    .Include(x=>x.OrdenCierre)
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
            //try
            //{
            //    if (!string.IsNullOrEmpty(ordenCierre.Folio_Perteneciente))
            //    {
            //        var cierre = context.OrdenCierre.Where(x => x.Folio == ordenCierre.Folio_Perteneciente).ToList();
            //        if (cierre is not null)
            //        {
            //            if (cierre.Where(x => x.CodPrd == ordenCierre.CodPrd).Count() == 0)
            //            {
            //                return BadRequest("El producto seleccionado no se encuentra en el cierre");
            //            }
            //        }
            //    }

            //    var id = await verifyUser.GetId(HttpContext, userManager);
            //    if (string.IsNullOrEmpty(id))
            //        return BadRequest();

            //    if (ordenCierre is null)
            //        return BadRequest("No se encontro ninguna orden");

            //    string folio = string.Empty;

            //    if (!string.IsNullOrEmpty(ordenCierre.Folio_Perteneciente))
            //    folio = context.OrdenCierre.FirstOrDefault(x => x.CodDes == ordenCierre.CodDes && x.CodCte == ordenCierre.CodCte && x.CodPrd == ordenCierre.CodPrd
            //    && x.CodPed != 0 && x.FchCierre == DateTime.Today && x.Estatus == true)?.Folio ?? string.Empty;

            //    //var user = await context.Usuario.FirstOrDefaultAsync(x => x.Usu == HttpContext.User.FindFirstValue(ClaimTypes.Name));
            //    //if (user == null)
            //    //    return NotFound();

            //    //Va y busca al usuario del cliente
            //    var user = await userManager.FindByNameAsync(HttpContext.User.FindFirstValue(ClaimTypes.Name)!);
            //    if (user == null)
            //        return NotFound();
            //    //Si el cliente es comprador
            //    if (await userManager.IsInRoleAsync(user, "Comprador"))
            //    {
            //        var userSis = context.Usuario.FirstOrDefault(x => x.Usu == user.UserName);
            //        if (userSis == null)
            //            return NotFound();
            //        ordenCierre.CodCte = userSis.CodCte;
            //        ordenCierre.CodGru = userSis.CodGru;
            //        ordenCierre.Vendedor = userSis.Den;
            //    }


            //    if (string.IsNullOrEmpty(folio))
            //    {
            //        var consecutivo = context.Consecutivo.First(x => x.Nombre == "Folio");
            //        if (consecutivo is null)
            //        {
            //            Consecutivo Nuevo_Consecutivo = new() { Numeracion = 1, Nombre = "Folio" };
            //            context.Add(Nuevo_Consecutivo);
            //            await context.SaveChangesAsync();
            //            consecutivo = Nuevo_Consecutivo;
            //        }
            //        else
            //        {
            //            consecutivo.Numeracion++;
            //            context.Update(consecutivo);
            //            await context.SaveChangesAsync();
            //        }

            //        context.Update(consecutivo);
            //        await context.SaveChangesAsync();

            //        var cliente = context.Cliente.FirstOrDefault(x => x.Cod == ordenCierre.CodCte);

            //        if (cliente is null)
            //            return BadRequest("No se encontro el cliente");

            //        if (!string.IsNullOrEmpty(ordenCierre.Folio_Perteneciente))
            //            ordenCierre.Folio = $"O{DateTime.Now:yy}-{consecutivo.Numeracion:000000}{(cliente is not null && !string.IsNullOrEmpty(cliente.CodCte) ? $"-{cliente.CodCte}" : "-DFT")}";
            //        else
            //            ordenCierre.Folio = $"OP{DateTime.Now:yy}-{consecutivo.Numeracion:000000}{(cliente is not null && !string.IsNullOrEmpty(cliente.CodCte) ? $"-{cliente.CodCte}" : "-DFT")}";

            //    }

            //    var bin = context.OrdenEmbarque.Select(x => x.Bin).OrderBy(x => x).LastOrDefault();

            //    var bincount = context.OrdenEmbarque.Count(x => x.Bin == bin);

            //    var count = context.OrdenCierre.Count(x => x.Folio == folio && x.CodDes == ordenCierre.CodDes && x.CodCte == ordenCierre.CodCte
            //    && x.CodPrd == ordenCierre.CodPrd);

            //    OrdenEmbarque ordenEmbarque = new()
            //    {
            //        Codest = 9,
            //        Codtad = ordenCierre.CodTad,
            //        Codprd = ordenCierre.CodPrd,
            //        Pre = ordenCierre.Precio,
            //        Vol = ordenCierre.Volumen,
            //        Coddes = ordenCierre.CodDes,
            //        Fchpet = DateTime.Now,
            //        Fchcar = ordenCierre.FchCar,
            //        Bin = count == 0 || bincount >= 2 ? ++bin : count % 2 == 0 ? ++bin : bin,
            //        //Codusu = user?.Cod,
            //        Moneda = ordenCierre.Moneda,
            //        Equibalencia = ordenCierre.Equibalencia
            //    };

            //    context.Add(ordenEmbarque);
            //    await context.SaveChangesAsync();

            //    ordenCierre.Producto = null!;
            //    ordenCierre.Destino = null!;
            //    ordenCierre.Grupo = null!;
            //    ordenCierre.OrdenEmbarque = null!;
            //    ordenCierre.OrdenPedidos = null!;
            //    ordenCierre.Cliente = null!;

            //    ordenCierre.CodPed = ordenEmbarque.Cod;
            //    ordenCierre.FchVencimiento = ordenCierre.FchCierre?.AddDays(5);

            //    context.Add(ordenCierre);

            //    await context.SaveChangesAsync();

            //    if (!string.IsNullOrEmpty(ordenCierre.Folio_Perteneciente))
            //    {
            //        var cierre = context.OrdenCierre.FirstOrDefault(x => x.Folio == ordenCierre.Folio_Perteneciente);

            //        if (cierre is not null)
            //        {
            //            ordenCierre.TipoPago = cierre.TipoPago ?? string.Empty;
            //            context.Update(ordenCierre);

            //            OrdenPedido ordenPedido = new()
            //            {
            //                CodPed = ordenEmbarque.Cod,
            //                CodCierre = cierre?.Cod ?? 0,
            //                Folio = ordenCierre.Folio_Perteneciente,
            //            };

            //            context.Add(ordenPedido);
            //            await context.SaveChangesAsync();
            //        }
            //    }

            //    var newOrden = context.OrdenCierre.Where(x => x.Cod == ordenCierre.Cod)
            //        .Include(x => x.Producto)
            //        .Include(x => x.Destino)
            //        .Include(x => x.Cliente)
            //        .Include(x => x.OrdenEmbarque)
            //        .ThenInclude(x => x.Tad)
            //        .Include(x => x.OrdenEmbarque)
            //        .ThenInclude(x => x.Estado)
            //        .Include(x => x.OrdenPedidos)
            //        .FirstOrDefault();

            //    return Ok(newOrden);
            //}
            //catch (Exception e)
            //{
            //    return BadRequest(e.Message);
            //}
            try
            {
                if (!string.IsNullOrEmpty(ordenCierre.Folio_Perteneciente))
                {
                    var cierre = context.OrdenCierre.Where(x => x.Folio == ordenCierre.Folio_Perteneciente).ToList();
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
                && x.CodPed != 0 && x.FchCierre == DateTime.Today && x.Estatus == true)?.Folio ?? string.Empty;

                var user = await context.Usuario.FirstOrDefaultAsync(x => x.Usu == HttpContext.User.FindFirstValue(ClaimTypes.Name));
                if (user == null)
                    return NotFound();

                if (string.IsNullOrEmpty(folio))
                {
                    var consecutivo = context.Consecutivo.First(x => x.Nombre == "Folio");
                    if (consecutivo is null)
                    {
                        Consecutivo Nuevo_Consecutivo = new() { Numeracion = 1, Nombre = "Folio" };
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
                        ordenCierre.Folio = $"O{DateTime.Now:yy}-{consecutivo.Numeracion:000000}{(cliente is not null && !string.IsNullOrEmpty(cliente.CodCte) ? $"-{cliente.CodCte}" : "-DFT")}";
                    else
                        ordenCierre.Folio = $"OP{DateTime.Now:yy}-{consecutivo.Numeracion:000000}{(cliente is not null && !string.IsNullOrEmpty(cliente.CodCte) ? $"-{cliente.CodCte}" : "-DFT")}";

                }
                else
                {
                    ordenCierre.Folio = folio;
                }

                var bin = context.OrdenEmbarque.Select(x => x.Bin).OrderBy(x => x).LastOrDefault();

                var bincount = context.OrdenEmbarque.Count(x => x.Bin == bin);

                var count = context.OrdenCierre.Count(x => x.Folio == folio && x.CodDes == ordenCierre.CodDes && x.CodCte == ordenCierre.CodCte
                && x.CodPrd == ordenCierre.CodPrd);

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

                var newOrden = context.OrdenCierre.Where(x => x.Cod == ordenCierre.Cod)
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
                    .ThenInclude(x=>x.Tonel)
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x=>x.OrdenCierre)
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
    }
}
