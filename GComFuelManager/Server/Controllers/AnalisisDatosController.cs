using AutoMapper;
using GComFuelManager.Server.Helpers;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;
using GComFuelManager.Shared.ReportesDTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Table;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalisisDatosController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IPrecioHelper precioHelper;

        public AnalisisDatosController(ApplicationDbContext context, IMapper mapper, IPrecioHelper precioHelper)
        {
            this.context = context;
            this.mapper = mapper;
            this.precioHelper = precioHelper;
        }

        [HttpGet("ordenescargadas")]
        public async Task<ActionResult> Buscar_Ordenes_Con_Archivos_Factura([FromQuery] AnalisisOrdenCargada param)
        {
            try
            {
                if (param.Fecha_Inicio > param.Fecha_Fin) { return BadRequest("La fecha de inicio no puede ser mayor a la fecha de fin"); }

                var ordenes = context.Orden.AsNoTracking()
                    .Where(x => x.Fchcar != null && x.Fchcar >= param.Fecha_Inicio && x.Fchcar <= param.Fecha_Fin && x.Codest == 20 && x.Bolguiid != null && x.isEnergas == true
                    && x.OrdenEmbarque != null && x.OrdenEmbarque.Codest == 20)
                    .Include(x => x.Producto)
                    .Include(x => x.Destino.Cliente)
                    .Include(x => x.Tonel.Transportista)
                    .Include(x => x.Chofer)
                    .Include(x => x.OrdenEmbarque)
                    .Include(x => x.Terminal)
                    .OrderByDescending(x => x.Fchcar)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(param.Terminal) && !string.IsNullOrWhiteSpace(param.Terminal))
                    ordenes = ordenes.Where(x => x.Terminal != null && !string.IsNullOrEmpty(x.Terminal.Den) && x.Terminal.Den.ToLower().StartsWith(param.Terminal.ToLower()));

                if (!string.IsNullOrEmpty(param.Cliente) && !string.IsNullOrWhiteSpace(param.Cliente))
                    ordenes = ordenes.Where(x => x.Destino != null && x.Destino.Cliente != null && !string.IsNullOrEmpty(x.Destino.Cliente.Den) && x.Destino.Cliente.Den.ToLower().StartsWith(param.Cliente.ToLower()));

                if (!string.IsNullOrEmpty(param.Destino) && !string.IsNullOrWhiteSpace(param.Destino))
                    ordenes = ordenes.Where(x => x.Destino != null && !string.IsNullOrEmpty(x.Destino.Den) && x.Destino.Den.ToLower().StartsWith(param.Destino.ToLower()));

                if (!string.IsNullOrEmpty(param.Producto) && !string.IsNullOrWhiteSpace(param.Producto))
                    ordenes = ordenes.Where(x => x.Producto != null && !string.IsNullOrEmpty(x.Producto.Den) && x.Producto.Den.ToLower().StartsWith(param.Producto.ToLower()));

                if (!string.IsNullOrEmpty(param.BOL) && !string.IsNullOrWhiteSpace(param.BOL))
                    ordenes = ordenes.Where(x => x.BatchId != null && x.BatchId != 0 && x.BatchId.ToString().StartsWith(param.BOL.ToLower()));

                var analisisordenescargadas = await ordenes.GroupBy(x => new
                {
                    x.Ref,
                    x.BatchId,
                    x.CompartmentId,
                    Terminal = x.Terminal.Den,
                    Destino = x.Destino.Den,
                    Producto = x.Producto.Den,
                    x.Tonel.Tracto,
                    x.Tonel.Placa,
                    x.Tonel.Placatracto,
                    Chofer = x.Chofer.Den,
                    ChoferApe = x.Chofer.Shortden,
                    Cliente = x.Destino.Cliente.Den,
                    x.Factura,
                    x.Importe,
                    x.Pedimento,
                    x.ValorUnitario,
                    Tran = x.Tonel.Transportista.Den,
                    x.Bolguiid,
                    x.OrdenEmbarque.TipoVenta,
                    x.Fchcar,
                    x.SealNumber,
                    x.NOrden,
                    x.OrdenEmbarque.Pre
                }, x => x, (baseres, ord) => new AnalisisOrdenCargada
                {
                    Terminal = baseres.Terminal ?? string.Empty,
                    Destino = baseres.Destino ?? string.Empty,
                    TipoVenta = baseres.TipoVenta ?? Shared.Enums.TipoVenta.Externo,
                    Producto = baseres.Producto ?? string.Empty,
                    Volumen = ord.Sum(x => x.Vol) ?? 0,
                    ImporteCompra = baseres.Importe ?? string.Empty,
                    PrecioCompra = baseres.ValorUnitario ?? 0,
                    BOL = baseres.BatchId.ToString() ?? string.Empty,
                    Factura = baseres.Factura ?? string.Empty,
                    FechaCarga = baseres.Fchcar ?? DateTime.MinValue,
                    Transportista = baseres.Tran ?? string.Empty,
                    Operador = baseres.Chofer,
                    OperadorApe = baseres.ChoferApe,
                    Unidad = $"{baseres.Tracto} {baseres.Placa} {baseres.Placatracto} {baseres.SealNumber} {baseres.Pedimento}",
                    Cliente = baseres.Cliente ?? string.Empty,
                    NumeroOrden = baseres.NOrden ?? string.Empty,
                    PrecioVenta = baseres.Pre ?? 0
                })
                .OrderByDescending(x => x.FechaCarga)
                .ToListAsync();

                if (param.Excel)
                {
                    ExcelPackage.LicenseContext = LicenseContext.Commercial;
                    ExcelPackage excel = new();

                    ExcelWorksheet ws = excel.Workbook.Worksheets.Add("Ordenes");

                    ws.Cells["A1"].LoadFromCollection(analisisordenescargadas.Select(mapper.Map<AnalisisOrdenCargadaExcelDTO>), c =>
                    {
                        c.PrintHeaders = true;
                        c.TableStyle = TableStyles.Medium2;
                    });
                    ws.Cells[1, 5, ws.Dimension.End.Row, 5].Style.Numberformat.Format = "#,##0.00";
                    ws.Cells[1, 10, ws.Dimension.End.Row, 10].Style.Numberformat.Format = "dd/MM/yyyy HH:mm:ss";
                    ws.Cells[1, 1, ws.Dimension.End.Row, ws.Dimension.End.Column].AutoFitColumns();
                    return Ok(excel.GetAsByteArray());
                }

                return Ok(analisisordenescargadas);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
