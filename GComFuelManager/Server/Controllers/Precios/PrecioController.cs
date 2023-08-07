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

        [HttpPost]
        [Route("upload")]
        public async Task<ActionResult> Convert(IFormFile file)
        {
            try
            {
                if (file == null)
                    return BadRequest("No se pudo leer el archivo enviado.");

                using var stream = new MemoryStream();
                file.CopyTo(stream);

                List<PreciosDTO> precios = new List<PreciosDTO>();

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                ExcelPackage package = new ExcelPackage();

                package.Load(stream);

                if (package.Workbook.Worksheets.Count > 0)
                {
                    using (ExcelWorksheet worksheet = package.Workbook.Worksheets.First())
                    {
                        for (int r = 2; r < (worksheet.Dimension.End.Row + 1); r++)
                        {
                            PreciosDTO precio = new PreciosDTO();

                            var row = worksheet.Cells[r, 1, r, worksheet.Dimension.End.Column].ToList();

                            if (row.Count == 6)
                            {
                                precio.Producto = row[0].Value.ToString();
                                precio.Zona = row[1].Value.ToString();
                                precio.Cliente = row[2].Value.ToString();
                                precio.Destino = row[3].Value.ToString();
                                precio.Fecha = row[4].Value.ToString();
                                precio.Precio = double.Parse(row[5].Value.ToString());
                                precios.Add(precio);
                            }
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

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Precio precio)
        {
            try
            {
                var acc = 0;
                precio.Producto = null!;
                precio.Zona = null!;
                precio.Destino = null!;
                precio.Cliente = null!;
                precio.FchActualizacion = DateTime.Now;

                if (precio.Cod != null)
                {
                    context.Update(precio);
                    acc = 6;
                }
                else
                {
                    if (context.Precio.Any(x => x.codDes == precio.codDes && x.codCte == precio.codCte && x.codPrd == precio.codPrd))
                        return BadRequest("El destino ya cuenta con un precio asignado para ese producto.");

                    context.Add(precio);
                    acc = 3;
                }
                var precioH = new PrecioHistorico
                {
                    Cod = null!,
                    pre = precio.Pre,
                    codCte = precio.codCte,
                    codDes = precio.codDes,
                    codGru = (short)precio.codGru!,
                    codPrd = precio.codPrd,
                    codZona = precio.codZona,
                    FchDia = precio.FchDia,
                    FchActualizacion = precio.FchActualizacion
                };

                context.Add(precioH);
                var id = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();
                await context.SaveChangesAsync(id,acc);

                return Ok();
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

                    //var zona = context.ZonaCliente.FirstOrDefault(x => x.CteCod == usuario.CodCte && x.DesCod == zonaCliente.DesCod);

                    //if (zona == null)
                    //    return BadRequest("No existe una relacion de precios con la zona y destino");

                    precios = await context.Precio.Where(x => x.codCte == usuario.CodCte
                    && x.codDes == zonaCliente.DesCod && x.Activo == true)
                    //&& x.codZona == zona.ZonaCod)
                        .Include(x => x.Producto)
                        .ToListAsync();

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

        [HttpPost("confirm/upload")]
        public async Task<ActionResult> PostPrecioDTO([FromBody] List<PreciosDTO> precios)
        {
            try
            {
                List<PrecioProgramado> prec = new List<PrecioProgramado>();
                foreach (var item in precios)
                {
                    var cliente = context.Cliente.FirstOrDefault(x => x.Den!.Equals(item.Cliente));
                    if (cliente is null)
                        return BadRequest($"No se encontro el cliente {item.Cliente}");

                    var producto = context.Producto.FirstOrDefault(x => x.Den!.Equals(item.Producto));
                    if (producto is null)
                        return BadRequest($"No se encontro el producto {item.Producto}");

                    var zona = context.Zona.FirstOrDefault(x => x.Nombre.Equals(item.Zona));
                    if (zona is null)
                        return BadRequest($"No se encontro la zona {item.Zona}");

                    var destino = context.Destino.FirstOrDefault(x => x.Den!.Equals(item.Destino!));
                    if (destino is null)
                        return BadRequest($"No se encontro el destino {item.Destino}");
                    
                    if (DateTime.Parse(item.Fecha) > DateTime.Today)
                    {
                        var precio = new PrecioProgramado
                        {
                            codCte = cliente.Cod,
                            codDes = destino.Cod,
                            codGru = cliente.codgru,
                            codPrd = producto.Cod,
                            codZona = zona.Cod,
                            FchDia = DateTime.Parse(item.Fecha),
                            FchActualizacion = DateTime.Now,
                            Pre = item.Precio
                        };

                        prec.Add(precio);
                    }
                    else if(DateTime.Parse(item.Fecha) == DateTime.Today)
                    {
                        var precio = new Precio
                        {
                            codCte = cliente.Cod,
                            codDes = destino.Cod,
                            codGru = cliente.codgru,
                            codPrd = producto.Cod,
                            codZona = zona.Cod,
                            FchDia = DateTime.Parse(item.Fecha),
                            FchActualizacion = DateTime.Now,
                            Pre = item.Precio
                        };

                        var p = context.Precio.FirstOrDefault(x => x.codGru == precio.codGru
                        && x.codZona == precio.codZona
                        && x.codCte == precio.codCte
                        && x.codPrd == precio.codPrd
                        && x.codDes == precio.codDes);

                        if (p is not null)
                        {
                            p.Pre = precio.Pre;
                            p.FchDia = precio.FchDia;
                            p.FchActualizacion = DateTime.Now;

                            context.Update(p);
                        }
                        else
                            context.Add(precio);

                        var precioH = new PrecioHistorico
                        {
                            Cod = null!,
                            pre = precio.Pre,
                            codCte = precio.codCte,
                            codDes = precio.codDes,
                            codGru = (short)precio.codGru!,
                            codPrd = precio.codPrd,
                            codZona = precio.codZona,
                            FchDia = precio.FchDia,
                            FchActualizacion = precio.FchActualizacion
                        };

                        context.Add(precioH);
                    }
                }

                if(prec.Count > 0)
                {
                    context.PrecioProgramado.ExecuteDelete();
                    context.AddRange(prec);
                }

                var id = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                await context.SaveChangesAsync(id,8);

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpPost("historial")]
        public async Task<ActionResult> GetDateHistorialPrecio([FromBody] FechasF fechas)
        {
            try
            {
                List<PrecioHistorico> precios = new List<PrecioHistorico>();

                precios = await context.PreciosHistorico
                    .Where(x => x.FchDia >= fechas.DateInicio && x.FchDia <= fechas.DateFin)
                    .Include(x => x.Destino)
                    .Include(x => x.Cliente)
                    .Include(x => x.Producto)
                    .Include(x => x.Zona)
                    .OrderBy(x => x.FchDia)
                    .Take(1000)
                    .ToListAsync();
                return Ok(precios);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("programado")]
        public async Task<ActionResult> GetDateHistorialPrecioProgramado()
        {
            try
            {
                var fchInicio = DateTime.Today.AddHours(1).AddMinutes(50);
                var fchHoy = DateTime.Now;
                var fchFin = DateTime.Today.AddHours(2).AddMinutes(20);
                if (fchInicio <= fchHoy && fchFin >= fchHoy)
                {
                    
                }

                List<PrecioProgramado> precios = new List<PrecioProgramado>();
                precios = context.PrecioProgramado.Where(x => x.FchDia == DateTime.Today).ToList();

                if (precios.Count > 0)
                {
                    foreach (var item in precios)
                    {
                        var precio = context.Precio.FirstOrDefault(x => x.codCte == item.codCte && x.codDes == item.codDes && x.codPrd == item.codPrd && x.Activo == true);
                        if (precio is null)
                        {
                            var precioN = new Precio
                            {
                                codCte = item.Cod,
                                codDes = item.Cod,
                                codGru = item.codGru,
                                codPrd = item.codPrd,
                                codZona = item.Cod,
                                FchDia = item.FchDia,
                                FchActualizacion = DateTime.Now,
                                Pre = item.Pre
                            };
                            context.Add(precioN);
                        }
                        else
                        {
                            precio.Pre = item.Pre;
                            precio.FchDia = item.FchDia;
                            precio.FchActualizacion = DateTime.Now;
                            context.Update(precio);
                        }

                        var precioH = new PrecioHistorico
                        {
                            Cod = null!,
                            pre = item.Pre,
                            codCte = item.codCte,
                            codDes = item.codDes,
                            codGru = (short)item.codGru!,
                            codPrd = item.codPrd,
                            codZona = item.codZona,
                            FchDia = item.FchDia,
                            FchActualizacion = item.FchActualizacion
                        };

                        context.Add(precioH);
                    }

                    await context.SaveChangesAsync();
                }
                return Ok(precios);

                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("programados/lista")]
        public async Task<ActionResult> GetPreciosProgramados()
        {
            try
            {
                var precios = await context.PrecioProgramado
                    .Where(x=>x.FchDia >= DateTime.Today)
                    .Include(x=>x.Zona)
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

        [HttpPost("programado")]
        public async Task<ActionResult> PostProgramado([FromBody] Precio precio)
        {
            try
            {
                precio.Producto = null!;
                precio.Zona = null!;
                precio.Destino = null!;
                precio.Cliente = null!;
                precio.FchActualizacion = DateTime.Now;

                var precioPro = new PrecioProgramado
                {
                    Cod = precio.Cod,
                    codCte = precio.codCte,
                    codDes = precio.codDes,
                    codGru = precio.codGru,
                    codPrd = precio.codPrd,
                    codZona = precio.Cod,
                    FchDia = DateTime.Parse(precio.Fecha),
                    FchActualizacion = DateTime.Now,
                    Pre = precio.Pre
                };

                if (precio.Cod != null)
                    context.Update(precioPro);
                else
                {
                    if (context.Precio.Any(x => x.codDes == precio.codDes && x.codCte == precio.codCte && x.codPrd == precio.codPrd && x.FchDia == precio.FchDia))
                        return BadRequest("El destino ya cuenta con un precio asignado para ese producto.");

                    context.Add(precioPro);
                }

                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}