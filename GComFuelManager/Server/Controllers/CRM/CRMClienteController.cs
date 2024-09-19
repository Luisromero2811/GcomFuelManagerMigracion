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
    public class CRMClienteController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUsuario> userManager;

        public CRMClienteController(ApplicationDbContext context, UserManager<IdentityUsuario> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] CRMCliente cliente)
        {
            try
            {
                var clientes = context.CRMClientes.AsNoTracking().Where(x => x.Activo).AsQueryable();

                if (!string.IsNullOrEmpty(cliente.Nombre))
                    clientes = clientes.Where(x => x.Nombre.Equals(cliente.Nombre));

                return Ok(clientes);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("contactos")]
        public async Task<ActionResult> GetClienteContacto([FromQuery] CRMCliente cliente)
        {
            try
            {
                var user = await userManager.FindByNameAsync(HttpContext.User.Identity?.Name ?? string.Empty);
                if (user is null) return NotFound();

                var clientes = new List<CRMCliente>().AsQueryable();

                if (await userManager.IsInRoleAsync(user, "Admin"))
                {
                    var contactos = await context.CRMContactos.AsNoTracking().Where(x => x.Activo).Select(x => x.CuentaId).ToListAsync();
                    clientes = context.CRMClientes.AsNoTracking().Where(x => x.Activo && contactos.Contains(x.Id)).AsQueryable();
                }
                else if (await userManager.IsInRoleAsync(user, "CRM_LIDER"))
                {
                    var comercial = await context.CRMOriginadores.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == user.Id);
                    if (comercial is null) return NotFound();

                    var equipos = await context.CRMEquipos.AsNoTracking().Where(x => x.Activo && x.LiderId == comercial.Id).Select(x => x.Id).ToListAsync();
                    var relacion = await context.CRMEquipoVendedores.AsNoTracking().Where(x => equipos.Contains(x.EquipoId)).Select(x => (int?)x.VendedorId).ToListAsync();
                    var contactos = await context.CRMContactos.AsNoTracking().Where(x => x.Activo && relacion.Contains(x.VendedorId)).Select(x => x.CuentaId).ToListAsync();
                    clientes = context.CRMClientes.AsNoTracking().Where(x => contactos.Contains(x.Id));
                }
                else
                {
                    var vendedor = await context.CRMVendedores.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == user.Id);
                    if (vendedor is null) return NotFound();

                    var contactos = await context.CRMContactos.AsNoTracking().Where(x => x.Activo && x.VendedorId == vendedor.Id).Select(x => x.CuentaId).ToListAsync();
                    clientes = context.CRMClientes.AsNoTracking().Where(x => contactos.Contains(x.Id));
                }

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
