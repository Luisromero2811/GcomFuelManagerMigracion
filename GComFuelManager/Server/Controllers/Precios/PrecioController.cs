﻿using AutoMapper;
using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using System.Net;

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
        private readonly User_Terminal _terminal;

        public PrecioController(ApplicationDbContext context, UserManager<IdentityUsuario> userManager, VerifyUserToken verifyUser, User_Terminal _Terminal)
        {
            this.context = context;
            this.userManager = userManager;
            this.verifyUser = verifyUser;
            this._terminal = _Terminal;
        }

        [HttpGet("formato")]
        public async Task<ActionResult> Descargar_Formato()
        {
            try
            {
                List<PreciosDTO> precios = new();

                var terminales = context.Tad.Where(x => !string.IsNullOrEmpty(x.Den) && x.Activo == true).ToList();
                for (int t = 0; t < terminales.Count; t++)
                {
                    var productos = context.Producto.Where(x => x.Activo == true && x.Id_Tad.Equals(terminales[t].Cod)).ToList();
                    for (int p = 0; p < productos.Count; p++)
                    {
                        precios.Add(new()
                        {
                            Terminal = terminales[t].Den,
                            Producto = productos[p].Den,
                            Fecha = DateTime.Today.ToString("d"),
                            Precio = 0,
                            Precio_Compra = 0
                        });
                    }
                }

                ExcelPackage.LicenseContext = LicenseContext.Commercial;

                ExcelPackage excel = new();

                excel.Workbook.Worksheets.Add("Precios_Globales");

                var ws = excel.Workbook.Worksheets.First();

                ws.Cells["A1"].LoadFromCollection(precios, c =>
                {
                    c.PrintHeaders = true;
                    c.TableStyle = TableStyles.Medium15;
                });

                ws.Cells[1, 10, ws.Dimension.End.Row, 11].Style.Numberformat.Format = "_-$* #,##0.00_-;-$* #,##0.00_-;_-$* \"-\"??_-;_-@_-";
                ws.Cells[1, 9, ws.Dimension.End.Row, 9].Style.Numberformat.Format = "dd/MM/yyyy";

                ws.Cells[1, 1, ws.Dimension.End.Row, ws.Dimension.End.Column].AutoFitColumns();

                return Ok(await excel.GetAsByteArrayAsync());
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("file")]
        public async Task<ActionResult> Subir_Precios_Masivos([FromForm] IEnumerable<IFormFile> files)
        {
            if (files is null) { throw new ArgumentNullException(nameof(files)); }

            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0) { return BadRequest(); }

                var id = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                var user = await userManager.FindByIdAsync(id);

                if (user is null)
                    return NotFound();

                var user_system = context.Usuario.Find(user.UserCod);
                if (user_system is null)
                    return NotFound();

                var MaxAllowedFiles = 10;
                var MaxAllowedSize = 1024 * 1024 * 15;
                var FilesProcesed = 0;

                List<UploadResult> uploadResults = new();

                List<Precio> precios = new();
                List<Precio> precios_editados = new();
                List<PrecioProgramado> precios_programados_editados = new();
                List<PrecioProgramado> precios_programados = new();
                List<PrecioHistorico> precios_historico = new();

                foreach (var file in files)
                {
                    var uploadResult = new UploadResult();

                    var unthrustFileName = file.FileName;
                    var thrustFileName = WebUtility.HtmlDecode(unthrustFileName);
                    uploadResult.FileName = thrustFileName;

                    if (FilesProcesed < MaxAllowedFiles)
                    {
                        if (file.Length == 0)
                        {
                            uploadResult.ErrorCode = 2;
                            uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {uploadResult.FileName} : Archivo vacio.";
                            return BadRequest(uploadResult.ErrorMessage);
                        }
                        else if (file.Length > MaxAllowedSize)
                        {
                            uploadResult.ErrorCode = 3;
                            uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {uploadResult.FileName} : {file.Length / 1000000} Mb es mayor a la capacidad permitida ({Math.Round((double)(MaxAllowedSize / 1000000))}) Mb ";
                            return BadRequest(uploadResult.ErrorMessage);
                        }
                        else
                        {
                            using var stream = new MemoryStream();
                            await file.CopyToAsync(stream);

                            ExcelPackage.LicenseContext = LicenseContext.Commercial;
                            ExcelPackage package = new();

                            package.Load(stream);

                            if (package.Workbook.Worksheets.Count > 0)
                            {
                                using ExcelWorksheet ws = package.Workbook.Worksheets.First();

                                for (int r = 2; r < (ws.Dimension.End.Row + 1); r++)
                                {
                                    var rows = ws.Cells[r, 1, r, 13].ToList();
                                    if (rows.Count > 0)
                                    {
                                        if (ws.Cells[r, 1].Value is null) { return BadRequest($"La terminal no puede estar vacia. (fila: {r}, columna: 1)"); }
                                        var terminal = ws.Cells[r, 1].Value.ToString();
                                        if (string.IsNullOrEmpty(terminal) || string.IsNullOrWhiteSpace(terminal)) { return BadRequest($"La terminal no puede estar vacia. (fila: {r}, columna: 1)"); }

                                        var ter = context.Tad.FirstOrDefault(x => !string.IsNullOrEmpty(x.Den) && x.Den.Equals(terminal) && x.Activo == true);
                                        if (ter is null) { return BadRequest($"No se encontro la terminal. (fila: {r}, columna: 1)"); }

                                        if (ws.Cells[r, 2].Value is null) { return BadRequest($"El producto no puede estar vacio. (fila: {r}, columna: 2)"); }
                                        var producto = ws.Cells[r, 2].Value.ToString();
                                        if (string.IsNullOrEmpty(producto) || string.IsNullOrWhiteSpace(producto)) { return BadRequest($"El producto no puede estar vacio. (fila: {r}, columna: 2)"); }

                                        var prd = context.Producto.FirstOrDefault(x => !string.IsNullOrEmpty(x.Den) && x.Den.Equals(producto) && x.Activo == true && x.Id_Tad == ter.Cod);
                                        if (prd is null) { return BadRequest($"No se encontro el producto en la terminal {ter?.Den}. (fila: {r}, columna: 2)"); }

                                        var zona = ws.Cells[r, 3].Value;
                                        zona ??= "Sin Zona";
                                        if (string.IsNullOrEmpty(zona.ToString()) || string.IsNullOrWhiteSpace(zona.ToString())) { zona = "Sin Zona"; }

                                        var z = context.Zona.FirstOrDefault(x => !string.IsNullOrEmpty(x.Nombre) && x.Nombre.Equals(zona));
                                        if (z is null) { return BadRequest($"No se encontro la zona ingresada. (fila: {r}, columna: 3)"); }

                                        if (ws.Cells[r, 4].Value is null) { return BadRequest($"El cliente no puede estar vacio. (fila: {r}, columna: 4)"); }
                                        var cliente = ws.Cells[r, 4].Value.ToString();
                                        if (string.IsNullOrEmpty(cliente) || string.IsNullOrWhiteSpace(cliente)) { return BadRequest($"El cliente no puede estar vacio. (fila: {r}, columna: 4)"); }

                                        var cte = context.Cliente.FirstOrDefault(x => !string.IsNullOrEmpty(x.Den) && x.Den.Equals(cliente) && x.Id_Tad == ter.Cod);
                                        if (cte is null) { return BadRequest($"No se encontro el cliente en la terminal {ter.Den}. (fila: {r}, columna: 4)"); }

                                        Destino des = new();

                                        if (ws.Cells[r, 6].Value is not null && (!string.IsNullOrEmpty(ws.Cells[r, 6].Value.ToString()) || !string.IsNullOrWhiteSpace(ws.Cells[r, 6].Value.ToString())))
                                        {
                                            var cod_des = ws.Cells[r, 6].Value.ToString();
                                            if (!string.IsNullOrEmpty(cod_des) || !string.IsNullOrWhiteSpace(cod_des))
                                            {
                                                var d = context.Destino.FirstOrDefault(x => !string.IsNullOrEmpty(x.Codsyn) && x.Codsyn.Equals(cod_des) && x.Id_Tad == ter.Cod);
                                                if (d is null) { return BadRequest($"No se encontro el destino con el codigo de synthesis: {cod_des} en la terminal {ter.Den}. (fila: {r}, columna: 6)"); }
                                                des = d;
                                            }
                                        }
                                        else
                                        {
                                            if (ws.Cells[r, 8].Value is not null && (!string.IsNullOrEmpty(ws.Cells[r, 8].Value.ToString()) || !string.IsNullOrWhiteSpace(ws.Cells[r, 8].Value.ToString())))
                                            {
                                                var cod_des_gob = ws.Cells[r, 8].Value.ToString();
                                                if (!string.IsNullOrEmpty(cod_des_gob) || !string.IsNullOrWhiteSpace(cod_des_gob))
                                                {
                                                    var d = context.Destino.FirstOrDefault(x => !string.IsNullOrEmpty(x.Id_DestinoGobierno) && x.Id_DestinoGobierno.Equals(cod_des_gob) && x.Id_Tad == ter.Cod);
                                                    if (d is null) { return BadRequest($"No se encontro el destino con el codigo de gobierno: {cod_des_gob} en la terminal {ter.Den}. (fila: {r}, columna: 8)"); }
                                                    des = d;
                                                }
                                            }
                                            else
                                            {
                                                if (ws.Cells[r, 5].Value is null) { return BadRequest($"El destino no puede estar vacio. (fila: {r}, columna: 5)"); }
                                                var destino = ws.Cells[r, 5].Value.ToString();
                                                if (string.IsNullOrEmpty(destino) || string.IsNullOrWhiteSpace(destino)) { return BadRequest($"El destino no puede estar vacio. (fila: {r}, columna: 5)"); }

                                                var des_db = context.Destino.FirstOrDefault(x => !string.IsNullOrEmpty(x.Den) && x.Den.Equals(destino) && x.Id_Tad == ter.Cod);
                                                if (des_db is null) { return BadRequest($"No se encontro el destino en la terminal {ter.Den}. (fila: {r}, columna: 5)"); }
                                                else
                                                    des = des_db;
                                            }

                                        }

                                        DateTime fecha_valida = DateTime.Today;

                                        if (ws.Cells[r, 9].Value is null) { return BadRequest($"La fehca no puede estar vacia. (fila: {r}, columna: 9)"); }
                                        var fecha = ws.Cells[r, 9].Value.ToString();
                                        if (string.IsNullOrEmpty(fecha) || string.IsNullOrWhiteSpace(fecha)) { return BadRequest($"La fehca no puede estar vacia. (fila: {r}, columna: 9)"); }

                                        if (DateTime.TryParse(fecha, out DateTime fch))
                                            fecha_valida = fch;
                                        else
                                            return BadRequest($"La fecha no tiene un formato valido. (fila: {r}, columna: 9)");

                                        double precio_final = 0;
                                        double precio_compra = 0;

                                        if (ws.Cells[r, 10].Value is null) { return BadRequest($"El precio final no puede estar vacio. (fila: {r}, columna: 10)"); }
                                        var preciofinal = ws.Cells[r, 10].Value.ToString();
                                        if (string.IsNullOrEmpty(preciofinal) || string.IsNullOrWhiteSpace(preciofinal)) { return BadRequest($"El precio final no puede estar vacio. (fila: {r}, columna: 10)"); }

                                        if (double.TryParse(preciofinal, out double pre))
                                            precio_final = pre;
                                        else
                                            return BadRequest($"No se pudo convertir el precio final a un dato valido. (fila: {r}, columna: 10)");

                                        if (ws.Cells[r, 11].Value is null) { return BadRequest($"El precio de compra no puede estar vacio. (fila: {r}, columna: 11)"); }
                                        var preciocompra = ws.Cells[r, 11].Value.ToString();
                                        if (string.IsNullOrEmpty(preciocompra) || string.IsNullOrWhiteSpace(preciocompra)) { return BadRequest($"El precio de compra no puede estar vacio. (fila: {r}, columna: 11)"); }

                                        if (double.TryParse(preciocompra, out double pre_com))
                                            precio_compra = pre_com;
                                        else
                                            return BadRequest($"No se pudo convertir el precio de compra a un dato valido. (fila: {r}, columna: 11)");

                                        var moneda = ws.Cells[r, 12].Value;
                                        moneda ??= "MXN";
                                        if (string.IsNullOrEmpty(moneda.ToString()) || string.IsNullOrWhiteSpace(moneda.ToString())) { moneda = "MXN"; }

                                        var mon = context.Moneda.FirstOrDefault(x => !string.IsNullOrEmpty(x.Nombre) && x.Nombre.Equals(moneda));
                                        if (mon is null) { return BadRequest($"No se encontro la moneda ingresada. (fila: {r}, columna: 12)"); }

                                        double equi = 0;
                                        var equibalencia = ws.Cells[r, 13].Value;
                                        equibalencia ??= "1";
                                        if (string.IsNullOrEmpty(equibalencia.ToString()) || string.IsNullOrWhiteSpace(equibalencia.ToString())) { equibalencia = "1"; }

                                        if (double.TryParse(equibalencia.ToString(), out double equiba))
                                            equi = equiba;
                                        else
                                            return BadRequest($"No se pudo convertir la equibalencia a un dato valido. (fila: {r}), columna: 13");

                                        if (fecha_valida > DateTime.Today)
                                        {
                                            var precioprogramado = new PrecioProgramado()
                                            {
                                                CodCte = cte.Cod,
                                                CodDes = des.Cod,
                                                CodGru = cte.Codgru,
                                                CodPrd = prd.Cod,
                                                CodZona = z.Cod,
                                                FchDia = fecha_valida,
                                                FchActualizacion = DateTime.Now,
                                                Pre = precio_final,
                                                Precio_Compra = precio_compra,
                                                ID_Moneda = mon.Id,
                                                Equibalencia = equi,
                                                ID_Usuario = user_system.Cod,
                                                Id_Tad = ter.Cod
                                            };

                                            var p = context.PrecioProgramado.IgnoreAutoIncludes().FirstOrDefault(x => x.CodGru == precioprogramado.CodGru
                                            && x.CodCte == precioprogramado.CodCte
                                            && x.CodPrd == precioprogramado.CodPrd
                                            && x.CodDes == precioprogramado.CodDes
                                            && x.Id_Tad == precioprogramado.Id_Tad);

                                            if (p is not null)
                                            {
                                                p.Pre = precioprogramado.Pre;
                                                p.Precio_Compra = precioprogramado.Precio_Compra;
                                                p.FchDia = precioprogramado.FchDia;
                                                p.FchActualizacion = DateTime.Now;
                                                p.ID_Moneda = precioprogramado.ID_Moneda;
                                                p.Equibalencia = precioprogramado.Equibalencia;
                                                p.ID_Usuario = precioprogramado.ID_Usuario;
                                                precios_programados_editados.Add(p);
                                            }
                                            else
                                                precios_programados.Add(precioprogramado);
                                        }
                                        else if (fecha_valida == DateTime.Today)
                                        {
                                            var precio = new Precio()
                                            {
                                                CodCte = cte.Cod,
                                                CodDes = des.Cod,
                                                CodGru = cte.Codgru,
                                                CodPrd = prd.Cod,
                                                CodZona = z.Cod,
                                                FchDia = fecha_valida,
                                                FchActualizacion = DateTime.Now,
                                                Pre = precio_final,
                                                Precio_Compra = precio_compra,
                                                ID_Moneda = mon.Id,
                                                Equibalencia = equi,
                                                ID_Usuario = user_system.Cod,
                                                Id_Tad = ter.Cod
                                            };

                                            var p = context.Precio.IgnoreAutoIncludes().FirstOrDefault(x => x.CodGru == precio.CodGru
                                            && x.CodCte == precio.CodCte
                                            && x.CodPrd == precio.CodPrd
                                            && x.CodDes == precio.CodDes
                                            && x.Id_Tad == precio.Id_Tad);

                                            if (p is not null)
                                            {
                                                p.Pre = precio.Pre;
                                                p.Precio_Compra = precio.Precio_Compra;
                                                p.FchDia = precio.FchDia;
                                                p.FchActualizacion = DateTime.Now;
                                                p.ID_Moneda = precio.ID_Moneda;
                                                p.Equibalencia = precio.Equibalencia;
                                                p.ID_Usuario = precio.ID_Usuario;
                                                precios_editados.Add(p);
                                            }
                                            else
                                                precios.Add(precio);

                                            var preciohistorico = new PrecioHistorico()
                                            {
                                                Cod = null!,
                                                CodCte = cte.Cod,
                                                CodDes = des.Cod,
                                                CodGru = cte.Codgru,
                                                CodPrd = prd.Cod,
                                                CodZona = z.Cod,
                                                FchDia = fecha_valida,
                                                FchActualizacion = DateTime.Now,
                                                pre = precio_final,
                                                Precio_Compra = precio_compra,
                                                ID_Moneda = mon.Id,
                                                Equibalencia = equi,
                                                ID_Usuario = user_system.Cod,
                                                Id_Tad = ter.Cod
                                            };

                                            precios_historico.Add(preciohistorico);
                                        }
                                    }
                                }

                                context.UpdateRange(precios_editados);
                                context.UpdateRange(precios_programados_editados);
                                context.AddRange(precios);
                                context.AddRange(precios_programados);
                                context.AddRange(precios_historico);

                                await context.SaveChangesAsync();
                            }
                        }
                    }

                    FilesProcesed++;
                }

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("upload")]//TODO: checar utilidad
        public async Task<ActionResult> Convert(IFormFile file)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                if (file == null)
                    return BadRequest("No se pudo leer el archivo enviado.");

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                //file.OpenReadStream();
                file.CopyTo(stream);

                List<PreciosDTO> precios = new();

                //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage.LicenseContext = LicenseContext.Commercial;

                ExcelPackage package = new();

                package.Load(stream);
                //package = new ExcelPackage(stream);
                if (package.Workbook.Worksheets.Count > 0)
                {
                    using (ExcelWorksheet worksheet = package.Workbook.Worksheets.First())
                    {
                        //for (int r = 2; r < (worksheet.Dimension.End.Row + 1); r++)
                        for (int r = 2; r < (worksheet.Dimension.End.Row + 1); r++)
                        {
                            PreciosDTO precio = new();

                            //var row = worksheet.Cells[r, 1, r, worksheet.Dimension.End.Column].ToList();
                            var row = worksheet.Cells[r, 1, r, 11].ToList();

                            if (row.Count == 11)
                            {
                                if (row[9].Value is not null)
                                    if (!context.Moneda.Any(x => x.Nombre == row[9].Value.ToString()))
                                        return BadRequest($"No existe la moneda ingresada. Moneda: {row[9].Value?.ToString()}");

                                if (context.Tad.Any(x => x.Cod == id_terminal && x.Activo == true))
                                {
                                    var terminal = context.Tad.Find(id_terminal);
                                    if (terminal is not null)
                                        precio.Terminal = terminal.Den;
                                }
                                else
                                    return BadRequest("Se se pudo encontrar la terminal o no se encuentra activa");

                                if (row[9].Value is null)
                                    row[9].Value = "MXN";

                                precio.Producto = row[0].Value?.ToString();
                                precio.Zona = row[1].Value?.ToString();
                                precio.Cliente = row[2].Value?.ToString();
                                precio.Destino = row[3].Value?.ToString();
                                precio.CodSyn = row[4].Value?.ToString();
                                precio.CodTux = row[5].Value?.ToString();
                                precio.CodDestinoGobierno = row[6].Value?.ToString();
                                precio.Fecha = row[7].Value?.ToString();
                                precio.Precio = Math.Round((double)row[8].Value, 4);
                                precio.Moneda = row[9].Value?.ToString();
                                precio.Equibalencia = Math.Round((double)row[10].Value, 4);
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

        [HttpGet("filtro")]
        public async Task<ActionResult> GetPreciosFiltro([FromQuery] ParametrosBusquedaPrecios parametros)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var precios = context.Precio.Where(x => x.Id_Tad == id_terminal)
                    .Include(x => x.Cliente)
                    .Include(x => x.Producto)
                    .Include(x => x.Destino)
                    .Include(x => x.Zona)
                    .Include(x => x.Moneda)
                    .Include(x => x.Usuario)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(parametros.cliente))
                    precios = precios.Where(x => x.Cliente != null && !string.IsNullOrEmpty(x.Cliente.Den) && x.Cliente.Den.ToLower().Contains(parametros.cliente.ToLower()));

                if (!string.IsNullOrEmpty(parametros.producto))
                    precios = precios.Where(x => x.Producto != null && !string.IsNullOrEmpty(x.Producto.Den) && x.Producto.Den.ToLower().Contains(parametros.producto.ToLower()));
                if (!string.IsNullOrEmpty(parametros.destino))
                    precios = precios.Where(x => x.Destino != null && !string.IsNullOrEmpty(x.Destino.Den) && x.Destino.Den.ToLower().Contains(parametros.destino.ToLower()));
                if (!string.IsNullOrEmpty(parametros.zona))
                    precios = precios.Where(x => x.Zona != null && !string.IsNullOrEmpty(x.Zona.Nombre) && x.Zona.Nombre.ToLower().Contains(parametros.zona.ToLower()));

                await HttpContext.InsertarParametrosPaginacion(precios, parametros.tamanopagina, parametros.pagina);

                if (HttpContext.Response.Headers.ContainsKey("pagina"))
                {
                    var pagina = HttpContext.Response.Headers["pagina"];
                    if (pagina != parametros.pagina && !string.IsNullOrEmpty(pagina))
                    {
                        parametros.pagina = int.Parse(pagina!);
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

        [HttpGet("filtrohist")]
        public async Task<ActionResult> GetPreciosHistoricosFiltro([FromQuery] ParametrosBusquedaPrecios parametros)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var precios = context.PreciosHistorico
                 .Where(x => x.FchDia >= parametros.Fecha_Inicio && x.FchDia <= parametros.Fecha_Fin && x.Id_Tad == id_terminal)
                    .Include(x => x.Destino)
                    .Include(x => x.Cliente)
                    .Include(x => x.Terminal)
                    .Include(x => x.Producto)
                    .Include(x => x.Zona)
                    .Include(x => x.Moneda)
                    .Include(x => x.Usuario)
                    .OrderBy(x => x.FchDia)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(parametros.cliente))
                    precios = precios.Where(x => x.Cliente != null && !string.IsNullOrEmpty(x.Cliente.Den) && x.Cliente.Den.ToLower().Contains(parametros.cliente.ToLower()));
                if (!string.IsNullOrEmpty(parametros.producto))
                    precios = precios.Where(x => x.Producto != null && !string.IsNullOrEmpty(x.Producto.Den) && x.Producto.Den.ToLower().Contains(parametros.producto.ToLower()));
                if (!string.IsNullOrEmpty(parametros.destino))
                    precios = precios.Where(x => x.Destino != null && !string.IsNullOrEmpty(x.Destino.Den) && x.Destino.Den.ToLower().Contains(parametros.destino.ToLower()));
                if (!string.IsNullOrEmpty(parametros.zona))
                    precios = precios.Where(x => x.Zona != null && !string.IsNullOrEmpty(x.Zona.Nombre) && x.Zona.Nombre.ToLower().Contains(parametros.zona.ToLower()));

                await HttpContext.InsertarParametrosPaginacion(precios, parametros.tamanopagina, parametros.pagina);

                if (HttpContext.Response.Headers.ContainsKey("pagina"))
                {
                    var pagina = HttpContext.Response.Headers["pagina"];
                    if (pagina != parametros.pagina && !string.IsNullOrEmpty(pagina))
                    {
                        parametros.pagina = int.Parse(pagina!);
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
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var precios = context.PrecioProgramado.Where(x => x.Id_Tad == id_terminal)
                    .Include(x => x.Cliente)
                    .Include(x => x.Producto)
                    .Include(x => x.Destino)
                    .Include(x => x.Zona)
                    .Include(x => x.Moneda)
                    .Include(x => x.Usuario)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(parametros.cliente))
                    precios = precios.Where(x => x.Cliente != null && !string.IsNullOrEmpty(x.Cliente.Den) && x.Cliente.Den.ToLower().Contains(parametros.cliente.ToLower()));
                if (!string.IsNullOrEmpty(parametros.producto))
                    precios = precios.Where(x => x.Producto != null && !string.IsNullOrEmpty(x.Producto.Den) && x.Producto.Den.ToLower().Contains(parametros.producto.ToLower()));
                if (!string.IsNullOrEmpty(parametros.destino))
                    precios = precios.Where(x => x.Destino != null && !string.IsNullOrEmpty(x.Destino.Den) && x.Destino.Den.ToLower().Contains(parametros.destino.ToLower()));
                if (!string.IsNullOrEmpty(parametros.zona))
                    precios = precios.Where(x => x.Zona != null && !string.IsNullOrEmpty(x.Zona.Nombre) && x.Zona.Nombre.ToLower().Contains(parametros.zona.ToLower()));

                await HttpContext.InsertarParametrosPaginacion(precios, parametros.tamanopagina, parametros.pagina);

                if (HttpContext.Response.Headers.ContainsKey("pagina"))
                {
                    var pagina = HttpContext.Response.Headers["pagina"];
                    if (pagina != parametros.pagina && !string.IsNullOrEmpty(pagina))
                    {
                        parametros.pagina = int.Parse(pagina!);
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

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Precio precio)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var acc = 0;
                precio.Producto = null!;
                precio.Zona = null!;
                precio.Destino = null!;
                precio.Cliente = null!;
                precio.FchActualizacion = DateTime.Now;
                precio.Terminal = null!;

                if (precio.Cod != null)
                {
                    context.Update(precio);
                    acc = 6;
                }
                else
                {
                    if (context.Precio.Any(x => x.CodDes == precio.CodDes && x.CodCte == precio.CodCte && x.CodPrd == precio.CodPrd && x.CodZona == precio.CodZona && x.Id_Tad == id_terminal))
                        return BadRequest("El destino ya cuenta con un precio asignado para ese producto.");

                    precio.Id_Tad = id_terminal;

                    context.Add(precio);
                    acc = 3;
                }
                var precioH = new PrecioHistorico
                {
                    Cod = null!,
                    pre = precio.Pre,
                    CodCte = precio.CodCte,
                    CodDes = precio.CodDes,
                    CodGru = precio.CodGru,
                    CodPrd = precio.CodPrd,
                    CodZona = precio.CodZona,
                    FchDia = precio.FchDia,
                    FchActualizacion = precio.FchActualizacion,
                    ID_Moneda = precio.ID_Moneda,
                    Equibalencia = precio.Equibalencia,
                    Id_Tad = id_terminal
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
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                List<Precio> precios = new();
                List<PrecioProgramado> preciosPro = new();
                var LimiteDate = DateTime.Today.AddHours(16);

                if (!string.IsNullOrEmpty(folio))
                {
                    var ordenes = await context.OrdenCierre.Where(x => x.Folio == folio && x.Estatus == true && x.Id_Tad == id_terminal)
                            .Include(x => x.Cliente)
                            .Include(x => x.Moneda)
                            .ToListAsync();
                    var ordenesUnic = ordenes.DistinctBy(x => x.CodPrd).Select(x => x);

                    foreach (var item in ordenesUnic)
                    {
                        if (item is not null)
                        {
                            var zona = context.ZonaCliente.FirstOrDefault(x => x.CteCod == item.CodCte);
                            Precio precio = new()
                            {
                                Pre = item.Precio,
                                CodCte = item.CodCte,
                                CodDes = item.CodDes,
                                CodPrd = item.CodPrd,
                                CodGru = item.Cliente?.Codgru,
                                CodZona = zona?.CteCod,
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
                if (zonaCliente is not null)
                    precios = await context.Precio.Where(x => x.CodCte == zonaCliente.CteCod && x.CodDes == zonaCliente.DesCod && x.Activo == true && x.Id_Tad == id_terminal)
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
                        if (porcentaje is not null)
                        {
                            var aumento = (porcentaje.Porcen / 100) + 1;
                            x.Pre = x.FchDia < DateTime.Today ? Math.Round((x.Pre * aumento), 4) : Math.Round(x.Pre, 4);
                        }
                    }
                });

                if (DateTime.Now > LimiteDate &&
                    DateTime.Today.DayOfWeek != DayOfWeek.Saturday &&
                    DateTime.Today.DayOfWeek != DayOfWeek.Sunday)
                {
                    if (zonaCliente is not null)
                        preciosPro = await context.PrecioProgramado.Where(x => x.CodCte == zonaCliente.CteCod && x.CodDes == zonaCliente.DesCod && x.Activo == true && x.Id_Tad == id_terminal)
                        //&& x.codZona == zona.ZonaCod)
                        .Include(x => x.Producto)
                        .ToListAsync();

                    foreach (var item in preciosPro)
                    {
                        if (item.FchDia > DateTime.Today)
                        {
                            var pre = precios.FirstOrDefault(x => x.CodDes == item.CodDes && x.CodCte == item.CodCte && x.CodPrd == item.CodPrd);
                            if (pre is null)
                            {
                                precios.Add(new Precio()
                                {
                                    Pre = item.Pre,
                                    CodCte = item.CodCte,
                                    CodDes = item.CodDes,
                                    CodPrd = item.CodPrd,
                                    CodGru = item.Cliente?.Codgru,
                                    Producto = context.Producto.FirstOrDefault(x => x.Cod == item.CodPrd)
                                });
                            }
                            else
                            {
                                precios.First(x => x.CodDes == item.CodDes && x.CodCte == item.CodCte && x.CodPrd == item.CodPrd).Pre = item.Pre;
                                precios.First(x => x.CodDes == item.CodDes && x.CodCte == item.CodCte && x.CodPrd == item.CodPrd).FchDia = item.FchDia;
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
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var id = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                var user = await userManager.FindByIdAsync(id);

                if (user is null)
                    return NotFound();

                var user_system = context.Usuario.Find(user.UserCod);
                if (user_system is null)
                    return NotFound();

                List<PrecioProgramado> prec = new();
                foreach (var item in precios)
                {
                    //Debug.WriteLine($"Destino: {item.Destino}, count :{precios.IndexOf(item)}");
                    var codcte = string.IsNullOrEmpty(item.Cliente) ? string.Empty : item.Cliente;
                    var cliente = context.Cliente.FirstOrDefault(x => x.Den!.Replace("\"", "").Equals(codcte) && x.Id_Tad == id_terminal);
                    if (cliente is null)
                        return BadRequest($"No se encontro el cliente {item.Cliente}");

                    if (!context.Cliente_Tad.Any(x => x.Id_Cliente == cliente.Cod && x.Id_Terminal == id_terminal))
                        return BadRequest($"No se encontro el cliente {item.Cliente} en la terminal");

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

                    Destino? destino = new();

                    if (!string.IsNullOrEmpty(item.CodSyn) || !string.IsNullOrWhiteSpace(item.CodSyn))
                    {
                        destino = context.Destino.FirstOrDefault(x => x.Codsyn == item.CodSyn);
                        if (destino is null)
                            return BadRequest($"No se encontro el destino {item.Destino} synthesis:{item.CodSyn} tuxpan {item.CodTux}");
                    }
                    else
                    {
                        //if (string.IsNullOrEmpty(item.CodDestinoGobierno) || string.IsNullOrWhiteSpace(item.CodDestinoGobierno))
                        //    return BadRequest("No se admiten destinos sin identificadores");
                        if (!string.IsNullOrEmpty(item.CodDestinoGobierno) || !string.IsNullOrWhiteSpace(item.CodDestinoGobierno))
                        {
                            destino = context.Destino.FirstOrDefault(x => x.Id_DestinoGobierno == item.CodDestinoGobierno && x.Id_Tad == id_terminal && x.Codcte == cliente.Cod);
                            if (destino is null)
                                return BadRequest($"No se encontro el destino {item.Destino}. Id gobierno: {item.CodDestinoGobierno}");
                        }
                        else
                        {
                            destino = context.Destino.FirstOrDefault(x => x.Den == item.Destino && x.Id_Tad == id_terminal && x.Codcte == cliente.Cod);
                            if (destino is null)
                                return BadRequest($"No se encontro el destino {item.Destino} synthesis:{item.CodSyn} tuxpan {item.CodTux}");
                        }

                    }

                    if (!context.Destino_Tad.Any(x => x.Id_Destino == destino.Cod && x.Id_Terminal == id_terminal))
                        return BadRequest($"No se encontro el destino {item.Destino} en la terminal. synthesis:{item.CodSyn} tuxpan: {item.CodTux}");

                    if (item?.Precio == null || item.Precio == 0)
                        return BadRequest($"El destino {destino.Den} no tiene un precio con valor");

                    if (string.IsNullOrEmpty(item.Fecha))
                        return BadRequest("No se admiten valores vacios en la fecha de vigencia");

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
                            CodCte = cliente.Cod,
                            CodDes = destino.Cod,
                            CodGru = cliente.Codgru,
                            CodPrd = producto.Cod,
                            CodZona = zona?.Cod,
                            FchDia = DateTime.Parse(item.Fecha),
                            FchActualizacion = DateTime.Now,
                            Pre = item.Precio,
                            ID_Moneda = moneda?.Id,
                            Equibalencia = (double)item.Equibalencia,
                            ID_Usuario = user_system.Cod,
                            Id_Tad = id_terminal
                        };

                        var p = context.PrecioProgramado.IgnoreAutoIncludes().FirstOrDefault(x => x.CodGru == precio.CodGru
                        //&& x.codZona == precio.codZona
                        && x.CodCte == precio.CodCte
                        && x.CodPrd == precio.CodPrd
                        && x.CodDes == precio.CodDes
                        && x.Id_Tad == id_terminal);

                        if (p is not null)
                        {
                            p.Pre = precio.Pre;
                            p.FchDia = precio.FchDia;
                            p.FchActualizacion = DateTime.Now;
                            p.ID_Moneda = precio.ID_Moneda;
                            p.Equibalencia = precio.Equibalencia;
                            p.ID_Usuario = precio.ID_Usuario;
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
                            CodCte = cliente.Cod,
                            CodDes = destino.Cod,
                            CodGru = cliente.Codgru,
                            CodPrd = producto.Cod,
                            CodZona = zona?.Cod,
                            FchDia = DateTime.Parse(item.Fecha),
                            FchActualizacion = DateTime.Now,
                            Pre = item.Precio,
                            ID_Moneda = moneda?.Id,
                            Equibalencia = (double)item.Equibalencia,
                            ID_Usuario = user_system.Cod,
                            Id_Tad = id_terminal
                        };

                        var p = context.Precio.IgnoreAutoIncludes().FirstOrDefault(x => x.CodGru == precio.CodGru
                        //&& x.codZona == precio.codZona
                        && x.CodCte == precio.CodCte
                        && x.CodPrd == precio.CodPrd
                        && x.CodDes == precio.CodDes
                        && x.Id_Tad == id_terminal);

                        if (p is not null)
                        {
                            p.Pre = precio.Pre;
                            p.FchActualizacion = DateTime.Now;
                            p.ID_Moneda = precio.ID_Moneda;
                            p.Equibalencia = precio.Equibalencia;
                            p.FchDia = precio.FchDia;
                            p.ID_Usuario = precio.ID_Usuario;
                            context.Update(p);
                        }
                        else
                            context.Add(precio);

                        var precioH = new PrecioHistorico
                        {
                            Cod = null!,
                            pre = precio.Pre,
                            CodCte = precio.CodCte == null ? 0 : precio.CodCte,
                            CodDes = precio.CodDes == null ? 0 : precio.CodDes!,
                            CodGru = precio.CodGru == null ? (short)0 : (short)precio.CodGru!,
                            CodPrd = precio.CodPrd == null ? 0 : precio.CodPrd,
                            CodZona = precio.CodZona == null ? 0 : precio.CodZona,
                            FchDia = precio.FchDia,
                            FchActualizacion = precio.FchActualizacion,
                            ID_Moneda = precio?.ID_Moneda,
                            Equibalencia = precio?.Equibalencia,
                            ID_Usuario = user_system.Cod,
                            Id_Tad = id_terminal
                        };

                        context.Add(precioH);
                    }
                }

                await context.SaveChangesAsync(id, 8);

                List<Destino> destinos = new();
                List<PreciosDTO> destinosSinPre = new();
                destinos = context.Destino.Where(x => x.Id_Tad == id_terminal).ToList();
                foreach (var item in destinos)
                    if (!context.PrecioProgramado.Any(x => x.CodDes == item.Cod && x.Id_Tad == id_terminal))
                    {
                        PreciosDTO dTO = new()
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
        [HttpGet("historial")]
        public async Task<ActionResult> GetDateHistorialPrecio([FromQuery] ParametrosBusquedaPrecios parametros)
        {
            try
            {

                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var precios = context.PreciosHistorico
                    .AsNoTracking()
                    .Where(x => x.FchDia >= parametros.Fecha_Inicio && x.FchDia <= parametros.Fecha_Fin && x.Id_Tad == id_terminal)
                   .Include(x => x.Destino)
                   .Include(x => x.Cliente)
                   .Include(x => x.Terminal)
                   .Include(x => x.Producto)
                   .Include(x => x.Zona)
                   .Include(x => x.Moneda)
                   .Include(x => x.Usuario)
                   .OrderBy(x => x.FchDia)
                   .Select(item => new HistorialPrecioDTO()
                   {
                       Fecha = item.FchDia.ToString("dd/MM/yyyy"),
                       Pre = item.pre,
                       PrecioCompra = item.Precio_Compra,
                       Producto = item.Producto!.Den,
                       Destino = item.Destino!.Den,
                       Zona = item.Zona!.Nombre,
                       Moneda = item.Moneda!.Nombre,
                       Cliente = item.Cliente!.Den,
                       Usuario = item.Usuario!.Den,
                       Fecha_De_Subida = item.FchActualizacion.ToString(),
                       Unidad_Negocio = item.Terminal!.Den,
                       Cambio = item.Equibalencia
                   })
                   .AsQueryable();

                if (!string.IsNullOrEmpty(parametros.cliente))
                    precios = precios.Where(x => x.Cliente != null && !string.IsNullOrEmpty(x.Cliente) && x.Cliente.ToLower().Contains(parametros.cliente.ToLower()));
                if (!string.IsNullOrEmpty(parametros.producto))
                    precios = precios.Where(x => x.Producto != null && !string.IsNullOrEmpty(x.Producto) && x.Producto.ToLower().Contains(parametros.producto.ToLower()));
                if (!string.IsNullOrEmpty(parametros.destino))
                    precios = precios.Where(x => x.Destino != null && !string.IsNullOrEmpty(x.Destino) && x.Destino.ToLower().Contains(parametros.destino.ToLower()));
                if (!string.IsNullOrEmpty(parametros.zona))
                    precios = precios.Where(x => x.Zona != null && !string.IsNullOrEmpty(x.Zona) && x.Zona.ToLower().Contains(parametros.zona.ToLower()));

                if (parametros.Excel)
                {
                    ExcelPackage.LicenseContext = LicenseContext.Commercial;
                    ExcelPackage excel = new();
                    ExcelWorksheet ws = excel.Workbook.Worksheets.Add("Inventarios");
                    ws.Cells["A1"].LoadFromCollection(precios, op =>
                    {
                        op.TableStyle = TableStyles.Medium2;
                        op.PrintHeaders = true;
                    });

                    ws.Cells[1, 2, ws.Dimension.End.Row, 3].Style.Numberformat.Format = "#,##0.00";
                    ws.Cells[1, 1, ws.Dimension.End.Row, 1].Style.Numberformat.Format = "dd/MM/yyyy";

                    ws.Cells[1, 1, ws.Dimension.End.Row, ws.Dimension.End.Column].AutoFitColumns();
                    return Ok(excel.GetAsByteArray());
                }

                await HttpContext.InsertarParametrosPaginacion(precios, parametros.tamanopagina, parametros.pagina);

                parametros.Pagina = HttpContext.ObtenerPagina();

                precios = precios.Skip((parametros.Pagina - 1) * parametros.Total_registros).Take(parametros.Total_registros);

                return Ok(precios);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("programado")]//no modificar
        public async Task<ActionResult> GetDateHistorialPrecioProgramado()
        {
            try
            {
                List<Precio> preciosDia = new();
                List<PrecioHistorico> precioHistoricos = new();

                List<PrecioProgramado> precios = new();
                precios = context.PrecioProgramado.Where(x => x.FchDia == DateTime.Today).ToList();

                if (precios.Count > 0)
                {
                    foreach (var item in precios)
                    {
                        if (item is not null)
                        {
                            var precio = context.Precio.FirstOrDefault(x => x.CodCte == item.CodCte && x.CodDes == item.CodDes && x.CodPrd == item.CodPrd && x.Activo == true);
                            if (precio is null)
                            {
                                var precioN = new Precio
                                {
                                    CodCte = item.CodCte,
                                    CodDes = item.CodDes,
                                    CodGru = item?.CodGru,
                                    CodPrd = item?.CodPrd,
                                    CodZona = item?.CodZona,
                                    FchDia = item?.FchDia ?? DateTime.MinValue,
                                    FchActualizacion = DateTime.Now,
                                    Pre = item?.Pre ?? 0,
                                    Equibalencia = item?.Equibalencia,
                                    ID_Moneda = item?.ID_Moneda,
                                    ID_Usuario = item?.ID_Usuario,
                                    Id_Tad = item?.Id_Tad
                                };
                                preciosDia.Add(precioN);
                                //context.Add(precioN);
                            }
                            else
                            {
                                precio.Pre = item.Pre;
                                precio.FchDia = item.FchDia;
                                precio.FchActualizacion = DateTime.Now;
                                precio.Equibalencia = item.Equibalencia;
                                precio.ID_Moneda = item.ID_Moneda;
                                precio.ID_Usuario = item.ID_Usuario;
                                context.Update(precio);
                            }

                            var precioH = new PrecioHistorico
                            {
                                Cod = null!,
                                pre = item!.Pre,
                                CodCte = item.CodCte,
                                CodDes = item.CodDes,
                                CodGru = item?.CodGru,
                                CodPrd = item?.CodPrd,
                                CodZona = item?.CodZona,
                                FchDia = item!.FchDia,
                                FchActualizacion = item.FchActualizacion,
                                Equibalencia = item?.Equibalencia,
                                ID_Moneda = item?.ID_Moneda,
                                ID_Usuario = item?.ID_Usuario,
                                Id_Tad = item?.Id_Tad
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
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("programados/lista")]//TODO: checar utilidad
        public async Task<ActionResult> GetPreciosProgramados()
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var precios = await context.PrecioProgramado
                    .Where(x => x.FchDia > DateTime.Today && x.Id_Tad == id_terminal)
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

        [HttpPost("programado")]//TODO: checar utilidad
        public async Task<ActionResult> PostProgramado([FromBody] Precio precio)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                precio.Producto = null!;
                precio.Zona = null!;
                precio.Destino = null!;
                precio.Cliente = null!;
                precio.FchActualizacion = DateTime.Now;
                precio.Terminal = null!;

                var precioPro = new PrecioProgramado
                {
                    Cod = precio.Cod,
                    CodCte = precio.CodCte,
                    CodDes = precio.CodDes,
                    CodGru = precio.CodGru,
                    CodPrd = precio.CodPrd,
                    CodZona = precio.CodZona,
                    FchDia = precio.FchDia,
                    FchActualizacion = DateTime.Now,
                    Pre = precio.Pre,
                    Activo = precio.Activo,
                    Equibalencia = precio.Equibalencia,
                    ID_Moneda = precio.ID_Moneda,
                    Id_Tad = id_terminal
                };

                if (precio.Cod != null)
                    context.Update(precioPro);
                else
                {
                    if (context.Precio.Any(x => x.CodDes == precio.CodDes && x.CodCte == precio.CodCte && x.CodPrd == precio.CodPrd && x.FchDia == precio.FchDia && x.CodZona == precio.CodZona && x.Id_Tad == id_terminal))
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

        [HttpGet("bol/{BOL}")]
        public ActionResult GetPrecioByBol([FromRoute] int BOL)
        {
            try
            {
                PrecioBolDTO precios = new();

                var ordenes = context.Orden.Where(x => x.BatchId == BOL)
                    .Include(x => x.Producto)
                    .Include(x => x.Destino)
                    .FirstOrDefault();

                if (ordenes is null)
                    return Ok(new PrecioBolDTO());

                PrecioBolDTO precio = new();

                OrdenEmbarque? orden = new();
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

                var precioVig = context.Precio.Where(x => ordenes != null && x.CodDes == ordenes.Coddes && x.CodPrd == ordenes.Codprd)
                    .OrderByDescending(x => x.FchDia)
                    .FirstOrDefault();

                var precioPro = context.PrecioProgramado.Where(x => ordenes != null && x.CodDes == ordenes.Coddes && x.CodPrd == ordenes.Codprd)
                    .OrderByDescending(x => x.FchDia)
                    .FirstOrDefault();

                var precioHis = context.PreciosHistorico.Where(x => ordenes != null && x.CodDes == ordenes.Coddes && x.CodPrd == ordenes.Codprd
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
                            precio.Moneda = cierre?.Moneda?.Nombre ?? "MXN";
                            precio.Tipo_De_Cambio = cierre?.Equibalencia ?? 1;
                            precio.Folio_Cierre = cierre.Folio ?? string.Empty;
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

        [HttpGet("{Orden_Compra}")]
        public ActionResult GetPrecioByEner([FromRoute] int Orden_Compra, Int16 Id_Terminal = 1)
        {
            try
            {
                List<PrecioBol> precios = new();

                var ordenes = context.OrdenEmbarque.IgnoreAutoIncludes().Where(x => x.Folio == Orden_Compra && x.Codtad == Id_Terminal)
                    .Include(x => x.Producto)
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Tad)
                    .ToList();

                if (ordenes is null)
                    return Ok(new List<PrecioBol>() { new PrecioBol() });

                foreach (var item in ordenes)
                {
                    PrecioBol precio = new();

                    Orden? orden = new();
                    orden = context.Orden.IgnoreAutoIncludes().Where(x => x.Ref == item.FolioSyn).Include(x => x.Producto).Include(x => x.Destino).ThenInclude(x => x.Cliente).Include(x => x.Terminal).FirstOrDefault();

                    precio.Fecha_De_Carga = orden?.Fchcar ?? item.Fchcar;

                    precio.Referencia = orden?.Ref ?? item.FolioSyn;

                    if (orden is not null)
                    {
                        if (orden.Producto is not null)
                            precio.Producto_Synthesis = orden.Producto.Den ?? string.Empty;

                        if (orden.Destino is not null)
                            precio.Destino_Synthesis = orden.Destino.Den ?? string.Empty;

                        if (orden.Terminal is not null)
                            if (!string.IsNullOrEmpty(orden.Terminal.Den))
                            {
                                precio.Terminal_Final = orden.Terminal.Den;
                                if (!string.IsNullOrEmpty(orden.Terminal.Codigo))
                                    precio.Codigo_Terminal_Final = orden.Terminal.Codigo;
                            }

                        precio.BOL = orden.BatchId ?? 0;
                        precio.Volumen_Cargado = orden.Vol;
                    }

                    if (item is not null)
                    {
                        if (item.Destino is not null)
                        {
                            precio.Destino_Original = item.Destino.Den;
                            if (item.Destino.Cliente is not null)
                                if (!string.IsNullOrEmpty(item.Destino.Cliente.Den))
                                    precio.Cliente_Original = item.Destino.Cliente.Den;

                        }

                        if (item.Producto is not null)
                            if (!string.IsNullOrEmpty(item.Producto.Den))
                                precio.Producto_Original = item.Producto.Den;

                        if (item.Tad is not null)
                        {
                            if (!string.IsNullOrEmpty(item.Tad.Den))
                                precio.Terminal_Original = item.Tad.Den;

                            if (!string.IsNullOrEmpty(item.Tad.Codigo))
                                precio.Codigo_Terminal_Original = item.Tad.Codigo;
                        }
                    }

                    var precioVig = context.Precio.IgnoreAutoIncludes()
                        .Where(x => item != null && x.CodDes == item.Coddes && x.CodPrd == item.Codprd && x.Id_Tad == item.Codtad)
                        .OrderByDescending(x => x.FchActualizacion).FirstOrDefault();

                    if (orden is not null)
                        precioVig = context.Precio.IgnoreAutoIncludes()
                        .Where(x => x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd && x.Id_Tad == orden.Id_Tad)
                        .OrderByDescending(x => x.FchActualizacion).FirstOrDefault();

                    var precioPro = context.PrecioProgramado.IgnoreAutoIncludes()
                        .Where(x => item != null && x.CodDes == item.Coddes && x.CodPrd == item.Codprd && x.Id_Tad == item.Codtad)
                        .OrderByDescending(x => x.FchActualizacion).FirstOrDefault();

                    if (orden is not null)
                        precioPro = context.PrecioProgramado.IgnoreAutoIncludes()
                        .Where(x => x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd && x.Id_Tad == orden.Id_Tad)
                        .OrderByDescending(x => x.FchActualizacion).FirstOrDefault();

                    var precioHis = context.PreciosHistorico.IgnoreAutoIncludes()
                        .Where(x => item != null && x.CodDes == item.Coddes && x.CodPrd == item.Codprd && x.FchDia <= DateTime.Today && x.Id_Tad == item.Codtad)
                        .OrderByDescending(x => x.FchActualizacion)
                        .FirstOrDefault();

                    if (orden is not null)
                        precioHis = context.PreciosHistorico.IgnoreAutoIncludes()
                        .Where(x => x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd && orden.Fchcar != null && x.FchDia <= orden.Fchcar.Value.Date && x.Id_Tad == orden.Id_Tad)
                        .OrderByDescending(x => x.FchDia)
                        .FirstOrDefault();

                    if (precioHis is not null)
                    {
                        precio.Precio = precioHis.pre;
                        precio.Fecha_De_Precio = precioHis.FchDia;
                        precio.Precio_Encontrado = true;
                        precio.Precio_Encontrado_En = "Historial";
                        precio.Moneda = precioHis?.Moneda?.Nombre ?? "MXN";
                        precio.Tipo_De_Cambio = precioHis?.Equibalencia ?? 1;
                    }

                    if (item != null && precioVig is not null && orden is null || orden is not null && precioVig is not null)
                    {
                        if (precioVig.FchDia == DateTime.Today)
                        {
                            precio.Precio = precioVig.Pre;
                            precio.Fecha_De_Precio = precioVig.FchDia;
                            precio.Precio_Encontrado = true;
                            precio.Precio_Encontrado_En = "Vigente";
                            precio.Moneda = precioHis?.Moneda?.Nombre ?? "MXN";
                            precio.Tipo_De_Cambio = precioHis?.Equibalencia ?? 1;
                        }
                    }

                    if (item != null && precioPro is not null && context.PrecioProgramado.Any() || orden is not null && precioPro is not null && context.PrecioProgramado.Any())
                    {
                        if (precioPro.FchDia == DateTime.Today)
                        {
                            precio.Precio = precioPro.Pre;
                            precio.Fecha_De_Precio = precioPro.FchDia;
                            precio.Precio_Encontrado = true;
                            precio.Precio_Encontrado_En = "Programado";
                            precio.Moneda = precioHis?.Moneda?.Nombre ?? "MXN";
                            precio.Tipo_De_Cambio = precioHis?.Equibalencia ?? 1;
                        }
                    }

                    if (item != null && context.OrdenPedido.Any(x => x.CodPed == item.Cod && x.Pedido_Original == 0 && string.IsNullOrEmpty(x.Folio_Cierre_Copia)))
                    {
                        var ordenepedido = context.OrdenPedido.Where(x => x.CodPed == item.Cod && !string.IsNullOrEmpty(x.Folio) && x.Pedido_Original == 0 && string.IsNullOrEmpty(x.Folio_Cierre_Copia)).FirstOrDefault();

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
                                precio.Moneda = precioHis?.Moneda?.Nombre ?? "MXN";
                                precio.Tipo_De_Cambio = precioHis?.Equibalencia ?? 1;
                                precio.Folio_Cierre = cierre.Folio ?? string.Empty;
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

        [HttpGet("modificar/orden")]// no modificar
        public async Task<ActionResult> Modificar_Precio_De_Orden()
        {
            try
            {
                if (DateTime.Now >= DateTime.Today.AddHours(23).AddMinutes(45) && DateTime.Now <= DateTime.Today.AddDays(1))
                {
                    List<OrdenEmbarque> Ordenes = context.OrdenEmbarque.Where(x => x.Orden == null && x.Fchcar <= DateTime.Today && x.Fchcar >= DateTime.Today.AddDays(-5) && x.Codest != 14)
                        .Include(x => x.Orden)
                        .IgnoreAutoIncludes()
                        .ToList();

                    foreach (var item in Ordenes)
                    {
                        item.Pre = Obtener_Precio_Del_Dia_De_Orden(item.Cod, item.Codtad).Precio;
                        context.Update(item);
                    }
                    await context.SaveChangesAsync();
                }
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("destino/{Id_Destino}/{Id_Orden}")]
        public async Task<ActionResult> Get_Precio_Destino(int Id_Destino, long Id_Orden)
        {
            try
            {
                PrecioBolDTO precios = new();

                var destino = await context.Destino.FindAsync(Id_Destino);
                var orden = await context.Orden.IgnoreAutoIncludes().Include(x => x.OrdenEmbarque).FirstOrDefaultAsync(x => x.Cod == Id_Orden);

                if (destino is null || orden is null)
                    return Ok(new PrecioBolDTO());

                PrecioBolDTO precio = new();

                var precioVig = await context.Precio.Where(x => x.CodDes == destino.Cod && x.CodPrd == orden.Codprd && x.Id_Tad == orden.Id_Tad)
                    .OrderByDescending(x => x.FchDia)
                    .FirstOrDefaultAsync();

                var precioPro = context.PrecioProgramado.Where(x => x.CodDes == destino.Cod && x.CodPrd == orden.Codprd && x.Id_Tad == orden.Id_Tad)
                    .OrderByDescending(x => x.FchDia)
                    .FirstOrDefault();

                var precioHis = context.PreciosHistorico.Where(x => x.CodDes == destino.Cod && x.CodPrd == orden.Codprd
                    && orden.Fchcar != null && x.FchDia <= orden.Fchcar.Value.Date && x.Id_Tad == orden.Id_Tad)
                    .OrderByDescending(x => x.FchDia)
                    .FirstOrDefault();

                if (precioHis is not null)
                    precio.Precio = precioHis.pre;

                if (precioVig is not null && orden.Fchcar is not null && orden.Fchcar.Value.Date == DateTime.Today)
                    precio.Precio = precioVig.Pre;

                if (precioPro is not null && orden.Fchcar is not null && orden.Fchcar.Value.Date == DateTime.Today && context.PrecioProgramado.Any())
                    precio.Precio = precioPro.Pre;

                if (context.OrdenPedido.Any(x => x.CodPed == orden.Cod))
                {
                    var ordenepedido = context.OrdenPedido.Where(x => x.CodPed == orden.Cod && !string.IsNullOrEmpty(x.Folio)).FirstOrDefault();

                    if (ordenepedido is not null)
                    {
                        var cierre = context.OrdenCierre.Where(x => x.Folio == ordenepedido.Folio
                         && x.CodPrd == orden.Codprd).FirstOrDefault();

                        if (cierre is not null)
                            precio.Precio = cierre.Precio;
                    }
                }

                if (precioHis is null && precioPro is null && precioVig is null && !precio.Es_Cierre)
                    precio.Precio = orden.OrdenEmbarque?.Pre;

                precio.Moneda = !string.IsNullOrEmpty(precio.Moneda) ? precio.Moneda : "MXN";

                return Ok(precio);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        private PrecioBolDTO Obtener_Precio_Del_Dia_De_Orden(int Id, short? id_terminal)
        {
            try
            {
                var orden = context.OrdenEmbarque.Where(x => x.Cod == Id && x.Codtad == id_terminal)
                    .Include(x => x.Orden)
                    .Include(x => x.OrdenCierre)
                    .IgnoreAutoIncludes()
                    .FirstOrDefault();

                if (orden is null)
                    return new PrecioBolDTO();

                PrecioBolDTO precio = new();

                var precioVig = context.Precio.Where(x => orden != null && x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd && x.Id_Tad == id_terminal)
                    .OrderByDescending(x => x.FchActualizacion)
                    .FirstOrDefault();

                if (orden.Orden is not null)
                    precioVig = context.Precio.Where(x => orden.Orden != null && x.CodDes == orden.Orden.Coddes && x.CodPrd == orden.Orden.Codprd && x.Id_Tad == id_terminal)
                    .OrderByDescending(x => x.FchActualizacion)
                    .FirstOrDefault();

                var precioPro = context.PrecioProgramado.Where(x => orden != null && x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd && x.Id_Tad == id_terminal)
                    .OrderByDescending(x => x.FchActualizacion)
                    .FirstOrDefault();

                if (orden.Orden is not null)
                    precioPro = context.PrecioProgramado.Where(x => orden.Orden != null && x.CodDes == orden.Orden.Coddes && x.CodPrd == orden.Orden.Codprd && x.Id_Tad == id_terminal)
                    .OrderByDescending(x => x.FchActualizacion)
                    .FirstOrDefault();

                var precioHis = context.PreciosHistorico.Where(x => orden != null && x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd
                    && x.FchDia <= DateTime.Today && x.Id_Tad == id_terminal)
                    .OrderByDescending(x => x.FchActualizacion)
                    .FirstOrDefault();

                if (orden.Orden is not null)
                    context.PreciosHistorico.Where(x => orden.Orden != null && x.CodDes == orden.Orden.Coddes && x.CodPrd == orden.Orden.Codprd
                    && x.FchDia <= orden.Orden.Fchcar && x.Id_Tad == id_terminal)
                    .OrderByDescending(x => x.FchActualizacion)
                    .FirstOrDefault();

                if (precioHis is not null)
                    precio.Precio = precioHis.pre;

                if (orden != null && precioVig is not null && precioVig.FchDia == DateTime.Today)
                    precio.Precio = precioVig.Pre;

                if (orden != null && precioPro is not null && (precioPro.FchDia == DateTime.Today || DateTime.Now.TimeOfDay >= new TimeSpan(23, 0, 0)) && context.PrecioProgramado.Any())
                    precio.Precio = precioPro.Pre;

                if (orden != null && context.OrdenPedido.Any(x => x.CodPed == orden.Cod))
                {
                    var ordenepedido = context.OrdenPedido.Where(x => x.CodPed == orden.Cod && !string.IsNullOrEmpty(x.Folio)).FirstOrDefault();

                    if (ordenepedido is not null)
                    {
                        var cierre = context.OrdenCierre.Where(x => x.Folio == ordenepedido.Folio
                         && x.CodPrd == orden.Codprd).FirstOrDefault();

                        if (cierre is not null)
                            precio.Precio = cierre.Precio;
                    }
                }

                if (orden is not null && precioHis is null && precioPro is null && precioVig is null && !precio.Es_Cierre)
                {
                    precio.Precio = orden.Pre;

                    if (orden.OrdenCierre is not null)
                        precio.Fecha_De_Precio = orden.OrdenCierre.fchPrecio;
                }

                return precio;
            }
            catch (Exception)
            {
                return new PrecioBolDTO();
            }
        }

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
            public string Folio_Cierre { get; set; } = string.Empty;
            public string Terminal_Original { get; set; } = string.Empty;
            public string Codigo_Terminal_Original { get; set; } = string.Empty;
            public string Terminal_Final { get; set; } = string.Empty;
            public string Codigo_Terminal_Final { get; set; } = string.Empty;
        }
    }
}