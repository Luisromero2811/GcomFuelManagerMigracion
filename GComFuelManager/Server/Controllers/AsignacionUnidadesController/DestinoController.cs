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
        private readonly IUsuarioHelper usuarioHelper;

        public DestinoController(ApplicationDbContext context, IUsuarioHelper usuarioHelper)
        {
            this.context = context;
            this.usuarioHelper = usuarioHelper;
        }

        [HttpGet]
        public async Task<ActionResult> GetClientComprador()
        {
            try
            {
                var user = await usuarioHelper.GetUsuario();



                return Ok();
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

