using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using GComFuelManager.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using GComFuelManager.Server.Helpers;
using Microsoft.AspNetCore.Identity;
using GComFuelManager.Server.Identity;

namespace GComFuelManager.Server.Controllers.AsignacionUnidadesController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DestinoController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly VerifyUserToken verifyUser;
        private readonly UserManager<IdentityUsuario> userManager;
        private readonly VerifyUserId verify;
        private readonly User_Terminal _terminal;

        public DestinoController(ApplicationDbContext context, VerifyUserToken verifyUser, UserManager<IdentityUsuario> userManager, VerifyUserId verify, User_Terminal _Terminal)
        {
            this.context = context;
            this.verifyUser = verifyUser;
            this.userManager = userManager;
            this.verify = verify;
            this._terminal = _Terminal;
        }

        //[HttpGet]
        //public async Task<ActionResult> Get()
        //{
        //    try
        //    {
        //        var destinos = await context.Destino
        //            .Where(x => x.Activo == true)
        //            .Select(x => new CodDenDTO { Cod = x.Cod, Den = x.Den })
        //            .ToListAsync();
        //        return Ok(destinos);
        //    }
        //    catch (Exception e)
        //    {

        //        return BadRequest(e.Message);
        //    }
        //}

        [HttpGet("comprador")]
        public ActionResult GetClientComprador()
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var userId = verifyUser.GetName(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return BadRequest();

                var user = context.Usuario.FirstOrDefault(x => x.Usu == userId);
                if (user == null)
                    return BadRequest();

                var cliente = context.Cliente.FirstOrDefault(x => x.Cod == user.CodCte);
                if (cliente is null) { return BadRequest("El cliente no existe"); }

                var cliente_terminal = context.Cliente.FirstOrDefault(x => x.Den == cliente.Den && x.Id_Tad == id_terminal);
                if (cliente_terminal is null) { return BadRequest("No existe el cliente en la terminal"); }

                var destinos = context.Destino.IgnoreAutoIncludes().Where(x => x.Codcte == cliente_terminal.Cod && x.Activo == true && x.Id_Tad == id_terminal)
                    .IgnoreAutoIncludes().OrderBy(x => x.Den).ToList();

                return Ok(destinos);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("{cod:int}")]
        public async Task<ActionResult> ChangeStatus([FromRoute] int cod, [FromBody] bool status)
        {
            try
            {
                if (cod == 0)
                {
                    return BadRequest();
                }

                var destino = context.Destino.Where(x => x.Cod == cod).FirstOrDefault();
                if (destino == null)
                {
                    return NotFound();
                }
                destino.Activo = status;

                context.Update(destino);
                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}

