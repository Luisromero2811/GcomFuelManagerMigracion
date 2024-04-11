using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Table;
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

                var MaxAllowedFile = 3;
                long MaxFileSize = 1024 * 10204 * 15;
                var FilesProcessed = 0;
                List<UploadResult> uploadResults = new();
                bool HasErrors = false;

                List<OrdenEmbarque> ordenEmbarques = new();
                OrdenEmbarque ordenEmbarque = new();

                foreach (var file in files)
                {
                    var uploadResult = new UploadResult();

                    var unthrushtedFileName = file.Name;
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
                                            else if (double.TryParse(ws.Cells[r, 1].Value.ToString(), out var date_excel))
                                            {
                                                ordenEmbarque.Fchcar = DateTime.FromOADate(date_excel);
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

                                    //ordencierre.CodPed = ordenembarque.Cod;

                                    //context.Add(ordencierre);
                                    //await context.SaveChangesAsync();
                                }
                            }
                        }
                    }
                }

                return Ok(ordenEmbarques);
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
                    .OrderByDescending(x => x.FchDia)
                    .FirstOrDefault();

                var precioPro = context.PrecioProgramado.Where(x => orden != null && x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd && x.Id_Tad == terminal)
                    .OrderByDescending(x => x.FchDia)
                    .FirstOrDefault();

                var precioHis = context.PreciosHistorico.Where(x => orden != null && x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd
                    && orden.Fchcar != null && x.FchDia <= DateTime.Today && x.Id_Tad == terminal)
                    .OrderByDescending(x => x.FchDia)
                    .FirstOrDefault();

                if (precioHis is not null)
                    precio.Precio = precioHis.pre;

                if (orden != null && precioVig is not null && orden.Fchcar is not null && orden.Fchcar.Value.Date == DateTime.Today)
                    precio.Precio = precioVig.Pre;

                if (orden != null && precioPro is not null && orden.Fchcar is not null && orden.Fchcar.Value.Date == DateTime.Today && context.PrecioProgramado.Any())
                    precio.Precio = precioPro.Pre;

                //if (orden != null && context.OrdenPedido.Any(x => x.CodPed == orden.Cod))
                //{
                //    var ordenepedido = context.OrdenPedido.Where(x => x.CodPed == orden.Cod && !string.IsNullOrEmpty(x.Folio)).FirstOrDefault();

                //    if (ordenepedido is not null)
                //    {
                //        var cierre = context.OrdenCierre.Where(x => x.Folio == ordenepedido.Folio && x.Id_Tad == terminal
                //         && x.CodPrd == orden.Codprd).FirstOrDefault();

                //        if (cierre is not null)
                //            precio.Precio = cierre.Precio;
                //    }
                //}

                //if (orden is not null && precioHis is null && precioPro is null && precioVig is null && !precio.Es_Cierre)
                //{
                //    precio.Precio = orden.Pre;

                //    if (orden.OrdenCierre is not null)
                //        precio.Fecha_De_Precio = orden.OrdenCierre.fchPrecio;
                //}

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
