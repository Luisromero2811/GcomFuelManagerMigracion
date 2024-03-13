using GComFuelManager.Server.Helpers;
using GComFuelManager.Shared.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, Administrador Sistema, Administrador de Usuarios ,Direccion, Gerencia, Ejecutivo de Cuenta Comercial, Comprador, Programador, Ejecutivo de Cuenta Operativo, Lectura de Cierre de Orden, Cierre Pedidos, Consulta Precios, Cliente Lectura, Contador, Precios, Revision Precios")]
    public class ClienteController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly User_Terminal _terminal;

        public ClienteController(ApplicationDbContext context, User_Terminal _Terminal)
        {
            this.context = context;
            this._terminal = _Terminal;
        }

        [HttpGet("{grupo:int}")]
        public async Task<ActionResult> Get(int grupo)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var grupos = await context.Cliente.IgnoreAutoIncludes()
                    .Where(x => x.Codgru == grupo && x.Activo == true && x.Terminales.Any(x => x.Cod == id_terminal))
                    .Include(x => x.Terminales).IgnoreAutoIncludes()
                    .Select(x => new CodDenDTO { Cod = x.Cod, Den = x.Den! })
                    .OrderBy(x => x.Den)
                    .ToListAsync();
                return Ok(grupos);
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }
    }
}
