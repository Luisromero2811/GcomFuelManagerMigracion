using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
using System.ServiceModel;
//using ServiceReference6;
using ServiceReference8;
using System.Drawing;
using System;
using Microsoft.AspNetCore.Identity;
using GComFuelManager.Server.Identity;
using System.Security.Claims;

namespace GComFuelManager.Server.Controllers.Cierres
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ClientesController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUsuario> userManager;

        public ClientesController(ApplicationDbContext context, UserManager<IdentityUsuario> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            try
            {
                var clientes = context.Cliente.AsEnumerable().Select(x => new CodDenDTO { Cod = x.Cod, Den = x.Den! }).OrderBy(x => x.Den);
                return Ok(clientes);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("one")]
        public async Task<ActionResult> GetByCod()
        {
            try
            {
                Cliente clientes = new Cliente();
                
                var usuario = await userManager.FindByNameAsync(HttpContext.User.FindFirstValue(ClaimTypes.Name)!);
                //Si el usuario no existe
                if (usuario == null)
                    return NotFound();

                var isClient = await userManager.IsInRoleAsync(usuario, "Comprador");
                if (isClient)
                {
                    var user = context.Usuario.Find(usuario.UserCod);
                    
                    if (user is null)
                        return BadRequest("No existe el usuario.");
                    if (!user.IsClient)
                        return BadRequest("No es cliente.");

                    clientes = context.Cliente.Find(user!.CodCte!);
                }
                
                return Ok(clientes);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("all")]
        public async Task<ActionResult> GetAll()
        {
            try
            {
                var clientes = context.Cliente.AsEnumerable().OrderBy(x => x.Den);
                return Ok(clientes);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("grupo/{cod:int}")]
        public async Task<ActionResult> Get(int cod)
        {
            try
            {
                var clientes = context.Cliente.Where(x => x.codgru == cod).AsEnumerable().OrderBy(x => x.Den);
                return Ok(clientes);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("asignar/{cod:int}")]
        public async Task<ActionResult> PostAsignar([FromBody] CodDenDTO codden, [FromRoute] int cod)
        {
            try
            {
                var destino = await context.Destino.FirstOrDefaultAsync(x => x.Cod == codden.Cod);

                if (destino == null)
                {
                    return NotFound();
                }

                destino.Codcte = cod;
                context.Update(destino);
                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut]
        public async Task<ActionResult> PutCliente([FromBody] Cliente cliente)
        {
            try
            {
                if (cliente == null)
                {
                    return BadRequest();
                }

                //cliente.Grupo = null;

                context.Update(cliente);
                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("folio/{cod:int}")]
        public async Task<ActionResult> GetFolio([FromRoute] int cod)
        {
            try
            {
                var cliente = await context.Cliente.FindAsync(cod);
                if (cliente == null)
                    return NotFound();

                cliente.Consecutivo = cliente.Consecutivo != null ? cliente.Consecutivo + 1 : 1;

                var folio = cliente.CodCte != null ? cliente.CodCte + Convert.ToString(cliente.Consecutivo) : string.Empty;

                //cliente.Grupo = null!;

                context.Update(cliente);
                await context.SaveChangesAsync();

                return Ok(folio);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

    }
}
