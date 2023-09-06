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

        //[HttpGet]
        //public async Task<ActionResult> GetPrecios()
        //{
        //    try
        //    {
        //        var precios = await context.Precio
        //            .Include(x => x.Zona)
        //            .Include(x => x.Cliente)
        //            .Include(x => x.Producto)
        //            .Include(x => x.Destino)
        //            .ToListAsync();

        //        return Ok(precios);
        //    }
        //    catch (Exception e)
        //    {
        //        return BadRequest(e.Message);
        //    }
        //}

        [HttpPost("productos/{folio?}")]
        public async Task<ActionResult> GetPrecios([FromBody] ZonaCliente? zonaCliente, [FromRoute] string? folio)
        {
            try
            {
                List<Precio> precios = new List<Precio>();
                List<PrecioProgramado> preciosPro = new List<PrecioProgramado>();
                var LimiteDate = DateTime.Today.AddHours(16);

                var user = await userManager.FindByNameAsync(HttpContext.User.FindFirstValue(ClaimTypes.Name)!);
                if (user == null)
                    return NotFound();

                var userSis = context.Usuario.FirstOrDefault(x => x.Usu == user.UserName);
                if (userSis == null)
                    return NotFound();

                if (!string.IsNullOrEmpty(folio))
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
                    return Ok(precios);
                }

                precios = await context.Precio.Where(x => x.codCte == userSis.CodCte && x.codDes == zonaCliente.DesCod && x.Activo == true)
                    //&& x.codZona == zona.ZonaCod)
                    .Include(x => x.Producto)
                    .ToListAsync();

                precios.ForEach(x =>
                {
                    if (x.FchDia < DateTime.Today
                    && DateTime.Today.DayOfWeek != DayOfWeek.Saturday
                    && DateTime.Today.DayOfWeek != DayOfWeek.Sunday
                    && DateTime.Today.DayOfWeek != DayOfWeek.Monday)
                    {
                        var porcentaje = context.Porcentaje.FirstOrDefault(x => x.Accion == "cliente");
                        var aumento = (porcentaje.Porcen / 100) + 1;
                        x.Pre = x.FchDia < DateTime.Today ? Math.Round((x.Pre * aumento), 4) : Math.Round(x.Pre, 4);
                    }
                });

                if (DateTime.Now > LimiteDate &&
                    DateTime.Today.DayOfWeek != DayOfWeek.Saturday &&
                    DateTime.Today.DayOfWeek != DayOfWeek.Sunday)
                {
                    preciosPro = await context.PrecioProgramado.Where(x => x.codCte == userSis.CodCte
                    && x.codDes == zonaCliente.DesCod && x.Activo == true)
                    //&& x.codZona == zona.ZonaCod)
                    .Include(x => x.Producto)
                    .ToListAsync();

                    foreach (var item in preciosPro)
                    {
                        precios.FirstOrDefault(x => x.codDes == item.codDes && x.codCte == item.codCte && x.codPrd == item.codPrd && x.FchDia < item.FchDia).Pre = item.Pre;
                        precios.FirstOrDefault(x => x.codDes == item.codDes && x.codCte == item.codCte && x.codPrd == item.codPrd && x.FchDia < item.FchDia).FchDia = item.FchDia;
                        var pre = precios.FirstOrDefault(x => x.codDes == item.codDes && x.codCte == item.codCte && x.codPrd == item.codPrd);
                        if (pre is null)
                        {
                            precios.Add(new Precio()
                            {
                                Pre = item.Pre,
                                codCte = item.codCte,
                                codDes = item.codDes,
                                codPrd = item.codPrd,
                                codGru = item.Cliente?.codgru,
                                Producto = context.Producto.FirstOrDefault(x => x.Cod == item.codPrd)
                            });
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