using AutoMapper;
using FluentValidation;
using GComFuelManager.Server.Helpers;
using GComFuelManager.Shared.Extensiones;
using GComFuelManager.Shared.ModelDTOs;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Table;
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

        public InventarioController(ApplicationDbContext context, IValidator<InventarioPostDTO> validator, IMapper mapper, User_Terminal _terminal)
        {
            this.context = context;
            this.validator = validator;
            this.mapper = mapper;
            terminal = _terminal;
        }

        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] InventarioDTO inventario)
        {
            try
            {
                var id_terminal = terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var inventarios = context.Inventario
                    .AsNoTracking()
                    .Where(x => x.Activo && x.TadId.Equals(id_terminal) && x.FechaRegistro >= inventario.Fecha_Inicio && x.FechaRegistro <= inventario.Fecha_Fin)
                    .Include(x => x.Producto)
                    .Include(x => x.Sitio)
                    .Include(x => x.Almacen)
                    .Include(x => x.Localidad)
                    .Include(x => x.UnidadMedida)
                    .Include(x => x.TipoMovimiento)
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

                //if (inventario.PorFecha)
                //    inventarios = inventarios.Where(x => x.FechaRegistro.Date >= inventario.Fecha_Inicio.Date && x.FechaRegistro.Date <= inventario.Fecha_Fin.Date);

                if (inventario.Excel)
                {
                    ExcelPackage.LicenseContext = LicenseContext.Commercial;
                    ExcelPackage excel = new();
                    ExcelWorksheet ws = excel.Workbook.Worksheets.Add("Inventarios");
                    ws.Cells["A1"].LoadFromCollection(inventarios.Select(x => mapper.Map<InventarioDTO>(x)), op =>
                    {
                        op.TableStyle = TableStyles.Medium2;
                        op.PrintHeaders = true;
                    });

                    ws.Cells[1, 8, ws.Dimension.End.Row, 8].Style.Numberformat.Format = "dd/MM/yyyy";
                    ws.Cells[1, 10, ws.Dimension.End.Row, 11].Style.Numberformat.Format = "dd/MM/yyyy";
                    //ws.Cells[1, 11, ws.Dimension.End.Row, 11].Style.Numberformat.Format = "#,##0.00";

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
                var result = validator.Validate(post);
                if (!result.IsValid) { return BadRequest(result.Errors.Select(x => x.ErrorMessage)); }

                var inventariodto = mapper.Map<Inventario>(post);
                if (!inventariodto.Id.IsZero())
                {
                    var inventariodb = await context.Inventario.FindAsync(inventariodto.Id);
                    if (inventariodb is null) { return NotFound(); }

                    var inventario = mapper.Map(inventariodto, inventariodb);
                    context.Update(inventario);
                }
                else
                    await context.AddAsync(inventariodto);

                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete]
        public async Task<ActionResult> Delete([FromRoute] int Id)
        {
            try
            {
                var inventario = await context.Inventario.FindAsync(Id);
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
        public async Task<ActionResult> PostCierre([FromBody] InventarioPostDTO post)
        {
            try
            {

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        [HttpGet("cierre")]
        public async Task<ActionResult> GetReporte([FromQuery] InventarioDTO inventario)
        {
            try
            {
                var id_terminal = terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var listinvinicial = new List<string> { "Inventario Inicial" };
                var listinvfisicareservada = new List<string> { "Fisica Reservada" };

                var listinvpedidototal = new List<string> {
                    "Pedido en Total (Por Recibir)",
                    "Entrada Compra",
                    "Entrada Traspaso",
                    "Entrada Devolución",
                    "Entrada Rápida",
                    "Entrada Ajuste"
                };

                var listinvordenreservada = new List<string> {
                    "Ordenada Reservada (Por Cargarse)",
                    "Salida Venta",
                    "Salida por Ajuste"
                };

                var listinvenorden = new List<string> { "En Orden (En Proceso de Carga)" };

                var grupoinventarios = context.Inventario
                    .AsNoTracking()
                    .Where(x => x.Activo && x.TadId.Equals(id_terminal) && x.FechaCierre == null)
                    .Include(x => x.Producto)
                    .Include(x => x.Sitio)
                    .Include(x => x.Almacen)
                    .Include(x => x.Localidad)
                    .Include(x => x.Terminal)
                    .GroupByMany(x => new { x.ProductoId, x.SitioId, x.AlmacenId, x.LocalidadId, x.FechaCierre })
                    .ToList();

                List<InventarioAnteriorNuevoCierreDTO> Cierres = new();

                var invinicial = await context.Catalogo_Fijo
                    .AsNoTracking()
                    .Where(x => listinvinicial.Contains(x.Valor))
                    .Select(x => x.Id)
                    .ToListAsync();

                var fisicareservada = await context.Catalogo_Fijo
                    .AsNoTracking()
                    .Where(x => listinvfisicareservada.Contains(x.Valor))
                    .Select(x => x.Id)
                    .ToListAsync();

                var pedidototal = await context.Catalogo_Fijo
                    .AsNoTracking()
                    .Where(x => listinvpedidototal.Contains(x.Valor))
                    .Select(x => x.Id)
                    .ToListAsync();

                var ordenreservada = await context.Catalogo_Fijo
                    .AsNoTracking()
                    .Where(x => listinvordenreservada.Contains(x.Valor))
                    .Select(x => x.Id)
                    .ToListAsync();

                var enorden = await context.Catalogo_Fijo
                    .AsNoTracking()
                    .Where(x => listinvenorden.Contains(x.Valor))
                    .Select(x => x.Id)
                    .ToListAsync();

                for (int i = 0; i < grupoinventarios.Count; i++)
                {
                    //var keys = JsonSerializer grupoinventarios[i].Key;
                    var json = JsonConvert.SerializeObject(grupoinventarios[i].Key);
                    InventarioCierre keys = JsonConvert.DeserializeObject<InventarioCierre>(json);

                    if (keys is not null)
                    {

                        var jsonlist = JsonConvert.SerializeObject(grupoinventarios[i].Items);
                        var inventarios = JsonConvert.DeserializeObject<List<Inventario>>(jsonlist);

                        if (inventarios is not null)
                        {
                            keys.Fisico = inventarios.Where(x => invinicial.Contains(x.TipoMovimientoId)).Sum(x => (double)x.Cantidad);
                            keys.Reservado = inventarios.Where(x => fisicareservada.Contains(x.TipoMovimientoId)).Sum(x => (double)x.Cantidad);
                            keys.Disponible = (keys.Fisico - keys.Reservado);
                            keys.PedidoTotal = inventarios.Where(x => pedidototal.Contains(x.TipoMovimientoId)).Sum(x => (double)x.Cantidad);
                            keys.OrdenReservado = inventarios.Where(x => ordenreservada.Contains(x.TipoMovimientoId)).Sum(x => (double)x.Cantidad);
                            keys.EnOrden = inventarios.Where(x => enorden.Contains(x.TipoMovimientoId)).Sum(x => (double)x.Cantidad);
                            keys.TotalDisponible = ((keys.Disponible + keys.PedidoTotal) - (keys.OrdenReservado - keys.EnOrden));

                            keys.Inventarios = inventarios;

                            keys.Producto = await context.Producto.FirstOrDefaultAsync(x => x.Cod == keys.ProductoId) ?? new();
                            keys.Sitio = await context.Catalogo_Fijo.FirstOrDefaultAsync(x => x.Id == keys.SitioId) ?? new();
                            keys.Almacen = await context.Catalogo_Fijo.FirstOrDefaultAsync(x => x.Id == keys.AlmacenId) ?? new();
                            keys.Localidad = await context.Catalogo_Fijo.FirstOrDefaultAsync(x => x.Id == keys.LocalidadId) ?? new();

                            Cierres.Add(new()
                            {
                                Nuevo = mapper.Map<InventarioCierreDTO>(keys)
                            });
                        }
                    }
                }

                return Ok(Cierres);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        #region Catalogos

        [HttpGet("catalogo/sitio")]
        public ActionResult CatalogoSitio()
        {
            try
            {
                var catalogo = context.Accion.FirstOrDefault(x => x.Nombre.Equals("Catalogo_Inventario_Sitios"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para sitios"); }

                var catalogo_fijo = context.Catalogo_Fijo.Where(x => x.Activo && x.Catalogo.Equals(catalogo.Cod)).ToList();

                return Ok(catalogo_fijo);
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
                var catalogo = context.Accion.FirstOrDefault(x => x.Nombre.Equals("Catalogo_Inventario_Almacenes"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para almacenes"); }

                var catalogo_fijo = context.Catalogo_Fijo.Where(x => x.Activo && x.Catalogo.Equals(catalogo.Cod)).ToList();

                return Ok(catalogo_fijo);
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
                var catalogo = context.Accion.FirstOrDefault(x => x.Nombre.Equals("Catalogo_Inventario_Localidades"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para localidades"); }

                var catalogo_fijo = context.Catalogo_Fijo.Where(x => x.Activo && x.Catalogo.Equals(catalogo.Cod)).ToList();

                return Ok(catalogo_fijo);
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
                var catalogo = context.Accion.FirstOrDefault(x => x.Nombre.Equals("Catalogo_Inventario_Unidad_Medida"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para unidad de medida"); }

                var catalogo_fijo = context.Catalogo_Fijo.Where(x => x.Activo && x.Catalogo.Equals(catalogo.Cod)).ToList();

                return Ok(catalogo_fijo);
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
                var catalogo = context.Accion.FirstOrDefault(x => x.Nombre.Equals("Catalogo_Inventario_Tipo_Movimientos"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para sitios"); }

                var catalogo_fijo = context.Catalogo_Fijo.Where(x => x.Activo && x.Catalogo.Equals(catalogo.Cod)).ToList();

                return Ok(catalogo_fijo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        #endregion

    }
}
