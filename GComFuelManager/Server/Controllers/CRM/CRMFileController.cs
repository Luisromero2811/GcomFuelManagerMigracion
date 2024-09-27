using AutoMapper;
using GComFuelManager.Client.Helpers;
using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.DTOs.CRM;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace GComFuelManager.Server.Controllers.CRM
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CRMFileController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUsuario> userManager;
        private readonly IMapper mapper;
        private readonly IWebHostEnvironment environment;
        private readonly IHttpContextAccessor httpContextAccessor;

        public CRMFileController(ApplicationDbContext context, UserManager<IdentityUsuario> userManager,
            IMapper mapper, IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
        {
            this.context = context;
            this.userManager = userManager;
            this.mapper = mapper;
            this.environment = environment;
            this.httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] CRMDocumentoDTO dTO)
        {
            try
            {
                var documentos = context.CRMDocumentos.Where(x => x.Activo)
                    .AsNoTracking()
                    .AsQueryable();

                if (!string.IsNullOrEmpty(dTO.NombreDocumento) && !string.IsNullOrWhiteSpace(dTO.NombreDocumento))
                    documentos = documentos.Where(x => x.NombreDocumento.ToLower().Contains(dTO.NombreDocumento.ToLower()));

                if (!dTO.OportunidadId.IsZero())
                    documentos = documentos.Where(x => x.Oportunidades.Select(x => x.Id).Contains(dTO.OportunidadId));

                //actividades

                await HttpContext.InsertarParametrosPaginacion(documentos, dTO.Registros_por_pagina, dTO.Pagina);

                dTO.Pagina = HttpContext.ObtenerPagina();

                documentos = documentos.OrderByDescending(x => x.FechaCreacion).Skip((dTO.Pagina - 1) * dTO.Registros_por_pagina).Take(dTO.Registros_por_pagina);

                var documentosdto = documentos.Select(x => mapper.Map<CRMDocumento, CRMDocumentoDTO>(x));

                return Ok(documentosdto);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("detalle")]
        public async Task<ActionResult> GetDetalle([FromQuery] CRMDocumentoDTO dTO)
        {
            try
            {
                var user = await userManager.FindByNameAsync(HttpContext.User.Identity?.Name ?? string.Empty);
                if (user is null) return NotFound();
                CRMDocumento documento = new();

                if (!dTO.OportunidadId.IsZero())
                {

                    var oportunidad = await context.CRMOportunidades
                        .AsNoTracking()
                        .Include(x => x.Documentos.OrderByDescending(x => x.FechaCreacion))
                        .FirstOrDefaultAsync(x => x.Id == dTO.OportunidadId);
                    if (oportunidad is null) return NotFound();
                    if (oportunidad.Documentos.Count > 0)
                        documento = oportunidad.Documentos.First();
                }

                if (!dTO.ActividadId.IsZero())
                {
                    //actividades
                }

                if (!dTO.Id.IsZero())
                {
                    if (await context.CRMDocumentos.AnyAsync(x => x.Id == dTO.Id))
                        documento = await context.CRMDocumentos.FirstAsync(x => x.Id == dTO.Id);
                }

                if (documento is null) return Ok(new CRMDocumentoDetalleDTO());

                var documentodto = mapper.Map<CRMDocumento, CRMDocumentoDetalleDTO>(documento);

                if (await context.CRMOriginadores.AnyAsync(x => x.UserId == documento.VersionCreadaPor))
                {
                    var comercial = await context.CRMOriginadores.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == documento.VersionCreadaPor);
                    if (comercial is not null)
                    {
                        documentodto.VersionCreadaPor = comercial.FullName;
                    }
                }
                else if (await context.CRMVendedores.AnyAsync(x => x.UserId == documento.VersionCreadaPor))
                {
                    var vendedor = await context.CRMVendedores.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == documento.VersionCreadaPor);
                    if (vendedor is not null)
                    {
                        documentodto.VersionCreadaPor = vendedor.FullName;
                    }
                }

                var documentosrelacionados = await context.CRMDocumentoRelacionados.AsNoTracking()
                    .Where(x => x.DocumentoId == documentodto.Id)
                    .Select(x => x.DocumentoRelacionadoId)
                    .ToListAsync();
                var ultimodocumentorelacionado = await context.CRMDocumentos
                    .Where(x => documentosrelacionados.Contains(x.Id))
                    .OrderByDescending(x => x.FechaCreacion)
                    .Select(x => mapper.Map<CRMDocumento, CRMDocumentoDTO>(x))
                    .FirstOrDefaultAsync();
                documentodto.DocumentoRelacionado = ultimodocumentorelacionado;

                var documentosrevision = await context.CRMDocumentoRevisiones.AsNoTracking()
                    .Where(x => x.DocumentoId == documentodto.Id)
                    .Select(x => x.RevisionId)
                    .ToListAsync();
                var ultimarevisionrelacionada = await context.CRMDocumentos.Where(x => documentosrevision.Contains(x.Id))
                    .OrderByDescending(x => x.FechaCreacion)
                    .Select(x => mapper.Map<CRMDocumento, CRMDocumentoDTO>(x))
                    .FirstOrDefaultAsync();
                documentodto.DocumentoRevision = ultimarevisionrelacionada;

                return Ok(documentodto);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("download/{Id:int}")]
        public async Task<ActionResult> GetDownloadFile(int Id)
        {
            try
            {
                var user = await userManager.FindByNameAsync(HttpContext.User.Identity?.Name ?? string.Empty);
                if (user is null) return NotFound();
                CRMDocumentoDTO documentoDTO = new();
                var documento = await context.CRMDocumentos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id);
                if (documento is null) return NotFound();
                //var fs = new FileStream(documento.Directorio, FileMode.Open, FileAccess.Read);
                //fs.Close();
                var bytes = System.IO.File.ReadAllBytes(documento.Directorio);
                var nombredoc = documento.NombreDocumento.Contains($".{documento.TipoDocumento}") ? documento.NombreDocumento : $"{documento.NombreDocumento}.{documento.TipoDocumento}";

                documentoDTO.InfoBytes = bytes;
                documentoDTO.NombreDocumento = nombredoc;

                return Ok(documentoDTO);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        [HttpPost]
        public async Task<ActionResult> Subir_Archivo_PDF([FromForm] IEnumerable<IFormFile> files)
        {
            try
            {
                var user = await userManager.FindByNameAsync(HttpContext.User.Identity?.Name ?? string.Empty);
                if (user is null) return NotFound();

                var MaxAllowedFile = 5;
                long MaxAllowedSize = 1024 * 1024 * 10;
                var FilesProcessed = 0;
                var extensionesPermitidas = new List<string>() { ".pdf", ".jpg", ".png", ".xlsx", ".xls", ".doc", ".docx" };
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
                                string extension = Path.GetExtension(file.FileName);
                                if (!extensionesPermitidas.Contains(extension)) return BadRequest("Formato de archivo no permitido");
                                string FileName = Path.ChangeExtension(trustFileNameForSave, extension);
                                var carpeta = "Files";
                                //var path = Path.Combine(environment.WebRootPath, environment.EnvironmentName, "PDF", FileName);
                                var path = $"{environment.WebRootPath}\\{carpeta}\\{FileName}";

                                await using FileStream fs = new(path, FileMode.Create);
                                await file.CopyToAsync(fs);

                                var URL = $"{httpContextAccessor.HttpContext?.Request.Scheme}://{httpContextAccessor.HttpContext?.Request.Host}{httpContextAccessor.HttpContext?.Request.PathBase}";
                                var URL_BD = Path.Combine(URL, carpeta, FileName);

                                CRMDocumento documento = new()
                                {
                                    NombreArchivo = FileName,
                                    NombreDocumento = file.FileName,
                                    TipoDocumento = extension.Split('.')[1],
                                    VersionCreadaPor = user.Id,
                                    Directorio = path,
                                    Url = URL_BD,
                                };

                                await context.AddAsync(documento);
                                await context.SaveChangesAsync();

                                var documentodto = mapper.Map<CRMDocumento, CRMDocumentoDTO>(documento);
                                return Ok(documentodto);
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
                        uploadResult.HasError = true;
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
    }
}
