using System;
using GComFuelManager.Shared.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.Modelos;

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

        [HttpPost("crear")]
        public async Task<ActionResult> Create([FromBody] UsuarioInfo info)
        {
			try
			{
				var userSistema = await context.Usuario.FirstOrDefaultAsync(x => x.Usu == info.UserName);

				if (userSistema != null)
				{
					return BadRequest("El usuario ya existe");
				}

				var userAsp = await userManager.FindByNameAsync(info.UserName);

				if (userAsp != null)
				{
					return BadRequest("El usuario ya existe");
				}

				var newUserSistema = new Usuario { Den = info.Nombre, Usu = info.UserName, Fch = DateTime.Now, Cve = info.Password  };
                //, Cve = info.Password
                context.Add(newUserSistema);
				await context.SaveChangesAsync();

				var newUserAsp = new IdentityUsuario { UserName = newUserSistema.Usu, UserCod = newUserSistema.Cod };
				var result = await userManager.CreateAsync(newUserAsp, newUserSistema.Cve);

				if (!result.Succeeded)
				{
					context.Remove(newUserSistema);
					await context.SaveChangesAsync();
					return BadRequest(result.Errors);
				}

				result = await userManager.AddToRolesAsync(newUserAsp, info.Roles);

				if (!result.Succeeded)
				{
					return BadRequest(result.Errors);
				}

                return Ok(newUserSistema);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}

