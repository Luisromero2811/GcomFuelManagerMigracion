using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using GComFuelManager.Shared.Modelos;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using Microsoft.EntityFrameworkCore;
using GComFuelManager.Shared.DTOs;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GComFuelManager.Server.Controllers.Autorizadores
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AutorizadoresController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly VerifyUserId verifyUser;
        private readonly UserManager<IdentityUsuario> userManager;
        private readonly User_Terminal _terminal;

        public AutorizadoresController(ApplicationDbContext context, VerifyUserId verifyUser, UserManager<IdentityUsuario> userManager, User_Terminal _Terminal)
        {
            this.context = context;
            this.verifyUser = verifyUser;
            this.userManager = userManager;
            this._terminal = _Terminal;
        }

        [HttpGet]
        public ActionResult Obtenee_Autorizadores_Activos_Y_Validos()
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var autorizadores = context.Autorizador.Where(x => x.Terminales.Any(y => y.Cod == id_terminal)).Include(x => x.Terminales).IgnoreAutoIncludes().ToList();

                return Ok(autorizadores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("save")]
        public async Task<ActionResult> PostAutorizador([FromBody] Autorizador autorizadores)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();
                if (autorizadores is null)
                {
                    return NotFound();
                }
                if (autorizadores.Cod == 0)
                {
                    autorizadores.Id_Tad = id_terminal;
                    context.Add(autorizadores);
                }
                else
                {
                    context.Update(autorizadores);
                }
                await context.SaveChangesAsync();
                return Ok();

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("filtraractivos")]
        public ActionResult Obtener_Grupos_Activos([FromQuery] Autorizador grupo)
        {
            try
            {
                var grupos = context.Autorizador
                     .Include(x => x.Terminales)
                    .IgnoreAutoIncludes().AsQueryable();

                if (!string.IsNullOrEmpty(grupo.Den))
                    grupos = grupos.Where(x => x.Den!.ToLower().Contains(grupo.Den.ToLower()));

                return Ok(grupos);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("gruposactivos")]
        public async Task<ActionResult> GetGrupos()
        {
            try
            {
                var grupostransporte = await context.Autorizador
                     .Include(x => x.Terminales)
                    .OrderBy(x => x.Den)
                    .ToListAsync();
                return Ok(grupostransporte);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("relacion")]
        public async Task<ActionResult> PostClienteTerminal([FromBody] ClienteTadDTO clienteTadDTO)
        {
            try
            {
                //Si el cliente es nulo, retornamos un notfound
                if (clienteTadDTO is null)
                    return NotFound();

                foreach (var terminal in clienteTadDTO.Tads)
                {
                    foreach (var grupotransportes in clienteTadDTO.Autorizadors)
                    {
                        if (!context.Autorizadores_Tad.Any(x => x.Id_Terminal == terminal.Cod && x.Id_Autorizador == grupotransportes.Cod))
                        {
                            Autorizadores_Tad grupotransportetad = new()
                            {
                                Id_Autorizador = grupotransportes.Cod,
                                Id_Terminal = terminal.Cod
                            };
                            context.Add(grupotransportetad);
                        }
                    }
                }
                await context.SaveChangesAsync();

                return Ok();

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("borrar/relaciones")]
        public async Task<ActionResult> Borrar_Relaciones([FromBody] Autorizadores_Tad transportista_Tad)
        {
            try
            {
                if (transportista_Tad is null)
                    return NotFound();

                //if (context.OrdenEmbarque.Any(x => x.Codtad == transportista_Tad.Id_Terminal) ||
                //    context.OrdenCierre.Any(x => x.Id_Tad == transportista_Tad.Id_Terminal) ||
                //    context.Orden.Any(x => x.Id_Tad == transportista_Tad.Id_Terminal))
                //{
                //    return BadRequest("Error, no puede eliminar la relación debido a pedidos u órdenes activas ligadas a este Grupo transportista y Unidad de negocio");
                //}

                var id = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                context.Remove(transportista_Tad);
                await context.SaveChangesAsync();

                return Ok(transportista_Tad);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

    }
}

