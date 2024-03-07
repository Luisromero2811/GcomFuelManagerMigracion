﻿using GComFuelManager.Server.Helpers;
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
        public async Task<ActionResult> PostClienteTerminal([FromBody] List<ClienteTadDTO> clienteTadDTOs)
        {
            try
            {
                //Instancia para la tabla de relación del cliente y terminal 
                Cliente_Tad clientetad = new Cliente_Tad();
                //Obtenemos la terminal
                var id_terminal = terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                {
                    return BadRequest();
                }
                //Si el cliente es nulo, retornamos un notfound
                if (clienteTadDTOs is null)
                    return NotFound();

                foreach (var item in clienteTadDTOs)
                {
                    //Agregamos las instancias a la colección del contexto de la base y guardamos
                    clientetad = new Cliente_Tad()
                    {
                        Id_Cliente = item.CodCte,
                        Id_Terminal = item.Cod
                    };
                    context.Add(clientetad);
                    await context.SaveChangesAsync();
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
