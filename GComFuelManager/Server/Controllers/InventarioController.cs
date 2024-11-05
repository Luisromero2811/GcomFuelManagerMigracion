﻿using AutoMapper;
using FluentValidation;
using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Enums;
using GComFuelManager.Shared.Extensiones;
using GComFuelManager.Shared.ModelDTOs;
using GComFuelManager.Shared.Modelos;
using GComFuelManager.Shared.ReportesDTO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using Org.BouncyCastle.Security;
using System.Linq.Dynamic.Core;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, Administrador Sistema, Inventarios, Abrir Cierres Inventario")]

    public class InventarioController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IValidator<InventarioPostDTO> validator;
        private readonly IMapper mapper;
        private readonly User_Terminal terminal;
        private readonly VerifyUserId verifyUser;
        private readonly UserManager<IdentityUsuario> userManager;

        public InventarioController(ApplicationDbContext context, IValidator<InventarioPostDTO> validator, IMapper mapper,
                                    User_Terminal _terminal, VerifyUserId verifyUser, UserManager<IdentityUsuario> userManager)
        {
            this.context = context;
            this.validator = validator;
            this.mapper = mapper;
            terminal = _terminal;
            this.verifyUser = verifyUser;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] InventarioDTO inventario)
        {
            try
            {
                var id_terminal = terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var inventarios = context.Inventarios
                    .AsNoTracking()
                    .Where(x => x.Activo && x.TadId.Equals(id_terminal))
                    .Include(x => x.Producto)
                    .Include(x => x.Sitio)
                    .Include(x => x.Almacen)
                    .Include(x => x.Localidad)
                    .Include(x => x.UnidadMedida)
                    .Include(x => x.TipoMovimiento)
                    .Include(x => x.Transportista)
                    .Include(x => x.Tonel)
                    .Include(x => x.OrigenDestino)
                    .OrderByDescending(x => x.FechaRegistro)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(inventario.Producto) && !string.IsNullOrWhiteSpace(inventario.Producto))
                    inventarios = inventarios.Where(x => !string.IsNullOrEmpty(x.Producto.Den) && x.Producto.Den.ToLower().Contains(inventario.Producto.ToLower()));

                if (!string.IsNullOrEmpty(inventario.Sitio) && !string.IsNullOrWhiteSpace(inventario.Sitio))
                    inventarios = inventarios.Where(x => x.Sitio.Valor.ToLower().Contains(inventario.Sitio.ToLower()));

                if (!string.IsNullOrEmpty(inventario.Almacen) && !string.IsNullOrWhiteSpace(inventario.Almacen))
                    inventarios = inventarios.Where(x => x.Almacen.Valor.ToLower().Contains(inventario.Almacen.ToLower()));

                if (!string.IsNullOrEmpty(inventario.Localidad) && !string.IsNullOrWhiteSpace(inventario.Localidad))
                    inventarios = inventarios.Where(x => x.Localidad.Valor.ToLower().Contains(inventario.Localidad.ToLower()));

                if (!string.IsNullOrEmpty(inventario.UnidadMedida) && !string.IsNullOrWhiteSpace(inventario.UnidadMedida))
                    inventarios = inventarios.Where(x => x.UnidadMedida.Valor.ToLower().Contains(inventario.UnidadMedida.ToLower()));

                if (!string.IsNullOrEmpty(inventario.TipoMovimiento) && !string.IsNullOrWhiteSpace(inventario.TipoMovimiento))
                    inventarios = inventarios.Where(x => x.TipoMovimiento.Valor.ToLower().Contains(inventario.TipoMovimiento.ToLower()));

                if (!inventario.ProductoId.IsZero())
                    inventarios = inventarios.Where(x => x.ProductoId == inventario.ProductoId);

                if (!inventario.SitioId.IsZero())
                    inventarios = inventarios.Where(x => x.SitioId == inventario.SitioId);

                if (!inventario.AlmacenId.IsZero())
                    inventarios = inventarios.Where(x => x.AlmacenId == inventario.AlmacenId);

                if (!inventario.LocalidadId.IsZero())
                    inventarios = inventarios.Where(x => x.LocalidadId == inventario.LocalidadId);

                if (!inventario.UnidadMedidaId.IsZero())
                    inventarios = inventarios.Where(x => x.UnidadMedidaId == inventario.UnidadMedidaId);

                if (!inventario.TipoMovimientoId.IsZero())
                    inventarios = inventarios.Where(x => x.TipoMovimientoId == inventario.TipoMovimientoId);

                if (!inventario.CierreId.IsZero())
                    inventarios = inventarios.Where(x => x.CierreId == inventario.CierreId);

                if (inventario.PorFecha)
                    inventarios = inventarios.Where(x => x.FechaRegistro >= inventario.Fecha_Inicio && x.FechaRegistro <= inventario.Fecha_Fin);

                if (inventario.FechaNULL)
                    inventarios = inventarios.Where(x => x.FechaCierre == null);

                if (!string.IsNullOrEmpty(inventario.Transportista) && !string.IsNullOrWhiteSpace(inventario.Transportista))
                    inventarios = inventarios.Where(x => x.Transportista != null && !string.IsNullOrEmpty(x.Transportista.Den) && x.Transportista.Den.ToLower().Contains(inventario.Transportista.ToLower()));

                if (!string.IsNullOrEmpty(inventario.Tonel) && !string.IsNullOrWhiteSpace(inventario.Tonel))
                    inventarios = inventarios.Where(x => x.Tonel != null && !string.IsNullOrEmpty(x.Tonel.Tracto) && x.Tonel.Tracto.ToLower().Contains(inventario.Tonel.ToLower()));

                if (inventario.Excel)
                {
                    ExcelPackage.LicenseContext = LicenseContext.Commercial;
                    ExcelPackage excel = new();
                    ExcelWorksheet ws = excel.Workbook.Worksheets.Add("Inventarios");
                    ws.Cells["A1"].LoadFromCollection(inventarios.Select(x => mapper.Map<InventarioExcelDTO>(x)), op =>
                    {
                        op.TableStyle = TableStyles.Medium2;
                        op.PrintHeaders = true;
                    });

                    ws.Cells[1, 9, ws.Dimension.End.Row, 9].Style.Numberformat.Format = "#,##0.00";
                    ws.Cells[1, 1, ws.Dimension.End.Row, 2].Style.Numberformat.Format = "dd/MM/yyyy";

                    ws.Cells[1, 1, ws.Dimension.End.Row, ws.Dimension.End.Column].AutoFitColumns();
                    return Ok(excel.GetAsByteArray());
                }

                await HttpContext.InsertarParametrosPaginacion(inventarios, inventario.Registros_por_pagina, inventario.Pagina);
                inventario.Pagina = HttpContext.ObtenerPagina();

                var inventariodto = inventarios
                    .Select(x => mapper.Map<InventarioDTO>(x))
                    .Skip((inventario.Pagina - 1) * inventario.Registros_por_pagina)
                    .Take(inventario.Registros_por_pagina);

                return Ok(inventariodto);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] InventarioPostDTO post)
        {
            try
            {
                var id_terminal = terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var result = validator.Validate(post);
                if (!result.IsValid) { return BadRequest(result.Errors.Select(x => x.ErrorMessage)); }

                var inventariodto = mapper.Map<Inventario>(post);

                inventariodto.TadId = id_terminal;

                var lts = await context.CatalogoValores.FirstOrDefaultAsync(x => x.Valor.ToLower().Equals("litro") && x.TadId == id_terminal);
                if (lts is null) return NotFound();

                var tm = await context.CatalogoValores.AsNoTracking().FirstOrDefaultAsync(x => x.Id == inventariodto.TipoMovimientoId);
                //if (tm is not null)
                //    if (int.TryParse(tm.Abreviacion, out int tipo))
                //        if (tipo >= 20)
                //            inventariodto.Cantidad *= -1;

                if (inventariodto.UnidadMedidaId == lts.Id)
                    inventariodto.CantidadLTS = inventariodto.Cantidad;
                else
                    inventariodto.CantidadLTS = inventariodto.Cantidad * 1000;

                InventarioCierre? cierre = null!;

                if (!inventariodto.CierreId.IsZero())
                {
                    cierre = await context.InventarioCierres
                        .FirstOrDefaultAsync(x => x.Id == inventariodto.CierreId && x.Activo);
                }
                else
                {
                    cierre = await context.InventarioCierres
                            .AsNoTracking()
                            .FirstOrDefaultAsync(x => x.SitioId.Equals(inventariodto.SitioId) &&
                            x.AlmacenId.Equals(inventariodto.AlmacenId) && x.SitioId.Equals(inventariodto.SitioId) &&
                            x.LocalidadId.Equals(inventariodto.LocalidadId) && x.Activo && x.Abierto && x.FechaCierre == null);
                }

                if (cierre is not null)
                    if (tm is not null)
                        if (int.TryParse(tm.Abreviacion, out int tipo))
                            if (tipo.Equals(8))
                            {
                                if (inventariodto.CantidadLTS >= cierre.PedidoTotal)
                                    return BadRequest("La cantidad solicitada no puede ser mayor al pedido total");
                            }
                            else if (tipo.Equals(9) || tipo.Equals(20))
                            {
                                if (post.TipoInventario.Equals(TipoInventario.Inicial))
                                    if (inventariodto.CantidadLTS > cierre.Fisico)
                                        return BadRequest("La cantidad solicitada no puede ser mayor a la cantidad disponible en Inventario");
                                if (post.TipoInventario.Equals(TipoInventario.FisicaReservada))
                                    if (inventariodto.CantidadLTS > cierre.Reservado)
                                        return BadRequest("La cantidad solicitada no puede ser mayor a la cantidad disponible en Fisico reservado");
                                if (post.TipoInventario.Equals(TipoInventario.OrdenReservada))
                                    if (inventariodto.CantidadLTS > cierre.OrdenReservado)
                                        return BadRequest("La cantidad solicitada no puede ser mayor a la cantidad disponible en Orden reservada");
                                if (post.TipoInventario.Equals(TipoInventario.EnOrden))
                                    if (inventariodto.CantidadLTS > cierre.EnOrden)
                                        return BadRequest("La cantidad solicitada no puede ser mayor a la cantidad disponible en En orden");
                            }
                            else if (tipo.Equals(21))
                            {
                                if (inventariodto.CantidadLTS > cierre.Fisico)
                                    return BadRequest("La cantidad solicitada no puede ser mayor a la cantidad disponible en inventario");
                            }
                            else if (tipo.Equals(22))
                            {
                                if (inventariodto.CantidadLTS > cierre.Fisico)
                                    return BadRequest("La cantidad solicitada no puede ser mayor a la cantidad disponible en inventario");
                            }
                            else if (tipo.Equals(23))
                            {
                                if (inventariodto.CantidadLTS > cierre.Fisico)
                                    return BadRequest("La cantidad solicitada no puede ser mayor a la cantidad disponible en inventario");
                            }

                if (!inventariodto.Id.IsZero())
                {
                    var inventariodb = await context.Inventarios.FindAsync(inventariodto.Id);
                    if (inventariodb is null) { return NotFound(); }

                    if (cierre is not null)
                        await RestarRegistroInventario(cierre, inventariodb);

                    var inventario = mapper.Map(inventariodto, inventariodb);

                    if (cierre is not null)
                        inventario.Cierre = cierre;

                    context.Inventarios.Attach(inventario);
                    context.Inventarios.Entry(inventario).State = EntityState.Modified;
                }
                else
                {
                    if (!post.CierreId.IsZero())
                    {
                        var cierredb = await context.InventarioCierres
                            .FirstOrDefaultAsync(x => x.Id == post.CierreId && x.Activo);
                        if (cierredb is not null)
                        {
                            if (cierredb.Abierto)
                            {
                                inventariodto.CierreId = post.CierreId;
                                inventariodto.FechaCierre = cierredb.FechaCierre;
                            }
                        }
                    }
                    else if (post.CierreId.IsZero())
                    {
                        var cierredb = await context.InventarioCierres
                            .AsNoTracking()
                            .FirstOrDefaultAsync(x => x.SitioId.Equals(inventariodto.SitioId) &&
                            x.AlmacenId.Equals(inventariodto.AlmacenId) && x.SitioId.Equals(inventariodto.SitioId) &&
                            x.LocalidadId.Equals(inventariodto.LocalidadId) && x.Activo && x.Abierto && x.FechaCierre == null);

                        if (cierredb is null)
                        {
                            var newCierre = new InventarioCierre()
                            {
                                ProductoId = inventariodto.ProductoId,
                                SitioId = inventariodto.SitioId,
                                AlmacenId = inventariodto.AlmacenId,
                                LocalidadId = inventariodto.LocalidadId,
                                FechaCierre = null,
                                TadId = id_terminal
                            };
                            inventariodto.Cierre = newCierre;
                            await context.AddAsync(newCierre);
                        }
                        else
                            inventariodto.CierreId = cierredb.Id;
                    }

                    await context.AddAsync(inventariodto);
                }

                await context.SaveChangesAsync();

                if (!inventariodto.CierreId.IsZero())
                {
                    cierre = await context.InventarioCierres.FindAsync(inventariodto.CierreId);
                    if (cierre is not null)
                    {
                        if (cierre.Abierto)
                        {
                            if (tm is not null)
                            {
                                if (int.TryParse(tm.Abreviacion, out int tipo))
                                {
                                    if (tipo.Equals(1))
                                        cierre.Fisico += inventariodto.CantidadLTS;
                                    else if (tipo.Equals(7))
                                        cierre.PedidoTotal += inventariodto.CantidadLTS;
                                    else if (tipo.Equals(8))
                                    {
                                        cierre.Fisico += inventariodto.CantidadLTS;
                                        cierre.PedidoTotal -= inventariodto.CantidadLTS;
                                    }
                                    else if (tipo.Equals(9) || tipo.Equals(20))
                                    {
                                        if (post.TipoInventario.Equals(TipoInventario.FisicaReservada))
                                            cierre.Reservado -= inventariodto.CantidadLTS;
                                        if (post.TipoInventario.Equals(TipoInventario.OrdenReservada))
                                            cierre.OrdenReservado -= inventariodto.CantidadLTS;
                                        if (post.TipoInventario.Equals(TipoInventario.EnOrden))
                                            cierre.EnOrden -= inventariodto.CantidadLTS;

                                        if (tipo.Equals(9))
                                            cierre.Fisico += inventariodto.CantidadLTS;
                                        if (tipo.Equals(20))
                                            cierre.Fisico -= inventariodto.CantidadLTS;
                                    }
                                    else if (tipo < 20)
                                        cierre.Fisico += inventariodto.CantidadLTS;
                                    else if (tipo.Equals(21))
                                        cierre.Reservado += inventariodto.CantidadLTS;
                                    else if (tipo.Equals(22))
                                        cierre.OrdenReservado += inventariodto.CantidadLTS;
                                    else if (tipo.Equals(23))
                                        cierre.EnOrden += inventariodto.CantidadLTS;
                                    else if (tipo >= 20)
                                        cierre.Fisico -= inventariodto.CantidadLTS;
                                }

                                cierre.Disponible = (cierre.Fisico - cierre.Reservado);
                                cierre.TotalDisponible = (cierre.Disponible + cierre.PedidoTotal) - (cierre.OrdenReservado + cierre.EnOrden);
                                cierre.TotalDisponibleFull = cierre.TotalDisponible.IsZero() ? 0 : cierre.TotalDisponible / 62000;

                                context.Update(cierre);
                            }
                        }
                    }

                }

                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("{Id:int}")]
        public async Task<ActionResult> Delete([FromRoute] int Id)
        {
            try
            {
                var inventario = await context.Inventarios.FindAsync(Id);
                if (inventario is null) { return NotFound(); }

                inventario.Activo = false;

                context.Update(inventario);

                var cierre = await context.InventarioCierres.FindAsync(inventario.CierreId);
                if (cierre is not null)
                {
                    await RestarRegistroInventario(cierre, inventario);
                    context.Update(cierre);
                }
                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("cierre/cerrar/{Id:int}")]
        public async Task<ActionResult> PostCierre([FromRoute] int Id)
        {
            try
            {
                var userid = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(userid))
                    return NotFound();

                var id_terminal = terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var cierre = await context.InventarioCierres.FindAsync(Id);
                if (cierre is null) return NotFound();

                cierre.TadId = id_terminal;
                cierre.UnidadMedidaId = 70;
                cierre.FechaCierre = DateTime.Today;

                var anteriorcierre = await context.InventarioCierres
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.ProductoId.Equals(cierre.ProductoId) &&
                    x.SitioId.Equals(cierre.SitioId) &&
                    x.AlmacenId.Equals(cierre.AlmacenId) &&
                    x.LocalidadId.Equals(cierre.LocalidadId) &&
                    x.FechaCierre == DateTime.Today &&
                    x.Activo && x.Id != cierre.Id);

                if (anteriorcierre is not null) { return BadRequest($"Ya existe un cierre el dia {DateTime.Today:D}"); }

                cierre.Abierto = false;

                context.Update(cierre);

                var inventarios = await context.Inventarios
                    .Where(x => x.ProductoId.Equals(cierre.ProductoId) &&
                    x.SitioId.Equals(cierre.SitioId) &&
                    x.AlmacenId.Equals(cierre.AlmacenId) &&
                    x.LocalidadId.Equals(cierre.LocalidadId) &&
                    x.FechaCierre == null &&
                    x.Activo)
                    .ToListAsync();

                var inventariosdecierre = inventarios.Select(x => { x.FechaCierre = cierre.FechaCierre; return x; });

                context.UpdateRange(inventariosdecierre);
                await context.SaveChangesAsync(userid, 61);

                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("cierre/changestatus/{Id:int}")]
        public async Task<ActionResult> CerrarAbrirCierre([FromRoute] int Id)
        {
            try
            {
                var userid = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(userid))
                    return NotFound();

                var id_terminal = terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var cierre = await context.InventarioCierres.FindAsync(Id);
                if (cierre is null) { return NotFound(); }

                if (!cierre.Abierto)
                {
                    cierre.Abierto = true;
                    context.Update(cierre);
                    await context.SaveChangesAsync(userid, 62);
                }
                else if (cierre.Abierto)
                {
                    cierre.Abierto = false;
                    context.Update(cierre);
                    await context.SaveChangesAsync(userid, 61);

                }
                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("resumen")]
        public async Task<ActionResult> GetResumen([FromQuery] InventarioCierreDTO cierre)
        {
            try
            {
                var id_terminal = terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var tmids = await ObtenerIdMovimientos();

                var inventariostotal = context.InventarioCierres
                    .AsNoTracking()
                    .Where(x => x.Activo && x.TadId.Equals(id_terminal) && x.FechaCierre == null)
                    .Include(x => x.Producto)
                    .Include(x => x.Sitio)
                    .Include(x => x.Almacen)
                    .Include(x => x.Localidad)
                    .Include(x => x.Terminal)
                    .Select(x => mapper.Map<InventarioCierreDTO>(x))
                    .AsQueryable();

                if (!cierre.ProductoId.IsZero())
                    inventariostotal = inventariostotal.Where(x => x.ProductoId == cierre.ProductoId);

                if (!cierre.SitioId.IsZero())
                    inventariostotal = inventariostotal.Where(x => x.SitioId == cierre.SitioId);

                if (!cierre.AlmacenId.IsZero())
                    inventariostotal = inventariostotal.Where(x => x.AlmacenId == cierre.AlmacenId);

                if (!cierre.LocalidadId.IsZero())
                    inventariostotal = inventariostotal.Where(x => x.LocalidadId == cierre.LocalidadId);

                if (cierre.Excel)
                {
                    ExcelPackage.LicenseContext = LicenseContext.Commercial;
                    ExcelPackage excel = new();
                    ExcelWorksheet ws = excel.Workbook.Worksheets.Add("Inventarios");
                    ws.Cells["A1"].LoadFromCollection(inventariostotal.Select(x => mapper.Map<InventarioActualExcelDTO>(x)), op =>
                    {
                        op.TableStyle = TableStyles.Medium2;
                        op.PrintHeaders = true;
                    });

                    ws.Cells[1, 5, ws.Dimension.End.Row, ws.Dimension.End.Column].Style.Numberformat.Format = "#,##0.00";
                    ws.Cells[1, 1, ws.Dimension.End.Row, ws.Dimension.End.Column].AutoFitColumns();

                    return Ok(excel.GetAsByteArray());
                }

                return Ok(inventariostotal);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("cierre")]
        public async Task<ActionResult> GetCierres([FromQuery] InventarioCierreDTO cierreDTO)
        {
            try
            {
                var id_terminal = terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var cierres = context.InventarioCierres
                    .AsNoTracking()
                    .Where(x => x.Activo && x.TadId.Equals(id_terminal))
                    .Include(x => x.Producto)
                    .Include(x => x.Sitio)
                    .Include(x => x.Almacen)
                    .Include(x => x.Localidad)
                    .Include(x => x.Terminal)
                    .AsQueryable();

                if (cierreDTO.PorFecha)
                    cierres = cierres.Where(x => x.FechaCierre >= cierreDTO.Fecha_Inicio && x.FechaCierre <= cierreDTO.Fecha_Fin);

                if (!string.IsNullOrEmpty(cierreDTO.Producto) && !string.IsNullOrWhiteSpace(cierreDTO.Producto))
                    cierres = cierres.Where(x => !string.IsNullOrEmpty(x.Producto.Den) && x.Producto.Den.ToLower().Contains(cierreDTO.Producto.ToLower()));

                if (!string.IsNullOrEmpty(cierreDTO.Sitio) && !string.IsNullOrWhiteSpace(cierreDTO.Sitio))
                    cierres = cierres.Where(x => x.Sitio.Valor.ToLower().Contains(cierreDTO.Sitio.ToLower()));

                if (!string.IsNullOrEmpty(cierreDTO.Almacen) && !string.IsNullOrWhiteSpace(cierreDTO.Almacen))
                    cierres = cierres.Where(x => x.Almacen.Valor.ToLower().Contains(cierreDTO.Almacen.ToLower()));

                if (!string.IsNullOrEmpty(cierreDTO.Localidad) && !string.IsNullOrWhiteSpace(cierreDTO.Localidad))
                    cierres = cierres.Where(x => x.Localidad.Valor.ToLower().Contains(cierreDTO.Localidad.ToLower()));

                if (!cierreDTO.ProductoId.IsZero())
                    cierres = cierres.Where(x => x.ProductoId == cierreDTO.ProductoId);

                if (!cierreDTO.SitioId.IsZero())
                    cierres = cierres.Where(x => x.SitioId == cierreDTO.SitioId);

                if (!cierreDTO.AlmacenId.IsZero())
                    cierres = cierres.Where(x => x.AlmacenId == cierreDTO.AlmacenId);

                if (!cierreDTO.LocalidadId.IsZero())
                    cierres = cierres.Where(x => x.LocalidadId == cierreDTO.LocalidadId);

                if (cierreDTO.Excel)
                {
                    ExcelPackage.LicenseContext = LicenseContext.Commercial;
                    ExcelPackage excel = new();
                    ExcelWorksheet ws = excel.Workbook.Worksheets.Add("Cierres de invetario");

                    ws.Cells["A1"].LoadFromCollection(cierres.Select(x => mapper.Map<InventarioCierreExcelDTO>(x)), op =>
                    {
                        op.TableStyle = TableStyles.Medium2;
                        op.PrintHeaders = true;
                    });

                    ws.Cells[1, 1, ws.Dimension.End.Row, ws.Dimension.End.Column].Style.Numberformat.Format = "dd/MM/yyyy";
                    ws.Cells[1, 6, ws.Dimension.End.Row, ws.Dimension.End.Column].Style.Numberformat.Format = "#,##0.00";
                    ws.Cells[1, 1, ws.Dimension.End.Row, ws.Dimension.End.Column].AutoFitColumns();

                    return Ok(excel.GetAsByteArray());
                }

                await HttpContext.InsertarParametrosPaginacion(cierres, cierreDTO.Registros_por_pagina, cierreDTO.Pagina);

                cierreDTO.Pagina = HttpContext.ObtenerPagina();

                cierres = cierres.Skip((cierreDTO.Pagina - 1) * cierreDTO.Registros_por_pagina).Take(cierreDTO.Registros_por_pagina);

                return Ok(cierres.Select(x => mapper.Map<InventarioCierreDTO>(x)));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("{Id:int}")]
        public async Task<ActionResult> GetById([FromRoute] int Id)
        {
            try
            {
                var inventario = await context.Inventarios
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id.Equals(Id));

                if (inventario is null) return NotFound();

                return Ok(mapper.Map<InventarioPostDTO>(inventario));
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpGet("cierre/{Id:int}")]
        public async Task<ActionResult> GetCierreById([FromRoute] int Id)
        {
            try
            {
                var inventario = await context.InventarioCierres
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id.Equals(Id));

                if (inventario is null) return NotFound();

                return Ok(mapper.Map<InventarioCierreDTO>(inventario));
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        #region Catalogos

        [HttpGet("catalogo/sitio")]
        public ActionResult CatalogoSitio()
        {
            try
            {
                var id_terminal = terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var catalogo = context.Catalogos
                    .Include(x => x.Valores.Where(y => y.Activo && y.TadId.Equals(id_terminal)))
                    .FirstOrDefault(x => x.Clave.Equals("Catalogo_Inventario_Sitios"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para sitios"); }

                var valores = catalogo.Valores.Select(x => mapper.Map<CatalogoValorDTO>(x));

                return Ok(valores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/almacen")]
        public ActionResult CatalogoAlmacen()
        {
            try
            {
                var id_terminal = terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var catalogo = context.Catalogos
                    .Include(x => x.Valores.Where(y => y.Activo && y.TadId.Equals(id_terminal)))
                    .FirstOrDefault(x => x.Clave.Equals("Catalogo_Inventario_Almacenes"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para almacenes"); }

                var valores = catalogo.Valores.Select(x => mapper.Map<CatalogoValorDTO>(x));

                return Ok(valores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/localidad")]
        public ActionResult CatalogoLocalidad()
        {
            try
            {
                var id_terminal = terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var catalogo = context.Catalogos
                    .Include(x => x.Valores.Where(y => y.Activo && y.TadId.Equals(id_terminal)))
                    .FirstOrDefault(x => x.Clave.Equals("Catalogo_Inventario_Localidades"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para localidades"); }

                var valores = catalogo.Valores.Select(x => mapper.Map<CatalogoValorDTO>(x));

                return Ok(valores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/unidadmedida")]
        public ActionResult CatalogoUnidadMedida()
        {
            try
            {
                var id_terminal = terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var catalogo = context.Catalogos
                    .Include(x => x.Valores.Where(y => y.Activo && y.TadId == id_terminal))
                    .FirstOrDefault(x => x.Clave.Equals("Catalogo_Inventario_Unidad_Medida"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para unidad de medida"); }

                var valores = catalogo.Valores.Select(x => mapper.Map<CatalogoValorDTO>(x));

                return Ok(valores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/tipomovimiento")]
        public ActionResult CatalogoTipoMovimiento()
        {
            try
            {
                var id_terminal = terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var catalogo = context.Catalogos
                    .Include(x => x.Valores.Where(y => y.Activo && y.TadId.Equals(id_terminal)))
                    .FirstOrDefault(x => x.Clave.Equals("Catalogo_Inventario_Tipo_Movimientos"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para sitios"); }

                var valores = catalogo.Valores.Select(x => mapper.Map<CatalogoValorDTO>(x)).OrderBy(x => x.Abreviacion);

                return Ok(valores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        #endregion

        [HttpGet("tipomovimiento/{Id:int}")]
        public async Task<ActionResult> GetMenu([FromRoute] int Id)
        {
            try
            {
                var tipomovimiento = await context.CatalogoValores
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == Id);

                if (tipomovimiento is null)
                    return NotFound();

                var menu = new MenuInventarioDTO();

                var tiposnoentradassalidas = new List<int> { 1 };

                if (!tiposnoentradassalidas.Contains(tipomovimiento.Id))
                {
                    if (int.TryParse(tipomovimiento.Abreviacion, out int tm))
                    {
                        if (tm >= 20)
                        {
                            menu.LabelMenu = "Destino";
                            var destinos = await context.Catalogos.AsNoTracking()
                                .Where(x => x.Clave.Equals("Catalogo_Inventario_Destinos") && x.Activo)
                                .Include(x => x.Valores.Where(y => y.Activo)).FirstOrDefaultAsync();

                            menu.Destinos = destinos?.Valores.Select(y => mapper.Map<CatalogoValorDTO>(y)).ToList() ?? new();

                            if (tm.Equals(20))
                            {
                                menu.EsVenta = true;
                                menu.MostrarMenuInventarios = true;
                            }
                        }
                        else if (tm > 0 && tm < 20)
                        {
                            menu.LabelMenu = "Origen";
                            var origenes = await context.Catalogos.AsNoTracking()
                                .Where(x => x.Clave.Equals("Catalogo_Inventario_Origen") && x.Activo)
                                .Include(x => x.Valores.Where(y => y.Activo)).FirstOrDefaultAsync();

                            menu.Origenes = origenes?.Valores.Select(y => mapper.Map<CatalogoValorDTO>(y)).ToList() ?? new();

                            if (tm.Equals(9))
                            {
                                menu.EsDevolucion = true;
                                menu.MostrarMenuInventarios = true;
                            }
                        }
                    }
                }

                return Ok(menu);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        private record TiposMovientoIds(
            int InventarioInicial,
            int FisicaReservada,
            int PedidoTotal,
            int OrdenReservada,
            int EnOrden,
            List<int> CargosAdicionales,
            List<int> Salidas,
            List<int> Entradas,
            List<int> SalidaVenta
            );

        private async Task<TiposMovientoIds> ObtenerIdMovimientos()
        {
            var id_terminal = terminal.Obtener_Terminal(context, HttpContext);
            if (id_terminal == 0)
                throw new InvalidParameterException();

            var invinicial = "01";

            var listSalidas = new List<string> { "Salida por Ajuste" };

            var listSalidaVenta = new List<string> { "Salida Venta" };

            var listEntradas = new List<string> { "02", "03", "04", "05", "06" };

            var invfisicareservada = "21";

            var invpedidototal = "07";

            var invordenreservada = "22";

            var invenorden = "23";

            var invinicialid = await context.CatalogoValores
                    .AsNoTracking()
                    .Where(x => !string.IsNullOrEmpty(x.Abreviacion) && x.Abreviacion.Equals(invinicial)
                    && x.Activo && !x.EsEditable && x.TadId.Equals(id_terminal) && x.CatalogoId.Equals(39))
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync();

            var fisicareservadaid = await context.CatalogoValores
                .AsNoTracking()
                .Where(x => !string.IsNullOrEmpty(x.Abreviacion) && x.Abreviacion.Equals(invfisicareservada)
                && x.Activo && !x.EsEditable && x.TadId.Equals(id_terminal) && x.CatalogoId.Equals(39))
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            var pedidototalid = await context.CatalogoValores
                .AsNoTracking()
                .Where(x => !string.IsNullOrEmpty(x.Abreviacion) && x.Abreviacion.Equals(invpedidototal)
                && x.Activo && !x.EsEditable && x.TadId.Equals(id_terminal) && x.CatalogoId.Equals(39))
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            var ordenreservadaid = await context.CatalogoValores
                .AsNoTracking()
                .Where(x => !string.IsNullOrEmpty(x.Abreviacion) && x.Abreviacion.Equals(invordenreservada)
                && x.Activo && !x.EsEditable && x.TadId.Equals(id_terminal) && x.CatalogoId.Equals(39))
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            var enordenid = await context.CatalogoValores
                .AsNoTracking()
                .Where(x => !string.IsNullOrEmpty(x.Abreviacion) && x.Abreviacion.Equals(invenorden)
                && x.Activo && !x.EsEditable && x.TadId.Equals(id_terminal) && x.CatalogoId.Equals(39))
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            var cargosadicionales = await context.Catalogos
                .AsNoTracking()
                .Where(x => x.Clave.Equals("Catalogo_Inventario_Tipo_Movimientos"))
                .Include(x => x.Valores.Where(y => y.Activo && y.EsEditable && y.TadId.Equals(id_terminal)))
                .FirstOrDefaultAsync() ?? new();

            var salidas = await context.CatalogoValores
                .AsNoTracking()
                .Where(x => listSalidas.Contains(x.Valor) && x.Activo && !x.EsEditable && x.TadId.Equals(id_terminal) && x.CatalogoId.Equals(39))
                .Select(x => x.Id)
                .ToListAsync();

            var entradas = await context.CatalogoValores
                .AsNoTracking()
                .Where(x => listEntradas.Contains(x.Valor) && x.Activo && !x.EsEditable && x.TadId.Equals(id_terminal) && x.CatalogoId.Equals(39))
                .Select(x => x.Id)
                .ToListAsync();

            var salidasventa = await context.CatalogoValores
                .AsNoTracking()
                .Where(x => listSalidas.Contains(x.Valor) && x.Activo && !x.EsEditable && x.TadId.Equals(id_terminal))
                .Select(x => x.Id)
                .ToListAsync();

            return new TiposMovientoIds(
                invinicialid,
                fisicareservadaid,
                pedidototalid,
                ordenreservadaid,
                enordenid,
                cargosadicionales.Valores.Select(x => x.Id).ToList(),
                salidas,
                entradas,
                salidasventa);
        }

        private async Task<InventarioCierre> RestarRegistroInventario(InventarioCierre cierre, Inventario inventariodto)
        {
            if (cierre is not null)
            {
                var tm = await context.CatalogoValores.AsNoTracking().FirstOrDefaultAsync(x => x.Id.Equals(inventariodto.TipoMovimientoId));
                if (cierre.Abierto)
                {
                    if (tm is not null)
                    {
                        if (int.TryParse(tm.Abreviacion, out int tipo))
                        {
                            if (tipo.Equals(1))
                                cierre.Fisico -= inventariodto.CantidadLTS;
                            else if (tipo.Equals(7))
                                cierre.PedidoTotal -= inventariodto.CantidadLTS;
                            else if (tipo.Equals(8))
                            {
                                cierre.Fisico -= inventariodto.CantidadLTS;
                                cierre.PedidoTotal += inventariodto.CantidadLTS;
                            }
                            else if (tipo.Equals(9) || tipo.Equals(20))
                            {
                                if (inventariodto.TipoInventario.Equals(TipoInventario.Inicial))
                                    cierre.Fisico += inventariodto.CantidadLTS;
                                if (inventariodto.TipoInventario.Equals(TipoInventario.FisicaReservada))
                                    cierre.Reservado += inventariodto.CantidadLTS;
                                if (inventariodto.TipoInventario.Equals(TipoInventario.OrdenReservada))
                                    cierre.OrdenReservado += inventariodto.CantidadLTS;
                                if (inventariodto.TipoInventario.Equals(TipoInventario.EnOrden))
                                    cierre.EnOrden += inventariodto.CantidadLTS;

                                if (tipo.Equals(9))
                                    cierre.Fisico -= inventariodto.CantidadLTS;
                                if (tipo.Equals(20))
                                    cierre.Fisico += inventariodto.CantidadLTS;
                            }
                            else if (tipo < 20)
                                cierre.Fisico -= inventariodto.CantidadLTS;
                            else if (tipo.Equals(21))
                                cierre.Reservado -= inventariodto.CantidadLTS;
                            else if (tipo.Equals(22))
                                cierre.OrdenReservado -= inventariodto.CantidadLTS;
                            else if (tipo.Equals(23))
                                cierre.EnOrden -= inventariodto.CantidadLTS;
                            else if (tipo >= 20)
                                cierre.Fisico += inventariodto.CantidadLTS;
                        }

                        cierre.Disponible = (cierre.Fisico - cierre.Reservado);
                        cierre.TotalDisponible = (cierre.Disponible + cierre.PedidoTotal) - (cierre.OrdenReservado + cierre.EnOrden);
                        cierre.TotalDisponibleFull = cierre.TotalDisponible.IsZero() ? 0 : cierre.TotalDisponible / 62000;
                    }
                }
            }
            return cierre!;
        }
    }
}