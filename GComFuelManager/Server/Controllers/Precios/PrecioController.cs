using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Server.Migrations;
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
                await file.CopyToAsync(stream);
                //file.OpenReadStream();
                file.CopyTo(stream);

                List<PreciosDTO> precios = new List<PreciosDTO>();

                //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage.LicenseContext = LicenseContext.Commercial;

                ExcelPackage package = new ExcelPackage();

                package.Load(stream);
                //package = new ExcelPackage(stream);
                if (package.Workbook.Worksheets.Count > 0)
                {
                    using (ExcelWorksheet worksheet = package.Workbook.Worksheets.First())
                    {
                        //for (int r = 2; r < (worksheet.Dimension.End.Row + 1); r++)
                        for (int r = 2; r < (worksheet.Dimension.End.Row + 1); r++)
                        {
                            PreciosDTO precio = new PreciosDTO();

                            //var row = worksheet.Cells[r, 1, r, worksheet.Dimension.End.Column].ToList();
                            var row = worksheet.Cells[r, 1, r, 10].ToList();

                            if (row.Count == 10)
                            {
                                if (row[8].Value is not null)
                                    if (!context.Moneda.Any(x => x.Nombre == row[8].Value.ToString()))
                                        return BadRequest($"No existe la moneda ingresada. Moneda: {row[8].Value?.ToString()}");

                                if (row[8].Value is null)
                                    row[8].Value = "MXN";

                                precio.Producto = row[0].Value?.ToString();
                                precio.Zona = row[1].Value?.ToString();
                                precio.Cliente = row[2].Value?.ToString();
                                precio.Destino = row[3].Value?.ToString();
                                precio.CodSyn = row[4].Value?.ToString();
                                precio.CodTux = row[5].Value?.ToString();
                                precio.Fecha = row[6].Value?.ToString();
                                precio.Precio = Math.Round((double)row[7].Value, 4);
                                precio.Moneda = row[8].Value?.ToString();
                                precio.Equibalencia = Math.Round((double)row[9].Value, 4);
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


        [HttpPost]
        [Route("uploads")]
        public async Task<ActionResult> ConvertExcell(IFormFile file)
        {
            try
            {
                if (file == null)
                    return BadRequest("No se pudo leer el archivo enviado.");
                using var streams = new MemoryStream();
                await file.CopyToAsync(streams);
                //file.OpenReadStream();
                file.CopyTo(streams);
                using (var stream = file.OpenReadStream())
                {
                    using (var packages = new OfficeOpenXml.ExcelPackage(stream))
                    {
                        List<PreciosDTO> precios = new List<PreciosDTO>();

                        //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        ExcelPackage.LicenseContext = LicenseContext.Commercial;

                        ExcelPackage package = new ExcelPackage();
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet != null)
                        {
                            using (ExcelWorksheet worksheets = package.Workbook.Worksheets.First())
                            {
                                for (int r = 2; r < (worksheet.Dimension.End.Row + 1); r++)
                                {
                                    PreciosDTO precio = new PreciosDTO();

                                    //var row = worksheet.Cells[r, 1, r, worksheet.Dimension.End.Column].ToList();
                                    var row = worksheet.Cells[r, 1, r, 10].ToList();

                                    if (row.Count == 10)
                                    {
                                        if (row[8].Value is not null)
                                            if (!context.Moneda.Any(x => x.Nombre == row[8].Value.ToString()))
                                                return BadRequest($"No existe la moneda ingresada. Moneda: {row[8].Value?.ToString()}");

                                        if (string.IsNullOrEmpty(row[8].Value.ToString()))
                                            row[8].Value = "MXN";

                                        precio.Producto = row[0].Value?.ToString();
                                        precio.Zona = row[1].Value?.ToString();
                                        precio.Cliente = row[2].Value?.ToString();
                                        precio.Destino = row[3].Value?.ToString();
                                        precio.CodSyn = row[4].Value?.ToString();
                                        precio.CodTux = row[5].Value?.ToString();
                                        precio.Fecha = row[6].Value?.ToString();
                                        precio.Precio = Math.Round((double)row[7].Value, 4);
                                        precio.Moneda = row[8].Value?.ToString();
                                        precio.Equibalencia = Math.Round((double)row[9].Value, 4);
                                        precios.Add(precio);
                                    }
                                }
                            }
                        }
                        package.Load(streams);


                        return Ok(precios);
                    }
                }
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

        [HttpGet("filtro")]
        public async Task<ActionResult> GetPreciosFiltro([FromQuery] ParametrosBusquedaPrecios parametros)
        {
            try
            {
                var precios = context.Precio
                    .Include(x => x.Cliente)
                    .Include(x => x.Producto)
                    .Include(x => x.Destino)
                    .Include(x => x.Zona)
                    .Include(x => x.Moneda)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(parametros.cliente))
                    precios = precios.Where(x => x.Cliente.Den.ToLower().Contains(parametros.cliente.ToLower()));

                if (!string.IsNullOrEmpty(parametros.producto))
                    precios = precios.Where(x => x.Producto.Den.ToLower().Contains(parametros.producto.ToLower()));
                if (!string.IsNullOrEmpty(parametros.destino))
                    precios = precios.Where(x => x.Destino.Den.ToLower().Contains(parametros.destino.ToLower()));
                if (!string.IsNullOrEmpty(parametros.zona))
                    precios = precios.Where(x => x.Zona.Nombre.ToLower().Contains(parametros.zona.ToLower()));

                await HttpContext.InsertarParametrosPaginacion(precios, parametros.tamanopagina, parametros.pagina);

                if (HttpContext.Response.Headers.ContainsKey("pagina"))
                {
                    var pagina = HttpContext.Response.Headers["pagina"];
                    if (pagina != parametros.pagina && !string.IsNullOrEmpty(pagina))
                    {
                        parametros.pagina = int.Parse(pagina);
                    }
                }

                precios = precios.Skip((parametros.pagina - 1) * parametros.tamanopagina).Take(parametros.tamanopagina);

                return Ok(precios);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("programado/filtro")]
        public async Task<ActionResult> GetPreciosProgramadosFiltro([FromQuery] ParametrosBusquedaPrecios parametros)
        {
            try
            {
                var precios = context.PrecioProgramado
                    .Include(x => x.Cliente)
                    .Include(x => x.Producto)
                    .Include(x => x.Destino)
                    .Include(x => x.Zona)
                    .Include(x => x.Moneda)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(parametros.cliente))
                    precios = precios.Where(x => x.Cliente.Den.ToLower().Contains(parametros.cliente.ToLower()));

                if (!string.IsNullOrEmpty(parametros.producto))
                    precios = precios.Where(x => x.Producto.Den.ToLower().Contains(parametros.producto.ToLower()));
                if (!string.IsNullOrEmpty(parametros.destino))
                    precios = precios.Where(x => x.Destino.Den.ToLower().Contains(parametros.destino.ToLower()));
                if (!string.IsNullOrEmpty(parametros.zona))
                    precios = precios.Where(x => x.Zona.Nombre.ToLower().Contains(parametros.zona.ToLower()));

                await HttpContext.InsertarParametrosPaginacion(precios, parametros.tamanopagina, parametros.pagina);

                if (HttpContext.Response.Headers.ContainsKey("pagina"))
                {
                    var pagina = HttpContext.Response.Headers["pagina"];
                    if (pagina != parametros.pagina && !string.IsNullOrEmpty(pagina))
                    {
                        parametros.pagina = int.Parse(pagina);
                    }
                }

                precios = precios.Skip((parametros.pagina - 1) * parametros.tamanopagina).Take(parametros.tamanopagina);

                //var preciosPre = precios.Select(x => new Precio
                //{
                //    Pre = x.Pre,
                //    Cliente = x.Cliente,
                //    Destino = x.Destino,
                //    Producto = x.Producto,
                //    Zona = x.Zona,
                //    FchDia = x.FchDia,
                //    Activo = x.Activo,
                //    codCte = x.codCte,
                //    cod
                //});

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
                    if (context.Precio.Any(x => x.codDes == precio.codDes && x.codCte == precio.codCte && x.codPrd == precio.codPrd && x.codZona == precio.codZona))
                        return BadRequest("El destino ya cuenta con un precio asignado para ese producto.");

                    context.Add(precio);
                    acc = 3;
                }
                var precioH = new PrecioHistorico
                {
                    Cod = null!,
                    pre = precio.Pre,
                    codCte = precio?.codCte,
                    codDes = precio?.codDes,
                    codGru = precio?.codGru,
                    codPrd = precio?.codPrd,
                    codZona = precio?.codZona,
                    FchDia = precio.FchDia,
                    FchActualizacion = precio.FchActualizacion,
                    Moneda = precio.Moneda,
                    Equibalencia = precio.Equibalencia
                };

                context.Add(precioH);
                var id = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();
                await context.SaveChangesAsync(id, acc);

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
                List<PrecioProgramado> preciosPro = new List<PrecioProgramado>();
                var LimiteDate = DateTime.Today.AddHours(16);

                if (!string.IsNullOrEmpty(folio))
                {
                    var ordenes = await context.OrdenCierre.Where(x => x.Folio == folio && x.Estatus == true)
                            .Include(x => x.Cliente)
                            .ToListAsync();
                    var ordenesUnic = ordenes.DistinctBy(x => x.CodPrd).Select(x => x);

                    foreach (var item in ordenesUnic)
                    {
                        if (item is not null)
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
                                Producto = context.Producto.FirstOrDefault(x => x.Cod == item.CodPrd),
                                Moneda = item.Moneda,
                                ID_Moneda = item.ID_Moneda,
                                Equibalencia = item.Equibalencia ?? 1
                            };
                            precios.Add(precio);
                        }
                    }
                    return Ok(precios);
                }

                precios = await context.Precio.Where(x => x.codCte == zonaCliente.CteCod
                    && x.codDes == zonaCliente.DesCod && x.Activo == true)
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
                    preciosPro = await context.PrecioProgramado.Where(x => x.codCte == zonaCliente.CteCod
                    && x.codDes == zonaCliente.DesCod && x.Activo == true)
                    //&& x.codZona == zona.ZonaCod)
                    .Include(x => x.Producto)
                    .ToListAsync();

                    foreach (var item in preciosPro)
                    {
                        if (item.FchDia > DateTime.Today)
                        {
                            precios.FirstOrDefault(x => x.codDes == item.codDes && x.codCte == item.codCte && x.codPrd == item.codPrd).Pre = item.Pre;
                            precios.FirstOrDefault(x => x.codDes == item.codDes && x.codCte == item.codCte && x.codPrd == item.codPrd).FchDia = item.FchDia;
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
                    //Debug.WriteLine($"Destino: {item.Destino}, count :{precios.IndexOf(item)}");
                    var codcte = string.IsNullOrEmpty(item.Cliente) ? string.Empty : item.Cliente;
                    var cliente = context.Cliente.FirstOrDefault(x => x.Den!.Replace("\"", "").Equals(codcte));
                    if (cliente is null)
                        return BadRequest($"No se encontro el cliente {item.Cliente}");

                    var codprd = string.Empty;
                    if (string.IsNullOrEmpty(item.Producto))
                        return BadRequest("No se admiten valores vacios en el nombre del producto");
                    var arrprd = item.Producto.Split(" ");
                    if (arrprd.Count() > 1)
                        codprd = arrprd[0];
                    else
                        codprd = item.Producto;

                    var producto = context.Producto.FirstOrDefault(x => x.Den!.Contains(codprd));
                    if (producto is null)
                        return BadRequest($"No se encontro el producto {item.Producto}");

                    var codzona = string.IsNullOrEmpty(item.Zona) ? "Sin Zona" : item.Zona;
                    var zona = context.Zona.FirstOrDefault(x => x.Nombre.Equals(codzona));

                    var coddes = string.IsNullOrEmpty(item.CodSyn) ? string.Empty : item.CodSyn;
                    var destino = context.Destino.FirstOrDefault(x => x.Codsyn == coddes);
                    if (destino is null)
                        return BadRequest($"No se encontro el destino {item.Destino} synthesis:{item.CodSyn} tuxpan {item.CodTux}");

                    if (item?.Precio == null || item.Precio == 0)
                        return BadRequest($"El destino {destino.Den} no tiene un precio con valor");

                    if (string.IsNullOrEmpty(item.Fecha))
                        return BadRequest("No se admiten valores vacios en la fecha de vigencia");

                    //if (string.IsNullOrEmpty(item.Moneda))
                    //    return BadRequest("No se admiten valores vacios en la moneda");

                    //if (!context.Moneda.Any(x => x.Nombre == item.Moneda))
                    //    return BadRequest($"No se encontro la moneda ingresada. Moneda: {item.Moneda}");

                    if (item.Equibalencia is null || item.Equibalencia == 0)
                        return BadRequest("La equibalencia no puede estar vacia o con valor 0");

                    var moneda = context.Moneda.FirstOrDefault(x => x.Nombre == item.Moneda && x.Estatus);

                    if (moneda is null)
                        moneda = context.Moneda.FirstOrDefault(x => x.Nombre.Equals("MXN"));
                    else
                        moneda = context.Moneda.FirstOrDefault(x => x.Nombre.Equals(item.Moneda));

                    if (DateTime.Parse(item.Fecha) > DateTime.Today)
                    {
                        var precio = new PrecioProgramado
                        {
                            codCte = cliente.Cod,
                            codDes = destino.Cod,
                            codGru = cliente.codgru,
                            codPrd = producto.Cod,
                            codZona = zona?.Cod,
                            FchDia = DateTime.Parse(item.Fecha),
                            FchActualizacion = DateTime.Now,
                            Pre = item.Precio,
                            ID_Moneda = moneda?.Id,
                            Equibalencia = (double)item.Equibalencia
                        };

                        var p = context.PrecioProgramado.IgnoreAutoIncludes().FirstOrDefault(x => x.codGru == precio.codGru
                        //&& x.codZona == precio.codZona
                        && x.codCte == precio.codCte
                        && x.codPrd == precio.codPrd
                        && x.codDes == precio.codDes);

                        if (p is not null)
                        {
                            p.Pre = precio.Pre;
                            p.FchDia = precio.FchDia;
                            p.FchActualizacion = DateTime.Now;
                            p.ID_Moneda = precio.ID_Moneda;
                            p.Equibalencia = precio.Equibalencia;
                            context.Update(p);
                        }
                        else
                            context.Add(precio);
                        //prec.Add(precio);
                    }
                    else if (DateTime.Parse(item.Fecha) == DateTime.Today)
                    {
                        var precio = new Precio
                        {
                            codCte = cliente.Cod,
                            codDes = destino.Cod,
                            codGru = cliente.codgru,
                            codPrd = producto.Cod,
                            codZona = zona?.Cod,
                            FchDia = DateTime.Parse(item.Fecha),
                            FchActualizacion = DateTime.Now,
                            Pre = item.Precio,
                            ID_Moneda = moneda?.Id,
                            Equibalencia = (double)item.Equibalencia
                        };

                        var p = context.Precio.IgnoreAutoIncludes().FirstOrDefault(x => x.codGru == precio.codGru
                        //&& x.codZona == precio.codZona
                        && x.codCte == precio.codCte
                        && x.codPrd == precio.codPrd
                        && x.codDes == precio.codDes);

                        if (p is not null)
                        {
                            p.Pre = precio.Pre;
                            p.FchActualizacion = DateTime.Now;
                            p.ID_Moneda = precio.ID_Moneda;
                            p.Equibalencia = precio.Equibalencia;

                            context.Update(p);
                        }
                        else
                            context.Add(precio);

                        var precioH = new PrecioHistorico
                        {
                            Cod = null!,
                            pre = precio.Pre,
                            codCte = precio.codCte == null ? 0 : precio.codCte,
                            codDes = precio.codDes == null ? 0 : precio.codDes!,
                            codGru = precio.codGru == null ? (short)0 : (short)precio.codGru!,
                            codPrd = precio.codPrd == null ? 0 : precio.codPrd,
                            codZona = precio.codZona == null ? 0 : precio.codZona,
                            FchDia = precio.FchDia,
                            FchActualizacion = precio.FchActualizacion,
                            ID_Moneda = precio?.ID_Moneda,
                            Equibalencia = precio.Equibalencia
                        };

                        context.Add(precioH);
                    }
                }

                //if (prec.Count > 0)
                //{
                //    context.PrecioProgramado.ExecuteDelete();
                //    context.AddRange(prec);
                //}

                var id = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                await context.SaveChangesAsync(id, 8);

                List<Destino> destinos = new List<Destino>();
                List<PreciosDTO> destinosSinPre = new List<PreciosDTO>();
                destinos = context.Destino.ToList();
                foreach (var item in destinos)
                    if (!context.PrecioProgramado.Any(x => x.codDes == item.Cod))
                    {
                        PreciosDTO dTO = new PreciosDTO()
                        {
                            Destino = item.Den,
                            Cliente = item.Cliente?.Den,
                            CodSyn = item.Codsyn,
                            CodTux = item.CodGamo.ToString(),
                            Moneda = "MXN",
                            Equibalencia = 1
                        };
                        destinosSinPre.Add(dTO);
                    }

                return Ok(destinosSinPre);
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
                List<Precio> preciosDia = new List<Precio>();
                List<PrecioHistorico> precioHistoricos = new List<PrecioHistorico>();
                //if (fchInicio <= fchHoy && fchFin >= fchHoy)
                //{
                List<PrecioProgramado> precios = new List<PrecioProgramado>();
                precios = context.PrecioProgramado.Where(x => x.FchDia == DateTime.Today).ToList();

                if (precios.Count > 0)
                {
                    foreach (var item in precios)
                    {
                        if (item is not null)
                        {
                            var precio = context.Precio.FirstOrDefault(x => x.codCte == item.codCte && x.codDes == item.codDes && x.codPrd == item.codPrd && x.Activo == true);
                            if (precio is null)
                            {
                                var precioN = new Precio
                                {
                                    codCte = item.codCte,
                                    codDes = item.codDes,
                                    codGru = item?.codGru,
                                    codPrd = item?.codPrd,
                                    codZona = item?.codZona,
                                    FchDia = item?.FchDia ?? DateTime.MinValue,
                                    FchActualizacion = DateTime.Now,
                                    Pre = item?.Pre ?? 0,
                                    Moneda = item.Moneda,
                                    Equibalencia = item.Equibalencia
                                };
                                preciosDia.Add(precioN);
                                //context.Add(precioN);
                            }
                            else
                            {
                                precio.Pre = item.Pre;
                                precio.FchDia = item.FchDia;
                                precio.FchActualizacion = DateTime.Now;
                                precio.Moneda = item.Moneda;
                                precio.Equibalencia = item.Equibalencia;
                                context.Update(precio);
                            }

                            var precioH = new PrecioHistorico
                            {
                                Cod = null!,
                                pre = item.Pre,
                                codCte = item.codCte,
                                codDes = item.codDes,
                                codGru = item?.codGru,
                                codPrd = item?.codPrd,
                                codZona = item?.codZona,
                                FchDia = item.FchDia,
                                FchActualizacion = item.FchActualizacion,
                                Moneda = item.Moneda,
                                Equibalencia = item.Equibalencia
                            };
                            precioHistoricos.Add(precioH);
                        }
                        //context.Add(precioH);
                    }
                    await context.AddRangeAsync(preciosDia);
                    await context.AddRangeAsync(precioHistoricos);

                    await context.SaveChangesAsync();

                    context.RemoveRange(precios);
                    await context.SaveChangesAsync();
                }
                return Ok(preciosDia);
                //}

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
                    .Where(x => x.FchDia > DateTime.Today)
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
                    codZona = precio.codZona,
                    FchDia = precio.FchDia,
                    FchActualizacion = DateTime.Now,
                    Pre = precio.Pre,
                    Activo = precio.Activo,
                    Moneda = precio.Moneda,
                    Equibalencia = precio.Equibalencia
                };

                if (precio.Cod != null)
                    context.Update(precioPro);
                else
                {
                    if (context.Precio.Any(x => x.codDes == precio.codDes && x.codCte == precio.codCte && x.codPrd == precio.codPrd && x.FchDia == precio.FchDia && x.codZona == precio.codZona))
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

        [HttpGet("{BOL}")]
        public ActionResult GetPrecioByBol([FromRoute] int BOL)
        {
            try
            {
                PrecioBol precios = new PrecioBol();

                var ordenes = context.Orden.Where(x => x.BatchId == BOL)
                    .Include(x => x.Producto)
                    .Include(x => x.Destino)
                    .FirstOrDefault();

                if (ordenes is null)
                    return Ok(new PrecioBol());

                PrecioBol precio = new PrecioBol();

                OrdenEmbarque? orden = new OrdenEmbarque();
                orden = context.OrdenEmbarque.Where(x => x.FolioSyn == ordenes.Ref).Include(x => x.Producto).Include(x => x.Destino).ThenInclude(x => x.Cliente).Include(x => x.OrdenCierre).FirstOrDefault();

                precio.Fecha_De_Carga = ordenes.Fchcar;

                precio.Referencia = ordenes.Ref;

                if (orden is not null)
                {
                    if (orden.Producto is not null)
                        precio.Producto_Original = orden.Producto.Den;

                    if (orden.Destino is not null)
                    {
                        precio.Destino_Original = orden.Destino.Den;
                        if (orden.Destino.Cliente is not null)
                            if (!string.IsNullOrEmpty(orden.Destino.Cliente.Den))
                                precio.Cliente_Original = orden.Destino.Cliente.Den;

                    }
                }

                precio.BOL = ordenes.BatchId;
                precio.Volumen_Cargado = ordenes.Vol;
                if (orden is not null)
                {
                    if (orden.Destino is not null)
                        precio.Destino_Original = orden.Destino.Den ?? "";

                    if (orden.Producto is not null)
                        precio.Producto_Original = orden.Producto.Den ?? "";
                }

                var precioVig = context.Precio.Where(x => ordenes != null && x.codDes == ordenes.Coddes && x.codPrd == ordenes.Codprd)
                    .OrderByDescending(x => x.FchDia)
                    .FirstOrDefault();

                var precioPro = context.PrecioProgramado.Where(x => ordenes != null && x.codDes == ordenes.Coddes && x.codPrd == ordenes.Codprd)
                    .OrderByDescending(x => x.FchDia)
                    .FirstOrDefault();

                var precioHis = context.PreciosHistorico.Where(x => ordenes != null && x.codDes == ordenes.Coddes && x.codPrd == ordenes.Codprd
                    && ordenes.Fchcar != null && x.FchDia <= ordenes.Fchcar.Value.Date)
                    .OrderByDescending(x => x.FchDia)
                    .FirstOrDefault();

                if (precioHis is not null)
                {
                    precio.Precio = precioHis.pre;
                    precio.Fecha_De_Precio = precioHis.FchDia;
                    precio.Precio_Encontrado = true;
                    precio.Precio_Encontrado_En = "Historial";
                    precio.Moneda = precioHis?.Moneda?.Nombre;
                    precio.Tipo_De_Cambio = precioHis?.Equibalencia ?? 1;
                }

                if (ordenes != null && precioVig is not null && ordenes.Fchcar is not null && ordenes.Fchcar.Value.Date == DateTime.Today)
                {
                    precio.Precio = precioVig.Pre;
                    precio.Fecha_De_Precio = precioVig.FchDia;
                    precio.Precio_Encontrado = true;
                    precio.Precio_Encontrado_En = "Vigente";
                    precio.Moneda = precioVig?.Moneda?.Nombre;
                    precio.Tipo_De_Cambio = precioVig?.Equibalencia ?? 1;
                }

                if (ordenes != null && precioPro is not null && ordenes.Fchcar is not null && ordenes.Fchcar.Value.Date == DateTime.Today && context.PrecioProgramado.Any())
                {
                    precio.Precio = precioPro.Pre;
                    precio.Fecha_De_Precio = precioPro.FchDia;
                    precio.Precio_Encontrado = true;
                    precio.Precio_Encontrado_En = "Programado";
                    precio.Moneda = precioPro?.Moneda?.Nombre;
                    precio.Tipo_De_Cambio = precioPro?.Equibalencia ?? 1;
                }

                if (orden != null && context.OrdenPedido.Any(x => x.CodPed == orden.Cod))
                {
                    var ordenepedido = context.OrdenPedido.Where(x => x.CodPed == orden.Cod && !string.IsNullOrEmpty(x.Folio)).FirstOrDefault();

                    if (ordenepedido is not null)
                    {
                        var cierre = context.OrdenCierre.Where(x => x.Folio == ordenepedido.Folio
                         && x.CodPrd == orden.Codprd).FirstOrDefault();

                        if (cierre is not null)
                        {
                            precio.Precio = cierre.Precio;
                            precio.Fecha_De_Precio = cierre.fchPrecio;
                            precio.Es_Cierre = true;
                            precio.Precio_Encontrado = true;
                            precio.Precio_Encontrado_En = "Cierre";
                            precio.Moneda = cierre?.Moneda?.Nombre;
                            precio.Tipo_De_Cambio = cierre?.Equibalencia ?? 1;
                        }
                    }
                }

                if (orden is not null && precioHis is null && precioPro is null && precioVig is null && !precio.Es_Cierre)
                {
                    precio.Precio = orden.Pre;

                    if (orden.OrdenCierre is not null)
                        precio.Fecha_De_Precio = orden.OrdenCierre.fchPrecio;

                    precio.Es_Precio_De_Creacion = true;
                    precio.Precio_Encontrado_En = "Creacion";
                }

                precio.Moneda = !string.IsNullOrEmpty(precio.Moneda) ? precio.Moneda : "MXN";

                return Ok(precio);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        //[HttpGet("{Orden_Compra}")]
        //public ActionResult GetPrecioByBol([FromRoute] int Orden_Compra)
        //{
        //    try
        //    {
        //        List<PrecioBol> precios = new List<PrecioBol>();

        //        var ordenes = context.OrdenEmbarque.Where(x => x.Folio == Orden_Compra)
        //            .Include(x => x.Producto)
        //            .Include(x => x.Destino)
        //            .ToList();

        //        if (ordenes is null)
        //            return BadRequest($"No se encontro la orden con el numero de compra: {Orden_Compra}");

        //        foreach (var item in ordenes)
        //        {
        //            PrecioBol precio = new PrecioBol();

        //            Orden? orden = new Orden();
        //            orden = context.Orden.Where(x => x.Ref == item.FolioSyn).Include(x => x.Producto).Include(x => x.Destino).FirstOrDefault();

        //            precio.Fecha_De_Carga = orden?.Fchcar ?? item.Fchcar;

        //            precio.Referencia = orden?.Ref ?? item.FolioSyn;

        //            if (orden is not null)
        //            {
        //                if (orden.Producto is not null)
        //                    precio.Producto_Synthesis = orden.Producto.Den;

        //                if (orden.Destino is not null)
        //                    precio.Destino_Synthesis = orden.Destino.Den;

        //                precio.BOL = orden.BatchId ?? 0;
        //            }

        //            if (item is not null)
        //            {
        //                if (item.Destino is not null)
        //                    precio.Destino_Original = item.Destino.Den ?? "";

        //                if (item.Producto is not null)
        //                    precio.Producto_Original = item.Producto.Den ?? "";
        //            }

        //            var precioVig = context.Precio.Where(x => item != null && x.codDes == item.Coddes && x.codPrd == item.Codprd).FirstOrDefault();

        //            if (orden is not null)
        //                precioVig = context.Precio.Where(x => x.codDes == orden.Coddes && x.codPrd == orden.Codprd).FirstOrDefault();

        //            var precioPro = context.PrecioProgramado.Where(x => item != null && x.codDes == item.Coddes && x.codPrd == item.Codprd).FirstOrDefault();

        //            if (orden is not null)
        //                precioPro = context.PrecioProgramado.Where(x => x.codDes == orden.Coddes && x.codPrd == orden.Codprd).FirstOrDefault();

        //            var precioHis = context.PreciosHistorico.Where(x => item != null && x.codDes == item.Coddes && x.codPrd == item.Codprd
        //                && item.Fchcar != null && x.FchDia <= item.Fchcar.Value.Date)
        //                .OrderByDescending(x => x.FchDia)
        //                .FirstOrDefault();

        //            if (orden is not null)
        //                precioHis = context.PreciosHistorico.Where(x => x.codDes == orden.Coddes && x.codPrd == orden.Codprd
        //                && orden.Fchcar != null && x.FchDia <= orden.Fchcar.Value.Date)
        //                .OrderByDescending(x => x.FchDia)
        //                .FirstOrDefault();

        //            if (precioHis is not null)
        //            {
        //                precio.Precio = precioHis.pre;
        //                precio.Fecha_De_Precio = precioHis.FchDia;
        //                precio.Precio_Encontrado = true;
        //                precio.Precio_Encontrado_En = "Historial";
        //            }

        //            if (item != null && precioVig is not null && item.Fchcar is not null && item.Fchcar.Value.Date == DateTime.Today ||
        //                orden is not null && orden.Fchcar is not null && orden.Fchcar.Value.Date == DateTime.Today && precioVig is not null)
        //            {
        //                precio.Precio = precioVig.Pre;
        //                precio.Fecha_De_Precio = precioVig.FchDia;
        //                precio.Precio_Encontrado = true;
        //                precio.Precio_Encontrado_En = "Vigente";
        //            }

        //            if (item != null && precioPro is not null && item.Fchcar is not null && item.Fchcar.Value.Date == DateTime.Today && DateTime.Now.TimeOfDay >= new TimeSpan(16, 0, 0)
        //                && context.PrecioProgramado.Any() ||
        //                orden is not null && precioPro is not null && orden.Fchcar is not null && orden.Fchcar.Value.Date == DateTime.Today && DateTime.Now.TimeOfDay >= new TimeSpan(16, 0, 0)
        //                && context.PrecioProgramado.Any())
        //            {
        //                precio.Precio = precioPro.Pre;
        //                precio.Fecha_De_Precio = precioPro.FchDia;
        //                precio.Precio_Encontrado = true;
        //                precio.Precio_Encontrado_En = "Programado";
        //            }

        //            if (item != null && context.OrdenPedido.Any(x => x.CodPed == item.Cod))
        //            {
        //                var ordenepedido = context.OrdenPedido.Where(x => x.CodPed == item.Cod && !string.IsNullOrEmpty(x.Folio)).FirstOrDefault();

        //                if (ordenepedido is not null)
        //                {
        //                    var cierre = context.OrdenCierre.Where(x => x.Folio == ordenepedido.Folio
        //                     && x.CodPrd == item.Codprd).FirstOrDefault();

        //                    if (cierre is not null)
        //                    {
        //                        precio.Precio = cierre.Precio;
        //                        precio.Fecha_De_Precio = cierre.fchPrecio;
        //                        precio.Es_Cierre = true;
        //                        precio.Precio_Encontrado = true;
        //                        precio.Precio_Encontrado_En = "Cierre";
        //                    }
        //                }
        //            }

        //            if (item is not null && precioHis is null && precioPro is null && precioVig is null && !precio.Es_Cierre)
        //            {
        //                precio.Precio = item.Pre;

        //                if (item.OrdenCierre is not null)
        //                    precio.Fecha_De_Precio = item.OrdenCierre.fchPrecio;

        //                precio.Es_Precio_De_Creacion = true;
        //                precio.Precio_Encontrado_En = "Creacion";
        //            }

        //            precios.Add(precio);
        //        }

        //        return Ok(precios);
        //    }
        //    catch (Exception e)
        //    {
        //        return BadRequest(e.Message);
        //    }
        //}

        public class PrecioBol
        {
            public double? Precio { get; set; } = 0;
            public string? Referencia { get; set; } = string.Empty;
            public int? BOL { get; set; } = 0;
            public DateTime? Fecha_De_Carga { get; set; } = DateTime.MinValue;
            public DateTime? Fecha_De_Precio { get; set; } = DateTime.MinValue;
            public string Destino_Synthesis { get; set; } = string.Empty;
            public string? Destino_Original { get; set; } = string.Empty;
            public string Producto_Synthesis { get; set; } = string.Empty;
            public string? Producto_Original { get; set; } = string.Empty;
            public bool Es_Cierre { get; set; } = false;
            public bool Es_Precio_De_Creacion { get; set; } = false;
            public bool Precio_Encontrado { get; set; } = false;
            public string Precio_Encontrado_En { get; set; } = string.Empty;
            public double Tipo_De_Cambio { get; set; } = 1;
            public string? Moneda { get; set; } = "MXN";
            public string? Cliente_Original { get; set; } = string.Empty;
            public double? Volumen_Cargado { get; set; } = 0;
        }
    }
}