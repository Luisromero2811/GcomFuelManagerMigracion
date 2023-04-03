using System;
using GComFuelManager.Shared.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GComFuelManager.Server.Identity;

namespace GComFuelManager.Server.Controllers.UsuarioController
{
	[Route("api/usuarios")]
	[ApiController]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class UsuarioController : ControllerBase
	{
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUsuario> userManager;

        public UsuarioController(ApplicationDbContext context, UserManager<IdentityUsuario> userManager)
		{
            this.context = context;
            this.userManager = userManager;
        }
		[HttpGet("UsuariosList")]
		public async Task<ActionResult> GetUsers()
		{
			try
			{
				var usuarios = context.Usuario.AsEnumerable();

				return Ok(usuarios);
			}
			catch (Exception e)
			{
				return BadRequest(e.Message);
			}
		}
		[HttpGet("roles")]
		public async Task<ActionResult<List<RolDTO>>> Get()
		{
			return await context.Roles.Select(x => new RolDTO { ID = x.Id, NombreRol = x.Name! }).ToListAsync();
		}
	}
}

