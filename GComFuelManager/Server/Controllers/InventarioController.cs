using AutoMapper;
using FluentValidation;
using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.Extensiones;
using GComFuelManager.Shared.ModelDTOs;
using GComFuelManager.Shared.Modelos;
using GComFuelManager.Shared.ReportesDTO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using Org.BouncyCastle.Security;
using System.Linq.Dynamic.Core;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

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
                    .Include(x => x.Grupo)
                    .Include(x => x.Cliente)
                    .Include(x => x.Transportista)
                    .Include(x => x.Tonel)
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
                    inventarios = inventarios.Where(x => !string.IsNullOrEmpty(x.Transportista.Den) && x.Transportista.Den.ToLower().Contains(inventario.Transportista.ToLower()));

                if (!string.IsNullOrEmpty(inventario.Grupo) && !string.IsNullOrWhiteSpace(inventario.Grupo))
                    inventarios = inventarios.Where(x => !string.IsNullOrEmpty(x.Grupo.Den) && x.Grupo.Den.ToLower().Contains(inventario.Grupo.ToLower()));

                if (!string.IsNullOrEmpty(inventario.Tonel) && !string.IsNullOrWhiteSpace(inventario.Tonel))
                    inventarios = inventarios.Where(x => !string.IsNullOrEmpty(x.Tonel.Tracto) && x.Tonel.Tracto.ToLower().Contains(inventario.Tonel.ToLower()));

                if (!string.IsNullOrEmpty(inventario.Cliente) && !string.IsNullOrWhiteSpace(inventario.Cliente))
                    inventarios = inventarios.Where(x => !string.IsNullOrEmpty(x.Cliente.Den) && x.Cliente.Den.ToLower().Contains(inventario.Cliente.ToLower()));

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

                if (inventariodto.UnidadMedidaId == lts.Id)
                    inventariodto.CantidadLTS = inventariodto.Cantidad;
                else
                    inventariodto.CantidadLTS = inventariodto.Cantidad * 1000;

                if (!inventariodto.Id.IsZero())
                {
                    var inventariodb = await context.Inventarios.FindAsync(inventariodto.Id);
                    if (inventariodb is null) { return NotFound(); }

                    var inventario = mapper.Map(inventariodto, inventariodb);
                    context.Inventarios.Attach(inventario);
                    context.Inventarios.Entry(inventario).State = EntityState.Modified;
                    //context.Update(inventario);
                }
                else
                {
                    if (!post.CierreId.IsZero())
                    {
                        var cierre = await context.InventarioCierres
                            .FirstOrDefaultAsync(x => x.Id == post.CierreId && x.Activo);
                        if (cierre is not null)
                        {
                            if (cierre.Abierto)
                            {
                                inventariodto.CierreId = post.CierreId;
                                inventariodto.FechaCierre = cierre.FechaCierre;

                                var tmids = await ObtenerIdMovimientos();

                                if (tmids.InventarioInicial.Contains(inventariodto.TipoMovimientoId))
                                    cierre.Fisico += inventariodto.CantidadLTS;

                                if (tmids.FisicaReservada.Contains(inventariodto.TipoMovimientoId))
                                    cierre.Reservado += inventariodto.CantidadLTS;

                                cierre.Disponible = (cierre.Fisico - cierre.Fisico);

                                if (tmids.PedidoTotal.Contains(inventariodto.TipoMovimientoId))
                                    cierre.PedidoTotal += inventariodto.CantidadLTS;

                                if (tmids.OrdenReservada.Contains(inventariodto.TipoMovimientoId))
                                    cierre.OrdenReservado += inventariodto.CantidadLTS;

                                if (tmids.EnOrden.Contains(inventariodto.TipoMovimientoId))
                                    cierre.EnOrden += inventariodto.CantidadLTS;

                                cierre.TotalDisponible = (cierre.Disponible + cierre.PedidoTotal) - (cierre.OrdenReservado + cierre.EnOrden);
                                cierre.TotalDisponibleFull = cierre.TotalDisponible.IsZero() ? 0 : cierre.TotalDisponible / 62000;
                                context.Update(cierre);
                            }
                        }
                    }

                    await context.AddAsync(inventariodto);
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
                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("cierre")]
        public async Task<ActionResult> PostCierre([FromBody] InventarioCierreDTO post)
        {
            try
            {
                var userid = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(userid))
                    return NotFound();

                var id_terminal = terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var cierre = mapper.Map<InventarioCierre>(post);

                if (cierre.Id.IsZero())
                {
                    var producto = await context.Producto.AsNoTracking().FirstOrDefaultAsync(x => !string.IsNullOrEmpty(x.Den) && x.Den.Equals(post.Producto) && x.Id_Tad.Equals(id_terminal));
                    if (producto is null) return NotFound();
                    var sitio = await context.CatalogoValores.AsNoTracking().FirstOrDefaultAsync(x => x.Valor.Equals(post.Sitio) && x.TadId.Equals(id_terminal));
                    if (sitio is null) return NotFound();
                    var almacen = await context.CatalogoValores.AsNoTracking().FirstOrDefaultAsync(x => x.Valor.Equals(post.Almacen) && x.TadId.Equals(id_terminal));
                    if (almacen is null) return NotFound();
                    var localidad = await context.CatalogoValores.AsNoTracking().FirstOrDefaultAsync(x => x.Valor.Equals(post.Localidad) && x.TadId.Equals(id_terminal));
                    if (localidad is null) return NotFound();

                    cierre.ProductoId = producto.Cod;
                    cierre.SitioId = sitio.Id;
                    cierre.AlmacenId = almacen.Id;
                    cierre.LocalidadId = localidad.Id;
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
                        x.Activo);

                    if (anteriorcierre is not null) { return BadRequest($"Ya existe un cierre el dia {DateTime.Today:D}"); }

                    cierre.Abierto = false;

                    await context.AddAsync(cierre);
                    await context.SaveChangesAsync();

                    var inventarios = await context.Inventarios
                        .Where(x => x.ProductoId.Equals(cierre.ProductoId) &&
                        x.SitioId.Equals(cierre.SitioId) &&
                        x.AlmacenId.Equals(cierre.AlmacenId) &&
                        x.LocalidadId.Equals(cierre.LocalidadId) &&
                        x.FechaCierre == null &&
                        x.Activo)
                        .ToListAsync();

                    inventarios.ForEach(x =>
                    {
                        x.FechaCierre = cierre.FechaCierre;
                        x.CierreId = cierre.Id;
                    });

                    context.UpdateRange(inventarios);
                    await context.SaveChangesAsync(userid, 61);
                }
                else
                {
                    if (cierre.Abierto)
                    {
                        var cierredb = await context.InventarioCierres.FindAsync(cierre.Id);
                        if (cierredb is null) { return NotFound(); }

                        cierredb.Abierto = true;
                        context.Update(cierredb);
                        await context.SaveChangesAsync(userid, 62);
                    }
                    else if (!cierre.Abierto)
                    {
                        var cierredb = await context.InventarioCierres.FindAsync(cierre.Id);
                        if (cierredb is null) { return NotFound(); }

                        cierredb.Abierto = false;
                        context.Update(cierredb);
                        await context.SaveChangesAsync(userid, 61);
                    }
                }
                return Ok();
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

                var inventariostotal = context.Inventarios
                    .AsNoTracking()
                    .Where(x => x.Activo && x.TadId.Equals(id_terminal) && x.FechaCierre == null)
                    .Include(x => x.Producto)
                    .Include(x => x.Sitio)
                    .Include(x => x.Almacen)
                    .Include(x => x.Localidad)
                    .Include(x => x.Terminal)
                    .AsQueryable();

                if (!cierre.ProductoId.IsZero())
                    inventariostotal = inventariostotal.Where(x => x.ProductoId == cierre.ProductoId);

                if (!cierre.SitioId.IsZero())
                    inventariostotal = inventariostotal.Where(x => x.SitioId == cierre.SitioId);

                if (!cierre.AlmacenId.IsZero())
                    inventariostotal = inventariostotal.Where(x => x.AlmacenId == cierre.AlmacenId);

                if (!cierre.LocalidadId.IsZero())
                    inventariostotal = inventariostotal.Where(x => x.LocalidadId == cierre.LocalidadId);

                var inventariosagrupados = inventariostotal
                    .GroupByMany(x => new { x.ProductoId, x.SitioId, x.AlmacenId, x.LocalidadId, x.FechaCierre });

                var grupoinventarios = inventariosagrupados.Select(x => x.Key).ToList();

                List<InventarioCierreDTO> Cierres = new();

                var invinicial = tmids.InventarioInicial;

                var fisicareservada = tmids.FisicaReservada;

                var pedidototal = tmids.PedidoTotal;

                var ordenreservada = tmids.OrdenReservada;

                var enorden = tmids.EnOrden;

                var cargosadicionales = tmids.CargosAdicionales;

                for (int i = 0; i < grupoinventarios.Count; i++)
                {
                    //var keys = JsonSerializer grupoinventarios[i].Key;
                    var json = JsonConvert.SerializeObject(grupoinventarios[i]);
                    InventarioCierre keys = JsonConvert.DeserializeObject<InventarioCierre>(json);

                    if (keys is not null)
                    {
                        var anteriorcierre = await context.InventarioCierres
                            .AsNoTracking()
                            .Where(x => x.ProductoId.Equals(keys.ProductoId) && x.SitioId.Equals(keys.SitioId) &&
                                        x.AlmacenId.Equals(keys.AlmacenId) && x.LocalidadId.Equals(keys.LocalidadId) &&
                                        x.TadId.Equals(id_terminal))
                            .Include(x => x.Producto)
                            .Include(x => x.Sitio)
                            .Include(x => x.Almacen)
                            .Include(x => x.Localidad)
                            .OrderByDescending(x => x.FechaCierre)
                            .Select(x => mapper.Map<InventarioCierreDTO>(x))
                            .FirstOrDefaultAsync() ?? new();

                        //var jsonlist = JsonConvert.SerializeObject(grupoinventarios[i].Items);
                        //var inventarios = JsonConvert.DeserializeObject<List<Inventario>>(jsonlist);

                        var inventarios = await context.Inventarios
                            .Where(x => x.ProductoId.Equals(keys.ProductoId) &&
                            x.SitioId.Equals(keys.SitioId) &&
                            x.AlmacenId.Equals(keys.AlmacenId) &&
                            x.LocalidadId.Equals(keys.LocalidadId) &&
                            x.FechaCierre == null && x.TadId.Equals(id_terminal))
                            .Select(x => new { x.TipoMovimientoId, x.CantidadLTS })
                            .ToListAsync();

                        if (inventarios is not null)
                        {
                            keys.Fisico += anteriorcierre.TotalDisponible;

                            keys.Fisico += inventarios.Where(x => invinicial.Contains(x.TipoMovimientoId))
                                .Sum(x => (double)x.CantidadLTS);

                            keys.Reservado = inventarios.Where(x => fisicareservada.Contains(x.TipoMovimientoId))
                                .Sum(x => (double)x.CantidadLTS);

                            keys.Disponible = (keys.Fisico - keys.Reservado);

                            keys.PedidoTotal = inventarios.Where(x => pedidototal.Contains(x.TipoMovimientoId))
                                .Sum(x => (double)x.CantidadLTS);
                            keys.OrdenReservado = inventarios.Where(x => ordenreservada.Contains(x.TipoMovimientoId))
                                .Sum(x => (double)x.CantidadLTS);
                            keys.EnOrden = inventarios.Where(x => enorden.Contains(x.TipoMovimientoId))
                                .Sum(x => (double)x.CantidadLTS);

                            keys.TotalDisponible = ((keys.Disponible + keys.PedidoTotal) - (keys.OrdenReservado + keys.EnOrden));

                            //foreach (var item in cargosadicionales?.Valores ?? new())
                            //{
                            //    keys.TotalDisponible += inventarios.Where(x => x.TipoMovimientoId.Equals(item.Id)).Sum(x => x.CantidadLTS);
                            //}

                            keys.TotalDisponible += inventarios.Where(x => cargosadicionales.Contains(x.TipoMovimientoId)).Sum(x => x.CantidadLTS);

                            keys.TotalDisponibleFull = !keys.TotalDisponible.IsZero() ? keys.TotalDisponible / 62000 : 0;

                            //keys.Inventarios = inventarios;

                            keys.Producto = await context.Producto.FirstOrDefaultAsync(x => x.Cod == keys.ProductoId) ?? new();
                            keys.Sitio = await context.CatalogoValores.FirstOrDefaultAsync(x => x.Id == keys.SitioId) ?? new();
                            keys.Almacen = await context.CatalogoValores.FirstOrDefaultAsync(x => x.Id == keys.AlmacenId) ?? new();
                            keys.Localidad = await context.CatalogoValores.FirstOrDefaultAsync(x => x.Id == keys.LocalidadId) ?? new();

                            //keys.Producto = Producto;
                            //keys.Sitio = Sitio;
                            //keys.Almacen = Almacen;
                            //keys.Localidad = Localidad;

                            //Cierres.Add(new()
                            //{
                            //    Producto = Producto.ToString(),
                            //    Sitio = Sitio.ToString(),
                            //    Almacen = Almacen.ToString(),
                            //    Localidad = Localidad.ToString(),
                            //    Nuevo = mapper.Map<InventarioCierreDTO>(keys),
                            //    Anterior = anteriorcierre
                            //});
                            Cierres.Add(mapper.Map<InventarioCierreDTO>(keys));
                        }
                    }
                }

                if (cierre.Excel)
                {
                    ExcelPackage.LicenseContext = LicenseContext.Commercial;
                    ExcelPackage excel = new();
                    ExcelWorksheet ws = excel.Workbook.Worksheets.Add("Inventarios");
                    ws.Cells["A1"].LoadFromCollection(Cierres.Select(x => mapper.Map<InventarioActualExcelDTO>(x)), op =>
                    {
                        op.TableStyle = TableStyles.Medium2;
                        op.PrintHeaders = true;
                    });

                    ws.Cells[1, 5, ws.Dimension.End.Row, ws.Dimension.End.Column].Style.Numberformat.Format = "#,##0.00";
                    ws.Cells[1, 1, ws.Dimension.End.Row, ws.Dimension.End.Column].AutoFitColumns();

                    return Ok(excel.GetAsByteArray());
                }

                return Ok(Cierres);
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

                var valores = catalogo.Valores.Select(x => mapper.Map<CatalogoValorDTO>(x));

                return Ok(valores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        #endregion

        private record TiposMovientoIds(
            List<int> InventarioInicial,
            List<int> FisicaReservada,
            List<int> PedidoTotal,
            List<int> OrdenReservada,
            List<int> EnOrden,
            List<int> CargosAdicionales
            );

        private async Task<TiposMovientoIds> ObtenerIdMovimientos()
        {
            var id_terminal = terminal.Obtener_Terminal(context, HttpContext);
            if (id_terminal == 0)
                throw new InvalidParameterException();

            var listinvinicial = new List<string> { "Inventario Inicial",
                "Entrada Compra",
                "Entrada Traspaso",
                "Entrada Devolución",
            };
            var listinvfisicareservada = new List<string> { "Fisica Reservada" };

            var listinvpedidototal = new List<string> {
                    "Pedido en Total (Por Recibir)"
                };

            var listinvordenreservada = new List<string> { "Ordenada Reservada (Por Cargarse)" };

            var listinvenorden = new List<string> { "En Orden (En Proceso de Carga)" };

            var invinicial = await context.CatalogoValores
                    .AsNoTracking()
                    .Where(x => listinvinicial.Contains(x.Valor) && x.Activo && x.EsEditable && x.TadId.Equals(id_terminal))
                    .Select(x => x.Id)
                    .ToListAsync();

            var fisicareservada = await context.CatalogoValores
                .AsNoTracking()
                .Where(x => listinvfisicareservada.Contains(x.Valor) && x.Activo && x.EsEditable && x.TadId.Equals(id_terminal))
                .Select(x => x.Id)
                .ToListAsync();

            var pedidototal = await context.CatalogoValores
                .AsNoTracking()
                .Where(x => listinvpedidototal.Contains(x.Valor) && x.Activo && x.EsEditable && x.TadId.Equals(id_terminal))
                .Select(x => x.Id)
                .ToListAsync();

            var ordenreservada = await context.CatalogoValores
                .AsNoTracking()
                .Where(x => listinvordenreservada.Contains(x.Valor) && x.Activo && x.EsEditable && x.TadId.Equals(id_terminal))
                .Select(x => x.Id)
                .ToListAsync();

            var enorden = await context.CatalogoValores
                .AsNoTracking()
                .Where(x => listinvenorden.Contains(x.Valor) && x.Activo && x.EsEditable && x.TadId.Equals(id_terminal))
                .Select(x => x.Id)
                .ToListAsync();

            var cargosadicionales = await context.Catalogos
                .AsNoTracking()
                .Where(x => x.Clave.Equals("Catalogo_Inventario_Tipo_Movimientos"))
                .Include(x => x.Valores.Where(y => y.Activo && y.EsEditable && y.TadId.Equals(id_terminal)))
                .FirstOrDefaultAsync() ?? new();

            return new TiposMovientoIds(
                invinicial,
                fisicareservada,
                pedidototal,
                ordenreservada,
                enorden,
                cargosadicionales.Valores.Select(x => x.Id).ToList());
        }

        private ResultadosCierre ObtenerResultados()
        {

            return new(0, 0, 0, 0, 0, 0, 0, 0);
        }

        private record ResultadosCierre(
            double Fisico,
            double Reservado,
            double Disponible,
            double PedidoTotal,
            double OrdenReservado,
            double EnOrden,
            double TotalDisponible,
            double TotalDisponibleFull);
    }
}
