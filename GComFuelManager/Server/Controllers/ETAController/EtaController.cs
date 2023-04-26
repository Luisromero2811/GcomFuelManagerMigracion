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
                //private ICollection<Orden> ordens { get; set; } = null!;
                List<Orden> OrdenesEta = new List<Orden>();
                //Conexión a tabla de Orden
                var Eta = context.Orden
                    .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Tonel!.Transportista.activo == true)
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Estado)
                    .Include(x => x.Producto)
                    .Include(x => x.Chofer)
                    .Include(x => x.Tonel)
                    .ThenInclude(x => x.Transportista)
                    .OrderBy(x => x.Fchcar)
                    .GroupByMany(x => (x.Destino.Den, x.Fchcar, x.Producto.Den, x.Destino.Cliente.Den, x.BatchId, x.Chofer.Den, x.Chofer.Shortden, x.Tonel.Placa, x.Tonel.Tracto, x.Tonel.Transportista.den, x.Coduni, x.Codprd2, x.Codest))
                    .Take(1000)
                    .ToList();
                OrdenesEta.AddRange((ICollection<Orden>)Eta);
                //Conexión con tabla de OrdEmbDet
                var Eta2 = await context.OrdEmbDet
                    .Take(1000)
                    .Select(x => new EtaDTO()
                    {
                        FechaDoc = x.FchDoc,
                        Eta = x.Eta,
                        FechaEst = x.Fchlleest,
                        Observaciones = x.Obs,
                        FechaRealEta = x.Fchrealledes,
                        LitEnt = x.Litent
                    })
                    .ToListAsync();
                OrdenesEta.AddRange((ICollection<Orden>)Eta2);

                //Retorno de datos
                return Ok(OrdenesEta);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}

