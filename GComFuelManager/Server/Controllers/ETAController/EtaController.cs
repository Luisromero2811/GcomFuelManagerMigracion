using System;
using GComFuelManager.Server.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Linq;
using System.Linq.Dynamic.Core;
using GComFuelManager.Server.Helpers;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using GComFuelManager.Shared.Enums;
using AutoMapper;

namespace GComFuelManager.Server.Controllers.ETAController
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, Administrador Sistema, Direccion, Gerencia, Ejecutivo de Cuenta Comercial, Programador, Coordinador, Analista Suministros, Auditor, Capturista Recepcion Producto, Comprador")]
    public class EtaController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUsuario> userManager;
        private readonly VerifyUserId verifyUser;
        private readonly User_Terminal _terminal;
        private readonly IMapper mapper;

        public EtaController(ApplicationDbContext context,
            UserManager<IdentityUsuario> userManager,
            VerifyUserId verifyUser,
            User_Terminal _Terminal,
            IMapper mapper)
        {
            this.context = context;
            this.userManager = userManager;
            this.verifyUser = verifyUser;
            this._terminal = _Terminal;
            this.mapper = mapper;
        }
        //Filtro para ordenes por Bol 
        [HttpPost("Filtro")]
        public ActionResult EtaGet([FromBody] EtaDTO etaDTO)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var eta = context.OrdEmbDet
                    .Where(x => x.Bol == etaDTO.Bol && x.Orden != null && x.Orden.Id_Tad == id_terminal)
                    .Include(x => x.Orden)
                    .ThenInclude(x => x.Tonel)
                    .Include(x => x.Orden)
                    .ThenInclude(x => x.Estado)
                    .FirstOrDefault();

                if (eta == null)
                    return NotFound("No se ha encontrado ningun registro.");
                return Ok(eta);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        //Method para enviar la fecha ESTIMADA de llegada al destino 1era parte
        [HttpPost]
        public async Task<ActionResult> SendEta([FromBody] OrdEmbDet ordEmb)
        {
            try
            {
                var id = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                var user = await userManager.FindByIdAsync(id);

                if (user == null) return BadRequest("No existe el usuario.");

                var acc = 0;
                if (ordEmb.Cod == 0)
                {
                    var Eta = context.Orden.FirstOrDefault(x => x.BatchId == ordEmb.Bol);
                    if (Eta is null)
                        return BadRequest("No se encontro la orden.");
                    //Como por ahora solo documentan la primera parte del ETA, entonces si el ETA tiene un número mayor a cero, al momento de registrar lo anexamos con estatus 26 de Trayecto
                    if (ordEmb.EtaNumber > 0)
                    {
                        ordEmb.Orden!.Codest = 26;
                    }
                    else
                    {
                        //ordEmb.Orden!.Codest = ordEmb.CodEst;
                    }
                    Eta.Codest = ordEmb.Orden!.Codest;

                    context.Update(Eta);
                    ordEmb.Orden = null!;

                    ordEmb.Codusu = user.UserCod;
                    // ordEmb.Eta = ordEmb.EtaNumber.ToString();

                    context.Add(ordEmb);
                    acc = 29;
                }

                else
                {
                    //Cuando los litros esten entregados, el estado pasa a Entregado
                    if (ordEmb.Litent > 0)
                        ordEmb.Orden!.Codest = 10;
                    else
                        //ordEmb.Orden!.Codest = ordEmb.CodEst;

                        ordEmb.Orden!.Estado = null!;
                    ordEmb.Fchmod = DateTime.Now;
                    ordEmb.Codusumod = user.UserCod;

                    context.Update(ordEmb.Orden);

                    ordEmb.Orden = null!;
                    context.Update(ordEmb);
                    acc = 30;
                }

                await context.SaveChangesAsync(id, acc);
                ordEmb.Orden = context.Orden.Where(x => x.BatchId == ordEmb.Bol).Include(x => x.Estado).Include(x => x.Tonel).FirstOrDefault();
                return Ok(ordEmb);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("{bol:int}")]
        public ActionResult GetEta(int? bol)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                if (bol == null)
                    return BadRequest("El bol no puede ir vacio.");

                var orden = context.Orden.Where(x => x.BatchId == bol && x.Id_Tad == id_terminal).Include(x => x.Tonel).Include(x => x.Estado).FirstOrDefault();

                if (orden == null)
                    return BadRequest($"No se contro el bol {bol} en las ordenes cargadas");

                var ordembdet = context.OrdEmbDet.Include(x => x.Orden).IgnoreAutoIncludes().FirstOrDefault(x => x.Bol == bol && x.Orden != null && x.Orden.Id_Tad == id_terminal) ?? new OrdEmbDet();

                if (ordembdet.Cod == 0)
                    ordembdet.Bol = bol;

                ordembdet.Orden = orden;

                return Ok(ordembdet);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("Reporte")]
        public async Task<ActionResult> GetReportes([FromBody] FechasF fechas)
        {
            try
            {
                List<EtaDTO> Ordenes = new List<EtaDTO>();

                //Órdenes sin asignación de transporte-chofer-vehiculo
                //órdenes sin asignar ordenar por BIN
                var ordensSinAsignar = await context.OrdenEmbarque
                    .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Codest == 3 && x.FchOrd != null
                    && x.Bolguidid == null && x.Folio == null && x.CodordCom != null && x.Tonel == null && fechas.TipVenta == x.Destino.Cliente.Tipven && !string.IsNullOrEmpty(fechas.TipVenta)
                    || x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Codest == 3 && x.FchOrd != null
                    && x.Bolguidid == null && x.Folio == null && x.CodordCom != null && x.Tonel == null && string.IsNullOrEmpty(fechas.TipVenta))
                    .Include(x => x.Chofer)
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Estado)
                    .Include(x => x.OrdenCompra)
                    .Include(x => x.Tad)
                    .Include(x => x.Producto)
                    .Include(x => x.Tonel)
                    .ThenInclude(x => x.Transportista)
                    .Include(x => x.OrdenCierre)
                     .Include(x => x.OrdenPedido)
                    .OrderBy(x => x.Fchpet)
                       .Select(e => new EtaDTO()
                       {
                           Referencia = e.FolioSyn,
                           FechaPrograma = e.Fchcar.Value.ToString("yyyy-MM-dd"),
                           EstatusOrden = "Pendiente de Asignar",
                           FechaCarga = null!,
                           Bol = null!,
                           DeliveryRack = e.Destino.Cliente.Tipven,
                           Cliente = e.Destino.Cliente.Den,
                           Destino = e.Destino.Den,
                           Producto = e.Producto.Den,
                           VolNat = null!,
                           VolCar = null!,
                           Transportista = null!,
                           Unidad = null!,
                           Operador = null!,
                           FechaDoc = null!,
                           Eta = null!,
                           FechaEst = null!,
                           Trayecto = null!,
                           Observaciones = null!,
                           FechaRealEta = null!,
                           LitEnt = null!
                       })
                .Take(10000)
                .ToListAsync();
                Ordenes.AddRange(ordensSinAsignar);

                //Órdenes programadas
                var ordens = await context.OrdenEmbarque
                 .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Codest == 3 && x.FchOrd != null
                 && x.Bolguidid == null && x.Folio == null && x.CodordCom != null && x.Tonel != null && fechas.TipVenta == x.Destino.Cliente.Tipven && !string.IsNullOrEmpty(fechas.TipVenta)
                 || x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Codest == 3 && x.FchOrd != null
                 && x.Bolguidid == null && x.Folio == null && x.CodordCom != null && x.Tonel != null && string.IsNullOrEmpty(fechas.TipVenta))
                 .Include(x => x.Chofer)
                 .Include(x => x.Destino)
                 .ThenInclude(x => x.Cliente)
                 .Include(x => x.Estado)
                 .Include(x => x.OrdenCompra)
                 .Include(x => x.Tad)
                 .Include(x => x.Producto)
                 .Include(x => x.Tonel)
                 .ThenInclude(x => x.Transportista)
                 .Include(x => x.OrdenCierre)
                 .OrderBy(x => x.Fchpet)
                 .ThenBy(x => x.Tonel!.Tracto)
                 .Include(x => x.OrdenPedido)
                   .Select(e => new EtaDTO()
                   {
                       Referencia = e.FolioSyn,
                       FechaPrograma = e.Fchcar.Value.ToString("yyyy-MM-dd"),
                       EstatusOrden = e.Estado.den,
                       FechaCarga = null!,
                       Bol = null!,
                       DeliveryRack = e.Destino.Cliente.Tipven,
                       Cliente = e.Destino.Cliente.Den,
                       Destino = e.Destino.Den,
                       Producto = e.Producto.Den,
                       VolNat = e.Compartment == 1 ? Convert.ToDouble(e.Tonel.Capcom) :
                        e.Compartment == 2 ? Convert.ToDouble(e.Tonel.Capcom2) :
                        e.Compartment == 3 ? Convert.ToDouble(e.Tonel.Capcom3) :
                        e.Compartment == 4 ? e.Tonel.Capcom4 : e.Vol,
                       VolCar = null!,
                       Transportista = e.Tonel.Transportista.Den,
                       Unidad = e.Tonel.Veh,
                       Operador = e.Chofer.FullName,
                       FechaDoc = null!,
                       Eta = null!,
                       FechaEst = null!,
                       Trayecto = null!,
                       Observaciones = null!,
                       FechaRealEta = null!,
                       LitEnt = null!
                   })
                 .Take(10000)
                 .ToListAsync();

                Ordenes.AddRange(ordens);

                //Órdenes sin carga-Pedientes de carga
                var pedidosDate = await context.OrdenEmbarque
                .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Bolguidid != null && x.FchOrd != null && x.Codest == 3 && x.Tonel!.Transportista!.Activo == true && fechas.TipVenta == x.Destino.Cliente.Tipven && !string.IsNullOrEmpty(fechas.TipVenta)
                || x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Bolguidid != null && x.FchOrd != null && x.Codest == 3 && x.Tonel!.Transportista!.Activo == true && string.IsNullOrEmpty(fechas.TipVenta)
                || x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.FchOrd != null && x.Codest == 22 && x.Bolguidid != null && x.Tonel.Transportista.Activo == true && fechas.TipVenta == x.Destino.Cliente.Tipven && !string.IsNullOrEmpty(fechas.TipVenta)
                || x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.FchOrd != null && x.Codest == 22 && x.Bolguidid != null && x.Tonel.Transportista.Activo == true && string.IsNullOrEmpty(fechas.TipVenta))
                .Include(x => x.Destino)
                .ThenInclude(x => x.Cliente)
                .Include(x => x.Tad)
                .Include(x => x.Producto)
                .Include(x => x.Tonel)
                .ThenInclude(x => x.Transportista)
                .Include(x => x.Chofer)
                .Include(x => x.Estado)
                .OrderBy(x => x.Fchcar)
                    .Select(e => new EtaDTO()
                    {
                        Referencia = e.FolioSyn,
                        FechaPrograma = e.Fchcar.Value.ToString("yyyy-MM-dd"),
                        EstatusOrden = e.Estado.den,
                        FechaCarga = null!,
                        Bol = null!,
                        DeliveryRack = e.Destino.Cliente.Tipven,
                        Cliente = e.Destino.Cliente.Den,
                        Destino = e.Destino.Den,
                        Producto = e.Producto.Den,
                        VolNat = e.Compartment == 1 ? Convert.ToDouble(e.Tonel.Capcom) :
                        e.Compartment == 2 ? Convert.ToDouble(e.Tonel.Capcom2) :
                        e.Compartment == 3 ? Convert.ToDouble(e.Tonel.Capcom3) :
                        e.Compartment == 4 ? e.Tonel.Capcom4 : e.Vol,
                        VolCar = null!,
                        Transportista = e.Tonel.Transportista.Den,
                        Unidad = e.Tonel.Veh,
                        Operador = e.Chofer.FullName,
                        FechaDoc = null!,
                        Eta = null!,
                        FechaEst = null!,
                        Trayecto = null!,
                        Observaciones = null!,
                        FechaRealEta = null!,
                        LitEnt = null!
                    })
                .Take(10000)
                .ToListAsync();
                Ordenes.AddRange(pedidosDate);
                //Órdenes cargadas
                var pedidosDate2 = await context.Orden
                .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Tonel!.Transportista.Activo == true && x.Codest == 20 && fechas.TipVenta == x.Destino.Cliente.Tipven && !string.IsNullOrEmpty(fechas.TipVenta)
                || x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Tonel!.Transportista.Activo == true && x.Codest == 20 && string.IsNullOrEmpty(fechas.TipVenta))
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Estado)
                    .Include(x => x.Producto)
                    .Include(x => x.Chofer)
                    .Include(x => x.Tonel)
                    .ThenInclude(x => x.Transportista)
                    .Include(x => x.OrdenEmbarque)
                    .Include(x => x.OrdEmbDet)
                    .OrderBy(x => x.Fchcar)
                      .Select(e => new EtaDTO()
                      {
                          Referencia = e.Ref,
                          FechaPrograma = e.OrdenEmbarque.Fchcar.Value.ToString("yyyy-MM-dd"),
                          EstatusOrden = e.Estado.den,
                          FechaCarga = e.Fchcar.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                          Bol = e.BatchId,
                          DeliveryRack = e.Destino.Cliente.Tipven,
                          Cliente = e.Destino.Cliente.Den,
                          Destino = e.Destino.Den,
                          Producto = e.Producto.Den,
                          VolNat = e.Vol2,
                          VolCar = e.Vol,
                          Transportista = e.Tonel.Transportista.Den,
                          Unidad = e.Tonel.Veh,
                          Operador = e.Chofer.FullName,
                          FechaDoc = null!,
                          Eta = null!,
                          FechaEst = null!,
                          Trayecto = null!,
                          Observaciones = null!,
                          FechaRealEta = null!,
                          LitEnt = null!
                      })
                    .Take(10000)
                    .ToListAsync();
                Ordenes.AddRange(pedidosDate2);

                //Órdenes ETA-Trayecto
                List<EtaDTO> newOrden = new List<EtaDTO>();
                var Eta = await context.Orden
                      .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Tonel!.Transportista!.Activo == true && fechas.TipVenta == x.Destino.Cliente.Tipven && !string.IsNullOrEmpty(fechas.TipVenta)
                          || x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Tonel!.Transportista!.Activo == true && string.IsNullOrEmpty(fechas.TipVenta)
                      )
                      .Include(x => x.OrdEmbDet)
                      .Include(x => x.Destino)
                      .ThenInclude(x => x.Cliente)
                      .Include(x => x.Estado)
                      .Include(x => x.Producto)
                      .Include(x => x.Chofer)
                      .Include(x => x.Tonel)
                      .ThenInclude(x => x.Transportista)
                      .Include(x => x.OrdEmbDet)
                      .OrderBy(x => x.Fchcar)
                       .Select(e => new EtaDTO()
                       {
                           Referencia = e.Ref,
                           FechaPrograma = e.OrdenEmbarque.Fchcar.Value.ToString("yyyy-MM-dd"),
                           EstatusOrden = e.Estado.den,
                           FechaCarga = e.Fchcar.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                           Bol = e.BatchId,
                           DeliveryRack = e.Destino.Cliente.Tipven,
                           Cliente = e.Destino.Cliente.Den,
                           Destino = e.Destino.Den,
                           Producto = e.Producto.Den,
                           VolNat = e.Vol2,
                           VolCar = e.Vol,
                           Transportista = e.Tonel.Transportista.Den,
                           Unidad = e.Tonel.Veh,
                           Operador = e.Chofer.FullName,
                           FechaDoc = e.OrdEmbDet.FchDoc.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                           Eta = e.OrdEmbDet.FchDoc!.Value.Subtract(e.OrdEmbDet.Fchlleest.Value!).ToString("hh\\:mm"),
                           FechaEst = e.OrdEmbDet.Fchlleest.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                           Trayecto = "ENTREGADO",
                           Observaciones = e.OrdEmbDet!.Obs,
                           FechaRealEta = e.OrdEmbDet.Fchrealledes.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                           LitEnt = e.OrdEmbDet.Litent
                       })
                      .Take(10000)
                      .ToListAsync();
                foreach (var item in Eta)
                    if (!newOrden.Contains(item))
                        newOrden.Add(item);


                Ordenes.AddRange(newOrden);

                return Ok(Ordenes);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("Reportes")]
        public async Task<ActionResult> TuxpanEtas([FromBody] FechasF fechas)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                //Si seleccionan los dos tipos de Delivery
                if (fechas.Estado == 1)
                {
                    List<EtaDTO> Ordenes = new List<EtaDTO>();
                    //&& fechas.Estado.Equals(1) == x.Destino!.Cliente!.Tipven!.Contains("terno")
                    var ordenesTotales = await context.OrdenEmbarque.IgnoreAutoIncludes()
                       .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Modelo_Venta_Orden == Tipo_Venta.Delivery && x.Codtad == id_terminal)
                        .Include(x => x.Orden)
                        .ThenInclude(x => x.OrdEmbDet)
                        .Include(x => x.Orden)
                        .Include(x => x.Destino)
                        .ThenInclude(x => x.Cliente)
                        .Include(x => x.Estado)
                        .Include(x => x.Orden)
                        .ThenInclude(x => x.Estado)
                        .Include(x => x.Tad)
                        .Include(x => x.Producto)
                        .Include(x => x.Tonel)
                        .ThenInclude(x => x.Transportista)
                        .Include(x => x.Chofer)
                        .Include(x => x.OrdenCierre)
                       .OrderBy(x => x.Fchpet)
                       .Select(x => x.Obtener_OrdenesETATuxpan())
                       .Take(10000)
                       .ToListAsync();
                    Ordenes.AddRange(ordenesTotales);


                    return Ok(Ordenes);
                }
                else if (fechas.Estado == 2)
                {
                    List<EtaDTO> Ordenes = new List<EtaDTO>();

                    var ordenesTotales = await context.OrdenEmbarque.IgnoreAutoIncludes()
                        .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Modelo_Venta_Orden == Tipo_Venta.Rack && x.Codtad == id_terminal)
                        .Include(x => x.Orden)
                        .ThenInclude(x => x.OrdEmbDet)
                        .Include(x => x.Orden)
                        .Include(x => x.Destino)
                        .ThenInclude(x => x.Cliente)
                        .Include(x => x.Estado)
                        .Include(x => x.Orden)
                        .ThenInclude(x => x.Estado)
                        .Include(x => x.Tad)
                        .Include(x => x.Producto)
                        .Include(x => x.Tonel)
                        .ThenInclude(x => x.Transportista)
                        .Include(x => x.Chofer)
                        .Include(x => x.OrdenCierre)
                        .OrderBy(x => x.Fchpet)
                        .Select(x => x.Obtener_OrdenesETATuxpan())
                        .Take(10000)
                        .ToListAsync();
                    Ordenes.AddRange(ordenesTotales);


                    return Ok(Ordenes);
                }
                else if (fechas.Estado == 3)
                {
                    List<EtaDTO> Ordenes = new List<EtaDTO>();

                    var ordenesTotales = await context.OrdenEmbarque.IgnoreAutoIncludes()
                        .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Codtad == id_terminal)
                        .Include(x => x.Orden)
                        .ThenInclude(x => x.OrdEmbDet)
                        .Include(x => x.Orden)
                        .Include(x => x.Destino)
                        .ThenInclude(x => x.Cliente)
                        .Include(x => x.Estado)
                        .Include(x => x.Orden)
                        .ThenInclude(x => x.Estado)
                        .Include(x => x.Tad)
                        .Include(x => x.Producto)
                        .Include(x => x.Tonel)
                        .ThenInclude(x => x.Transportista)
                        .Include(x => x.Chofer)
                        .Include(x => x.OrdenCierre)
                        .OrderBy(x => x.Fchpet)
                        .Select(x => x.Obtener_OrdenesETATuxpan())
                        .Take(10000)
                        .ToListAsync();
                    Ordenes.AddRange(ordenesTotales);


                    return Ok(Ordenes);
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

        [HttpGet("Reportesnotuxpan")]
        public async Task<ActionResult> EtaNoTuxpan([FromQuery] EtaNTDTO eta)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var Ordenes = context.OrdenEmbarque
                    .IgnoreAutoIncludes()
                    .AsNoTracking()
                    .Where(x => x.Fchcar >= eta.Fecha_Inicio && x.Fchcar <= eta.Fecha_Fin && x.Codtad == id_terminal)
                    .Include(x => x.Datos_Facturas)
                    .Include(x => x.Orden)
                    .ThenInclude(x => x.OrdEmbDet)
                    .Include(x => x.Chofer)
                    .Include(x => x.Orden)
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Estado)
                    .Include(x => x.OrdenCompra)
                    .Include(x => x.Tad)
                    .Include(x => x.Producto)
                    .Include(x => x.Tonel)
                    .ThenInclude(x => x.Transportista)
                    .Include(x => x.OrdenCierre)
                    .Include(x => x.OrdenPedido)
                    .Include(x => x.HistorialEstados)
                    .ThenInclude(x => x.Estado)
                    .Include(x => x.Orden)
                    .ThenInclude(x => x.Redireccionamiento)
                    .OrderBy(x => x.Fchpet)
                    .AsQueryable();

                if (eta.ModeloCompra == TipoCompraFiltro.Delivery)
                    Ordenes = Ordenes.Where(x => x.ModeloCompra == TipoCompra.Delivery);

                if (eta.ModeloCompra == TipoCompraFiltro.Rack)
                    Ordenes = Ordenes.Where(x => x.ModeloCompra == TipoCompra.Rack);

                if (eta.MdVenta == TipoVentaFiltro.Interno)
                    Ordenes = Ordenes.Where(x => x.Destino != null && x.Destino.ModeloVenta == Modelo_Venta.Interno);

                if (eta.MdVenta == TipoVentaFiltro.Externo)
                    Ordenes = Ordenes.Where(x => x.Destino != null && x.Destino.ModeloVenta == Modelo_Venta.Externo);

                if (eta.MdVenta == TipoVentaFiltro.AmbosDelivery)
                    Ordenes = Ordenes.Where(x => x.Destino != null && (x.Destino.ModeloVenta == Modelo_Venta.Externo || x.Destino.ModeloVenta == Modelo_Venta.Interno));

                if (eta.MdVenta == TipoVentaFiltro.Rack)
                    Ordenes = Ordenes.Where(x => x.Destino != null && x.Destino.ModeloVenta == Modelo_Venta.Rack);

                if (eta.Excel)
                {
                    ExcelPackage.LicenseContext = LicenseContext.Commercial;
                    var excel = new ExcelPackage();
                    var worksheet = excel.Workbook.Worksheets.Add("ETA");
                    //Formación de Excel
                    var tablebody = worksheet.Cells["A1"].LoadFromCollection(Ordenes.Select(x => mapper.Map<EtaNTDTO>(x)), c =>
                        {
                            c.PrintHeaders = true;
                            c.TableStyle = TableStyles.Medium2;
                        });

                    worksheet.Cells[1, 1, worksheet.Dimension.End.Row, worksheet.Dimension.End.Column].AutoFitColumns();

                    return Ok(excel.GetAsByteArray());
                }

                return Ok(await Ordenes.Select(x => mapper.Map<EtaNTDTO>(x)).ToListAsync());
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("etaexterno")]
        public async Task<ActionResult> GetReporteEta([FromQuery] Folio_Activo_Vigente param)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var ordenes = context.OrdenEmbarque.Where(x => ((x.Orden == null && x.Fchcar >= param.Fecha_Inicio && x.Fchcar <= param.Fecha_Fin && x.Codtad == id_terminal && x.Codest != 14)
                || (x.Orden != null && x.Orden.Fchcar >= param.Fecha_Inicio && x.Fchcar <= param.Fecha_Fin && x.Orden.Id_Tad == id_terminal && x.Orden.Codest != 14)))
                .Select(x => new EtaDTO()
                {
                    Referencia = x.FolioSyn,
                    Cliente = x.Obtener_Cliente_De_Orden,
                    Destino = x.Obtener_Cliente_De_Orden,
                    Producto = x.Obtener_Producto_De_Orden,
                    FechaCarga = x.Obtener_Fecha_De_Carga_De_Orden.ToString("yyyy-MM-dd"),
                    FechaPrograma = x.Fchcar.Value.ToString("yyyy-MM-dd"),
                    FechaDoc = x.Orden.OrdEmbDet.FchDoc.Value.ToString("yyyy-MM-dd"),
                    EstatusOrden = x.Obtener_Estado_De_Orden,
                    Bol = x.Orden.BatchId,
                    DeliveryRack = x.Obtener_Modelo_Venta_Orden,
                    VolCar = x.Obtener_Volumen_De_Orden(),
                    Transportista = x.Orden == null ? x.Tonel.Transportista.Den : x.Orden.Tonel.Transportista.Den,
                    Unidad = x.Obtener_Tonel_De_Orden,
                    Eta = x.Orden.Eta,
                    FechaEst = x.Orden.OrdEmbDet.Fchlleest.Value.ToString("yyyy-MM-dd"),
                    Observaciones = x.Orden.OrdEmbDet.Obs,
                    FechaRealEta = x.Orden.OrdEmbDet.Fchrealledes.Value.ToString("yyyy-MM-dd"),
                    LitEnt = x.Orden.OrdEmbDet.Litent,
                    Unidad_Negocio = x.Orden == null ? x.Tad.Den : x.Orden.Terminal.Den,
                    Numero_Orden = x.Orden.NOrden,
                    Pedimento = x.Orden.Pedimento,
                    Sellos = x.Orden.SealNumber
                }).AsQueryable();

                if (!string.IsNullOrEmpty(param.Comentarios))
                    ordenes = ordenes.Where(x => x.DeliveryRack.ToLower().Equals(param.Comentarios.ToLower()));

                return Ok(ordenes);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        //Method para exportación de reportes mediante lapso de fechas
        [HttpPost("Etareporte")]
        public async Task<ActionResult> GetEta([FromBody] FechasF fechas)
        {
            try
            {
                List<EtaDTO> newOrden = new List<EtaDTO>();
                var Eta = await context.Orden
                        .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Tonel!.Transportista!.Activo == true && fechas.TipVenta == x.Destino.Cliente.Tipven && !string.IsNullOrEmpty(fechas.TipVenta) || x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Tonel!.Transportista!.Activo == true && string.IsNullOrEmpty(fechas.TipVenta))
                        .Include(x => x.OrdEmbDet)
                        .Include(x => x.Destino)
                        .ThenInclude(x => x.Cliente)
                        .Include(x => x.Estado)
                        .Include(x => x.Producto)
                        .Include(x => x.Chofer)
                        .Include(x => x.Tonel)
                        .ThenInclude(x => x.Transportista)

                        .OrderBy(x => x.Fchcar)

                         .Select(e => new EtaDTO()
                         {
                             Referencia = e.Ref,
                             FechaPrograma = e.OrdenEmbarque.Fchcar.Value.ToString("yyyy-MM-dd"),
                             EstatusOrden = "CLOSED",
                             FechaCarga = e.Fchcar.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                             Bol = e.BatchId,
                             DeliveryRack = e.Destino.Cliente.Tipven,
                             Cliente = e.Destino.Cliente.Den,
                             Destino = e.Destino.Den,
                             Producto = e.Producto.Den,
                             VolNat = e.Vol2,
                             VolCar = e.Vol,
                             Transportista = e.Tonel.Transportista.Den,
                             Unidad = e.Tonel.Veh,
                             Operador = e.Chofer.FullName,
                             FechaDoc = e.OrdEmbDet.FchDoc.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                             Eta = e.OrdEmbDet.FchDoc!.Value.Subtract(e.OrdEmbDet.Fchlleest.Value!).ToString("hh\\:mm") ?? string.Empty,
                             FechaEst = e.OrdEmbDet.Fchlleest.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                             Trayecto = "ENTREGADO",
                             Observaciones = e.OrdEmbDet!.Obs,
                             FechaRealEta = e.OrdEmbDet.Fchrealledes.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                             LitEnt = e.OrdEmbDet.Litent
                         })
                        .Take(10000)
                        .ToListAsync();

                foreach (var item in Eta)
                    if (!newOrden.Contains(item))
                        newOrden.Add(item);

                return Ok(newOrden);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}

