using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Server.Controllers.CRM
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class CRMDivisionController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUsuario> manager;

        public CRMDivisionController(ApplicationDbContext context, UserManager<IdentityUsuario> manager)
        {
            this.context = context;
            this.manager = manager;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            try
            {
                var divisiones = await context.CRMDivisiones.Where(x => x.Activo).ToListAsync();
                return Ok(divisiones);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("usuario")]
        public async Task<ActionResult> GetDivisionesUsuario()
        {
            try
            {
                if (HttpContext.User.Identity is null) { return NotFound(); }
                if (string.IsNullOrEmpty(HttpContext.User.Identity.Name) || string.IsNullOrWhiteSpace(HttpContext.User.Identity.Name)) { return NotFound(); }

                var user = await manager.FindByNameAsync(HttpContext.User.Identity.Name);
                if (user is null) { return NotFound(); }

                List<CRMUsuarioDivision> usuariodivisiones = await context.CRMUsuarioDivisiones.Where(x => x.UsuarioId == user.Id).ToListAsync();
                var divisiones = await context.CRMDivisiones.Where(x => usuariodivisiones.Any(y => y.DivisionId == x.Id)).ToListAsync();
                return Ok(divisiones);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
