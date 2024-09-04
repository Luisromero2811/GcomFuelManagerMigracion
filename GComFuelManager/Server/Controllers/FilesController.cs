using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using GComFuelManager.Server;
using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    public class FilesController : Controller
    {
        private readonly ILogger<FilesController> _logger;
        private readonly ApplicationDbContext context;
        private readonly User_Terminal terminal;
        private readonly IWebHostEnvironment environment;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly UserManager<IdentityUsuario> userManager;
        private readonly VerifyUserToken verifyUser;

        public FilesController(ILogger<FilesController> logger, ApplicationDbContext context, User_Terminal terminal, IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor, 
            UserManager<IdentityUsuario> userManager, VerifyUserToken verifyUser)
        {
            this.environment = environment;
            this.httpContextAccessor = httpContextAccessor;
            this.userManager = userManager;
            this.verifyUser = verifyUser;
            this.terminal = terminal;
            this.context = context;
            _logger = logger;
        }

        [HttpPost]
        [Route("upload/pdf/{id}")]
        public async Task<ActionResult> Subir_Archivo_PDF([FromForm] IEnumerable<IFormFile> files, [FromRoute] int id)
        {
            try
            {
                var id_user = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id_user))
                    return BadRequest();

                var id_terminal = terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0) { return BadRequest(); }

                var MaxAllowedFile = 1;
                long MaxAllowedSize = 1024 * 1024 * 15;
                var FilesProcessed = 0;
                bool HasErrors = false;

                foreach (var file in files)
                {
                    var uploadResult = new UploadResult();

                    var untrustedFileName = file.FileName;
                    uploadResult.FileName = untrustedFileName;
                    var trustFileName = WebUtility.HtmlDecode(untrustedFileName);

                    if (FilesProcessed < MaxAllowedFile)
                    {
                        if (file.Length == 0)
                        {
                            uploadResult.ErrorCode = 2;
                            uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {trustFileName} : Archivo vacio.";
                            return BadRequest(uploadResult.ErrorMessage);
                        }
                        else if (file.Length > MaxAllowedSize)
                        {
                            uploadResult.ErrorCode = 3;
                            uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {trustFileName} : {file.Length / 1000000} Mb es mayor a la capacidad permitida ({Math.Round((double)(MaxAllowedSize / 1000000))}) Mb ";
                            return BadRequest(uploadResult.ErrorMessage);
                        }
                        else
                        {
                            try
                            {
                                string trustFileNameForSave = Path.GetRandomFileName();
                                string FileName = Path.ChangeExtension(trustFileNameForSave, ".pdf");
                                var carpeta = "PDF";
                                //var path = Path.Combine(environment.WebRootPath, environment.EnvironmentName, "PDF", FileName);
                                var path = $"{environment.WebRootPath}\\{carpeta}\\{FileName}";

                                await using FileStream fs = new(path, FileMode.Create);
                                await file.CopyToAsync(fs);

                                var URL = $"{httpContextAccessor.HttpContext?.Request.Scheme}://{httpContextAccessor.HttpContext?.Request.Host}{httpContextAccessor.HttpContext?.Request.PathBase}";
                                var URL_BD = Path.Combine(URL, carpeta, FileName);

                                Archivo archivo = new() { Id_Registro = id, Directorio = path, Id_Tad = id_terminal, Tipo_Archivo = Tipo_Archivo.PDF_FACTURA, URL = URL_BD };

                                if (!context.Archivos.Any(x => x.Id_Registro == id && x.Tipo_Archivo == Tipo_Archivo.PDF_FACTURA && x.Id_Tad == id_terminal))
                                {
                                    context.Add(archivo);
                                    await context.SaveChangesAsync();
                                }
                                else
                                {
                                    var archivo_existente = context.Archivos.Single(x => x.Id_Registro == id && x.Tipo_Archivo == Tipo_Archivo.PDF_FACTURA && x.Id_Tad == id_terminal);

                                    archivo_existente.URL = archivo.URL;
                                    archivo_existente.Directorio = archivo.Directorio;

                                    context.Update(archivo_existente);
                                    await context.SaveChangesAsync(id_user, 59);
                                }

                            }
                            catch (IOException ex)
                            {
                                uploadResult.ErrorCode = 3;
                                uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {trustFileName} : {ex.Message}";
                                return BadRequest(uploadResult.ErrorMessage);
                            }
                        }
                    }
                    else
                    {
                        uploadResult.ErrorCode = 6;
                        uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {trustFileName} : El limite de archivo es de {MaxAllowedFile}.";
                        uploadResult.HasError = true; HasErrors = true;
                        return BadRequest(uploadResult.ErrorMessage);
                    }

                    FilesProcessed++;
                }

                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("upload/xml/{id}")]
        public async Task<ActionResult> Subir_Archivo_XML([FromForm] IEnumerable<IFormFile> files, [FromRoute] int id)
        {
            try
            {
                var id_user = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id_user))
                    return BadRequest();

                var id_terminal = terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0) { return BadRequest(); }

                var MaxAllowedFile = 1;
                long MaxAllowedSize = 1024 * 1024 * 15;
                var FilesProcessed = 0;
                bool HasErrors = false;

                Orden? orden_factura = new();

                foreach (var file in files)
                {
                    var uploadResult = new UploadResult();

                    orden_factura = new();

                    var untrustedFileName = file.FileName;
                    uploadResult.FileName = untrustedFileName;
                    var trustFileName = WebUtility.HtmlDecode(untrustedFileName);

                    if (FilesProcessed < MaxAllowedFile)
                    {
                        if (file.Length == 0)
                        {
                            uploadResult.ErrorCode = 2;
                            uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {trustFileName} : Archivo vacio.";
                            return BadRequest(uploadResult.ErrorMessage);
                        }
                        else if (file.Length > MaxAllowedSize)
                        {
                            uploadResult.ErrorCode = 3;
                            uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {trustFileName} : {file.Length / 1000000} Mb es mayor a la capacidad permitida ({Math.Round((double)(MaxAllowedSize / 1000000))}) Mb ";
                            return BadRequest(uploadResult.ErrorMessage);
                        }
                        else
                        {
                            try
                            {
                                string trustFileNameForSave = Path.GetRandomFileName();
                                string FileName = Path.ChangeExtension(trustFileNameForSave, ".xml");
                                var carpeta = "XML";
                                //var path = Path.Combine(environment.WebRootPath, environment.EnvironmentName, "PDF", FileName);
                                var path = $"{environment.WebRootPath}\\{carpeta}\\{FileName}";

                                await using FileStream fs = new(path, FileMode.Create);
                                await file.CopyToAsync(fs);

                                var URL = $"{httpContextAccessor.HttpContext?.Request.Scheme}://{httpContextAccessor.HttpContext?.Request.Host}{httpContextAccessor.HttpContext?.Request.PathBase}";
                                var URL_BD = Path.Combine(URL, carpeta, FileName);

                                Archivo archivo = new() { Id_Registro = id, Directorio = path, Id_Tad = id_terminal, Tipo_Archivo = Tipo_Archivo.XML_FACTURA, URL = URL_BD };

                                if (!context.Archivos.Any(x => x.Id_Registro == id && x.Tipo_Archivo == Tipo_Archivo.XML_FACTURA && x.Id_Tad == id_terminal))
                                {
                                    context.Add(archivo);
                                    await context.SaveChangesAsync();
                                }
                                else
                                {
                                    var archivo_existente = context.Archivos.Single(x => x.Id_Registro == id && x.Tipo_Archivo == Tipo_Archivo.XML_FACTURA && x.Id_Tad == id_terminal);

                                    archivo_existente.URL = archivo.URL;
                                    archivo_existente.Directorio = archivo.Directorio;

                                    context.Update(archivo_existente);
                                    await context.SaveChangesAsync(id_user, 59);
                                }

                                XmlDocument doc = new();

                                doc.Load(file.OpenReadStream());
                                var xml_comprobante = doc.GetElementsByTagName("cfdi:Comprobante");

                                if (xml_comprobante is not null)
                                {
                                    var xml_folio = xml_comprobante[0]?.Attributes?["Folio"]?.Value;
                                    if (!string.IsNullOrEmpty(xml_folio))
                                    {
                                        orden_factura.Factura = xml_folio;
                                    }
                                    var xml_importe = xml_comprobante[0]?.Attributes?["Total"]?.Value;
                                    if (!string.IsNullOrEmpty(xml_folio))
                                    {
                                        orden_factura.Importe = xml_importe;
                                    }
                                }

                                var xml_concepto = doc.GetElementsByTagName("cfdi:Concepto");

                                if (xml_concepto is not null)
                                {
                                    if (xml_concepto.Count > 0)
                                    {
                                        if (xml_concepto[0] is not null)
                                        {
                                            if (xml_concepto[0]!.HasChildNodes)
                                            {
                                                //var xml_pedimento = xml_concepto[0]?.SelectSingleNode("InformacionAduanera");
                                                //orden.Pedimento = xml_pedimento?.Attributes?["NumeroPedimento"]?.Value ?? string.Empty;
                                                var xml_info = xml_concepto[0]?.ChildNodes[1]?.Attributes?["NumeroPedimento"]?.Value.Split(" ");
                                                string pedimento = string.Empty;
                                                if (xml_info is not null)
                                                {
                                                    if (xml_info.Length > 0)
                                                    {
                                                        for (int i = 0; i < xml_info.Length; i++)
                                                        {
                                                            pedimento += xml_info[i];
                                                        }
                                                        orden_factura.Pedimento = pedimento;
                                                    }
                                                }

                                            }
                                        }

                                        if (double.TryParse(xml_concepto[0]?.Attributes?["Cantidad"]?.Value, out double volumen))
                                            orden_factura.Vol = volumen;

                                        if (double.TryParse(xml_concepto[0]?.Attributes?["ValorUnitario"]?.Value, out double valorunitario))
                                            orden_factura.ValorUnitario = valorunitario;
                                    }
                                }
                            }
                            catch (IOException ex)
                            {
                                uploadResult.ErrorCode = 3;
                                uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {trustFileName} : {ex.Message}";
                                return BadRequest(uploadResult.ErrorMessage);
                            }
                        }
                    }
                    else
                    {
                        uploadResult.ErrorCode = 6;
                        uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {trustFileName} : El limite de archivo es de {MaxAllowedFile}.";
                        uploadResult.HasError = true; HasErrors = true;
                        return BadRequest(uploadResult.ErrorMessage);
                    }

                    FilesProcessed++;
                }

                return Ok(orden_factura);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("upload/pdf/bol/{id}")]
        public async Task<ActionResult> Subir_Archivo_PDF_Bol_Embarque([FromForm] IEnumerable<IFormFile> files, [FromRoute] int id)
        {
            try
            {
                var id_user = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id_user))
                    return BadRequest();

                var id_terminal = terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0) { return BadRequest(); }

                var MaxAllowedFile = 1;
                long MaxAllowedSize = 1024 * 1024 * 15;
                var FilesProcessed = 0;
                bool HasErrors = false;

                foreach (var file in files)
                {
                    var uploadResult = new UploadResult();

                    var untrustedFileName = file.FileName;
                    uploadResult.FileName = untrustedFileName;
                    var trustFileName = WebUtility.HtmlDecode(untrustedFileName);

                    if (FilesProcessed < MaxAllowedFile)
                    {
                        if (file.Length == 0)
                        {
                            uploadResult.ErrorCode = 2;
                            uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {trustFileName} : Archivo vacio.";
                            return BadRequest(uploadResult.ErrorMessage);
                        }
                        else if (file.Length > MaxAllowedSize)
                        {
                            uploadResult.ErrorCode = 3;
                            uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {trustFileName} : {file.Length / 1000000} Mb es mayor a la capacidad permitida ({Math.Round((double)(MaxAllowedSize / 1000000))}) Mb ";
                            return BadRequest(uploadResult.ErrorMessage);
                        }
                        else
                        {
                            try
                            {
                                string trustFileNameForSave = Path.GetRandomFileName();
                                string extension = Path.GetExtension(trustFileName);
                                string FileName = Path.ChangeExtension(trustFileNameForSave, extension);
                                var carpeta = "PDF-BOL";
                                //var path = Path.Combine(environment.WebRootPath, environment.EnvironmentName, "PDF", FileName);
                                var path = $"{environment.WebRootPath}\\{carpeta}\\{FileName}";

                                await using FileStream fs = new(path, FileMode.Create);
                                await file.CopyToAsync(fs);

                                var URL = $"{httpContextAccessor.HttpContext?.Request.Scheme}://{httpContextAccessor.HttpContext?.Request.Host}{httpContextAccessor.HttpContext?.Request.PathBase}";
                                var URL_BD = Path.Combine(URL, carpeta, FileName);

                                Archivo archivo = new() { Id_Registro = id, Directorio = path, Id_Tad = id_terminal, Tipo_Archivo = Tipo_Archivo.ARCHIVO_BOL, URL = URL_BD };

                                if (!context.Archivos.Any(x => x.Id_Registro == id && x.Tipo_Archivo == Tipo_Archivo.ARCHIVO_BOL && x.Id_Tad == id_terminal))
                                {
                                    context.Add(archivo);
                                    await context.SaveChangesAsync();
                                }
                                else
                                {
                                    var archivo_existente = context.Archivos.Single(x => x.Id_Registro == id && x.Tipo_Archivo == Tipo_Archivo.ARCHIVO_BOL && x.Id_Tad == id_terminal);

                                    archivo_existente.URL = archivo.URL;
                                    archivo_existente.Directorio = archivo.Directorio;

                                    context.Update(archivo_existente);
                                    await context.SaveChangesAsync(id_user, 59);
                                }

                            }
                            catch (IOException ex)
                            {
                                uploadResult.ErrorCode = 3;
                                uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {trustFileName} : {ex.Message}";
                                return BadRequest(uploadResult.ErrorMessage);
                            }
                        }
                    }
                    else
                    {
                        uploadResult.ErrorCode = 6;
                        uploadResult.ErrorMessage = $"(Error: {uploadResult.ErrorCode}) {trustFileName} : El limite de archivo es de {MaxAllowedFile}.";
                        uploadResult.HasError = true; HasErrors = true;
                        return BadRequest(uploadResult.ErrorMessage);
                    }

                    FilesProcessed++;
                }

                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Obtener_Archivos_De_Orden([FromRoute] int id)
        {
            try
            {
                var archivos = await context.Archivos.Where(x => x.Id_Registro == id).ToListAsync();
                return Ok(archivos);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("download/{id_orden}/{tipo_archivo}")]
        public async Task<ActionResult> Download_File([FromRoute] int id_orden = 0, [FromRoute] Tipo_Archivo tipo_archivo = Tipo_Archivo.NONE)
        {
            try
            {
                if (!context.OrdenEmbarque.Any(x => x.Cod == id_orden)) { return NotFound(); }

                var archivo = await context.Archivos.FirstOrDefaultAsync(x => x.Id_Registro == id_orden && x.Tipo_Archivo == tipo_archivo);
                if (archivo is null) { return NotFound(); }

                var file_bytes = System.IO.File.ReadAllBytes(archivo.Directorio);
                //string  bytes = Convert.ToBase64String(file_bytes);

                var extension = Path.GetExtension(archivo.Directorio);

                FileUploadDTO file = new()
                {
                    Extension = extension,
                    ArrayBytes = file_bytes
                };

                return Ok(file);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}