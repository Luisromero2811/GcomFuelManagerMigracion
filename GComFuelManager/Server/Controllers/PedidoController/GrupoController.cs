using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, Administrador Sistema, Revision Precios ,Administrador de Usuarios ,Direccion, Gerencia, Ejecutivo de Cuenta Comercial, Comprador, Programador, Ejecutivo de Cuenta Operativo, Lectura de Cierre de Orden, Cierre Pedidos, Consulta Precios, Cliente Lectura, Contador, Precios, Revision Precios")]
    public class GrupoController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly VerifyUserId verifyUser;
        private readonly UserManager<IdentityUsuario> userManager;
        private readonly User_Terminal user_Terminal;

        public GrupoController(ApplicationDbContext context, VerifyUserId verifyUser, UserManager<IdentityUsuario> userManager, User_Terminal user_Terminal)
        {
            this.context = context;
            this.verifyUser = verifyUser;
            this.userManager = userManager;
            this.user_Terminal = user_Terminal;
        }

        [HttpPost("cliente")]
        public async Task<ActionResult> AsignCliente([FromBody] Cliente cliente)
        {
            try
            {
                var id_terminal = user_Terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                if (cliente is null)
                {
                    return BadRequest();
                }
                cliente.Id_Tad = id_terminal;
                cliente.Tad = null!;
                cliente.Terminales = null!;
                context.Update(cliente);
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
