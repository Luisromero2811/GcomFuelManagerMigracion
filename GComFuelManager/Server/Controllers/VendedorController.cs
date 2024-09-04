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
using System.Globalization;
using System.Linq;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, Administrador, Reportes De Venta, Direccion, Gerencia, Ejecutivo de Cuenta Comercial")]
    public class VendedorController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUsuario> userManager;
        private readonly VerifyUserId verifyUser;
        private readonly User_Terminal _terminal;

        public VendedorController(ApplicationDbContext context, UserManager<IdentityUsuario> userManager, VerifyUserId verifyUser, User_Terminal _Terminal)
        {
            this.context = context;
            this.userManager = userManager;
            this.verifyUser = verifyUser;
            this._terminal = _Terminal;
        }


        [HttpGet]
        public ActionResult Obtener_Vendedores([FromQuery] Vendedor vendedor)
        {
            try
            {
                var vendedores = context.Vendedores.Include(x => x.Originadores).IgnoreAutoIncludes().OrderBy(x => x.Nombre).AsQueryable();

                if (!string.IsNullOrEmpty(vendedor.Nombre))
                    vendedores = vendedores.Where(x => x.Nombre.ToLower().Contains(vendedor.Nombre.ToLower())).OrderBy(x => x.Nombre);

                if (!string.IsNullOrEmpty(vendedor.Nombre_Originador) && vendedor.Originadores is not null)
                    vendedores = vendedores.Where(x => x.Originadores.Any(x => x.Nombre.ToLower().Contains(vendedor.Nombre_Originador.ToLower())));

                return Ok(vendedores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("list")]
        public async Task<ActionResult> GetListV()
        {
            try
            {
                var vendedores = context.Vendedores
                    .Where(x => x.Activo == true)
                    .ToList();
                return Ok(vendedores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("filtrar")]
        public ActionResult Obtener_Vendedores_Filtrados([FromQuery] Vendedor vendedor)
        {
            try
            {
                var vendedores = context.Vendedores.Where(x => x.Activo).OrderBy(x => x.Nombre).IgnoreAutoIncludes().AsQueryable();

                if (!string.IsNullOrEmpty(vendedor.Nombre))
                    vendedores = vendedores.Where(x => x.Nombre.ToLower().Contains(vendedor.Nombre.ToLower())).OrderBy(x => x.Nombre);

                return Ok(vendedores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("clientes")]//TODO: checar utilidad
        public ActionResult Obtener_Clientes_De_Vendedores_Filtrados([FromQuery] Cliente cliente)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var vendedores = context.Cliente_Tad.IgnoreAutoIncludes().Where(x => x.Cliente != null && x.Cliente.Activo && x.Cliente.Id_Vendedor == cliente.Id_Vendedor && x.Id_Terminal == id_terminal)
                    .OrderBy(x => x.Cliente!.Den)
                    .Include(x => x.Cliente).ThenInclude(x => x.Originador).Select(x => x.Cliente).AsQueryable();

                //var vendedores = context.Cliente.IgnoreAutoIncludes().Where(x => x.Activo && x.Id_Vendedor == cliente.Id_Vendedor).Include(x => x.Originador).OrderBy(x => x.Den).AsQueryable();

                if (!string.IsNullOrEmpty(cliente.Den) || !string.IsNullOrWhiteSpace(cliente.Den))
                    vendedores = vendedores.Where(x => x != null && (!string.IsNullOrEmpty(x.Den) || !string.IsNullOrWhiteSpace(x.Den)) && x.Den.ToLower().Contains(cliente.Den.ToLower())).OrderBy(x => x!.Den);

                return Ok(vendedores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Guardar_Vendedores([FromBody] Vendedor vendedor)
        {
            try
            {
                if (vendedor is null)
                    return NotFound();

                if (string.IsNullOrEmpty(vendedor.Nombre) || string.IsNullOrWhiteSpace(vendedor.Nombre))
                    return BadRequest("Nombre de vendedor no valido");

                var id = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                if (vendedor.Id != 0)
                {
                    //vendedor.Vendedor_Originador = null!;
                    vendedor.Originadores = null!;

                    context.Update(vendedor);
                    await context.SaveChangesAsync(id, 38);
                }
                else
                {
                    context.Add(vendedor);
                    await context.SaveChangesAsync(id, 37);

                    if (vendedor.Id_Originador != 0)
                    {
                        Vendedor_Originador vendedor_Originador = new()
                        {
                            VendedorId = vendedor.Id,
                            OriginadorId = vendedor.Id_Originador
                        };

                        context.Add(vendedor_Originador);
                        await context.SaveChangesAsync(id, 41);
                    }

                    if (!context.Metas_Vendedor.Any(x => x.VendedorId == vendedor.Id && x.Mes.Year == DateTime.Today.Year))
                    {
                        for (int i = 1; i <= 12; i++)
                        {
                            Metas_Vendedor metas_Vendedor = new()
                            {
                                VendedorId = vendedor.Id,
                                Mes = new DateTime(DateTime.Today.Year, i, 1)
                            };

                            if (!context.Metas_Vendedor.Any(x => x.Mes.Month == metas_Vendedor.Mes.Month && x.VendedorId == vendedor.Id && x.Mes.Year == metas_Vendedor.Mes.Year))
                            {
                                context.Add(metas_Vendedor);
                                await context.SaveChangesAsync();
                            }
                        }
                    }
                }

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("relacionar/cliente")]
        public async Task<ActionResult> Guardar_Relacion_Cliente_Vendedor([FromBody] List<Cliente> clientes, [FromQuery] Vendedor vendedor)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                if (clientes is null)
                    return NotFound();

                var id = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                foreach (var cliente in clientes)
                {
                    if (context.Cliente_Tad.Any(x => x.Id_Cliente == cliente.Cod && x.Id_Terminal == id_terminal))
                    {
                        var cliente_buscado = context.Cliente.FirstOrDefault(x => x.Cod == cliente.Cod);
                        if (cliente_buscado is not null)
                        {
                            cliente_buscado.Id_Vendedor = vendedor.Id;
                            cliente_buscado.Id_Originador = vendedor.Id_Originador;
                            context.Update(cliente_buscado);
                            await context.SaveChangesAsync(id, 38);
                        }
                    }
                    else
                        return BadRequest($"El cliente {cliente.Den} no se encuentra en esta terminal");
                }
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("borrar/relacion/cliente")]
        public async Task<ActionResult> Guardar_Relacion_Vendeor_Originador([FromQuery] Cliente cliente)
        {
            try
            {
                if (cliente is null)
                    return NotFound();

                var id = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                var cliente_encontrado = context.Cliente.Find(cliente.Cod);

                if (cliente_encontrado is not null)
                {
                    if (cliente_encontrado.Id_Vendedor == cliente.Id_Vendedor)
                    {
                        cliente_encontrado.Id_Vendedor = 0;
                        cliente_encontrado.Id_Originador = 0;
                        context.Update(cliente_encontrado);
                        await context.SaveChangesAsync(id, 42);
                        return Ok();
                    }
                }

                return NotFound();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("relacionar/originador")]
        public async Task<ActionResult> Guardar_Relacion_Vendeor_Originador([FromQuery] Vendedor_Originador vendedor_Originador)
        {
            try
            {
                if (vendedor_Originador is null)
                    return NotFound();

                if (vendedor_Originador.OriginadorId == 0)
                    return BadRequest("Originador no valido");

                if (vendedor_Originador.VendedorId == 0)
                    return BadRequest("Vendedor no valido");

                var id = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                if (vendedor_Originador.Borrar)
                    context.Remove(vendedor_Originador);
                else
                    context.Add(vendedor_Originador);


                await context.SaveChangesAsync(id, 41);

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

    }
}
