using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<ActionResult<IList<ActionResult>>> Subir_Ordenes([FromForm] IEnumerable<IFormFile> files)
        {
            var id_terminal = terminal.Obtener_Terminal(context, HttpContext);
            if (id_terminal == 0) { return BadRequest(); }

            var MaxAllowedFile = 3;
            long MaxFileSize = 1024 * 10204 * 15;
            var FilesProcessed = 0;
            List<UploadResult> uploadResults = new();
            bool HasErrors = false;
            bool Existe = false;

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
                    }
                    else if (file.Length > MaxFileSize)
                    {
                        uploadResult.ErrorCode = 3;
                        uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {thrustFileName} : {file.Length / 1000000} Mb es mayor a la capacidad permitida ({Math.Round((double)(MaxFileSize / 1000000))}) Mb ";
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
                                        else if(double.TryParse(ws.Cells[r, 1].Value.ToString(),out var date_excel))
                                        {
                                            ordenEmbarque.Fchcar = DateTime.FromOADate(date_excel);
                                        }
                                        else
                                        {
                                            uploadResult.ErrorCode = 5;
                                            uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {thrustFileName} : La fecha de carga no pudo ser convertida en un formato correcto. (fila: {r})";
                                            uploadResult.HasError = true; HasErrors = true; break;
                                        }
                                    }
                                    else
                                    {
                                        uploadResult.ErrorCode = 5;
                                        uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {thrustFileName} : La fecha de carga no puede estar vacia. (fila: {r})";
                                        uploadResult.HasError = true; HasErrors = true; break;
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
                                            uploadResult.HasError = true; HasErrors = true; break;
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
                                            uploadResult.HasError = true; HasErrors = true; break;
                                        }
                                    }
                                    else
                                    {
                                        uploadResult.ErrorCode = 5;
                                        uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {thrustFileName} : La fecha de carga no puede estar vacia. (fila: {r})";
                                        uploadResult.HasError = true; HasErrors = true; break;
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
                                        uploadResult.HasError = true; HasErrors = true; break;
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
                                            uploadResult.HasError = true; HasErrors = true; break;
                                        }
                                    }
                                    else
                                    {
                                        uploadResult.ErrorCode = 5;
                                        uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {thrustFileName} : La fecha de carga no puede estar vacia. (fila: {r})";
                                        uploadResult.HasError = true; HasErrors = true; break;
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
                                        uploadResult.HasError = true;HasErrors = true; break;
                                        }
                                    }
                                    else
                                    {
                                        uploadResult.ErrorCode = 5;
                                        uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {thrustFileName} : La fecha de carga no puede estar vacia. (fila: {r})";
                                        uploadResult.HasError = true; HasErrors = true; break;
                                    }

                                    if (ws.Cells[r, 7].Value is not null)
                                    {
                                        ordenEmbarque.OrdenCierre.Turno = ws.Cells[r, 7].Value.ToString();
                                    }
                                    else
                                    {
                                        uploadResult.ErrorCode = 5;
                                        uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {thrustFileName} : La fecha de carga no puede estar vacia. (fila: {r})";
                                        uploadResult.HasError = true; HasErrors = true; break;
                                    }

                                    if (!HasErrors)
                                    {

                                    }
                                }
                            }
                        }
                    }
                }
            }

            return Ok(uploadResults);
        }
    }
}
