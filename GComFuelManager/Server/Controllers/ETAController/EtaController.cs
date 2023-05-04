﻿using System;
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
                    return Ok(eta);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        //Method para enviar la fecha ESTIMADA de llegada al destino 1era parte
        [HttpPost("SendEta")]
        public async Task<ActionResult> SendEta([FromBody] EtaDTO etaDTO)
        {
            try
            {
                context.Add(etaDTO);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        //Method para enviar la fecha real de llegada al destino 2da y ultima parte del formulario 
        [HttpPost("SendRealEta")]
        public async Task<ActionResult> SendRealEta([FromBody] EtaDTO etaDTO)
        {
            try
            {
                context.Add(etaDTO);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        //Method para editar fecha ESTIMADA de llegada a destino 1era parte
        [HttpPut("EditarEta")]
        public async Task<ActionResult> PutEta(OrdEmbDet ordEmb)
        {
            try
            {
                context.Update(ordEmb);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        //Method para editar la fecha real de llegada 2da y última parte
        [HttpPut("EditarEtaReal")]
        public async Task<ActionResult> PutRealEta(OrdEmbDet ordEmb)
        {
            try
            {
                context.Update(ordEmb);
                await context.SaveChangesAsync();
                return Ok();
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