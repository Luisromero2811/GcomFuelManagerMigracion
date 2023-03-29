using System;
using GComFuelManager.Shared.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Server.Controllers.UsuarioController
{
	[Route("api/[controller]")]
	[ApiController]
	public class UsuarioController : ControllerBase
	{
        private readonly ApplicationDbContext context;

        public UsuarioController(ApplicationDbContext context)
		{
            this.context = context;
        }
		[HttpGet("Usuarios")]
		public async Task<ActionResult> GetUsers()
		{
			try
			{
				var usuarios = "Luis";

				return Ok(usuarios);
			}
			catch (Exception e)
			{
				return BadRequest(e.Message);
			}
		}
	}
}

