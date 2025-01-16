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
using SixLabors.ImageSharp;
using System.Security.Claims;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, Lectura Asignacion, Administrador Sistema, Direccion, Gerencia, Ejecutivo de Cuenta Comercial, Programador, Coordinador, Analista Credito, Contador, Auditor, Comprador, Ejecutivo de Cuenta Operativo")]
    public class PedidoController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUsuario> userManager;
        private readonly IUsuarioHelper helper;

        public PedidoController(ApplicationDbContext context, UserManager<IdentityUsuario> userManager, IUsuarioHelper helper)
        {
            this.context = context;
            this.userManager = userManager;
            this.helper = helper;
        }

        //Borrar pedido
        [HttpDelete("{cod:int}/cancel")]
        public async Task<ActionResult> PutCancel([FromRoute] int cod)
        {
            try
            {
                var id_terminal = await helper.GetTerminalIdAsync();

                OrdenEmbarque? pedido = await context.OrdenEmbarque.FirstOrDefaultAsync(x => x.Cod == cod && x.Codtad == id_terminal);

                if (pedido is null)
                    return NotFound();

                pedido.Codest = 14;
                context.Update(pedido);

                var id = await helper.GetUserId();

                await context.SaveChangesAsync(id, 4);

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        //Method para obtener pedidos mediante un rango de fechas
        [HttpGet("filtrar")]
        public async Task<ActionResult> GetDate([FromQuery] Folio_Activo_Vigente _param)
        {
            try
            {
                var id_terminal = await helper.GetTerminalIdAsync();

                if (!DateTime.TryParse(_param.Fecha_Inicio.ToString(), out DateTime Fecha_Inicio)) { return BadRequest("La fecha de inicio no tiene un formato valido."); }
                if (!DateTime.TryParse(_param.Fecha_Fin.ToString(), out DateTime Fecha_Fin)) { return BadRequest("La fecha de fin no tiene un formato valido."); }

                var terminal = context.Tad.FirstOrDefault(x => x.Cod == id_terminal);
                if (terminal is null) { return NotFound(); }

                List<OrdenEmbarque> ordens = new();
                List<OrdenEmbarque> newOrdens = new();
                //órdenes asignadas ordenar por orden compartimento 
                ordens = context.OrdenEmbarque
                    .AsNoTracking()
                    .Where(x => x.Fchcar >= Fecha_Inicio && x.Fchcar <= Fecha_Fin && x.Codest == 3 && x.FchOrd != null
                    && x.Bolguidid == null && x.Folio == null && x.CodordCom != null && x.Tonel != null && x.Codtad == id_terminal)
                    .Include(x => x.Chofer)
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Estado)
                    .Include(x => x.OrdenCompra)
                    .Include(x => x.Tad)
                    .Include(x => x.Producto)
                    .Include(x => x.Tonel)
                    .ThenInclude(x => x.Transportista)
                    .Include(x => x.OrdenCierre)
                    .Include(x => x.Tad)
                    .OrderBy(x => x.Fchpet)
                    .ThenBy(x => x.Tonel!.Tracto)
                    .Include(x => x.OrdenPedido)
                    .Take(10000)
                    .ToList();
                //órdenes sin asignar ordenar por BIN
                var ordensSinAsignar = context.OrdenEmbarque
                    .AsNoTracking()
                    .Where(x => x.Fchcar >= Fecha_Inicio && x.Fchcar <= Fecha_Fin && x.Codest == 3 && x.FchOrd != null
                    && x.Bolguidid == null && x.Folio == null && x.CodordCom != null && x.Tonel == null && x.Codtad == id_terminal)
                    .Include(x => x.Chofer)
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Estado)
                    .Include(x => x.OrdenCompra)
                    .Include(x => x.Tad)
                    .Include(x => x.Producto)
                    .Include(x => x.Tonel)
                    .ThenInclude(x => x.Transportista)
                    .Include(x => x.OrdenCierre)
                    .Include(x => x.OrdenPedido)
                     .OrderBy(x => x.Fchpet)
                    .Take(10000)
                    .ToList();

                ordens.AddRange(ordensSinAsignar);
                ordens.OrderByDescending(x => x.Bin);

                var ordenes = ordens.AsQueryable();

                if (!string.IsNullOrEmpty(_param.Cliente_Filtrado) && !string.IsNullOrWhiteSpace(_param.Cliente_Filtrado))
                    ordenes = ordenes.Where(x => x.Destino != null && x.Destino.Cliente != null && !string.IsNullOrEmpty(x.Destino.Cliente.Den)
                    && x.Destino.Cliente.Den.ToLower().Contains(_param.Cliente_Filtrado.ToLower()));

                if (!string.IsNullOrEmpty(_param.Destino_Filtrado) && !string.IsNullOrWhiteSpace(_param.Destino_Filtrado))
                    ordenes = ordenes.Where(x => x.Destino != null && !string.IsNullOrEmpty(x.Destino.Den) && x.Destino.Den.ToLower().Contains(_param.Destino_Filtrado.ToLower()));

                if (!string.IsNullOrEmpty(_param.Producto_Filtrado) && !string.IsNullOrWhiteSpace(_param.Producto_Filtrado))
                    ordenes = ordenes.Where(x => x.Producto != null && !string.IsNullOrEmpty(x.Producto.Den) && x.Producto.Den.ToLower().Contains(_param.Producto_Filtrado.ToLower()));

                if (_param.Excel)
                {
                    ExcelPackage.LicenseContext = LicenseContext.Commercial;
                    ExcelPackage package = new();
                    package.Workbook.Worksheets.Add("Asignacion de unidades");
                    ExcelWorksheet ws = package.Workbook.Worksheets.First();
                    var ords = ordenes.ToList();

                    if (!string.IsNullOrEmpty(terminal.Den) && terminal.Den.ToUpper() == "TAS SAN JOSE ITURBIDE")
                    {
                        var ordenes_excel = ords.Select(x => new Reporte_Cargas_Tad_SJI
                        {
                            ShipToNo = x.Destino?.Cliente?.Identificador_Externo ?? string.Empty,
                            ShipToName = x.Destino?.Cliente?.Den ?? string.Empty,
                            CustPONo = x.Folio_Vale,
                            SONo = x.Folio_Vale,
                            SOCreateDt = x.Fchcar,
                            CustRDD = x.Fchcar,
                            Material = x.Producto?.Identificador_Externo ?? string.Empty,
                            MaterialDescriptionName = x.Producto?.Den ?? string.Empty,
                            SOOrderQty = x.Tonel != null ? (double?)(x.Tonel.Capcom.HasValue ? (double)x.Tonel.Capcom : x.Tonel.Capcom2.HasValue ? (double)x.Tonel.Capcom2 : x.Tonel.Capcom3.HasValue ? (double)x.Tonel.Capcom3 : (double?)x.Tonel.Capcom4) ?? 0 : 0,
                            StationNameMX = x.Destino?.Den ?? string.Empty,
                            TankCount = x.Tonel?.Tanque,
                            EQUIPO = x.Tonel?.Tracto ?? string.Empty,
                            OPERADOR = x.Chofer?.FullName ?? string.Empty,
                            TRANSPORTISTA = x.Tonel?.Transportista?.Den ?? string.Empty,
                            LICENCIAOPERADOR = x.Chofer?.Licencia ?? string.Empty
                        });

                        ws.Cells["A1"].LoadFromCollection(ordenes_excel, c =>
                        {
                            c.PrintHeaders = true;
                            c.TableStyle = OfficeOpenXml.Table.TableStyles.Medium2;
                        });

                        ws.Cells[1, 13, ws.Dimension.End.Row, 13].Style.Numberformat.Format = "#,##0";
                        ws.Cells[1, 7, ws.Dimension.End.Row, 8].Style.Numberformat.Format = "dd/MM/yyyy";

                        ws.Cells[1, 1, ws.Dimension.End.Row, ws.Dimension.End.Column].AutoFitColumns();

                        return Ok(package.GetAsByteArray());
                    }
                    else
                    {
                        var ordenes_excel = ords.Select(x => new TablaAsignacionUnidadesDTO
                        {
                            OrdenCompra = x.OrdenCompra?.den ?? string.Empty,
                            Referencia = x.OrdenCierre?.Folio ?? string.Empty,
                            Cliente = x.Obtener_Cliente_De_Orden,
                            Destino = x.Obtener_Destino_De_Orden,
                            Producto = x.Obtener_Producto_De_Orden,
                            Volumen = x.Obtener_Volumen_De_Orden(),
                            FechaCarga = x.Fchcar?.ToString("dd/MM/yyyy") ?? string.Empty,
                            ModeloCompra = x.ModeloCompra,
                            TipoVenta = x.TipoVenta,
                            Transportista = x.Tonel?.Transportista?.Den ?? string.Empty,
                            Unidad = x.Tonel?.Veh ?? string.Empty,
                            Compartimento = x.Compartment,
                            Operador = x.Chofer?.FullName ?? string.Empty,
                            Bin = x.Bin,
                            Fecha = x.OrdenCierre?.FechaLlegada ?? string.Empty,
                            Turno = x.OrdenCierre?.Turno ?? string.Empty,
                            Unidad_Negocio = x.Tad?.Den ?? string.Empty
                        });

                        ws.Cells["A1"].LoadFromCollection(ordenes_excel, c =>
                        {
                            c.PrintHeaders = true;
                            c.TableStyle = OfficeOpenXml.Table.TableStyles.Medium2;
                        });

                        ws.Cells[1, 1, ws.Dimension.End.Row, ws.Dimension.End.Column].AutoFitColumns();

                        return Ok(package.GetAsByteArray());
                    }
                }

                return Ok(ordenes);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("filtrar/pendientes")]
        public async Task<ActionResult> GetOrdenPendiente([FromBody] FechasF fechas)
        {
            try
            {
                var id_terminal = await helper.GetTerminalIdAsync();

                List<OrdenEmbarque> ordens = new();

                ordens = await context.OrdenEmbarque
                    .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Codest == 9 && x.Bolguidid == null
                    && x.OrdenCierre != null && !string.IsNullOrEmpty(x.OrdenCierre.Folio) && x.Codtad == id_terminal)
                    .Include(x => x.Chofer)
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Estado)
                    .Include(x => x.OrdenCompra)
                    .Include(x => x.Tad)
                    .Include(x => x.Producto)
                    .Include(x => x.Tonel)
                    .ThenInclude(x => x.Transportista)
                    .Include(x => x.OrdenCierre)
                    .Include(x => x.OrdenPedido)
                    .OrderBy(x => x.Fchpet)
                    .Take(10000)
                    .ToListAsync();

                ordens.OrderByDescending(x => x.Bin);

                return Ok(ordens);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        //Method para obtener pedidos mediante rango de fechas y checbox seleccionado
        //REALIZAR TRES CONDICIONES, UNA POR CADA RADIOBUTTON, QUE SEA EN TRUE SE HARÁ EL MISMO FILTRO POR FECHAS EN WHERE AÑADIENDO LOS CAMPOS QUE UTILIZAN CADA CLAUSULA
        [HttpGet("filtro")]
        public async Task<ActionResult> GetDateRadio([FromQuery] ParametrosBusquedaOrdenes fechas)
        {
            try
            {
                var id_terminal = await helper.GetTerminalIdAsync();

                //editar registros de orden con el nuevo campo de folio en 0 al remplazar los registros
                if (fechas.Estado == 1)
                {

                    //Traerme al bolguid is not null, codest =3 y transportista activo en 1 --Ordenes Sin Cargar--
                    var pedidosDate = context.OrdenEmbarque.IgnoreAutoIncludes()
                    .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.FchOrd != null && x.Codest == 3 && x.Bolguidid != null
                    && x.Tonel != null && x.Tonel.Transportista != null && x.Tonel.Transportista.Activo == true && x.Codtad == id_terminal
                    || x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.FchOrd != null && x.Codest == 22 && x.Bolguidid != null
                    && x.Tonel != null && x.Tonel.Transportista != null && x.Tonel.Transportista.Activo == true && x.Codtad == id_terminal)
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Tad)
                    .Include(x => x.Producto)
                    .Include(x => x.Tonel)
                    .ThenInclude(x => x.Transportista)
                    .Include(x => x.Chofer)
                    .Include(x => x.Estado)
                    .Include(x => x.Orden)
                    .Include(x => x.OrdenCierre)
                    .ThenInclude(x => x.Grupo)
                    //.OrderBy(x => x.Bin)
                    .OrderByDescending(x => x.Bin)
                    //.Select(x => new OrdenesDTO() { Referencia = x.Folio })
                    .Select(o => new Orden()
                    {
                        Cod = o.Cod,
                        Terminal = o.Tad,
                        Ref = o.FolioSyn,
                        Fchcar = o.Fchcar,
                        Estado = o.Estado,
                        Destino = o.Destino,
                        Producto = o.Producto,
                        Vol2 = null!,
                        Vol = o.Vol,
                        Bolguiid = null!,
                        BatchId = null!,
                        Tonel = o.Tonel,
                        Chofer = o.Chofer,
                        Compartimento = o.Compartment,
                        OrdenEmbarque = o
                    })
                    //.OrderBy(x => x.Fchcar)
                    //ordens.OrderByDescending(x => x.Bin);
                    .OrderBy(x => x.Ref)
                    .Take(10000)
                    .AsQueryable();
                    //pedidosDate.OrderByDescending(x => x.Fchcar);

                    //foreach (var item in pedidosDate)
                    //    if (!newOrden.Contains(item))
                    //        newOrden.Add(item);
                    if (!string.IsNullOrEmpty(fechas.producto))
                        pedidosDate = pedidosDate.Where(x => x.Producto != null && !string.IsNullOrEmpty(x.Producto.Den) && x.Producto.Den.ToLower().Contains(fechas.producto.ToLower()));
                    if (!string.IsNullOrEmpty(fechas.destino))
                        pedidosDate = pedidosDate.Where(x => x.Destino != null && !string.IsNullOrEmpty(x.Destino.Den) && x.Destino.Den.ToLower().Contains(fechas.destino.ToLower()));
                    if (!string.IsNullOrEmpty(fechas.cliente))
                        pedidosDate = pedidosDate.Where(x => x.Destino.Cliente != null && !string.IsNullOrEmpty(x.Destino.Cliente.Den) && x.Destino.Cliente.Den.ToLower().Contains(fechas.cliente.ToLower()));
                    if (!string.IsNullOrEmpty(fechas.grupo))
                        pedidosDate = pedidosDate.Where(x => x.OrdenEmbarque.OrdenCierre.Grupo != null && !string.IsNullOrEmpty(x.OrdenEmbarque.OrdenCierre.Grupo.Den) && x.OrdenEmbarque.OrdenCierre.Grupo.Den.ToLower().Contains(fechas.grupo.ToLower()));

                    return Ok(pedidosDate);
                }
                else if (fechas.Estado == 2)
                {
                    //List<Orden> pedidosDate = new List<Orden>();
                    List<Orden> newOrden = new();
                    //Traerme al transportista activo en 1 y codest = 26 --Ordenes Cargadas--
                    var pedidosDate = context.Orden.IgnoreAutoIncludes()
                    .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Codest == 20 && x.Id_Tad == id_terminal)
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Estado)
                    .Include(x => x.Producto)
                    .Include(x => x.Chofer)
                    .Include(x => x.Tonel)
                    .ThenInclude(x => x.Transportista)
                    .Include(x => x.OrdenEmbarque)
                    .Include(x => x.Terminal)
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x.OrdenCierre)
                    .ThenInclude(x => x.Grupo)
                    .Include(x => x.OrdenEmbarque)
                     .OrderBy(x => x.Fchcar)
                    .Take(10000)
                    .AsQueryable();
                    if (!string.IsNullOrEmpty(fechas.producto))
                        pedidosDate = pedidosDate.Where(x => x.Producto != null && !string.IsNullOrEmpty(x.Producto.Den) && x.Producto.Den.ToLower().Contains(fechas.producto.ToLower()));
                    if (!string.IsNullOrEmpty(fechas.destino))
                        pedidosDate = pedidosDate.Where(x => x.Destino != null && !string.IsNullOrEmpty(x.Destino.Den) && x.Destino.Den.ToLower().Contains(fechas.destino.ToLower()));
                    if (!string.IsNullOrEmpty(fechas.cliente))
                        pedidosDate = pedidosDate.Where(x => x.Destino.Cliente != null && !string.IsNullOrEmpty(x.Destino.Cliente.Den) && x.Destino.Cliente.Den.ToLower().Contains(fechas.cliente.ToLower()));
                    if (!string.IsNullOrEmpty(fechas.grupo))
                        pedidosDate = pedidosDate.Where(x => x.OrdenEmbarque.OrdenCierre.Grupo != null && !string.IsNullOrEmpty(x.OrdenEmbarque.OrdenCierre.Grupo.Den) && x.OrdenEmbarque.OrdenCierre.Grupo.Den.ToLower().Contains(fechas.grupo.ToLower()));

                    return Ok(pedidosDate);
                }

                //else if (fechas.Estado == 3)
                //{
                //    List<Orden> newOrden = new();
                //    //Traerme al transportista activo en 1 --Ordenes en trayecto-- 
                //    var pedidosDate = context.Orden.IgnoreAutoIncludes()
                //    .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Tonel != null && x.Tonel.Transportista != null
                //    && x.Tonel.Transportista.Activo == true && x.Codest == 26 && x.Id_Tad == id_terminal)
                //    .Include(x => x.Destino)
                //    .ThenInclude(x => x.Cliente)
                //    .Include(x => x.Producto)
                //    .Include(x => x.Tonel)
                //    .ThenInclude(x => x.Transportista)
                //    .Include(x => x.Estado)
                //    .Include(x => x.Chofer)
                //    .Include(x => x.Terminal)
                //    .Include(x => x.OrdenEmbarque)
                //    .OrderBy(x => x.Fchcar)
                //    .Take(10000)
                //    .AsQueryable();
                //    //pedidosDate.OrderByDescending(x => x.Fchcar);
                //    return Ok(pedidosDate);
                //}

                else if (fechas.Estado == 4)
                {
                    //Ordenes canceladas
                    List<Orden> ordenesCanceladas = new();

                    var pedidosDate = context.Orden.IgnoreAutoIncludes()
                        .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Codest == 14 && x.Id_Tad == id_terminal)
                        .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Estado)
                    .Include(x => x.Producto)
                    .Include(x => x.Chofer)
                    .Include(x => x.Tonel)
                    .ThenInclude(x => x.Transportista)
                    .Include(x => x.OrdenEmbarque)
                    .Include(x => x.Terminal)
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x.OrdenCierre)
                    .ThenInclude(x => x.Grupo)
                    .Include(x => x.OrdenEmbarque)
                     .OrderBy(x => x.Fchcar)
                    .Take(10000)
                    .AsQueryable();

                    //pedidosDate.OrderByDescending(x => x.Fchcar);
                    if (pedidosDate is not null)
                        ordenesCanceladas.AddRange(pedidosDate);

                    var ordenes = context.OrdenEmbarque.IgnoreAutoIncludes()
                    .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.FchOrd != null && x.Codest == 14 && x.Bolguidid != null && x.Codtad == id_terminal)
                  .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Tad)
                    .Include(x => x.Producto)
                    .Include(x => x.Tonel)
                    .ThenInclude(x => x.Transportista)
                    .Include(x => x.Chofer)
                    .Include(x => x.Estado)
                    .Include(x => x.Orden)
                    .Include(x => x.OrdenCierre)
                    .ThenInclude(x => x.Grupo)
                    .Select(o => new Orden()
                    {
                        Cod = o.Cod,
                        Ref = o.FolioSyn,
                        Terminal = o.Tad,
                        //Ref = o.ref
                        Fchcar = o.Fchcar,
                        Estado = o.Estado,
                        Destino = o.Destino,
                        Producto = o.Producto,
                        Vol2 = o.Vol,
                        Vol = null!,
                        Bolguiid = null!,
                        BatchId = null!,
                        Tonel = o.Tonel,
                        Chofer = o.Chofer,
                        Compartimento = o.Compartment,
                        OrdenEmbarque = o
                    })
                    .OrderBy(x => x.Fchcar)
                    .Take(10000)
                    .AsQueryable();

                    if (!string.IsNullOrEmpty(fechas.producto))
                        ordenes = ordenes.Where(x => x.Producto != null && !string.IsNullOrEmpty(x.Producto.Den) && x.Producto.Den.ToLower().Contains(fechas.producto.ToLower()));
                    if (!string.IsNullOrEmpty(fechas.destino))
                        ordenes = ordenes.Where(x => x.Destino != null && !string.IsNullOrEmpty(x.Destino.Den) && x.Destino.Den.ToLower().Contains(fechas.destino.ToLower()));
                    if (!string.IsNullOrEmpty(fechas.cliente))
                        ordenes = ordenes.Where(x => x.Destino.Cliente != null && !string.IsNullOrEmpty(x.Destino.Cliente.Den) && x.Destino.Cliente.Den.ToLower().Contains(fechas.cliente.ToLower()));
                    if (!string.IsNullOrEmpty(fechas.grupo))
                        ordenes = ordenes.Where(x => x.OrdenEmbarque.OrdenCierre.Grupo != null && !string.IsNullOrEmpty(x.OrdenEmbarque.OrdenCierre.Grupo.Den) && x.OrdenEmbarque.OrdenCierre.Grupo.Den.ToLower().Contains(fechas.grupo.ToLower()));

                    if (ordenes is not null)
                        ordenesCanceladas.AddRange(ordenes);

                    return Ok(ordenes);
                }

                else if (fechas.Estado == 5)
                {
                    //Traerme al bolguid is not null, codest =3 y transportista activo en 1 --Ordenes Sin Cargar--
                    var pedidosDate = context.OrdenEmbarque.IgnoreAutoIncludes()
                    .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.FchOrd != null && x.Codest == 14 && x.Bolguidid != null && x.Codtad == id_terminal
                    || x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.FchOrd != null && x.Codest == 22 && x.Bolguidid != null
                    && x.Tonel != null && x.Tonel.Transportista != null && x.Tonel.Transportista.Activo == true && x.Codtad == id_terminal
                    || x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Codest == 20 && x.Codtad == id_terminal)
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Tad)
                    .Include(x => x.Producto)
                    .Include(x => x.Tonel)
                    .ThenInclude(x => x.Transportista)
                    .Include(x => x.Chofer)
                    .Include(x => x.Estado)
                    .Include(x => x.Orden)
                    .Include(x => x.OrdenCierre)
                    .ThenInclude(x => x.Grupo)
                    //.OrderBy(x => x.Bin)
                    .OrderByDescending(x => x.Bin)
                    //.Select(x => new OrdenesDTO() { Referencia = x.Folio })
                    .Select(o => new Orden()
                    {
                        Cod = o.Cod,
                        Terminal = o.Tad,
                        Ref = o.FolioSyn,
                        Fchcar = o.Orden!.Fchcar,
                        Estado = o.Estado,
                        Destino = o.Destino,
                        Producto = o.Producto,
                        Vol2 = o.Vol,
                        Vol = o.Orden.Vol,
                        Bolguiid = null!,
                        BatchId = o.Orden!.BatchId,
                        Tonel = o.Tonel,
                        Chofer = o.Chofer,
                        Compartimento = o.Compartment,
                        OrdenEmbarque = o
                    })
                    .OrderBy(x => x.Ref)
                    .Take(10000)
                    .AsQueryable();
                    if (!string.IsNullOrEmpty(fechas.producto))
                        pedidosDate = pedidosDate.Where(x => x.Producto != null && !string.IsNullOrEmpty(x.Producto.Den) && x.Producto.Den.ToLower().Contains(fechas.producto.ToLower()));
                    if (!string.IsNullOrEmpty(fechas.destino))
                        pedidosDate = pedidosDate.Where(x => x.Destino != null && !string.IsNullOrEmpty(x.Destino.Den) && x.Destino.Den.ToLower().Contains(fechas.destino.ToLower()));
                    if (!string.IsNullOrEmpty(fechas.cliente))
                        pedidosDate = pedidosDate.Where(x => x.Destino.Cliente != null && !string.IsNullOrEmpty(x.Destino.Cliente.Den) && x.Destino.Cliente.Den.ToLower().Contains(fechas.cliente.ToLower()));
                    if (!string.IsNullOrEmpty(fechas.grupo))
                        pedidosDate = pedidosDate.Where(x => x.OrdenEmbarque.OrdenCierre.Grupo != null && !string.IsNullOrEmpty(x.OrdenEmbarque.OrdenCierre.Grupo.Den) && x.OrdenEmbarque.OrdenCierre.Grupo.Den.ToLower().Contains(fechas.grupo.ToLower()));

                    return Ok(pedidosDate);

                }


                else
                {
                    return BadRequest();
                }


            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("Historial")]
        public async Task<ActionResult> GetHistorial([FromBody] FechasF fechas)
        {

            try
            {
                var id_terminal = await helper.GetTerminalIdAsync();

                List<Orden> Ordenes = new();
                var pedidosDate = await context.OrdenEmbarque.IgnoreAutoIncludes()
                .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Bolguidid != null && x.FchOrd != null && x.Codest == 3 && x.Tonel!.Transportista!.Activo == true || x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.FchOrd != null && x.Codest == 22 && x.Bolguidid != null
                && x.Tonel != null && x.Tonel.Transportista != null && x.Tonel.Transportista.Activo == true && x.Codtad == id_terminal)
                .Include(x => x.Destino)
                .ThenInclude(x => x.Cliente)
                .Include(x => x.Tad)
                .Include(x => x.Producto)
                .Include(x => x.Tonel)
                .ThenInclude(x => x.Transportista)
                .Include(x => x.Chofer)
                .Include(x => x.Estado)
                .Select(o => new Orden()
                {
                    Cod = o.Cod,
                    Ref = o.FolioSyn,
                    Fchcar = o.Fchcar,
                    Estado = o.Estado,
                    Destino = o.Destino,
                    Producto = o.Producto,
                    Vol2 = null!,
                    Vol = o.Vol,
                    Bolguiid = null!,
                    BatchId = null!,
                    Tonel = o.Tonel,
                    Chofer = o.Chofer,
                    Compartimento = o.Compartment
                })
                .OrderBy(x => x.Fchcar)
                .Take(10000)
                .ToListAsync();
                Ordenes.AddRange(pedidosDate);
                var pedidosDate2 = await context.Orden.IgnoreAutoIncludes()
                .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Tonel != null && x.Tonel.Transportista != null
                && x.Tonel.Transportista.Activo == true && x.Codest == 20 && x.Id_Tad == id_terminal)
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Estado)
                    .Include(x => x.Producto)
                    .Include(x => x.Chofer)
                    .Include(x => x.Tonel)
                    .ThenInclude(x => x.Transportista)
                    .Include(x => x.OrdenEmbarque)
                    .OrderBy(x => x.Fchcar)
                    .Take(10000)
                    .ToListAsync();
                Ordenes.AddRange(pedidosDate2);

                var pedidosDate3 = await context.Orden.IgnoreAutoIncludes()
                .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Tonel != null && x.Tonel.Transportista != null
                && x.Tonel.Transportista.Activo == true && x.Codest == 26)
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Producto)
                    .Include(x => x.Tonel)
                    .ThenInclude(x => x.Transportista)
                    .Include(x => x.Estado)
                    .Include(x => x.Chofer)
                    .Include(x => x.OrdenEmbarque)
                    .OrderBy(x => x.Fchcar)
                    .Take(10000)
                    .ToListAsync();
                Ordenes.AddRange(pedidosDate3);
                //Ordenes Canceladas
                List<Orden> ordenesCanceladas = new();
                var pedidosDate4 = await context.Orden.IgnoreAutoIncludes()
                    .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Codest == 14 && x.Id_Tad == id_terminal)
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Producto)
                    .Include(x => x.Tonel)
                    .ThenInclude(x => x.Transportista)
                    .Include(x => x.Estado)
                    .Include(x => x.Chofer)
                    .Include(x => x.OrdenEmbarque)
                    .Take(10000)
                    .ToListAsync();
                if (pedidosDate4 is not null)
                    ordenesCanceladas.AddRange(pedidosDate4);
                var orden = await context.OrdenEmbarque.IgnoreAutoIncludes()
                  .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.FchOrd != null && x.Codest == 14 && x.Bolguidid != null && x.Codtad == id_terminal)
                  .Include(x => x.Destino)
                  .ThenInclude(x => x.Cliente)
                  .Include(x => x.Tad)
                  .Include(x => x.Producto)
                  .Include(x => x.Tonel)
                  .ThenInclude(x => x.Transportista)
                  .Include(x => x.Chofer)
                  .Include(x => x.Estado)
                  //.Select(x => new OrdenesDTO() { Referencia = x.Folio })
                  .Select(o => new Orden()
                  {
                      Cod = o.Cod,
                      Ref = o.FolioSyn,
                      //Ref = o.ref
                      Fchcar = o.Fchcar,
                      Estado = o.Estado,
                      Destino = o.Destino,
                      Producto = o.Producto,
                      Vol2 = o.Vol,
                      Vol = null!,
                      Bolguiid = null!,
                      BatchId = null!,
                      Tonel = o.Tonel,
                      Chofer = o.Chofer,
                      Compartimento = o.Compartment
                  })
                  .OrderBy(x => x.Fchcar)
                  .Take(10000)
                  .ToListAsync();
                if (orden is not null)
                    ordenesCanceladas.AddRange(orden);
                Ordenes.AddRange(orden);

                return Ok(Ordenes);

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        //Method para realizar (agregar) pedido
        //[HttpPost]
        private async Task<ActionResult> Post(OrdenEmbarque orden)
        {
            try
            {
                var id_terminal = await helper.GetTerminalIdAsync();

                var user = await context.Usuario.FirstOrDefaultAsync(x => x.Usu == HttpContext.User.FindFirstValue(ClaimTypes.Name));
                if (user == null)
                    return NotFound();

                orden.Codusu = user!.Cod;
                orden.Codtad = id_terminal;
                context.Add(orden);

                var id = await helper.GetUserId();
                await context.SaveChangesAsync(id, 2);

                var NewOrden = await context.OrdenEmbarque.Where(x => x.Cod == orden.Cod && x.Codtad == id_terminal)
                    .Include(x => x.Destino)
                    .Include(x => x.Tad)
                    .Include(x => x.Producto)
                    .Include(x => x.Tonel)
                    .Include(x => x.Estado)
                    .FirstOrDefaultAsync();

                return Ok(NewOrden);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        //Confirmar pedido
        [HttpPost("confirm")]
        public async Task<ActionResult<OrdenCompra>> PostConfirm(List<OrdenEmbarque> orden)
        {
            try
            {
                var id_terminal = await helper.GetTerminalIdAsync();

                var terminal = context.Tad.Single(x => x.Cod == id_terminal);

                OrdenCompra newFolio = new();
                var folio = await context.OrdenCompra.Select(x => x.cod).OrderBy(x => x).LastOrDefaultAsync();
                if (folio != 0)
                {
                    ++folio;
                    newFolio = new OrdenCompra() { den = $"{terminal.CodigoOrdenes}_{DateTime.Now:yyyy-MM-dd}_{folio}" };
                    context.Add(newFolio);
                }
                orden.ForEach(x =>
                {
                    x.Destino = null!;
                    x.Estado = null!;
                    x.Tad = null!;
                    x.Chofer = null!;
                    x.Tonel = null!;
                    x.Producto = null;
                    x.OrdenCierre = null!;
                    x.Cliente = null!;
                    x.OrdenPedido = null!;
                    x.Orden = null!;
                    x.Codest = 3;
                    x.CodordCom = folio;
                    x.FchOrd = DateTime.Today.Date;
                });

                context.UpdateRange(orden);

                var id = await helper.GetUserId();

                await context.SaveChangesAsync(id, 15);
                return Ok(newFolio);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost("check/chofer")]//TODO: checar utilidad
        public async Task<ActionResult> PostConfirmChofer([FromBody] CheckChofer checkChofer)
        {
            try
            {
                var id_terminal = await helper.GetTerminalIdAsync();

                var chofer = await context.OrdenEmbarque.FirstOrDefaultAsync(x => x.Codton == checkChofer.Tonel
                && x.Codchf == checkChofer.Chofer && x.CompartmentId == checkChofer.Compartimento && x.Fchcar == checkChofer.FechaCarga
                && x.Bolguidid == null && x.Codtad == id_terminal);
                if (chofer == null)
                {
                    return Ok(0);
                }
                return Ok(chofer.Cod);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("desasignar/{code:int}")]
        public async Task<ActionResult> PutAsignacion(int code)
        {
            try
            {
                var id_terminal = await helper.GetTerminalIdAsync();

                var orden = await context.OrdenEmbarque.FirstOrDefaultAsync(x => x.Cod == code && x.Codtad == id_terminal);

                if (orden is null)
                    return NotFound();

                orden.Chofer = null;
                orden.Tonel = null;
                //orden.Tad = null;
                orden.Codchf = null;
                orden.Codton = null;
                orden.Compartment = null;
                orden.CompartmentId = null;

                context.Update(orden);
                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("update")]
        public async Task<ActionResult> PutPedido([FromBody] OrdenCierre cierre)
        {
            try
            {
                var id_terminal = await helper.GetTerminalIdAsync();

                OrdenEmbarque? orden = cierre.OrdenEmbarque;

                cierre.Producto = null!;
                cierre.Destino = null!;
                cierre.Cliente = null!;
                cierre.ContactoN = null!;
                cierre.OrdenEmbarque = null!;
                cierre.OrdenPedidos = null!;
                cierre.Terminal = null!;

                context.Update(cierre);

                if (orden != null)
                {
                    orden.Producto = null!;
                    orden.Chofer = null!;
                    orden.Destino = null!;
                    orden.Tonel = null!;
                    orden.Tad = null!;
                    orden.OrdenCompra = null!;
                    orden.Estado = null!;
                    orden.Cliente = null!;
                    orden.OrdenCierre = null!;
                    orden.OrdenPedido = null!;

                    orden.Codprd = cierre.CodPrd;
                    orden.Coddes = cierre.CodDes;
                    orden.Fchcar = cierre.FchCar;
                    orden.Codtad = cierre.Id_Tad;
                    orden.Vol = cierre.Volumen;
                    orden.Pre = cierre.Precio;

                    context.Update(orden);
                }

                await context.SaveChangesAsync();

                var newOrden = context.OrdenCierre.Where(x => x.Cod == cierre.Cod && x.Id_Tad == id_terminal)
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x.Tad)
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x.Estado)
                    .Include(x => x.Destino)
                    .Include(x => x.Producto)
                    .Include(x => x.Cliente)
                    .IgnoreAutoIncludes()
                    .FirstOrDefault();

                return Ok(newOrden);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost("asignar/unidad")]
        public async Task<ActionResult> AsignarPedido([FromBody] OrdenEmbarque orden)
        {
            try
            {
                var id_terminal = await helper.GetTerminalIdAsync();

                orden.Producto = null;
                orden.Chofer = null;
                orden.Destino = null;
                orden.Tonel = null;
                orden.Tad = null;
                orden.OrdenCompra = null;
                orden.Estado = null;
                orden.Cliente = null!;
                orden.OrdenCierre = null!;
                orden.OrdenPedido = null!;

                context.Update(orden);
                await context.SaveChangesAsync();

                var ord = await context.OrdenEmbarque.Where(x => x.Cod == orden.Cod && x.Codtad == id_terminal)
                    .Include(x => x.Producto)
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Tonel)
                    .ThenInclude(x => x.Transportista)
                    .Include(x => x.Tad)
                    .Include(x => x.Estado)
                    .Include(x => x.OrdenCompra)
                    .Include(x => x.Chofer)
                    .Include(x => x.OrdenCierre)
                    .FirstOrDefaultAsync();

                return Ok(ord);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost("historial/envios")]
        public async Task<ActionResult> GetDateHistorico([FromBody] FechasF fechas)
        {
            try
            {
                var id_terminal = await helper.GetTerminalIdAsync();

                List<OrdenEmbarque> ordens = new();

                ordens = await context.OrdenEmbarque
                    .Where(x => x.Fchcar >= fechas.DateInicio && x.Fchcar <= fechas.DateFin && x.Codtad == id_terminal)
                    .Include(x => x.Chofer)
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Estado)
                    .Include(x => x.OrdenCompra)
                    .Include(x => x.Tad)
                    .Include(x => x.Producto)
                    .Include(x => x.Tonel)
                    .ThenInclude(x => x.Transportista)
                    .Include(x => x.OrdenCierre)
                    .ThenInclude(x => x.Grupo)
                    .Include(x => x.OrdenPedido)
                    .OrderBy(x => x.Fchpet)
                    .Take(10000)
                    .ToListAsync();

                ordens.OrderByDescending(x => x.Bin);

                return Ok(ordens.DistinctBy(x => x.Cod));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("verificar/carga/{ID_Cierre:int}")]//TODO: checar utilidad
        public async Task<ActionResult> Verificar_Volumen_Creacion_Orden([FromBody] OrdenCierre orden, [FromRoute] int ID_Cierre)
        {
            try
            {
                //return Ok(true);
                var id_terminal = await helper.GetTerminalIdAsync();

                if (orden == null)
                    return BadRequest();

                OrdenCierre? cierre = new();

                if (ID_Cierre != 0)
                    cierre = context.OrdenCierre.FirstOrDefault(x => x.Cod == ID_Cierre && x.Id_Tad == id_terminal);
                else
                    cierre = context.OrdenCierre.FirstOrDefault(x => x.Folio == orden.Folio_Perteneciente && x.CodPrd == orden.CodPrd && x.Id_Tad == id_terminal);

                if (cierre is null)
                    return BadRequest("No existe el cierre.");

                if (context.OrdenPedido.Any(x => !string.IsNullOrEmpty(cierre.Folio) && x.Folio == cierre.Folio))
                {
                    var VolumenDisponible = cierre.Volumen;

                    var listConsumido = context.OrdenPedido.Where(x => x.OrdenEmbarque != null && x.OrdenEmbarque.Tonel != null && !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(cierre.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Tonel != null && x.OrdenEmbarque.Codprd == cierre.CodPrd
                            && x.OrdenEmbarque.Codest == 22
                            && x.OrdenEmbarque.Folio != null
                            && x.OrdenEmbarque.Bolguidid != null)
                            .Include(x => x.OrdenEmbarque)
                            .ThenInclude(x => x.Tonel).ToList();

                    var VolumenCongelado = listConsumido.Sum(item => item.OrdenEmbarque!.Compartment == 1 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom!.ToString())
                                            : item.OrdenEmbarque!.Compartment == 2 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom2!.ToString())
                                            : item.OrdenEmbarque!.Compartment == 3 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom3!.ToString())
                                            : item.OrdenEmbarque!.Compartment == 4 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom4!.ToString())
                                            : item.OrdenEmbarque!.Vol);

                    var VolumenConsumido = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(cierre.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Orden != null && x.OrdenEmbarque.Folio != null
                                && x.OrdenEmbarque.Orden.Codest != 14
                                && x.OrdenEmbarque.Codest != 14
                            && x.OrdenEmbarque.Codprd == cierre.CodPrd
                            && x.OrdenEmbarque.Orden.BatchId != null)
                                .Include(x => x.OrdenEmbarque)
                                .ThenInclude(x => x.Orden)
                                .Sum(x => x.OrdenEmbarque!.Orden!.Vol);

                    var VolumenProgramado = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(cierre.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Folio == null
                                && x.OrdenEmbarque.Codprd == cierre.CodPrd
                                && x.OrdenEmbarque.Bolguidid == null && x.OrdenEmbarque.Codest == 3
                                && x.OrdenEmbarque.FchOrd != null)
                                .Include(x => x.OrdenEmbarque)
                                    .Sum(x => x.OrdenEmbarque!.Vol);

                    var VolumenSolicitado = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(cierre.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Folio == null
                            && x.OrdenEmbarque.Codprd == cierre.CodPrd
                            && x.OrdenEmbarque.Bolguidid == null && x.OrdenEmbarque.Codest == 9
                            && x.OrdenEmbarque.FchOrd == null)
                                .Include(x => x.OrdenEmbarque)
                                .Sum(x => x.OrdenEmbarque!.Vol);

                    var VolumenTotalDisponible = VolumenDisponible - (VolumenConsumido + VolumenCongelado + VolumenProgramado + VolumenSolicitado);

                    if (VolumenTotalDisponible < orden.Volumen)
                        return Ok(false);
                }

                return Ok(true);
            }
            catch (ArgumentNullException)
            {
                return BadRequest("La orden o el codigo de cierre no pueden ir vacios o con valores nulos");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("orden/{referencia?}")]//TODO: checar utilidad
        public async Task<ActionResult> GetOrdens([FromRoute] string? referencia)
        {
            try
            {
                var id_terminal = await helper.GetTerminalIdAsync();

                List<OrdenEmbarque> ordenes = new();

                if (!string.IsNullOrEmpty(referencia) || !string.IsNullOrWhiteSpace(referencia))
                    ordenes = await context.OrdenEmbarque.Where(x => x.FolioSyn == referencia && x.Codtad == id_terminal)
                        .Include(x => x.Producto)
                        .Include(x => x.Estado)
                        .Include(x => x.Destino)
                        .Include(x => x.Tonel)
                        .Include(x => x.Chofer)
                        .Include(x => x.OrdenCierre)
                        .IgnoreAutoIncludes()
                        .ToListAsync();

                foreach (var item in ordenes)
                {
                    List<Orden> Ordenes_Synthesis = context.Orden.Where(x => x.Ref == item.FolioSyn)
                        .Include(x => x.Destino)
                        .Include(x => x.Producto)
                        .Include(x => x.Estado)
                        .IgnoreAutoIncludes()
                        .ToList();

                    if (Ordenes_Synthesis is not null)
                        item.Ordenes_Synthesis.AddRange(Ordenes_Synthesis);
                }

                return Ok(ordenes.OrderByDescending(x => x.Fchcar));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("cancel/{cod:int}")]//TODO: checar utilidad
        public async Task<ActionResult> CancelPedido([FromRoute] int cod)
        {
            try
            {
                var id_terminal = await helper.GetTerminalIdAsync();

                OrdenEmbarque? pedido = context.OrdenEmbarque.FirstOrDefault(x => x.Cod == cod && x.Codtad == id_terminal);

                if (pedido is null)
                    return NotFound(pedido);

                OrdenCierre? cierre = context.OrdenCierre.FirstOrDefault(x => x.CodPed == pedido.Cod);

                pedido.Codest = 14;

                if (cierre is not null)
                    cierre.Estatus = false;

                context.Update(pedido);

                if (cierre is not null)
                    context.Update(cierre);

                var id = await helper.GetUserId();

                await context.SaveChangesAsync(id, 4);

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("cancel/orden/{cod:int}")]// TODO: checar utilidad
        public async Task<ActionResult> Cancelar_Orden_Y_Ordenes_Synthesis([FromRoute] int cod)
        {
            try
            {
                var id_terminal = await helper.GetTerminalIdAsync();

                OrdenEmbarque? pedido = context.OrdenEmbarque.FirstOrDefault(x => x.Cod == cod && x.Codtad == id_terminal);

                if (pedido is null)
                    return NotFound();

                OrdenCierre? cierre = context.OrdenCierre.FirstOrDefault(x => x.CodPed == pedido.Cod);

                pedido.Codest = 14;

                if (cierre is not null)
                    cierre.Estatus = false;

                context.Update(pedido);

                if (cierre is not null)
                    context.Update(cierre);

                List<Orden>? ordenes = context.Orden.Where(x => x.Folio == pedido.Folio).ToList();

                foreach (var item in ordenes)
                {
                    item.Codest = 14;
                    context.Update(item);
                }

                var id = await helper.GetUserId();

                await context.SaveChangesAsync(id, 4);

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("cancel/orden/synthesis/{cod:int}")]//TODO: checar utilidad
        public async Task<ActionResult> Cancelar_Ordenes_Synthesis([FromRoute] int cod)
        {
            try
            {
                var id_terminal = await helper.GetTerminalIdAsync();

                Orden? orden = context.Orden.FirstOrDefault(x => x.Cod == cod && x.Id_Tad == id_terminal);

                if (orden is not null)
                {
                    orden.Codest = 14;
                    context.Update(orden);
                }

                var id = await helper.GetUserId();

                await context.SaveChangesAsync(id, 4);

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        [HttpPost("recrear")]
        public async Task<ActionResult> RecrearOrden([FromBody] List<OrdenEmbarque> ordenes)
        {
            try
            {
                var id_terminal = await helper.GetTerminalIdAsync();

                List<OrdenCierre> cierres = new();
                List<OrdenEmbarque> ordenEmbarques = new();
                OrdenCierre? ordenCierre = new();
                OrdenEmbarque ordenEmbarque = new();
                OrdenPedido ordenPedido = new();

                Cliente? Cliente = new();
                Grupo? Grupo = new();

                var consecutivo = context.Consecutivo.Include(x => x.Terminal).FirstOrDefault(x => x.Nombre == "Orden" && x.Id_Tad == id_terminal);
                if (consecutivo is null)
                {
                    Consecutivo Nuevo_Consecutivo = new() { Numeracion = 1, Nombre = "Orden", Id_Tad = id_terminal };
                    context.Add(Nuevo_Consecutivo);
                    await context.SaveChangesAsync();
                    consecutivo = Nuevo_Consecutivo;
                }
                else
                {
                    consecutivo.Numeracion++;
                    context.Update(consecutivo);
                    await context.SaveChangesAsync();
                }

                var guidfolio = $"RE{DateTime.Now:yy}-{consecutivo.Numeracion:0000000}-{consecutivo.Obtener_Codigo_Terminal}";

                foreach (var item in ordenes)
                {
                    var ordercopy = item.ShallowCopy();

                    var destino = context.Destino.FirstOrDefault(x => x.Cod == ordercopy.Coddes);
                    if (destino is null)
                        return BadRequest("No se encontro el destino");

                    var cliente = context.Cliente.FirstOrDefault(x => x.Cod == destino.Codcte);
                    if (cliente is null)
                        return BadRequest("No se encontro el cliente");

                    ordercopy.Cod = 0;
                    ordercopy.Codest = 3;
                    ordercopy.Fchcar = DateTime.Today;
                    ordercopy.Fchpet = DateTime.Now;
                    ordercopy.Bolguidid = null;
                    ordercopy.Folio = null;
                    ordercopy.CodordCom = null;
                    ordercopy.FchOrd = DateTime.Now;
                    ordenCierre = ordercopy.OrdenCierre;

                    ordercopy.Chofer = null!;
                    ordercopy.Destino = null!;
                    ordercopy.Producto = null!;
                    ordercopy.Cliente = null!;
                    ordercopy.OrdenCierre = null!;
                    ordercopy.OrdenCompra = null!;
                    ordercopy.OrdenPedido = null!;
                    ordercopy.Estado = null!;
                    ordercopy.Transportista = null!;
                    ordercopy.Tonel = null!;
                    ordercopy.Tad = null!;

                    context.Add(ordercopy);
                    await context.SaveChangesAsync();

                    if (ordenCierre != null)
                    {
                        var ordencierrecopy = ordenCierre.ShallowCopy();
                        ordencierrecopy.OrdenPedidos = null!;
                        ordencierrecopy.Folio = guidfolio;
                        ordencierrecopy.CodPed = ordercopy.Cod;
                        ordencierrecopy.Cod = 0;
                        ordencierrecopy.FchCar = DateTime.Today;
                        ordencierrecopy.FchCierre = DateTime.Today;

                        ordencierrecopy.Destino = null!;
                        ordencierrecopy.Cliente = null!;
                        ordencierrecopy.Producto = null!;
                        ordencierrecopy.OrdenEmbarque = null!;
                        ordencierrecopy.Grupo = null!;
                        ordencierrecopy.Terminal = null!;

                        context.Add(ordencierrecopy);
                        await context.SaveChangesAsync();

                        if (context.OrdenPedido.Any(x => x.CodPed == item.Cod))
                        {
                            var op = context.OrdenPedido.FirstOrDefault(x => x.CodPed == item.Cod);

                            ordenPedido = new OrdenPedido()
                            {
                                Folio = op.Folio,
                                CodPed = ordercopy.Cod,
                                CodCierre = ordencierrecopy.Cod,
                                Folio_Cierre_Copia = guidfolio,
                                Pedido_Original = item.Cod
                            };

                            context.Add(ordenPedido);
                            await context.SaveChangesAsync();
                        }
                        else
                        {
                            ordenPedido = new OrdenPedido()
                            {
                                Folio = ordenCierre?.Folio,
                                CodPed = ordercopy.Cod,
                                CodCierre = ordencierrecopy.Cod,
                                Pedido_Original = item?.Cod,
                                Folio_Cierre_Copia = guidfolio
                            };

                            context.Add(ordenPedido);
                            await context.SaveChangesAsync();
                        }
                    }

                    if (ordenCierre is null)
                    {
                        var cierre = new OrdenCierre()
                        {
                            Folio = guidfolio,
                            CodPed = ordercopy.Cod,
                            FchCar = DateTime.Today,
                            FchCierre = DateTime.Today,
                            fchPrecio = DateTime.Now,
                            FchVencimiento = DateTime.Today.AddDays(5),
                            FchLlegada = DateTime.Today.AddDays(1),
                            Precio = ordercopy.Pre ?? 0,
                            CodDes = ordercopy.Coddes,
                            CodGru = cliente?.Codgru,
                            CodCte = cliente?.Cod,
                            CodPrd = ordercopy.Codprd,
                            Id_Tad = ordercopy.Codtad,
                            Volumen = int.Parse($"{ordercopy.Vol}"),
                            Moneda = ordercopy.Moneda,
                            Equibalencia = ordercopy.Equibalencia
                        };

                        context.Add(cierre);
                        await context.SaveChangesAsync();

                        if (context.OrdenPedido.Any(x => x.CodPed == item.Cod))
                        {
                            var op = context.OrdenPedido.FirstOrDefault(x => x.CodPed == item.Cod);

                            ordenPedido = new OrdenPedido()
                            {
                                Folio = op.Folio,
                                CodPed = ordercopy.Cod,
                                CodCierre = cierre.Cod,
                                Pedido_Original = item?.Cod,
                                Folio_Cierre_Copia = guidfolio
                            };

                            context.Add(ordenPedido);
                            await context.SaveChangesAsync();
                        }
                        else
                        {
                            ordenPedido = new OrdenPedido()
                            {
                                Folio = ordenCierre?.Folio,
                                CodPed = ordercopy.Cod,
                                CodCierre = cierre.Cod,
                                Pedido_Original = item?.Cod,
                                Folio_Cierre_Copia = guidfolio
                            };

                            context.Add(ordenPedido);
                            await context.SaveChangesAsync();
                        }
                    }
                }

                return Ok(new CodDenDTO() { Den = guidfolio });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpGet("folios")]//TODO: checar utilidad
        public async Task<ActionResult> GetFoliosFromPedidos([FromQuery] OrdenCierre ordenCierre)
        {
            try
            {
                var id_terminal = await helper.GetTerminalIdAsync();

                List<FolioDetalleDTO> Folios = new();
                if (ordenCierre is null)
                    return BadRequest("Datos vacios");

                var foliosquery = context.OrdenCierre.Where(x => !string.IsNullOrEmpty(x.Folio) && x.CodPed == 0 && x.Estatus == true && x.Activa == true
                    && !x.Folio.StartsWith("RE") && !x.Folio.StartsWith("OP") && x.FchVencimiento > DateTime.Today && x.Id_Tad == id_terminal)
                    .Include(x => x.Producto).Include(x => x.Cliente).Include(x => x.Destino).AsQueryable();

                if (ordenCierre.CodGru != 0 && ordenCierre.CodGru != null)
                    foliosquery = foliosquery.Where(x => x.CodGru == ordenCierre.CodGru);
                if (ordenCierre.CodCte != 0 && ordenCierre.CodCte != null)
                    foliosquery = foliosquery.Where(x => x.CodCte == ordenCierre.CodCte || x.Folio.StartsWith("G"));
                if (ordenCierre.CodDes != 0 && ordenCierre.CodDes != null)
                    foliosquery = foliosquery.Where(x => x.CodDes == ordenCierre.CodDes || x.Folio.StartsWith("G"));

                if (foliosquery is not null)
                {
                    var folioDetalle = foliosquery
                        .Select(x => new ListadoFolioDetalle()
                        {
                            Folio = x.Folio,
                            Producto = x.Producto.Den ?? string.Empty,
                            Cliente = x.Cliente.Den ?? string.Empty,
                            Destino = x.Destino.Den ?? string.Empty,
                            Comentarios = x.Observaciones,
                            Fecha_Cierre = x.FchCierre
                        }).OrderBy(x => x.Fecha_Cierre);
                    return Ok(folioDetalle);
                }
                else
                    return Ok(new List<FolioDetalleDTO>());
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("create/all")]
        public async Task<ActionResult> GetFolioToOrden([FromBody] OrdenCierre ordenCierre)
        {
            try
            {
                var id_terminal = await helper.GetTerminalIdAsync();

                if (!string.IsNullOrEmpty(ordenCierre.Folio_Perteneciente))
                {
                    var cierre = context.OrdenCierre.Where(x => x.Folio == ordenCierre.Folio_Perteneciente && x.Id_Tad == id_terminal).ToList();
                    if (cierre is not null)
                    {
                        if (!cierre.Where(x => x.CodPrd == ordenCierre.CodPrd).Any())
                        {
                            return BadRequest("El producto seleccionado no se encuentra en el cierre");
                        }
                    }
                }

                var id = await helper.GetUserId();

                if (ordenCierre is null)
                    return BadRequest("No se encontro ninguna orden");

                string folio = string.Empty;

                //if (!string.IsNullOrEmpty(ordenCierre.Folio_Perteneciente))
                folio = context.OrdenCierre.FirstOrDefault(x => x.CodDes == ordenCierre.CodDes && x.CodCte == ordenCierre.CodCte && x.CodPrd == ordenCierre.CodPrd
                && x.CodPed != 0 && x.FchCierre == DateTime.Today && x.Estatus == true && x.Id_Tad == id_terminal)?.Folio ?? string.Empty;

                var user = await context.Usuario.FirstOrDefaultAsync(x => x.Usu == HttpContext.User.FindFirstValue(ClaimTypes.Name));
                if (user == null)
                    return NotFound();

                if (string.IsNullOrEmpty(folio))
                {
                    var consecutivo = context.Consecutivo.Include(x => x.Terminal).FirstOrDefault(x => x.Nombre == "Orden" && x.Id_Tad == id_terminal);
                    if (consecutivo is null)
                    {
                        Consecutivo Nuevo_Consecutivo = new() { Numeracion = 1, Nombre = "Orden", Id_Tad = id_terminal };
                        context.Add(Nuevo_Consecutivo);
                        await context.SaveChangesAsync();
                        consecutivo = Nuevo_Consecutivo;
                    }
                    else
                    {
                        consecutivo.Numeracion++;
                        context.Update(consecutivo);
                        await context.SaveChangesAsync();
                    }

                    context.Update(consecutivo);
                    await context.SaveChangesAsync();

                    var cliente = context.Cliente.FirstOrDefault(x => x.Cod == ordenCierre.CodCte);

                    if (cliente is null)
                        return BadRequest("No se encontro el cliente");

                    if (!string.IsNullOrEmpty(ordenCierre.Folio_Perteneciente))
                        ordenCierre.Folio = $"O{DateTime.Now:yy}-{consecutivo.Numeracion:0000000}{(cliente is not null && !string.IsNullOrEmpty(cliente.CodCte) ? $"-{cliente.CodCte}" : "-DFT")}-{consecutivo.Obtener_Codigo_Terminal}";
                    else
                        ordenCierre.Folio = $"OP{DateTime.Now:yy}-{consecutivo.Numeracion:0000000}{(cliente is not null && !string.IsNullOrEmpty(cliente.CodCte) ? $"-{cliente.CodCte}" : "-DFT")}-{consecutivo.Obtener_Codigo_Terminal}";

                }
                else
                {
                    ordenCierre.Folio = folio;
                }

                var bin = context.OrdenEmbarque.Select(x => x.Bin).OrderBy(x => x).LastOrDefault();

                var bincount = context.OrdenEmbarque.Count(x => x.Bin == bin);

                var count = context.OrdenCierre.Count(x => x.Folio == folio && x.CodDes == ordenCierre.CodDes && x.CodCte == ordenCierre.CodCte
                && x.CodPrd == ordenCierre.CodPrd);

                ordenCierre.Id_Tad = id_terminal;

                OrdenEmbarque ordenEmbarque = new()
                {
                    Codest = 9,
                    Codtad = ordenCierre.Id_Tad,
                    Codprd = ordenCierre.CodPrd,
                    Pre = ordenCierre.Precio,
                    Vol = ordenCierre.Volumen,
                    Coddes = ordenCierre.CodDes,
                    Fchpet = DateTime.Now,
                    Fchcar = ordenCierre.FchCar,
                    Bin = count == 0 || bincount >= 2 ? ++bin : count % 2 == 0 ? ++bin : bin,
                    Codusu = user?.Cod,
                    Moneda = ordenCierre.Moneda,
                    Equibalencia = ordenCierre.Equibalencia
                };

                context.Add(ordenEmbarque);
                await context.SaveChangesAsync();

                ordenCierre.Producto = null!;
                ordenCierre.Destino = null!;
                ordenCierre.Grupo = null!;
                ordenCierre.OrdenEmbarque = null!;
                ordenCierre.OrdenPedidos = null!;
                ordenCierre.Cliente = null!;
                ordenCierre.Terminal = null!;
                ordenCierre.CodPed = ordenEmbarque.Cod;
                ordenCierre.FchVencimiento = ordenCierre.FchCierre?.AddDays(5);

                context.Add(ordenCierre);

                await context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(ordenCierre.Folio_Perteneciente))
                {
                    var cierre = context.OrdenCierre.FirstOrDefault(x => x.Folio == ordenCierre.Folio_Perteneciente);

                    if (cierre is not null)
                    {
                        ordenCierre.TipoPago = cierre.TipoPago ?? string.Empty;
                        context.Update(ordenCierre);

                        OrdenPedido ordenPedido = new()
                        {
                            CodPed = ordenEmbarque.Cod,
                            CodCierre = cierre?.Cod ?? 0,
                            Folio = ordenCierre.Folio_Perteneciente,
                        };

                        context.Add(ordenPedido);
                        await context.SaveChangesAsync();
                    }
                }

                var newOrden = context.OrdenCierre.Where(x => x.Cod == ordenCierre.Cod && x.Id_Tad == id_terminal)
                    .Include(x => x.Producto)
                    .Include(x => x.Destino)
                    .Include(x => x.Cliente)
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x.Tad)
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x.Estado)
                    .FirstOrDefault();

                return Ok(newOrden);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("folios/activo/vigente")]
        public async Task<ActionResult> GetFoliosActivoVigente([FromQuery] Folio_Activo_Vigente parametros)
        {
            try
            {
                var id_terminal = await helper.GetTerminalIdAsync();

                Porcentaje porcentaje = new();
                var por = context.Porcentaje.FirstOrDefault(x => x.Accion == "cierre");
                if (por != null)
                    porcentaje = por;

                IEnumerable<Folio_Activo_Vigente> folios = new List<Folio_Activo_Vigente>();
                List<OrdenCierre> ordenCierres = new();
                var orden_cierres_query = context.OrdenCierre.Where(x => !string.IsNullOrEmpty(x.Folio) && x.FchVencimiento >= DateTime.Today
                && x.CodPed == 0 && !x.Folio.StartsWith("RE") && x.Activa == true && x.Estatus == true && x.Id_Tad == id_terminal)
                    .Include(x => x.Destino).Include(x => x.Cliente).Include(x => x.Producto).Include(x => x.Grupo).OrderBy(x => x.FchCierre).IgnoreAutoIncludes().AsQueryable();

                if (orden_cierres_query is not null)
                {
                    if (!string.IsNullOrEmpty(parametros.Grupo_Filtrado))
                        orden_cierres_query = orden_cierres_query.Where(x => x.Grupo != null && !string.IsNullOrEmpty(x.Grupo.Den)
                        && x.Grupo.Den.ToLower().Contains(parametros.Grupo_Filtrado.ToLower()));
                    if (!string.IsNullOrEmpty(parametros.Cliente_Filtrado))
                        orden_cierres_query = orden_cierres_query.Where(x => x.Cliente != null && !string.IsNullOrEmpty(x.Cliente.Den)
                        && x.Cliente.Den.ToLower().Contains(parametros.Cliente_Filtrado.ToLower()));
                    if (!string.IsNullOrEmpty(parametros.Destino_Filtrado))
                        orden_cierres_query = orden_cierres_query.Where(x => x.Destino != null && !string.IsNullOrEmpty(x.Destino.Den)
                        && x.Destino.Den.ToLower().Contains(parametros.Destino_Filtrado.ToLower()));
                    if (!string.IsNullOrEmpty(parametros.Producto_Filtrado))
                        orden_cierres_query = orden_cierres_query.Where(x => x.Producto != null && !string.IsNullOrEmpty(x.Producto.Den)
                        && x.Producto.Den.ToLower().Contains(parametros.Producto_Filtrado.ToLower()));

                    ordenCierres = orden_cierres_query.ToList();
                }

                foreach (var item in ordenCierres)
                {
                    if (!string.IsNullOrEmpty(item.Folio) && item.CodPrd != null)
                    {
                        ordenCierres.First(x => x.Cod == item.Cod).VolumenDisponible = ObtenerVolumenDisponibleDeProducto(item.Folio, item.CodPrd);
                        if (item.VolumenDisponible is not null)
                            foreach (var item1 in item.VolumenDisponible.Productos)
                            {
                                if (item1.PromedioCarga >= (item1.Disponible * (porcentaje.Porcen / 100)))
                                {
                                    item.Activa = false;
                                }
                            }
                    }
                }

                folios = ordenCierres.Where(x => x.Activa == true).Select(x => new Folio_Activo_Vigente()
                {
                    Folio = x.Folio ?? string.Empty,
                    Grupo = x.Grupo,
                    Cliente = x.Cliente,
                    Destino = x.Destino,
                    Producto = x.Producto,
                    Fecha_Vigencia = x.FchVencimiento ?? DateTime.MinValue,
                    ID_Cierre = x.Cod,
                    ID_Producto = x.CodPrd,
                    Fecha_Cierre = x.FchCierre ?? DateTime.MinValue,
                    VolumenDisponibleDTO = x.VolumenDisponible,
                    Comentarios = x.Observaciones
                });

                //foreach (var item in folios)
                //{
                //    var o = context.OrdenCierre.FirstOrDefault(x => x.Cod == item.ID_Cierre);
                //    if (o != null)
                //        if (o.CodGru != null && folios.FirstOrDefault(x => x.ID_Cierre == item.ID_Cierre) is not null)
                //            folios.First(x => x.ID_Cierre == item.ID_Cierre).Grupo = context.Grupo.FirstOrDefault(x => x.Cod == o.CodGru);
                //}

                return Ok(folios);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("dividir")]
        public async Task<ActionResult> DividirOrdenes([FromBody] OrdenEmbarque ordenEmbarque)
        {
            try
            {
                if (ordenEmbarque is null)
                    return BadRequest("No se encontro ninguna orden");

                if (ordenEmbarque.Ordenes_A_Crear == 0)
                    return BadRequest("No se aceptan valores iguales o menores a 0");

                if (ordenEmbarque.Ordenes_A_Crear > 4)
                    return BadRequest("No se aceptan valores mayores a 4");

                var destino = context.Destino.FirstOrDefault(x => x.Cod == ordenEmbarque.Coddes);
                if (destino is null)
                    return BadRequest("No se encontro el destino");

                var cliente = context.Cliente.FirstOrDefault(x => x.Cod == destino.Codcte);
                if (cliente is null)
                    return BadRequest("No se encontro el cliente");

                var guid = Guid.NewGuid().ToString().Split("-");
                var guidfolio = $"O-{guid[4]}";

                var Volumen_A_Dividir = Math.Round((ordenEmbarque.Vol / ordenEmbarque.Ordenes_A_Crear) ?? 10000, 2);
                var ordenCierre = ordenEmbarque.OrdenCierre;

                OrdenPedido ordenPedido = new();

                for (int i = 0; i < ordenEmbarque?.Ordenes_A_Crear; i++)
                {
                    var orden = ordenEmbarque.ShallowCopy();
                    orden.Cod = 0;
                    orden.Vol = Volumen_A_Dividir;
                    orden.Chofer = null!;
                    orden.Destino = null!;
                    orden.Producto = null!;
                    orden.Cliente = null!;
                    orden.OrdenCierre = null!;
                    orden.OrdenCompra = null!;
                    orden.OrdenPedido = null!;
                    orden.Estado = null!;
                    orden.Transportista = null!;
                    orden.Tonel = null!;
                    orden.Tad = null!;

                    context.Add(orden);
                    await context.SaveChangesAsync();

                    if (ordenCierre != null)
                    {
                        var ordencierrecopy = ordenCierre.ShallowCopy();
                        ordencierrecopy.OrdenPedidos = null!;
                        ordencierrecopy.CodPed = orden.Cod;
                        ordencierrecopy.Cod = 0;
                        ordencierrecopy.Volumen = Convert.ToInt32(Volumen_A_Dividir);

                        ordencierrecopy.Destino = null!;
                        ordencierrecopy.Cliente = null!;
                        ordencierrecopy.Producto = null!;
                        ordencierrecopy.OrdenEmbarque = null!;
                        ordencierrecopy.Grupo = null!;
                        ordencierrecopy.Terminal = null!;

                        context.Add(ordencierrecopy);
                        await context.SaveChangesAsync();

                        if (context.OrdenPedido.Any(x => x.CodPed == ordenEmbarque.Cod))
                        {
                            var op = context.OrdenPedido.FirstOrDefault(x => x.CodPed == ordenEmbarque.Cod);
                            if (op is not null)
                            {
                                ordenPedido = new OrdenPedido()
                                {
                                    Folio = op.Folio,
                                    CodPed = orden.Cod,
                                    CodCierre = ordencierrecopy.Cod,
                                    Pedido_Original = ordenEmbarque.Cod,
                                    Folio_Cierre_Copia = ordenCierre.Folio
                                };

                                context.Add(ordenPedido);
                                await context.SaveChangesAsync();
                            }
                        }
                        else
                        {
                            ordenPedido = new OrdenPedido()
                            {
                                Folio = ordenCierre?.Folio,
                                CodPed = orden.Cod,
                                CodCierre = ordencierrecopy.Cod,
                                Pedido_Original = ordenEmbarque?.Cod,
                                Folio_Cierre_Copia = ordenCierre?.Folio
                            };

                            context.Add(ordenPedido);
                            await context.SaveChangesAsync();
                        }
                    }

                    if (ordenCierre is null)
                    {
                        var cierre = new OrdenCierre()
                        {
                            Folio = guidfolio,
                            CodPed = orden.Cod,
                            FchCar = DateTime.Today,
                            FchCierre = DateTime.Today,
                            fchPrecio = DateTime.Now,
                            FchVencimiento = DateTime.Today.AddDays(5),
                            FchLlegada = DateTime.Today.AddDays(1),
                            Precio = orden.Pre ?? 0,
                            CodDes = orden.Coddes,
                            CodGru = cliente?.Codgru,
                            CodCte = cliente?.Cod,
                            CodPrd = orden.Codprd,
                            Id_Tad = orden.Codtad,
                            Volumen = Convert.ToInt32(orden.Vol),
                            Moneda = orden.Moneda,
                            Equibalencia = orden.Equibalencia
                        };

                        context.Add(cierre);
                        await context.SaveChangesAsync();

                        if (context.OrdenPedido.Any(x => ordenEmbarque != null && x.CodPed == ordenEmbarque.Cod))
                        {
                            var op = context.OrdenPedido.FirstOrDefault(x => ordenEmbarque != null && x.CodPed == ordenEmbarque.Cod);

                            ordenPedido = new OrdenPedido()
                            {
                                Folio = op.Folio,
                                CodPed = orden.Cod,
                                CodCierre = cierre.Cod,
                                Pedido_Original = ordenEmbarque?.Cod,
                                Folio_Cierre_Copia = guidfolio
                            };

                            context.Add(ordenPedido);
                            await context.SaveChangesAsync();
                        }
                        else
                        {
                            ordenPedido = new OrdenPedido()
                            {
                                Folio = ordenCierre?.Folio,
                                CodPed = orden.Cod,
                                CodCierre = cierre.Cod,
                                Pedido_Original = ordenEmbarque?.Cod,
                                Folio_Cierre_Copia = guidfolio
                            };

                            context.Add(ordenPedido);
                            await context.SaveChangesAsync();
                        }
                    }

                }

                ordenEmbarque.Chofer = null!;
                ordenEmbarque.Destino = null!;
                ordenEmbarque.Producto = null!;
                ordenEmbarque.Cliente = null!;
                ordenEmbarque.OrdenCierre = null!;
                ordenEmbarque.OrdenCompra = null!;
                ordenEmbarque.OrdenPedido = null!;
                ordenEmbarque.Estado = null!;
                ordenEmbarque.Transportista = null!;
                ordenEmbarque.Tonel = null!;
                ordenEmbarque.Tad = null!;

                ordenEmbarque.Codest = 14;
                context.Update(ordenEmbarque);

                await context.SaveChangesAsync();

                return Ok();
            }
            catch (DivideByZeroException)
            {
                return BadRequest("No se pueden divir valores entre 0");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("ordenes/despachadas/{bol:int}")]
        public async Task<ActionResult> Obtener_Ordenes_Synhtesis_Por_BOL([FromRoute] int bol)
        {
            try
            {
                var id_terminal = await helper.GetTerminalIdAsync();

                if (bol == 0)
                    return NotFound();

                List<Orden> ordenes = await context.Orden.Where(x => x.BatchId == bol && x.Id_Tad == id_terminal)
                    .Include(x => x.OrdenEmbarque)
                    .Include(x => x.Producto)
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Tonel)
                    .Include(x => x.Estado)
                    .Include(x => x.Redireccionamiento)
                    .ThenInclude(x => x.Destino)
                    .Include(x => x.Redireccionamiento)
                    .ThenInclude(x => x.Cliente)
                    .IgnoreAutoIncludes()
                    .ToListAsync();

                ordenes.ForEach(x =>
                {
                    if (x.OrdenEmbarque is not null)
                        x.OrdenEmbarque.Pre = Obtener_Precio_Del_Dia_De_Orden_Synthesis(x.Cod).Precio;
                });

                return Ok(ordenes);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("confirmar/asignacion")]
        public async Task<ActionResult> Confirmar_Asignacion_Unidades([FromBody] List<OrdenEmbarque> ordens)
        {
            try
            {
                var id_terminal = await helper.GetTerminalIdAsync();

                var codigo_terminal = context.Tad.Single(x => x.Cod == id_terminal).CodigoOrdenes;

                foreach (var orden in ordens)
                {
                    var guid = Guid.NewGuid().ToString();
                    var folio = context.OrdenEmbarque.Where(x => x.Codtad == id_terminal).OrderByDescending(X => X.Folio).Select(x => x.Folio).FirstOrDefault();
                    folio ??= 0;

                    folio++;

                    List<OrdenEmbarque> ordenEmbarques = new List<OrdenEmbarque>();

                    ordenEmbarques = context.OrdenEmbarque.IgnoreAutoIncludes().Where(x => x.Codchf == orden.Codchf && x.Codton == orden.Codton && x.Fchcar == orden.Fchcar
                    && x.Codest == 3 && string.IsNullOrEmpty(x.Bolguidid))
                        .Include(x => x.Chofer)
                        .Include(x => x.Estado)
                        .Include(x => x.OrdenCierre)
                        .Include(x => x.Destino)
                        .ThenInclude(x => x.Cliente)
                        .Include(x => x.Tonel)
                        .ThenInclude(x => x.Transportista)
                        .Include(x => x.Producto)
                        .ToList();

                    foreach (var item in ordenEmbarques.Where(x => ordens.Any(y => y.Cod == x.Cod)).DistinctBy(x => new { x.Compartment, x.Codchf, x.Fchcar, x.Codton }))
                    {
                        item.Bolguidid = guid;
                        item.FolioSyn = $"{codigo_terminal}-{folio}_{item.Compartment}";
                        item.Folio = folio;
                        item.Codest = 22;
                    }

                    context.UpdateRange(ordenEmbarques);
                    await context.SaveChangesAsync();
                }
                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("orden")]
        public async Task<ActionResult> Obtener_Ordenes_Synhtesis_Por_Referencia([FromQuery] Folio_Activo_Vigente folio_)
        {
            try
            {
                var id_terminal = await helper.GetTerminalIdAsync();

                if (string.IsNullOrEmpty(folio_.Folio) || string.IsNullOrWhiteSpace(folio_.Folio))
                    return NotFound();

                var orden = await context.OrdenEmbarque.Where(x => x.Codtad == id_terminal && !string.IsNullOrEmpty(x.FolioSyn) && x.FolioSyn.Equals(folio_.Folio))
                    .Include(x => x.Tad)
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Tonel)
                    .ThenInclude(x => x.Transportista)
                    .Include(x => x.Chofer)
                    .Include(x => x.Producto)
                    .Include(x => x.Estado)
                    .Include(x => x.Orden).IgnoreAutoIncludes()
                    .Include(x => x.Estatus_Orden)
                    .Include(x => x.HistorialEstados)
                    .ThenInclude(x => x.Estado)
                    .Include(x => x.Archivos)
                    .FirstOrDefaultAsync();

                if (orden is null) { return NotFound(); }
                return Ok(orden);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("asignar/embarque")]
        public async Task<ActionResult> Asignar_Embarque([FromBody] Orden orden)
        {
            try
            {
                var codigo_accion = 46;

                var id = await helper.GetUserId();

                var id_terminal = await helper.GetTerminalIdAsync();

                var usuario = await userManager.FindByIdAsync(id);
                if (usuario is null) { return NotFound(); }

                var user = context.Usuario.Find(usuario.UserCod);
                if (user is null) { return NotFound(); }

                //var bol_embarque

                if (orden.Fchcar is null) { return BadRequest("Fecha de carga invalida"); }
                if (string.IsNullOrEmpty(orden.Ref)) { return BadRequest("Fecha de carga invalida"); }


                var ordenembarque = context.OrdenEmbarque.IgnoreAutoIncludes().Include(x => x.Archivos).FirstOrDefault(x => !string.IsNullOrEmpty(x.FolioSyn) && x.FolioSyn.Equals(orden.Ref) && x.Codtad == id_terminal);
                if (ordenembarque is null) { return NotFound(); }

                if (context.Orden.Any(x => x.Ref == ordenembarque.FolioSyn && x.Cod != orden.Cod))
                    return BadRequest($"La orden con referencia: {ordenembarque.FolioSyn}, ya cuenta con datos registrados.");

                orden.Codchf = ordenembarque.Codchf;
                orden.Coddes = ordenembarque.Coddes;
                orden.Coduni = ordenembarque.Codton;
                orden.Codprd = ordenembarque.Codprd;
                orden.Codest = 20;
                if (orden.isEnergas is true)
                {
                    if (ordenembarque.Archivos != null)
                    {
                        if (!ordenembarque.Archivos.Any(x => x.Tipo_Archivo == Tipo_Archivo.ARCHIVO_BOL && x.Id_Registro == ordenembarque.Cod))
                        {
                            return BadRequest($"El archivo BOL / Embarque no ha sido seleccionado");
                        }

                        if (!ordenembarque.Archivos.Any(x => x.Tipo_Archivo == Tipo_Archivo.PDF_FACTURA && x.Id_Registro == ordenembarque.Cod))
                        {
                            return BadRequest($"El archivo PDF de Factura no ha sido seleccionado");
                        }

                        if (!ordenembarque.Archivos.Any(x => x.Tipo_Archivo == Tipo_Archivo.XML_FACTURA && x.Id_Registro == ordenembarque.Cod))
                        {
                            return BadRequest($"El archivo XML de Factura no ha sido seleccionado");
                        }

                    }
                    else
                    {
                        return BadRequest("Debe de subir archivos");
                    }
                }

                ordenembarque.Codest = 20;

                ordenembarque.Pre = GetPrecioByEner(orden.Ref!, id_terminal, (DateTime)orden.Fchcar!).Precio;

                orden.Fch = DateTime.Now;
                orden.Folio = ordenembarque.Folio;
                orden.Id_Tad = id_terminal;

                var destino = context.Destino.FirstOrDefault(x => x.Cod == ordenembarque.Coddes);
                if (destino is null) { return NotFound(); }

                orden.Dendes = destino.Den;

                var guid = Guid.NewGuid().ToString();
                orden.Bolguiid = guid;

                var tonel = context.Tonel.FirstOrDefault(x => x.Cod == ordenembarque.Codton);
                if (tonel is null) { return NotFound(); }

                if (ordenembarque.Compartment == 1) { orden.CompartmentId = tonel.Idcom; }
                if (ordenembarque.Compartment == 2) { orden.CompartmentId = tonel.Idcom2; }
                if (ordenembarque.Compartment == 3) { orden.CompartmentId = tonel.Idcom3; }
                if (ordenembarque.Compartment == 4) { orden.CompartmentId = tonel.Idcom4; }

                if (context.Orden.Any(x => x.BatchId == orden.BatchId && x.Cod != orden.Cod && x.Id_Tad == id_terminal))
                {
                    return BadRequest($"Ya existe una orden con el bol/embarque: {orden.BatchId}.");
                }
                if (context.Orden.Any(x => x.Factura == orden.Factura && x.Cod != orden.Cod && x.Id_Tad == id_terminal))
                {
                    return BadRequest($"Ya existe una orden con la factura de proveedor: {orden.Factura}.");
                }

                context.Update(ordenembarque);

                if (orden.Cod is null)
                {
                    codigo_accion = 46;
                    context.Add(orden);
                }
                else
                {
                    codigo_accion = 50;
                    var orden_existente = context.Orden.FirstOrDefault(x => x.Cod == orden.Cod);
                    if (orden_existente is not null)
                    {
                        var ordembdet = context.OrdEmbDet.FirstOrDefault(x => x.Bol == orden_existente.BatchId && x.Id_Tad == id_terminal);
                        if (ordembdet is not null)
                        {
                            context.Remove(ordembdet);
                            await context.SaveChangesAsync();
                        }

                        orden_existente.Vol = orden.Vol;
                        orden_existente.BatchId = orden.BatchId;
                        orden_existente.Fchcar = orden.Fchcar;
                        orden_existente.SealNumber = orden.SealNumber;
                        orden_existente.Pedimento = orden.Pedimento;
                        orden_existente.NOrden = orden.NOrden;
                        orden_existente.Factura = orden.Factura;
                        orden_existente.Importe = orden.Importe;
                        orden_existente.ValorUnitario = orden.ValorUnitario;
                        orden_existente.isEnergas = orden.isEnergas;
                        context.Update(orden_existente);
                    }
                }
                await context.SaveChangesAsync(id, codigo_accion);

                OrdEmbDet ordEmbDet = new()
                {
                    Fch = DateTime.Now,
                    FchDoc = orden.Fchcar,
                    Codusu = user.Cod,
                    Codusumod = user.Cod,
                    Fchmod = DateTime.Now,
                    Bol = orden.BatchId,
                    Id_Tad = id_terminal,
                    Fchlleest = orden.Fecha_llegada
                };

                context.Add(ordEmbDet);
                await context.SaveChangesAsync();
                HistorialEstados historialEstados = new()
                {
                    Id_Estado = 43,
                    Id_Orden = ordenembarque.Cod,
                    Id_Usuario = id,
                    Fecha_Actualizacion = orden.Fchcar ?? DateTime.Now
                };
                context.Add(historialEstados);
                await context.SaveChangesAsync();

                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        private VolumenDisponibleDTO ObtenerVolumenDisponibleDeProducto(string Folio, byte? ID_Producto)
        {
            VolumenDisponibleDTO volumen = new();

            var cierres = context.OrdenCierre.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio == Folio && x.CodPrd == ID_Producto && x.Estatus == true).Include(x => x.Producto).ToList() ?? new List<OrdenCierre>();
            foreach (var item in cierres)
            {
                var VolumenDisponible = item.Volumen;

                var listConsumido = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Tonel != null && x.OrdenEmbarque.Codprd == item.CodPrd
                    && x.OrdenEmbarque.Codest == 22
                    && x.OrdenEmbarque.Folio != null
                    && x.OrdenEmbarque.Bolguidid != null
                && x.OrdenEmbarque.Orden == null && x.OrdenEmbarque.Codtad == item.Id_Tad)
                .Include(x => x.OrdenEmbarque)
                .ThenInclude(x => x.Tonel)
                .Include(x => x.OrdenEmbarque)
                .ThenInclude(x => x.Orden)
                .ThenInclude(x => x.Tonel).ToList();

                var VolumenCongelado = listConsumido.Sum(item => item.OrdenEmbarque!.Compartment == 1 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom!.ToString())
                                : item.OrdenEmbarque!.Compartment == 2 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom2!.ToString())
                                : item.OrdenEmbarque!.Compartment == 3 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom3!.ToString())
                                : item.OrdenEmbarque!.Compartment == 4 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom4!.ToString())
                                : item.OrdenEmbarque!.Vol);

                var countCongelado = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Codprd == item.CodPrd
                && x.OrdenEmbarque.Codest == 22
                && x.OrdenEmbarque.Folio != null
            && x.OrdenEmbarque.Orden == null && x.OrdenEmbarque.Codtad == item.Id_Tad)
                .Include(x => x.OrdenEmbarque)
                .ThenInclude(x => x.Orden)
                .Count();

                var VolumenConsumido = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Orden != null && x.OrdenEmbarque.Folio != null
                    && x.OrdenEmbarque.Orden.Codest != 14
                    && x.OrdenEmbarque.Codest != 14
                && x.OrdenEmbarque.Codprd == item.CodPrd
                && x.OrdenEmbarque.Orden.BatchId != null && x.OrdenEmbarque.Orden.Id_Tad == item.Id_Tad)
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x.Orden)
                    .Sum(x => x.OrdenEmbarque!.Orden!.Vol);

                var countConsumido = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Orden != null && x.OrdenEmbarque.Folio != null
                    && x.OrdenEmbarque.Orden.Codest != 14
                    && x.OrdenEmbarque.Codest != 14
                    && x.OrdenEmbarque.Codprd == item.CodPrd
                    && x.OrdenEmbarque.Orden.BatchId != null && x.OrdenEmbarque.Orden.Id_Tad == item.Id_Tad)
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x.Orden)
                    .Count();

                var VolumenProgramado = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Folio == null
                    && x.OrdenEmbarque.Codprd == item.CodPrd
                    && x.OrdenEmbarque.Bolguidid == null && x.OrdenEmbarque.Codest == 3
                    && x.OrdenEmbarque.FchOrd != null && x.OrdenEmbarque.Codtad == item.Id_Tad)
                    .Include(x => x.OrdenEmbarque)
                        .Sum(x => x.OrdenEmbarque!.Vol);

                var CountVolumenProgramado = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Folio == null
                    && x.OrdenEmbarque.Codprd == item.CodPrd
                    && x.OrdenEmbarque.Bolguidid == null && x.OrdenEmbarque.Codest == 3
                    && x.OrdenEmbarque.FchOrd != null && x.OrdenEmbarque.Codtad == item.Id_Tad)
                    .Include(x => x.OrdenEmbarque).Count();

                var VolumenSolicitado = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(item.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Folio == null
                && x.OrdenEmbarque.Codprd == item.CodPrd
                && x.OrdenEmbarque.Bolguidid == null && x.OrdenEmbarque.Codest == 9
                && x.OrdenEmbarque.FchOrd == null && x.OrdenEmbarque.Codtad == item.Id_Tad)
                    .Include(x => x.OrdenEmbarque)
                    .Sum(x => x.OrdenEmbarque!.Vol);

                var VolumenTotalDisponible = VolumenDisponible - (VolumenConsumido + VolumenCongelado + VolumenProgramado);
                double? PromedioCargas = 0;
                var sumVolumen = VolumenConsumido + VolumenCongelado + VolumenProgramado;
                var sumCount = countCongelado + countConsumido + CountVolumenProgramado;

                if (sumVolumen != 0 && sumCount != 0)
                    PromedioCargas = sumVolumen / sumCount;

                ProductoVolumen productoVolumen = new();

                productoVolumen.Nombre += item.Producto?.Den;
                productoVolumen.Disponible += VolumenTotalDisponible;
                productoVolumen.Congelado += VolumenCongelado;
                productoVolumen.Consumido += VolumenConsumido;
                productoVolumen.Total += VolumenDisponible;
                productoVolumen.PromedioCarga += PromedioCargas;
                productoVolumen.Solicitud += VolumenSolicitado;
                productoVolumen.Programado += VolumenProgramado;

                volumen?.Productos?.Add(productoVolumen);


            }

            return volumen;
        }

        private PrecioBolDTO Obtener_Precio_Del_Dia_De_Orden_Synthesis(long? Id)
        {
            try
            {
                var id_terminal = helper.GetTerminalId();

                var orden = context.Orden.Where(x => x.Cod == Id && x.Id_Tad == id_terminal)
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x.OrdenCierre)
                    .IgnoreAutoIncludes()
                    .FirstOrDefault();

                if (orden is null)
                    return new PrecioBolDTO();

                PrecioBolDTO precio = new();

                var precioVig = context.Precio.Where(x => orden != null && x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd && x.Id_Tad == id_terminal)
                    .OrderByDescending(x => x.FchActualizacion)
                    .FirstOrDefault();

                var precioPro = context.PrecioProgramado.Where(x => orden != null && x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd && x.Id_Tad == id_terminal)
                    .OrderByDescending(x => x.FchActualizacion)
                    .FirstOrDefault();

                var precioHis = context.PreciosHistorico.Where(x => orden != null && x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd && x.FchDia <= orden.Fchcar && x.Id_Tad == id_terminal)
                    .OrderByDescending(x => x.FchActualizacion)
                    .FirstOrDefault();

                if (precioHis is not null)
                    precio.Precio = precioHis.pre;

                if (orden != null && precioVig is not null && precioVig.FchDia == DateTime.Today)
                    precio.Precio = precioVig.Pre;

                if (orden != null && precioPro is not null && precioPro.FchDia == DateTime.Today && context.PrecioProgramado.Any())
                    precio.Precio = precioPro.Pre;

                if (orden != null && orden.OrdenEmbarque is not null && context.OrdenPedido.Any(x => x.CodPed == orden.OrdenEmbarque.Cod && x.Pedido_Original == 0 && string.IsNullOrEmpty(x.Folio_Cierre_Copia)))
                {
                    var ordenepedido = context.OrdenPedido.Where(x => x.CodPed == orden.OrdenEmbarque.Cod && !string.IsNullOrEmpty(x.Folio) && x.Pedido_Original == 0 && string.IsNullOrEmpty(x.Folio_Cierre_Copia)).FirstOrDefault();

                    if (ordenepedido is not null)
                    {
                        var cierre = context.OrdenCierre.Where(x => x.Folio == ordenepedido.Folio
                         && x.CodPrd == orden.Codprd).FirstOrDefault();

                        if (cierre is not null)
                            precio.Precio = cierre.Precio;
                    }
                }

                if (orden is not null && orden.OrdenEmbarque is not null && precioHis is null && precioPro is null && precioVig is null && !precio.Es_Cierre)
                {
                    precio.Precio = orden.OrdenEmbarque.Pre;

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

        public PrecioBolDTO GetPrecioByEner(string Orden_Compra, Int16 Id_Terminal, DateTime date)
        {
            try
            {
                var orden = context.OrdenEmbarque.IgnoreAutoIncludes().Where(x => !string.IsNullOrEmpty(x.FolioSyn) && x.FolioSyn.Equals(Orden_Compra) && x.Codtad == Id_Terminal)
                    .Include(x => x.Producto)
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Tad)
                    .FirstOrDefault() ?? throw new ArgumentNullException();

                PrecioBolDTO precio = new();

                var precioVig = context.Precio.IgnoreAutoIncludes()
                    .Where(x => orden != null && x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd && x.Id_Tad == orden.Codtad)
                    .OrderByDescending(x => x.FchActualizacion).FirstOrDefault();

                var precioPro = context.PrecioProgramado.IgnoreAutoIncludes()
                    .Where(x => orden != null && x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd && x.Id_Tad == orden.Codtad)
                    .OrderByDescending(x => x.FchActualizacion).FirstOrDefault();

                var precioHis = context.PreciosHistorico.IgnoreAutoIncludes()
                    .Where(x => orden != null && x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd && x.FchDia <= date && x.Id_Tad == orden.Codtad)
                    .OrderByDescending(x => x.FchActualizacion)
                    .FirstOrDefault();

                if (precioHis is not null)
                {
                    precio.Precio = precioHis.pre;
                }

                if (orden is not null && precioVig is not null)
                {
                    if (precioVig.FchDia == date.Date)
                    {
                        precio.Precio = precioVig.Pre;
                    }
                }

                if (orden is not null && precioPro is not null && context.PrecioProgramado.Any())
                {
                    if (precioPro.FchDia == date.Date)
                    {
                        precio.Precio = precioPro.Pre;
                    }
                }

                if (orden != null && context.OrdenPedido.Any(x => x.CodPed == orden.Cod && x.Pedido_Original == 0 && string.IsNullOrEmpty(x.Folio_Cierre_Copia)))
                {
                    var ordenepedido = context.OrdenPedido.Where(x => x.CodPed == orden.Cod && !string.IsNullOrEmpty(x.Folio) && x.Pedido_Original == 0 && string.IsNullOrEmpty(x.Folio_Cierre_Copia)).FirstOrDefault();

                    if (ordenepedido is not null)
                    {
                        var cierre = context.OrdenCierre.Where(x => x.Folio == ordenepedido.Folio
                         && x.CodPrd == orden.Codprd).FirstOrDefault();

                        if (cierre is not null)
                        {
                            precio.Precio = cierre.Precio;
                        }
                    }
                }

                if (orden is not null && precioHis is null && precioPro is null && precioVig is null && !precio.Es_Cierre)
                {
                    precio.Precio = orden.Pre;
                }

                return precio;
            }
            catch (Exception e)
            {
                return new();
            }
        }

    }
}