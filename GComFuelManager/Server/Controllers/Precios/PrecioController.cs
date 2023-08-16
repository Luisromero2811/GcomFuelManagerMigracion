using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GComFuelManager.Server.Controllers.Precios
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PrecioController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUsuario> userManager;
        private readonly VerifyUserToken verifyUser;

        public PrecioController(ApplicationDbContext context, UserManager<IdentityUsuario> userManager, VerifyUserToken verifyUser)
        {
            this.context = context;
            this.userManager = userManager;
            this.verifyUser = verifyUser;
        }

        [HttpGet]
        public async Task<ActionResult> GetPrecios()
        {
            try
            {
                var precios = await context.Precio
                    .Include(x => x.Zona)
                    .Include(x => x.Cliente)
                    .Include(x => x.Producto)
                    .Include(x => x.Destino)
                    .ToListAsync();

                return Ok(precios);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("productos/{folio?}")]
        public async Task<ActionResult> GetPrecios([FromBody] ZonaCliente? zonaCliente, [FromRoute] string? folio)
        {
            try
            {
                List<Precio> precios = new List<Precio>();

                var userId = verifyUser.GetName(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return BadRequest();

                var user = await userManager.FindByNameAsync(userId);

                if (user == null)
                    return BadRequest();

                var role = await userManager.IsInRoleAsync(user, "Comprador");

                if (role == true)
                {
                    var usuario = context.Usuario.FirstOrDefault(x => x.Usu == user.UserName);
                    if (usuario == null)
                        return BadRequest();

                    if (string.IsNullOrEmpty(folio))
                    {
                        precios = await context.Precio.Where(x => x.codDes == zonaCliente.DesCod && x.Activo == true)
                        //&& x.codZona == zona.ZonaCod)
                        .Include(x => x.Producto)
                        .ToListAsync();

                        precios.ForEach(x =>
                        {
                            if (x.FchDia != DateTime.Today || context.Cliente.FirstOrDefault(x => x.Cod == zonaCliente.CteCod)?.precioSemanal is true)
                            {
                                var porcentaje = context.Porcentaje.FirstOrDefault(x => x.Accion == "cliente");
                                var aumento = (porcentaje.Porcen / 100) + 1;
                                x.Pre = x.FchDia != DateTime.Today ? (x.Pre * aumento) : x.Pre;
                            }
                        });
                    }
                    else
                    {
                        var ordenes = await context.OrdenCierre.Where(x => x.Folio == folio)
                                .Include(x => x.Cliente)
                                .ToListAsync();
                        var ordenesUnic = ordenes.DistinctBy(x => x.CodPrd).Select(x => x);

                        foreach (var item in ordenesUnic)
                        {
                            var zona = context.ZonaCliente.FirstOrDefault(x => x.CteCod == item.CodCte);
                            Precio precio = new Precio()
                            {
                                Pre = item.Precio,
                                codCte = item.CodCte,
                                codDes = item.CodDes,
                                codPrd = item.CodPrd,
                                codGru = item.Cliente?.codgru,
                                codZona = zona?.CteCod,
                                Producto = context.Producto.FirstOrDefault(x => x.Cod == item.CodPrd)
                            };
                            precios.Add(precio);
                        }

                        if (context.Cliente.FirstOrDefault(x => x.Cod == zonaCliente.CteCod)?.precioSemanal is true)
                        {
                            precios.ForEach(x =>
                            {
                                var porcentaje = context.Porcentaje.FirstOrDefault(x => x.Accion == "cliente");
                                var aumento = (porcentaje.Porcen / 100) + 1;
                                x.Pre = x.FchDia != DateTime.Today ? (x.Pre * aumento) : x.Pre;
                            });
                        }
                    }
                }
                else
                {
                    //var zona = context.ZonaCliente.FirstOrDefault(x => x.CteCod == zonaCliente.CteCod && x.DesCod == zonaCliente.DesCod);

                    //if (zona == null)
                    //    return BadRequest("No tiene una zona relacionada");

                    if (string.IsNullOrEmpty(folio))
                    {
                        precios = await context.Precio.Where(x => x.codCte == zonaCliente.CteCod
                        && x.codDes == zonaCliente.DesCod && x.Activo == true)
                        //&& x.codZona == zona.ZonaCod)
                        .Include(x => x.Producto)
                        .ToListAsync();

                        precios.ForEach(x =>
                        {
                            if (x.FchDia != DateTime.Today || context.Cliente.FirstOrDefault(x => x.Cod == zonaCliente.CteCod)?.precioSemanal is true)
                            {
                                var porcentaje = context.Porcentaje.FirstOrDefault(x => x.Accion == "cliente");
                                var aumento = (porcentaje.Porcen / 100) + 1;
                                x.Pre = x.FchDia != DateTime.Today ? (x.Pre * aumento) : x.Pre;
                            }
                        });
                    }
                    else
                    {
                        var ordenes = await context.OrdenCierre.Where(x => x.Folio == folio)
                            .Include(x => x.Cliente)
                            .ToListAsync();
                        var ordenesUnic = ordenes.DistinctBy(x => x.CodPrd).Select(x => x);

                        foreach (var item in ordenesUnic)
                        {
                            var zona = context.ZonaCliente.FirstOrDefault(x => x.CteCod == item.CodCte);
                            Precio precio = new Precio()
                            {
                                Pre = item.Precio,
                                codCte = item.CodCte,
                                codDes = item.CodDes,
                                codPrd = item.CodPrd,
                                codGru = item.Cliente?.codgru,
                                codZona = zona?.CteCod,
                                Producto = context.Producto.FirstOrDefault(x => x.Cod == item.CodPrd)
                            };
                            precios.Add(precio);
                        }
                    }
                }

                return Ok(precios);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


    }
}