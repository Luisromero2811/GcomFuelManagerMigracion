using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OriginadorController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUsuario> userManager;
        private readonly VerifyUserId verifyUser;

        public OriginadorController(ApplicationDbContext context, UserManager<IdentityUsuario> userManager, VerifyUserId verifyUser)
        {
            this.context = context;
            this.userManager = userManager;
            this.verifyUser = verifyUser;
        }

        [HttpGet]
        public ActionResult Obtener_Originadores([FromQuery] Originador originador)
        {
            try
            {
                var originadores = context.Originadores.IgnoreAutoIncludes().AsQueryable();

                if (!string.IsNullOrEmpty(originador.Nombre))
                    originadores = originadores.Where(x => x.Nombre.ToLower().Contains(originador.Nombre.ToLower()));

                return Ok(originadores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("relacion/{id:int}")]
        public ActionResult Obtener_Originadores([FromRoute] int id, [FromQuery] Originador originador)
        {
            try
            {
                var originadores = context.Originadores.IgnoreAutoIncludes().Where(x => x.Vendedor_Originador.Any(x => x.VendedorId == id)).Include(x => x.Vendedor_Originador).AsQueryable();

                if (!string.IsNullOrEmpty(originador.Nombre) || !string.IsNullOrWhiteSpace(originador.Nombre))
                    originadores = originadores.Where(x => x.Nombre.ToLower().Contains(originador.Nombre));

                return Ok(originadores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("filtrar")]
        public ActionResult Obtener_Originadores_Activos([FromQuery] Originador originador)
        {
            try
            {
                var originadores = context.Originadores.Where(x => x.Activo).IgnoreAutoIncludes().AsQueryable();

                if (!string.IsNullOrEmpty(originador.Nombre))
                    originadores = originadores.Where(x => x.Nombre.ToLower().Contains(originador.Nombre.ToLower()));

                return Ok(originadores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Guardar_Originadores([FromBody] Originador originador)
        {
            try
            {
                if (originador is null)
                    return NotFound();

                if (string.IsNullOrEmpty(originador.Nombre) || string.IsNullOrWhiteSpace(originador.Nombre))
                    return BadRequest("Nombre de originador no valido");

                var id = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                if (originador.Id != 0)
                {
                    context.Update(originador);
                    await context.SaveChangesAsync(id, 40);
                }
                else
                {
                    context.Add(originador);
                    await context.SaveChangesAsync(id, 39);
                }

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
