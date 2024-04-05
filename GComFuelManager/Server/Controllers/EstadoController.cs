using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class EstadoController : ControllerBase
    {
        readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUsuario> userManager;
        private readonly VerifyUserToken verifyUser;

        public EstadoController(ApplicationDbContext context, UserManager<IdentityUsuario> userManager, VerifyUserToken verifyUser)
        {
            this.context = context;
            this.userManager = userManager;
            this.verifyUser = verifyUser;
        }

        [HttpGet("{id_tipo}")]
        public ActionResult Get([FromRoute] short id_tipo)
        {
            try
            {
                var estados = context.Estado.Where(x => x.Id_Tipo == id_tipo).ToList();
                return Ok(estados);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("cambio/estado")]
        public async Task<ActionResult> Post([FromBody] OrdenEmbarque ordenEmbarque)
        {
            try
            {
                var id = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                if (ordenEmbarque.Estatus is null) { return NotFound(); }
                
                ordenEmbarque.Destino = null!;
                ordenEmbarque.Tad = null!;
                ordenEmbarque.Producto = null!;
                ordenEmbarque.Tonel = null!;
                ordenEmbarque.Chofer = null!;
                ordenEmbarque.Estado = null!;
                ordenEmbarque.Orden = null!;
                ordenEmbarque.HistorialEstados = null!;
                ordenEmbarque.Estatus_Orden = null!;

                HistorialEstados historialEstados = new()
                {
                    Id_Orden = ordenEmbarque.Cod,
                    Id_Estado = (byte)ordenEmbarque.Estatus,
                    Id_Usuario = id
                };
                
                historialEstados.Estado = null!;
                historialEstados.OrdenEmbarque = null!;

                context.Add(historialEstados);
                context.Update(ordenEmbarque);

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
