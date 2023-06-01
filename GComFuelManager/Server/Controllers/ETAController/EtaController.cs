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

namespace GComFuelManager.Server.Controllers.ETAController
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, Administrador Sistema, Direccion, Gerencia, Ejecutivo de Cuenta Comercial, Programador, Coordinador, Analista Suministros, Auditor, Capturista Recepcion Producto")]
    public class EtaController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUsuario> userManager;

        public EtaController(ApplicationDbContext context, UserManager<IdentityUsuario> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }
        //Filtro para ordenes por Bol 
        [HttpPost("Filtro")]
        public async Task<ActionResult> EtaGet([FromBody] EtaDTO etaDTO)
        {
            try
            {
                var eta = await context.OrdEmbDet
                    .Where(x => x.Bol == etaDTO.Bol)
                    .Include(x => x.Orden)
                    .ThenInclude(x => x.Tonel)
                    .Include(x => x.Orden)
                    .ThenInclude(x => x.Estado)
                    .FirstOrDefaultAsync();
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
                
                if (ordEmb.Cod == 0)
                {
                    var Eta = context.Orden.FirstOrDefault(x => x.BatchId == ordEmb.Bol);

                   Eta.Codest = ordEmb.Orden!.Codest;

                    context.Update(Eta);

                    ordEmb.Orden = null!;
                    context.Add(ordEmb);
                }

                else
                {
                    ordEmb.Orden!.Estado = null!;

                    context.Update(ordEmb.Orden);

                    ordEmb.Orden = null!;
                    TimeSpan? eta = ordEmb.Fchlleest?.Subtract(ordEmb.FchDoc!.Value);
                    ordEmb.Eta = $"{eta?.Hours}{eta?.Seconds}";
                    context.Update(ordEmb);
                }
                   

                await context.SaveChangesAsync();

                return Ok(ordEmb);
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
                var Eta = await context.Orden.OrderBy(x => x.Destino.Den).ThenBy(x => x.Fchcar).ThenBy(x => x.Producto.Den).ThenBy(x => x.BatchId)
                    .ThenBy(x => x.Chofer.Den).ThenBy(x => x.Tonel.Placa).ThenBy(x => x.Tonel.Tracto).ThenBy(x => x.Tonel.Transportista.Den)
                    .ThenBy(x => x.Coduni).ThenBy(x => x.Ref).ThenBy(x => x.Codprd2).ThenBy(x => x.Codest)
                    .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Tonel!.Transportista!.Activo == true && fechas.TipVenta == x.Destino.Cliente.Tipven && !string.IsNullOrEmpty(fechas.TipVenta) || x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Tonel!.Transportista!.Activo == true && string.IsNullOrEmpty(fechas.TipVenta))
                    .Include(x => x.OrdEmbDet)
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Estado)
                    .Include(x => x.Producto)
                    .Include(x => x.Chofer)
                    .Include(x => x.Tonel)
                    .ThenInclude(x => x.Transportista)

                     .Select(e => new EtaDTO()
                     {
                         Referencia = e.Ref,
                         FechaPrograma = e.Fch.Value.ToString("yyyy-MM-dd"),
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
                         Operador = e.Chofer.Den,
                         FechaDoc = e.OrdEmbDet.FchDoc.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                         //Eta = e.OrdEmbDet.Eta,
                         FechaEst = e.OrdEmbDet.Fchlleest.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                         Trayecto = "ENTREGADO",
                         Observaciones = e.OrdEmbDet!.Obs,
                         FechaRealEta = e.OrdEmbDet.Fchrealledes.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                         LitEnt = e.OrdEmbDet.Litent
                     })

                    .Take(1000)
                    .ToListAsync();
                return Ok(Eta);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}

