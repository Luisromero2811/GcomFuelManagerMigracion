﻿using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductServiceProd;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProductoController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUsuario> userManager;
        private readonly VerifyUserToken verifyUser;
        private readonly User_Terminal _terminal;

        public ProductoController(ApplicationDbContext context, UserManager<IdentityUsuario> userManager, VerifyUserToken verifyUser, User_Terminal _Terminal)
        {
            this.context = context;
            this.userManager = userManager;
            this.verifyUser = verifyUser;
            this._terminal = _Terminal;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var productos = await context.Producto.Where(x => x.Activo == true && x.Id_Tad == id_terminal).Include(x => x.TipoProducto).OrderBy(x => x.Den).ToListAsync();
                return Ok(productos);
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
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var productos = await context.Producto.Where(x => x.Id_Tad == id_terminal).Include(x => x.TipoProducto).OrderBy(x => x.Den).ToListAsync();
                return Ok(productos);
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Producto producto)
        {
            try
            {
                var id = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                if (producto is null) { return BadRequest(); }

                if (string.IsNullOrEmpty(producto.Den) || string.IsNullOrWhiteSpace(producto.Den)) { return BadRequest("Nombre no valido"); }

                if (producto.Id_Tipo == 0) { return BadRequest("Tipo no valido"); }

                producto.Id_Tad = id_terminal;

                if (producto.Cod != 0)
                {
                    producto.Terminal = null!;
                    producto.TipoProducto = null!;

                    context.Update(producto);
                    await context.SaveChangesAsync(id, 48);
                }
                else
                {
                    if (string.IsNullOrEmpty(producto.Codsyn) || string.IsNullOrWhiteSpace(producto.Codsyn))
                        producto.Codsyn = GetRandomCarid();

                    context.Add(producto);
                    await context.SaveChangesAsync(id, 47);
                }

                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("tipos")]
        public ActionResult Obtener_Tipos()
        {
            try
            {
                var tipos = context.TipoProducto.ToList();

                return Ok(tipos);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        private string GetRandomCarid()
        {
            var random = new Random().Next(1, 999999).ToString();

            if (context.Producto.Any(x => !string.IsNullOrEmpty(x.Codsyn) && x.Codsyn.Equals(random)))
                GetRandomCarid();

            return random;
        }

        [Route("service")]
        [HttpGet]
        public async Task<ActionResult> GetClienteService()
        {
            try
            {
                if (HttpContext.User.Identity is null)
                    return NotFound();

                if (string.IsNullOrEmpty(HttpContext.User.Identity.Name))
                    return NotFound();

                var user = await userManager.FindByNameAsync(HttpContext.User.Identity.Name);
                if (user is null)
                    return NotFound();
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0 && !await userManager.IsInRoleAsync(user, "Obtencion de Ordenes"))
                    return BadRequest();

                if (id_terminal != 1 && !await userManager.IsInRoleAsync(user, "Obtencion de Ordenes"))
                    return BadRequest("No esta permitida esta accion en esta terminal");

                ProductServiceClient client = new ProductServiceClient(ProductServiceClient.EndpointConfiguration.BasicHttpBinding_ProductService2);
                client.ClientCredentials.UserName.UserName = "energasws";
                client.ClientCredentials.UserName.Password = "Energas23!";
                client.Endpoint.Binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
                client.Endpoint.Binding.SendTimeout = TimeSpan.FromMinutes(5);

                try
                {

                    var svc = client.ChannelFactory.CreateChannel();

                    WsGetProductsRequest getReq = new();

                    getReq.IncludeChildObjects = new NBool();
                    getReq.IncludeChildObjects.Value = false;

                    getReq.ActiveIndicator = new NEnumOfActiveIndicatorEnum();
                    getReq.ActiveIndicator.Value = ActiveIndicatorEnum.ACTIVE;

                    var respuesta = await svc.GetProductsAsync(getReq);

                    foreach (var item in respuesta.Products)
                    {
                    }
                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }
                finally
                {
                    client.Close();
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
