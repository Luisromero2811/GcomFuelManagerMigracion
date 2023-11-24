
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

        [HttpGet("{Orden_Compra}")]
        public ActionResult GetPrecioByBol([FromRoute] int Orden_Compra)
        {
            try
            {
                List<PrecioBol> precios = new List<PrecioBol>();

                var ordenes = context.OrdenEmbarque.Where(x => x.Folio == Orden_Compra)
                    .Include(x => x.Producto)
                    .Include(x => x.Destino)
                    .ToList();

                if (ordenes is null)
                    return BadRequest($"No se encontro la orden con el numero de compra: {Orden_Compra}");

                foreach (var item in ordenes)
                {
                    PrecioBol precio = new PrecioBol();

                    Orden? orden = new Orden();
                    orden = context.Orden.Where(x => x.Ref == item.FolioSyn).Include(x => x.Producto).Include(x => x.Destino).FirstOrDefault();

                    precio.Fecha_De_Carga = orden?.Fchcar ?? item.Fchcar;

                    precio.Referencia = orden?.Ref ?? item.FolioSyn;

                    if (orden is not null)
                    {
                        if (orden.Producto is not null)
                            precio.Producto_Synthesis = orden.Producto.Den;

                        if (orden.Destino is not null)
                            precio.Destino_Synthesis = orden.Destino.Den;

                        precio.BOL = orden.BatchId ?? 0;
                    }

                    if (item is not null)
                    {
                        if (item.Destino is not null)
                            precio.Destino_Original = item.Destino.Den ?? "";

                        if (item.Producto is not null)
                            precio.Producto_Original = item.Producto.Den ?? "";
                    }

                    var precioVig = context.Precio.Where(x => item != null && x.codDes == item.Coddes && x.codPrd == item.Codprd).FirstOrDefault();

                    if (orden is not null)
                        precioVig = context.Precio.Where(x => x.codDes == orden.Coddes && x.codPrd == orden.Codprd).FirstOrDefault();

                    var precioPro = context.PrecioProgramado.Where(x => item != null && x.codDes == item.Coddes && x.codPrd == item.Codprd).FirstOrDefault();

                    if (orden is not null)
                        precioPro = context.PrecioProgramado.Where(x => x.codDes == orden.Coddes && x.codPrd == orden.Codprd).FirstOrDefault();

                    var precioHis = context.PreciosHistorico.Where(x => item != null && x.codDes == item.Coddes && x.codPrd == item.Codprd
                        && item.Fchcar != null && x.FchDia <= item.Fchcar.Value.Date)
                        .OrderByDescending(x => x.FchDia)
                        .FirstOrDefault();

                    if (orden is not null)
                        precioHis = context.PreciosHistorico.Where(x => x.codDes == orden.Coddes && x.codPrd == orden.Codprd
                        && orden.Fchcar != null && x.FchDia <= orden.Fchcar.Value.Date)
                        .OrderByDescending(x => x.FchDia)
                        .FirstOrDefault();

                    if (precioHis is not null)
                    {
                        precio.Precio = precioHis.pre;
                        precio.Fecha_De_Precio = precioHis.FchDia;
                        precio.Precio_Encontrado = true;
                        precio.Precio_Encontrado_En = "Historial";
                    }

                    if (item != null && precioVig is not null && item.Fchcar is not null && item.Fchcar.Value.Date == DateTime.Today ||
                        orden is not null && orden.Fchcar is not null && orden.Fchcar.Value.Date == DateTime.Today && precioVig is not null)
                    {
                        precio.Precio = precioVig.Pre;
                        precio.Fecha_De_Precio = precioVig.FchDia;
                        precio.Precio_Encontrado = true;
                        precio.Precio_Encontrado_En = "Vigente";
                    }

                    if (item != null && precioPro is not null && item.Fchcar is not null && item.Fchcar.Value.Date == DateTime.Today && DateTime.Now.TimeOfDay >= new TimeSpan(16, 0, 0)
                        && context.PrecioProgramado.Any() ||
                        orden is not null && precioPro is not null && orden.Fchcar is not null && orden.Fchcar.Value.Date == DateTime.Today && DateTime.Now.TimeOfDay >= new TimeSpan(16, 0, 0)
                        && context.PrecioProgramado.Any())
                    {
                        precio.Precio = precioPro.Pre;
                        precio.Fecha_De_Precio = precioPro.FchDia;
                        precio.Precio_Encontrado = true;
                        precio.Precio_Encontrado_En = "Programado";
                    }

                    if (item != null && context.OrdenPedido.Any(x => x.CodPed == item.Cod))
                    {
                        var ordenepedido = context.OrdenPedido.Where(x => x.CodPed == item.Cod && !string.IsNullOrEmpty(x.Folio)).FirstOrDefault();

                        if (ordenepedido is not null)
                        {
                            var cierre = context.OrdenCierre.Where(x => x.Folio == ordenepedido.Folio
                             && x.CodPrd == item.Codprd).FirstOrDefault();

                            if (cierre is not null)
                            {
                                precio.Precio = cierre.Precio;
                                precio.Fecha_De_Precio = cierre.fchPrecio;
                                precio.Es_Cierre = true;
                                precio.Precio_Encontrado = true;
                                precio.Precio_Encontrado_En = "Cierre";
                            }
                        }
                    }

                    if (item is not null && precioHis is null && precioPro is null && precioVig is null && !precio.Es_Cierre)
                    {
                        precio.Precio = item.Pre;

                        if (item.OrdenCierre is not null)
                            precio.Fecha_De_Precio = item.OrdenCierre.fchPrecio;

                        precio.Es_Precio_De_Creacion = true;
                        precio.Precio_Encontrado_En = "Creacion";
                    }

                    precios.Add(precio);
                }

                return Ok(precios);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        public class PrecioBol
        {
            public double? Precio { get; set; } = 0;
            public string? Referencia { get; set; } = string.Empty;
            public int? BOL { get; set; } = 0;
            public DateTime? Fecha_De_Carga { get; set; } = DateTime.MinValue;
            public DateTime? Fecha_De_Precio { get; set; } = DateTime.MinValue;
            public string? Destino_Synthesis { get; set; } = string.Empty;
            public string? Destino_Original { get; set; } = string.Empty;
            public string? Producto_Synthesis { get; set; } = string.Empty;
            public string? Producto_Original { get; set; } = string.Empty;
            public bool Es_Cierre { get; set; } = false;
            public bool Es_Precio_De_Creacion { get; set; } = false;
            public bool Precio_Encontrado { get; set; } = false;
            public string Precio_Encontrado_En { get; set; } = string.Empty;
        }
    }
}