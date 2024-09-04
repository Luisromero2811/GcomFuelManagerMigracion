using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Server.Controllers.CRM
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CRMClienteController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public CRMClienteController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] CRMCliente cliente)
        {
            try
            {
                var clientes = context.CRMClientes.AsQueryable();
                
                if (!string.IsNullOrEmpty(cliente.Nombre))
                    clientes = clientes.Where(x => x.Nombre.Equals(cliente.Nombre));

                if (!string.IsNullOrEmpty(cliente.Nombre))
                    clientes = clientes.Where(x => x.Nombre.Equals(cliente.Nombre));

                return Ok(clientes);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("default/data")]
        public async Task<IActionResult> SetClientesGcom()
        {
            try
            {
                var clientes = await context.Cliente.Where(x => !string.IsNullOrEmpty(x.Den) && x.Den.Length > 1)
                    .GroupBy(x => x.Den)
                    .Select(x => new CRMCliente { Nombre = (string)x.Key! })
                    .ToListAsync();
                foreach (var cliente in clientes)
                {
                    if (!context.CRMClientes.Any(x => x.Nombre == cliente.Nombre))
                    {
                        await context.AddAsync(cliente);
                        await context.SaveChangesAsync();
                    }
                }
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
