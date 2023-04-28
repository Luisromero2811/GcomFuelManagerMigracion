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

    [Route("api/Eta")]
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
        [HttpPost("Etareporte")]
        public async Task<ActionResult> GetEta([FromBody] FechasF fechas)
        {
            try
            {
                var Eta = await context.Orden.OrderBy(x => x.Destino.Den).ThenBy(x => x.Fchcar).ThenBy(x => x.Producto.Den).ThenBy(x => x.BatchId)
                    .ThenBy(x => x.Chofer.Den).ThenBy(x => x.Tonel.Placa).ThenBy(x => x.Tonel.Tracto).ThenBy(x => x.Tonel.Transportista.den)
                    .ThenBy(x => x.Coduni).ThenBy(x => x.Ref).ThenBy(x => x.Codprd2).ThenBy(x => x.Codest)
                    .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Tonel!.Transportista!.activo == true )
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
                         Cliente = e.Destino.Cliente.Den,
                         Destino = e.Destino.Den,
                         Producto = e.Producto.Den,
                         VolNat = e.Vol2,
                         VolCar = e.Vol,
                         Transportista = e.Tonel.Transportista.den,
                         Unidad = e.Tonel.Veh,
                         Operador = e.Chofer.Den,
                         FechaDoc = e.OrdEmbDet.FchDoc.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                         Eta = e.OrdEmbDet.Eta,
                         FechaEst = e.OrdEmbDet.Fchlleest.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                         Trayecto = "ENTREGADO",
                         Observaciones = e.OrdEmbDet!.Obs,
                         FechaRealEta = e.OrdEmbDet.Fchrealledes.Value.ToString("yyyy-MM-dd"),
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

//d.den, o.fchcar, p.den, ct.den, o.batchId, ch.den, ch.shortden,
//t.placa, t.tracto, tr.den, o.coduni, o.ref, o.codprd2, o.codest
