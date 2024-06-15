using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using System.Linq.Dynamic.Core;
using System.Net;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdenController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly User_Terminal terminal;
        private readonly VerifyUserToken verifyUser;
        private readonly UserManager<IdentityUsuario> userManager;

        public OrdenController(ApplicationDbContext context, User_Terminal _Terminal, VerifyUserToken verifyUser, UserManager<IdentityUsuario> userManager)
        {
            this.context = context;
            terminal = _Terminal;
            this.verifyUser = verifyUser;
            this.userManager = userManager;
        }

        private record Productos_Grafica(string Producto, double? Volumen);

        [HttpGet("grafico")]
        public ActionResult Datos_Graficos()
        {
            try
            {
                DateTime fecha_inicio = new(2023, 12, 1);
                DateTime fecha_fin = new(2023, 12, 31);

                var ordenes = context.Orden.Where(x => x.Fchcar >= fecha_inicio && x.Fchcar <= fecha_fin && x.Codest != 14).Select(x => new { x.Vol, x.Producto.Den }).ToList();

                var productos = ordenes.DistinctBy(x => x.Den).Select(x => x.Den);

                List<Productos_Grafica> producto_total = new();

                foreach (var product in productos)
                {
                    var total = ordenes.Where(x => x.Den == product).Sum(x => x.Vol);
                    producto_total.Add(new(product, total));
                }

                return Ok(producto_total);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("{Referencia}")]
        public ActionResult Obtener_Por_Referencia([FromRoute] string Referencia)
        {
            try
            {
                var orden = context.OrdenEmbarque.IgnoreAutoIncludes().Include(x => x.Datos_Facturas)
                    .FirstOrDefault(x => !string.IsNullOrEmpty(x.FolioSyn) && x.FolioSyn.Equals(Referencia));

                if (orden is null) { return NotFound(); }

                return Ok(orden);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("factura")]
        public async Task<ActionResult> Guardar_Datos_Factura([FromBody] Datos_Facturas _Facturas)
        {
            try
            {
                if (!string.IsNullOrEmpty(_Facturas.Numero_Orden) && _Facturas.Fecha_Numero_Orden is null)
                    _Facturas.Fecha_Numero_Orden = DateTime.Now;

                if (!string.IsNullOrEmpty(_Facturas.Factura_MGC) && _Facturas.Fecha_Factura_MGC is null)
                    _Facturas.Fecha_Factura_MGC = DateTime.Now;

                if (!string.IsNullOrEmpty(_Facturas.Factura_MexicoS) && _Facturas.Fecha_Factura_MexicoS is null)
                    _Facturas.Fecha_Factura_MexicoS = DateTime.Now;

                if (!string.IsNullOrEmpty(_Facturas.Factura_DCL) && _Facturas.Fecha_Factura_DCL is null)
                    _Facturas.Fecha_Factura_DCL = DateTime.Now;

                if (!string.IsNullOrEmpty(_Facturas.Factura_Energas) && _Facturas.Fecha_Factura_Energas is null)
                    _Facturas.Fecha_Factura_Energas = DateTime.Now;

                if (_Facturas.Id == 0)
                {
                    context.Add(_Facturas);
                    await context.SaveChangesAsync();
                }
                else
                {
                    _Facturas.OrdenEmbarque = null!;
                    context.Update(_Facturas);
                    await context.SaveChangesAsync();
                }

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("formato")]
        public ActionResult Formato()
        {
            try
            {
                List<OrdenEmbarque_Excel> _Excel = new();

                ExcelPackage.LicenseContext = LicenseContext.Commercial;

                var package = new ExcelPackage();

                var ws = package.Workbook.Worksheets.Add("Ordenes");

                var table = ws.Cells["A1"].LoadFromCollection(_Excel, c =>
                {
                    c.PrintHeaders = true;
                    c.TableStyle = TableStyles.Medium2;
                });

                ws.Cells["A1:B2"].Style.Numberformat.Format = "dd-mm-yyyy";
                //ws.Cells["A2:B2"].Style.Numberformat.Format = $"=DATE({DateTime.Today.Year},{DateTime.Today.Month},{DateTime.Today.Day})";

                ws.Cells[$"D1:D2"].Style.Numberformat.Format = "0";

                ws.Cells[1, 1, 2, ws.Dimension.End.Column].AutoFitColumns();

                ws.Calculate();

                return Ok(package.GetAsByteArray());
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("subir")]
        public async Task<ActionResult<List<OrdenEmbarque>>> Subir_Ordenes([FromForm] IEnumerable<IFormFile> files)
        {
            try
            {
                var id_terminal = terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0) { return BadRequest(); }

                var MaxAllowedFile = 1;
                long MaxFileSize = 1024 * 10204 * 15;
                var FilesProcessed = 0;
                List<UploadResult> uploadResults = new();
                bool HasErrors = false;

                List<OrdenEmbarque> ordenEmbarques = new();
                OrdenEmbarque ordenEmbarque = new();

                foreach (var file in files)
                {
                    var uploadResult = new UploadResult();

                    var unthrushtedFileName = file.FileName;
                    uploadResult.FileName = unthrushtedFileName;
                    var thrustFileName = WebUtility.HtmlDecode(unthrushtedFileName);

                    if (FilesProcessed < MaxAllowedFile)
                    {
                        if (file.Length == 0)
                        {
                            uploadResult.ErrorCode = 2;
                            uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {thrustFileName} : Archivo vacio.";
                            return BadRequest($"(Error: {uploadResult.ErrorCode}) {thrustFileName} : Archivo vacio.");
                        }
                        else if (file.Length > MaxFileSize)
                        {
                            uploadResult.ErrorCode = 3;
                            uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {thrustFileName} : {file.Length / 1000000} Mb es mayor a la capacidad permitida ({Math.Round((double)(MaxFileSize / 1000000))}) Mb ";
                            return BadRequest($"(Error: {uploadResult.ErrorCode}) {thrustFileName} : {file.Length / 1000000} Mb es mayor a la capacidad permitida ({Math.Round((double)(MaxFileSize / 1000000))}) Mb ");
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

                                HasErrors = false;
                                for (int r = 2; r < (ws.Dimension.End.Row + 1); r++)
                                {
                                    ordenEmbarque = new()
                                    {
                                        Fchcar = DateTime.Now,
                                        Codtad = id_terminal,
                                        OrdenCierre = new()
                                        {
                                            Id_Tad = id_terminal
                                        }
                                    };

                                    var row = ws.Cells[r, 1, r, ws.Dimension.End.Column].ToList();
                                    if (row.Count > 0)
                                    {
                                        if (ws.Cells[r, 1].Value is not null)
                                        {
                                            if (DateTime.TryParse(ws.Cells[r, 1].Value.ToString(), out var date))
                                            {
                                                ordenEmbarque.Fchcar = date;
                                            }
                                            else if (double.TryParse(ws.Cells[r, 1].Value.ToString(), out var date_excel))
                                            {
                                                ordenEmbarque.Fchcar = DateTime.FromOADate(date_excel);
                                            }
                                            else
                                            {
                                                uploadResult.ErrorCode = 5;
                                                uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {thrustFileName} : La fecha de carga no pudo ser convertida en un formato correcto. (fila: {r})";
                                                uploadResult.HasError = true; HasErrors = true;
                                                return BadRequest($"(Error: {uploadResult.ErrorCode}) {thrustFileName} : La fecha de carga no pudo ser convertida en un formato correcto. (fila: {r})");
                                            }
                                        }
                                        else
                                        {
                                            uploadResult.ErrorCode = 5;
                                            uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {thrustFileName} : La fecha de carga no puede estar vacia. (fila: {r})";
                                            uploadResult.HasError = true; HasErrors = true;
                                            return BadRequest($"(Error: {uploadResult.ErrorCode}) {thrustFileName} : La fecha de carga no puede estar vacia. (fila: {r})");
                                        }

                                        if (ws.Cells[r, 2].Value is not null)
                                        {
                                            if (DateTime.TryParse(ws.Cells[r, 2].Value.ToString(), out var date))
                                            {
                                                ordenEmbarque.OrdenCierre.FchLlegada = date;
                                            }
                                            else if (double.TryParse(ws.Cells[r, 2].Value.ToString(), out var date_excel))
                                            {
                                                ordenEmbarque.OrdenCierre.FchLlegada = DateTime.FromOADate(date_excel);
                                            }
                                            else
                                            {
                                                uploadResult.ErrorCode = 5;
                                                uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {thrustFileName} : La fecha de llegada estimada no pudo ser convertida en un formato correcto. (fila: {r})";
                                                uploadResult.HasError = true; HasErrors = true;
                                                return BadRequest($"(Error: {uploadResult.ErrorCode}) {thrustFileName} : La fecha de llegada estimada no pudo ser convertida en un formato correcto. (fila: {r})");
                                            }
                                        }

                                        if (ws.Cells[r, 3].Value is not null)
                                        {
                                            var producto = context.Producto.FirstOrDefault(x => x.Id_Tad == id_terminal && !string.IsNullOrEmpty(x.Den) && x.Den.Equals(ws.Cells[r, 3].Value.ToString()));
                                            if (producto is not null)
                                            {
                                                ordenEmbarque.Codprd = producto.Cod;
                                                ordenEmbarque.OrdenCierre.CodPrd = producto.Cod;
                                            }
                                            else
                                            {
                                                uploadResult.ErrorCode = 4;
                                                uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {thrustFileName} : No se encontro el prodcuto. (fila: {r})";
                                                uploadResult.HasError = true; HasErrors = true;
                                                return BadRequest($"(Error: {uploadResult.ErrorCode}) {thrustFileName} : No se encontro el prodcuto. (fila: {r})");
                                            }
                                        }
                                        else
                                        {
                                            uploadResult.ErrorCode = 5;
                                            uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {thrustFileName} : El producto no puede estar vacio. (fila: {r})";
                                            uploadResult.HasError = true; HasErrors = true;
                                            return BadRequest($"(Error: {uploadResult.ErrorCode}) {thrustFileName} : El producto no puede estar vacio. (fila: {r})");
                                        }

                                        if (ws.Cells[r, 4].Value is not null)
                                        {
                                            if (double.TryParse(ws.Cells[r, 4].Value.ToString(), out var vol))
                                            {
                                                ordenEmbarque.Vol = vol;
                                                ordenEmbarque.OrdenCierre.Volumen = Convert.ToInt32(vol.ToString());
                                            }
                                        }
                                        else
                                        {
                                            uploadResult.ErrorCode = 5;
                                            uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {thrustFileName} : La fecha de carga no puede estar vacia. (fila: {r})";
                                            uploadResult.HasError = true; HasErrors = true;
                                            return BadRequest($"(Error: {uploadResult.ErrorCode}) {thrustFileName} : La fecha de carga no puede estar vacia. (fila: {r})");
                                        }

                                        if (ws.Cells[r, 5].Value is not null)
                                        {
                                            var cliente = context.Cliente.FirstOrDefault(x => x.Id_Tad == id_terminal && !string.IsNullOrEmpty(x.Den) && x.Den.Equals(ws.Cells[r, 5].Value.ToString()));
                                            if (cliente is not null)
                                            {
                                                ordenEmbarque.OrdenCierre.CodCte = cliente.Cod;
                                            }
                                            else
                                            {
                                                uploadResult.ErrorCode = 4;
                                                uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {thrustFileName} : No se encontro el cliente. (fila: {r})";
                                                uploadResult.HasError = true; HasErrors = true;
                                                return BadRequest($"(Error: {uploadResult.ErrorCode}) {thrustFileName} : No se encontro el cliente. (fila: {r})");
                                            }
                                        }
                                        else
                                        {
                                            uploadResult.ErrorCode = 5;
                                            uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {thrustFileName} : El cliente no puede estar vacio. (fila: {r})";
                                            uploadResult.HasError = true; HasErrors = true;
                                            return BadRequest($"(Error: {uploadResult.ErrorCode}) {thrustFileName} : El cliente no puede estar vacio. (fila: {r})");
                                        }

                                        if (ws.Cells[r, 6].Value is not null)
                                        {
                                            var destino = context.Destino.FirstOrDefault(x => x.Id_Tad == id_terminal && !string.IsNullOrEmpty(x.Den) && x.Den.Equals(ws.Cells[r, 6].Value.ToString()));
                                            if (destino is not null)
                                            {
                                                ordenEmbarque.Coddes = destino.Cod;
                                                ordenEmbarque.OrdenCierre.CodDes = destino.Cod;
                                            }
                                            else
                                            {
                                                uploadResult.ErrorCode = 4;
                                                uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {thrustFileName} : No se encontro el destino. (fila: {r})";
                                                uploadResult.HasError = true; HasErrors = true;
                                                return BadRequest($"(Error: {uploadResult.ErrorCode}) {thrustFileName} : No se encontro el destino. (fila: {r})");
                                            }
                                        }
                                        else
                                        {
                                            uploadResult.ErrorCode = 5;
                                            uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {thrustFileName} : El destino no puede estar vacio. (fila: {r})";
                                            uploadResult.HasError = true; HasErrors = true;
                                            return BadRequest($"(Error: {uploadResult.ErrorCode}) {thrustFileName} : El destino no puede estar vacio. (fila: {r})");
                                        }

                                        if (ws.Cells[r, 7].Value is not null)
                                        {
                                            ordenEmbarque.OrdenCierre.Turno = ws.Cells[r, 7].Value.ToString();
                                        }
                                        //else
                                        //{
                                        //    uploadResult.ErrorCode = 5;
                                        //    uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {thrustFileName} : El turno no puede estar vacio. (fila: {r})";
                                        //    uploadResult.HasError = true; HasErrors = true;
                                        //    return BadRequest($"(Error: {uploadResult.ErrorCode}) {thrustFileName} : El turno no puede estar vacio. (fila: {r})");
                                        //}

                                        if (!HasErrors)
                                        {
                                            ordenEmbarques.Add(ordenEmbarque);
                                        }
                                    }
                                }

                                for (int i = 0; i < ordenEmbarques.Count; i++)
                                {
                                    var ordenembarque = ordenEmbarques[i];
                                    var ordencierre = ordenembarque.OrdenCierre;

                                    if (ordencierre is null) { return BadRequest($"No se encontraron los datos necesarios para la orden. fila: {(i + 2)}"); }

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

                                    var Cliente = context.Cliente.FirstOrDefault(x => x.Cod == ordencierre.CodCte && x.Id_Tad == id_terminal);

                                    var bin = await context.OrdenEmbarque.Select(x => x.Bin).OrderBy(x => x).LastOrDefaultAsync();
                                    ordenembarque.Bin = i == 0 ? ++bin : i % 2 == 0 ? ++bin : bin;

                                    var folio = string.Empty;
                                    folio = $"O{DateTime.Now:yy}-{consecutivo.Numeracion:0000000}{(Cliente is not null && !string.IsNullOrEmpty(Cliente.CodCte) ? $"-{Cliente.CodCte}" : "-DFT")}-{consecutivo.Obtener_Codigo_Terminal}";
                                    ordencierre.Folio = folio;

                                    ordenembarque.Pre = Obtener_Precio_Del_Dia_De_Orden(ordenembarque, id_terminal).Precio;

                                    ordenembarque.Codest = 9;
                                    ordenembarque.Fchpet = DateTime.Now;

                                    if (ordenembarque.Pre is not null)
                                        ordencierre.Precio = (double)ordenembarque.Pre;

                                    context.Add(ordenembarque);
                                    await context.SaveChangesAsync();

                                    ordenembarque.Estado = context.Estado.Single(x => x.Cod == ordenembarque.Codest);
                                    //ordencierre.CodPed = ordenembarque.Cod;

                                    //context.Add(ordencierre);
                                    //await context.SaveChangesAsync();
                                }
                            }
                        }
                    }
                    else
                    {
                        uploadResult.ErrorCode = 6;
                        uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {thrustFileName} : El limite de archivo es de {MaxAllowedFile}.";
                        uploadResult.HasError = true; HasErrors = true;
                        return BadRequest($"(Error: {uploadResult.ErrorCode}) {thrustFileName} : El limite de archivo es de {MaxAllowedFile}.");
                    }

                    FilesProcessed++;
                }

                return Ok(ordenEmbarques);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("facturacion")]
        public ActionResult Buscar_Ordenes_Con_Archivos_Factura([FromQuery] Orden_Facturacion param)
        {
            try
            {

                if (param.Fecha_Inicio > param.Fecha_Fin) { return BadRequest("La fecha de inicio no puede ser mayor a la fecha de fin"); }

                var dias = param.Fecha_Fin - param.Fecha_Inicio;

                if (dias.Days > 3) { return BadRequest("No puede consultar en un rango mayor a 3 dias"); }

                var ordenes = context.Orden.Where(x => x.Fchcar != null && x.Fchcar >= param.Fecha_Inicio && x.Fchcar <= param.Fecha_Fin && x.Codest == 20 && x.Bolguiid != null && x.isEnergas == true)
                    .Include(x => x.Producto)
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Tonel)
                    .ThenInclude(x => x.Transportista)
                    .Include(x => x.Chofer)
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x.Archivos)
                    .Include(x => x.Terminal)
                    .OrderByDescending(x => x.Fchcar)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(param.Terminal) && !string.IsNullOrWhiteSpace(param.Terminal))
                    ordenes = ordenes.Where(x => x.Terminal != null && !string.IsNullOrEmpty(x.Terminal.Den) && x.Terminal.Den.ToLower().Contains(param.Terminal));

                if (!string.IsNullOrEmpty(param.Cliente) && !string.IsNullOrWhiteSpace(param.Cliente))
                    ordenes = ordenes.Where(x => x.Destino != null && x.Destino.Cliente != null && !string.IsNullOrEmpty(x.Destino.Cliente.Den) && x.Destino.Cliente.Den.ToLower().Contains(param.Cliente));

                if (!string.IsNullOrEmpty(param.Destino) && !string.IsNullOrWhiteSpace(param.Destino))
                    ordenes = ordenes.Where(x => x.Destino != null && !string.IsNullOrEmpty(x.Destino.Den) && x.Destino.Den.ToLower().Contains(param.Destino));

                if (!string.IsNullOrEmpty(param.Producto) && !string.IsNullOrWhiteSpace(param.Producto))
                    ordenes = ordenes.Where(x => x.Producto != null && !string.IsNullOrEmpty(x.Producto.Den) && x.Producto.Den.ToLower().Contains(param.Producto));

                if (!string.IsNullOrEmpty(param.BOL) && !string.IsNullOrWhiteSpace(param.BOL))
                    ordenes = ordenes.Where(x => x.BatchId != null && x.BatchId != 0 && x.BatchId.Value.ToString().Contains(param.BOL));

                var ordenes_validas = ordenes.ToList();

                List<Orden> orden_Facturacion = new();

                for (int i = 0; i < ordenes_validas.Count; i++)
                {
                    if (ordenes_validas[i].OrdenEmbarque is not null)
                    {
                        if (ordenes_validas[i].OrdenEmbarque!.Pre is not null)
                            ordenes_validas[i].OrdenEmbarque!.Pre = Obtener_Precio_Del_Dia_De_Orden_Cargada(ordenes_validas[i].Cod).Precio;

                        if (ordenes_validas.Count(x => x.Ref == ordenes_validas[i].Ref && x.Bolguiid == ordenes_validas[i].Bolguiid
                        && x.BatchId == ordenes_validas[i].BatchId && x.CompartmentId == ordenes_validas[i].CompartmentId) > 1)
                        {
                            if (!orden_Facturacion.Any(x => x.Ref == ordenes_validas[i].Ref && x.Bolguiid == ordenes_validas[i].Bolguiid
                                && x.BatchId == ordenes_validas[i].BatchId && x.CompartmentId == ordenes_validas[i].CompartmentId))
                            {
                                if (context.Orden.Count(x => x.Ref == ordenes_validas[i].Ref && x.Bolguiid == ordenes_validas[i].Bolguiid
                                && x.BatchId == ordenes_validas[i].BatchId && x.CompartmentId == ordenes_validas[i].CompartmentId) > 1)
                                {
                                    var volumen = context.Orden.Where(x => x.Ref == ordenes_validas[i].Ref && x.Bolguiid == ordenes_validas[i].Bolguiid
                                    && x.BatchId == ordenes_validas[i].BatchId && x.CompartmentId == ordenes_validas[i].CompartmentId).Sum(x => x.Vol);

                                    ordenes_validas[i].Vol = volumen;
                                    orden_Facturacion.Add(ordenes_validas[i]);
                                }
                            }
                        }
                        else
                            orden_Facturacion.Add(ordenes_validas[i]);
                    }
                }

                if (param.Excel)
                {
                    ExcelPackage.LicenseContext = LicenseContext.Commercial;
                    ExcelPackage excel = new();

                    ExcelWorksheet ws = excel.Workbook.Worksheets.Add("Ordenes");

                    var ordenes_excel = orden_Facturacion.Select(x => new Orden_Facturacion()
                    {
                        Terminal = x.Terminal.Den ?? string.Empty,
                        Cliente = x.Obtener_Cliente_De_Orden,
                        Destino = x.Obtener_Destino_De_Orden,
                        Producto = x.Obtener_Nombre_Producto,
                        Precio = x.Obtener_Precio_Original,
                        Tranportista = x.Tonel.Transportista.Den ?? string.Empty,
                        Unidad = x.Tonel.Nombre_Placas ?? string.Empty,
                        Chofer = x.Chofer.FullName ?? string.Empty,
                        Sellos = x.SealNumber,
                        Pedimento = x.Pedimento,
                        No_Orden = x.NOrden,
                        Factura_Proveeedor = x.Factura,
                        BOL = x.BatchId.ToString(),
                        Fecha_Carga = x.Fchcar
                    });

                    ws.Cells["A1"].LoadFromCollection(ordenes_excel, c =>
                    {
                        c.PrintHeaders = true;
                        c.TableStyle = TableStyles.Medium2;
                    });
                    ws.Cells[1, 1, ws.Dimension.End.Row, ws.Dimension.End.Column].AutoFitColumns();
                    return Ok(excel.GetAsByteArray());
                }

                return Ok(orden_Facturacion);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        private PrecioBolDTO Obtener_Precio_Del_Dia_De_Orden(OrdenEmbarque orden, short terminal)
        {
            try
            {
                //var orden = context.OrdenEmbarque.Where(x => x.Cod == Id && x.Codtad == terminal)
                //    .Include(x => x.Orden)
                //    .Include(x => x.OrdenCierre)
                //    .IgnoreAutoIncludes()
                //    .FirstOrDefault();

                //if (orden is null)
                //    return new PrecioBolDTO();

                PrecioBolDTO precio = new();

                var precioVig = context.Precio.Where(x => orden != null && x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd && x.Id_Tad == terminal)
                    .Select(x => new { x.Pre, x.FchDia })
                    .OrderByDescending(x => x.FchDia)
                    .FirstOrDefault();

                var precioPro = context.PrecioProgramado.Where(x => orden != null && x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd && x.Id_Tad == terminal)
                    .Select(x => new { x.Pre, x.FchDia })
                    .OrderByDescending(x => x.FchDia)
                    .FirstOrDefault();

                var precioHis = context.PreciosHistorico.Where(x => orden != null && x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd
                    && orden.Fchcar != null && x.FchDia <= DateTime.Today && x.Id_Tad == terminal)
                    .Select(x => new { x.pre, x.FchDia })
                    .OrderByDescending(x => x.FchDia)
                    .FirstOrDefault();

                if (precioHis is not null)
                    precio.Precio = precioHis.pre;

                if (orden != null && precioVig is not null && orden.Fchcar is not null && orden.Fchcar.Value.Date == DateTime.Today)
                    precio.Precio = precioVig.Pre;

                if (orden != null && precioPro is not null && orden.Fchcar is not null && orden.Fchcar.Value.Date == DateTime.Today && context.PrecioProgramado.Any())
                    precio.Precio = precioPro.Pre;

                if (orden != null && context.OrdenPedido.Any(x => x.CodPed == orden.Cod))
                {
                    var ordenepedido = context.OrdenPedido.Where(x => x.CodPed == orden.Cod && !string.IsNullOrEmpty(x.Folio)).Select(x => x.Folio).FirstOrDefault();

                    if (!string.IsNullOrEmpty(ordenepedido) && !string.IsNullOrWhiteSpace(ordenepedido))
                    {
                        var cierre = context.OrdenCierre.Where(x => x.Folio == ordenepedido && x.Id_Tad == terminal
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

                precio.Moneda = !string.IsNullOrEmpty(precio.Moneda) ? precio.Moneda : "MXN";

                return precio;
            }
            catch (Exception e)
            {
                return new PrecioBolDTO();
            }
        }

        private PrecioBolDTO Obtener_Precio_Del_Dia_De_Orden_Cargada(long? id_orden)
        {
            try
            {
                var orden = context.Orden.FirstOrDefault(x => x.Cod == id_orden);

                PrecioBolDTO precio = new();

                var precioVig = context.Precio.Where(x => orden != null && x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd && x.Id_Tad == orden.Id_Tad)
                    .Select(x => new { x.Pre, x.FchDia })
                    .OrderByDescending(x => x.FchDia)
                    .FirstOrDefault();

                var precioPro = context.PrecioProgramado.Where(x => orden != null && x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd && x.Id_Tad == orden.Id_Tad)
                    .Select(x => new { x.Pre, x.FchDia })
                    .OrderByDescending(x => x.FchDia)
                    .FirstOrDefault();

                var precioHis = context.PreciosHistorico.Where(x => orden != null && x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd
                    && orden.Fchcar != null && x.FchDia.Date <= orden.Fchcar.Value.Date && x.Id_Tad == orden.Id_Tad)
                    .Select(x => new { x.pre, x.FchDia })
                    .OrderByDescending(x => x.FchDia)
                    .FirstOrDefault();

                if (precioHis is not null)
                    precio.Precio = precioHis.pre;

                if (orden != null && precioVig is not null && orden.Fchcar is not null && orden.Fchcar.Value.Date == DateTime.Today)
                    if (precioVig.FchDia.Date == DateTime.Today.Date)
                        precio.Precio = precioVig.Pre;

                if (orden != null && precioPro is not null && orden.Fchcar is not null && orden.Fchcar.Value.Date == DateTime.Today && context.PrecioProgramado.Any())
                    if (precioPro.FchDia.Date == DateTime.Today.Date)
                        precio.Precio = precioPro.Pre;

                if (orden != null && context.OrdenPedido.Any(x => x.CodPed == orden.Cod))
                {
                    var ordenepedido = context.OrdenPedido.Where(x => x.CodPed == orden.Cod && !string.IsNullOrEmpty(x.Folio)).Select(x => x.Folio).FirstOrDefault();

                    if (!string.IsNullOrEmpty(ordenepedido) && !string.IsNullOrWhiteSpace(ordenepedido))
                    {
                        var cierre = context.OrdenCierre.Where(x => x.Folio == ordenepedido && x.Id_Tad == orden.Id_Tad
                         && x.CodPrd == orden.Codprd).FirstOrDefault();

                        if (cierre is not null)
                            precio.Precio = cierre.Precio;
                    }
                }

                if (orden is not null && precioHis is null && precioPro is null && precioVig is null && !precio.Es_Cierre)
                {
                    precio.Precio = orden.OrdenEmbarque?.Pre;

                    if (orden.OrdenCierre is not null)
                        precio.Fecha_De_Precio = orden.OrdenCierre.fchPrecio;
                }

                precio.Moneda = !string.IsNullOrEmpty(precio.Moneda) ? precio.Moneda : "MXN";

                return precio;
            }
            catch (Exception e)
            {
                return new PrecioBolDTO();
            }
        }
    }
}
