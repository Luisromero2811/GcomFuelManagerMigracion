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
        private readonly User_Terminal _terminal;
        private readonly UserManager<IdentityUsuario> userManager;
        private readonly VerifyUserId verifyUser;

        public ClienteController(ApplicationDbContext context, User_Terminal _Terminal, UserManager<IdentityUsuario> userManager, VerifyUserId verifyUser)
        {
            this.context = context;
            this._terminal = _Terminal;
            this.userManager = userManager;
            this.verifyUser = verifyUser;
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


        [HttpGet("listado")]
        public async Task<ActionResult> GetCliente([FromQuery] ParametrosBusquedaCatalogo grupo)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var grupos = context.Cliente
                    .Include(x => x.Terminales)
                    .Where(x => x.Codgru == grupo.codgru && x.Terminales.Any(x => x.Cod == id_terminal))
                    .OrderBy(x => x.Den)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(grupo.nombrecliente))
                    grupos = grupos.Where(x => x.Den != null && !string.IsNullOrEmpty(x.Den) && x.Den.ToLower().Contains(grupo.nombrecliente.ToLower()));

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

        [HttpPost("crear")]
        public async Task<ActionResult> PostCliente([FromBody] Cliente cliente)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                if (cliente is null)
                    return BadRequest();
                //Si el cliente viene en ceros del front lo agregamos como nuevo sino actualizamos
                if (cliente.Cod == 0)
                {
                    cliente.Id_Tad = id_terminal;
                    //Agregamos cliente
                    if (string.IsNullOrEmpty(cliente.Codsyn) || string.IsNullOrWhiteSpace(cliente.Codsyn))
                        cliente.Codsyn = Codsyn_Random();

                    context.Add(cliente);
                    await context.SaveChangesAsync();
                    if (!context.Cliente_Tad.Any(x => x.Id_Terminal == id_terminal && x.Id_Cliente == cliente.Cod))
                    {
                        Cliente_Tad clientetad = new()
                        {
                            Id_Cliente = cliente.Cod,
                            Id_Terminal = id_terminal
                        };
                        context.Add(clientetad);
                        await context.SaveChangesAsync();
                    }
                }
                else
                {
                    cliente.Id_Tad = id_terminal;
                    cliente.Tad = null!;
                    cliente.Terminales = null!;
                    context.Update(cliente);
                    await context.SaveChangesAsync();
                }

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

        //[HttpPost("borrar/relacion")]
        //public async Task<ActionResult> Borrar_Relacion([FromBody] Cliente_Tad clienteterminal)
        //{
        //    try
        //    {
        //        if (clienteterminal is null)
        //            return NotFound();

        //        if (context.OrdenEmbarque.Include(x => x.Destino).Any(x => x.Codtad == clienteterminal.Id_Terminal && x.Destino!.Cliente!.Cod == clienteterminal.Id_Cliente) ||
        //            context.OrdenCierre.Any(x => x.Id_Tad == clienteterminal.Id_Terminal && x.CodCte == clienteterminal.Id_Cliente) ||
        //            context.Orden.Include(x => x.Destino).Any(x => x.Id_Tad == clienteterminal.Id_Terminal && x.Destino!.Cliente!.Cod == clienteterminal.Id_Cliente))
        //        {
        //            return BadRequest("Error, no puede eliminar la relación debido a pedidos u órdenes activas ligadas a este Cliente y Unidad de Negocio");
        //        }

        //        var id = await verifyUser.GetId(HttpContext, userManager);
        //        if (string.IsNullOrEmpty(id))
        //            return BadRequest();

        //        context.Remove(clienteterminal);
        //        await context.SaveChangesAsync();

        //        return Ok(clienteterminal);
        //    }
        //    catch (Exception e)
        //    {
        //        return BadRequest(e.Message);
        //    }
        //}

        [HttpPut("{cod:int}")]
        public async Task<ActionResult> ChangeStatus([FromRoute] int cod, [FromBody] bool status)
        {
            try
            {
                if (cod == 0)
                    return BadRequest();

                var destino = context.Cliente.Where(x => x.Cod == cod).FirstOrDefault();
                if (destino == null)
                {
                    return NotFound();
                }
                destino.Activo = status;

                context.Update(destino);
                var acc = destino.Activo ? 26 : 27;
                var id = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        private string Codsyn_Random()
        {
            var codsyn = new Random().Next(1, 999999).ToString();

            if (context.Cliente.Any(x => !string.IsNullOrEmpty(x.Codsyn) && x.Codsyn.Equals(codsyn)))
                Codsyn_Random();

            return codsyn;
        }
    }
}
