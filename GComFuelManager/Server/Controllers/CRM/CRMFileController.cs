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
                var documentos = context.CRMDocumentos
                    .AsNoTracking()
                    .AsQueryable();

                if (!string.IsNullOrEmpty(dTO.NombreDocumento) && !string.IsNullOrWhiteSpace(dTO.NombreDocumento))
                    documentos = documentos.Where(x => x.NombreDocumento.ToLower().Contains(dTO.NombreDocumento.ToLower()));

                if (!dTO.OportunidadId.IsZero())
                    documentos = documentos.Where(x => x.Oportunidades.Select(x => x.Id).Contains(dTO.OportunidadId));

                //actividades

                await HttpContext.InsertarParametrosPaginacion(documentos, dTO.Registros_por_pagina, dTO.Pagina);

                dTO.Pagina = HttpContext.ObtenerPagina();

                documentos = documentos.Skip((dTO.Pagina - 1) * dTO.Registros_por_pagina).Take(dTO.Registros_por_pagina);
                
                var documentosdto = documentos.Select(x => mapper.Map<CRMDocumento, CRMDocumentoDTO>(x));

                return Ok(documentosdto);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Subir_Archivo_PDF([FromForm] IEnumerable<IFormFile> files, [FromRoute] int id)
        {
            try
            {
                var user = await userManager.FindByNameAsync(HttpContext.User.Identity?.Name ?? string.Empty);
                if (user is null) return NotFound();

                var MaxAllowedFile = 5;
                long MaxAllowedSize = 1024 * 1024 * 10;
                var FilesProcessed = 0;

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
