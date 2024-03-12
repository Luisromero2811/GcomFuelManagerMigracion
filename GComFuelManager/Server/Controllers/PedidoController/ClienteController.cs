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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, Administrador Sistema, Administrador de Usuarios ,Direccion, Gerencia, Ejecutivo de Cuenta Comercial, Comprador, Programador, Ejecutivo de Cuenta Operativo, Lectura de Cierre de Orden, Cierre Pedidos, Consulta Precios, Cliente Lectura, Contador, Precios, Revision Precios")]
    public class ClienteController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUsuario> userManager;
        private readonly VerifyUserId verifyUserId;
        private readonly User_Terminal terminal;

        public ClienteController(ApplicationDbContext context, UserManager<IdentityUsuario> userManager, VerifyUserId verifyUserId, User_Terminal _Terminal)
        {
            this.context = context;
            this.userManager = userManager;
            this.verifyUserId = verifyUserId;
            terminal = _Terminal;
        }

        [HttpGet("{grupo:int}")]
        public async Task<ActionResult> Get(int grupo)
        {
            try
            {
                var grupos = await context.Cliente
                    .Where(x => x.codgru == grupo && x.Activo == true)
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

        [HttpGet("filtraractivos")]
        public ActionResult Obtener_Clientes_Activos([FromQuery] Cliente cliente)
        {
            try
            {
                var clientes = context.Cliente.Where(x => x.Activo).IgnoreAutoIncludes().AsQueryable();

                if (!string.IsNullOrEmpty(cliente.Den))
                    clientes = clientes.Where(x => x.Den!.ToLower().Contains(cliente.Den.ToLower()));

                return Ok(clientes);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("Grupo/{grupo:int}")]
        public async Task<ActionResult> GetCliente(int grupo)
        {
            try
            {
                var grupos = await context.Cliente
                    .Where(x => x.codgru == grupo && x.Activo == true)
                    .OrderBy(x => x.Den)
                    .ToListAsync();

                return Ok(grupos);
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }

        [HttpPost("crear")]
        public async Task<ActionResult> PostCliente([FromBody] Cliente cliente)
        {
            try
            {
                if (cliente is null)
                    return BadRequest();
                //Si el cliente viene en ceros del front lo agregamos como nuevo sino actualizamos
                if (cliente.Cod == 0)
                {
                    cliente.grupo = cliente.grupo;
                    //Agregamos cliente
                    context.Add(cliente);
                }
                else
                {
                    context.Update(cliente);
                }
                await context.SaveChangesAsync();
                return Ok();

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        [HttpPost("relacion")]
        public async Task<ActionResult> PostClienteTerminal([FromBody] ClienteTadDTO clienteTadDTO)
        {
            try
            {
                //Si el cliente es nulo, retornamos un notfound
                if (clienteTadDTO is null)
                    return NotFound();

                foreach (var terminal in clienteTadDTO.Tads)
                {
                    foreach (var cliente in clienteTadDTO.Clientes)
                    {
                        if (!context.Cliente_Tad.Any(x => x.Id_Terminal == terminal.Cod && x.Id_Cliente == cliente.Cod))
                        {
                            Cliente_Tad clientetad = new()
                            {
                                Id_Cliente = cliente.Cod,
                                Id_Terminal = terminal.Cod
                            };
                            context.Add(clientetad);
                        }
                    }
                }
                await context.SaveChangesAsync();

                return Ok();

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        //[HttpPost("relacion")]
        //public async Task<ActionResult> PostClienteTerminal([FromBody] List<Cliente> clientes, [FromQuery] Tad tads)
        //{
        //    try
        //    {
        //        Cliente_Tad cliente_Tad = new Cliente_Tad();
        //        //Si el cliente viene nulo, mandamos un notfound
        //        if (clientes is null)
        //            return NotFound();

        //        foreach (var item in clientes)
        //        {
        //            cliente_Tad = new Cliente_Tad()
        //            {
        //                Id_Cliente = item.Cod,
        //                Id_Terminal = tads.Cod
        //            };
        //            context.Add(cliente_Tad);
        //            await context.SaveChangesAsync();
        //        }
        //        return Ok();

        //    }
        //    catch (Exception e)
        //    {
        //        return BadRequest(e.Message);
        //    }
        //}

        //    [HttpPost("relacion")]
        //    public async Task<ActionResult> PostClienteTerminal([FromBody] List<Cliente> clientes, [FromQuery] Tad tads)
        //    {
        //        try
        //        {
        //            Cliente_Tad cliente_Tad = new Cliente_Tad();
        //            //Si el cliente viene nulo, mandamos un notfound
        //            if (clientes is null)
        //                return NotFound();

        //            foreach (var item in clientes)
        //            {
        //                cliente_Tad = new Cliente_Tad()
        //                {
        //                    Id_Cliente = item.Cod,
        //                    Id_Terminal = tads.Cod
        //                };
        //                context.Add(cliente_Tad);
        //                await context.SaveChangesAsync();
        //            }
        //            return Ok();

        //        }
        //        catch (Exception e)
        //        {
        //            return BadRequest(e.Message);
        //        }
        //    }

    }
}
//Obtenemos la terminal
//var id_terminal = terminal.Obtener_Terminal(context, HttpContext);
//if (id_terminal == 0)
//{
//    return BadRequest();
//}