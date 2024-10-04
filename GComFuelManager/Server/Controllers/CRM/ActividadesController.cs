using AutoMapper;
using FluentValidation;
using GComFuelManager.Client.Helpers;
using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.DTOs.CRM;
using GComFuelManager.Shared.DTOs.Reportes.CRM;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using OfficeOpenXml;
using OfficeOpenXml.Table;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class ActividadesController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IValidator<CRMActividadPostDTO> validator;
        private readonly IMapper mapper;
        private readonly UserManager<IdentityUsuario> manager;

        public ActividadesController(ApplicationDbContext context, IValidator<CRMActividadPostDTO> validator, IMapper mapper, UserManager<IdentityUsuario> manager)
        {
            this.context = context;
            this.validator = validator;
            this.mapper = mapper;
            this.manager = manager;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create_Actividad([FromBody] CRMActividadPostDTO cRMActividades)
        {
            try
            {
                var result = await validator.ValidateAsync(cRMActividades);
                if (!result.IsValid) { return BadRequest(result.Errors); }
                var actividad = mapper.Map<CRMActividadPostDTO, CRMActividades>(cRMActividades);

                //Si el ID de la actividad viene en 0 se agrega un nuevo registro de lo contrario se edita el registro
                if (actividad.Id != 0)
                {
                    actividad.Fecha_Mod = DateTime.Now;
                    context.Update(actividad);
                }
                else
                {
                    await context.AddAsync(actividad);
                }

                if (!cRMActividades.DocumentoId.IsZero())
                {
                    var doc = await context.CRMDocumentos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == cRMActividades.DocumentoId);
                    if (doc is not null)
                    {
                        var documento = mapper.Map<CRMActividadPostDTO, CRMDocumento>(cRMActividades);
                        if (documento is not null)
                        {
                            var docupdate = mapper.Map(documento, doc);

                            if (!cRMActividades.DocumentoRelacionado.IsZero())
                            {
                                var docrelacionad = await context.CRMDocumentos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == cRMActividades.DocumentoRelacionado);
                                if (docrelacionad is not null)
                                {
                                    var docrelacionadorelacion = new CRMDocumentoRelacionado() { DocumentoId = documento.Id, DocumentoRelacionadoId = docrelacionad.Id };
                                    await context.AddAsync(docrelacionadorelacion);
                                }
                            }

                            if (!cRMActividades.DocumentoRevision.IsZero())
                            {
                                var docrelacionad = await context.CRMDocumentos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == cRMActividades.DocumentoRevision);
                                if (docrelacionad is not null)
                                {
                                    var docrelacionadorelacion = new CRMDocumentoRevision() { DocumentoId = documento.Id, RevisionId = docrelacionad.Id };
                                    await context.AddAsync(docrelacionadorelacion);
                                }
                            }

                            context.Update(docupdate);
                            if (!await context.CRMActividadDocumentos.AnyAsync(x => x.DocumentoId == documento.Id))
                            {
                                var docop = new CRMActividadDocumento() { DocumentoId = documento.Id, Actividad = actividad, Documento = null! };
                                await context.AddAsync(docop);
                            }
                        }
                    }

                }

                await context.SaveChangesAsync();

                return NoContent();

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("changeStatus/{Id:int}")]
        public async Task<ActionResult> ChangeStatus([FromRoute] int Id, [FromBody] bool status)
        {
            try
            {
                if (Id == 0)
                {
                    return BadRequest();
                }

                var actividad = await context.CRMActividades.FindAsync(Id);

                if (actividad is null)
                {
                    return NotFound();
                }

                actividad.Activo = status;
                actividad.Fecha_Mod = DateTime.Now;
                context.Update(actividad);
                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/asunto")]
        public ActionResult Obtener_Catalogo_Asunto()
        {
            try
            {
                var catalogo = context.CRMCatalogos.AsNoTracking().Include(x => x.Valores).FirstOrDefault(x => x.Nombre.Equals("Catalogo_Actividad_Asunto"));
                if (catalogo is null) return BadRequest("No existe el catalogo para asunto");
                return Ok(catalogo.Valores);

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/prioridad")]
        public ActionResult Obtener_Catalogo_Prioridad()
        {
            try
            {
                var catalogo = context.CRMCatalogos.AsNoTracking().Include(x => x.Valores).FirstOrDefault(x => x.Nombre.Equals("Catalogo_Actividad_Prioridad"));
                if (catalogo is null) return BadRequest("No existe el catalogo para la prioridad");
                return Ok(catalogo.Valores);

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/estatus")]
        public ActionResult Obtener_Catalogo_Estatus()
        {
            try
            {
                var catalogo = context.CRMCatalogos.AsNoTracking().Include(x=>x.Valores).FirstOrDefault(x => x.Nombre.Equals("Catalogo_Actividad_Estatus"));
                if (catalogo is null) return BadRequest("No existe el catalogo para los estatus");
                return Ok(catalogo.Valores);

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("contactlist")]
        public async Task<ActionResult> Obtener_Catalogo_Contacto()
        {
            try
            {
                if (HttpContext.User.Identity is null)
                {
                    return NotFound();
                }

                if (string.IsNullOrEmpty(HttpContext.User.Identity.Name) || string.IsNullOrWhiteSpace(HttpContext.User.Identity.Name))
                {
                    return NotFound();
                }

                var user = await manager.FindByNameAsync(HttpContext.User.Identity.Name);
                if (user is null) { return NotFound(); }

                var contactocrm = new List<CRMContacto>().ToList();
                if (await manager.IsInRoleAsync(user, "Admin"))
                {
                    contactocrm = context.CRMContactos
                                     .Where(x => x.Activo == true)
                                     .Include(x => x.Vendedor)
                                     .ToList();
                }
                else if (await manager.IsInRoleAsync(user, "LIDER_DE_EQUIPO"))
                {
                    var comercial = await context.CRMOriginadores.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == user.Id);
                    if (comercial is null) return NotFound();
                    var equipos = await context.CRMEquipos.AsNoTracking().Where(x => x.Activo && x.LiderId == comercial.Id).Select(x => x.Id).ToListAsync();
                    var relaciones = await context.CRMEquipoVendedores.AsNoTracking().Where(x => equipos.Contains(x.EquipoId))
                        .GroupBy(x => x.VendedorId)
                        .Select(x => x.Key).ToListAsync();
                    contactocrm = context.CRMContactos
                        .Where(x => x.Activo == true && relaciones.Contains(x.Id))
                        .Include(x => x.Vendedor)
                                     .ToList();
                }
                else if (await manager.IsInRoleAsync(user, "CREAR_ACTIVIDAD"))
                {
                    var vendedor = await context.CRMVendedores.FirstOrDefaultAsync(x => x.UserId == user.Id);
                    if (vendedor is null) { return NotFound(); }

                    contactocrm = context.CRMContactos
                        .Where(x => x.Activo == true && x.VendedorId == vendedor.Id)
                        .Include(x => x.Vendedor)
                        .ToList();
                }

                return Ok(contactocrm);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetListActivities([FromQuery] CRMActividadDTO activo)
        {
            try
            {
                if (HttpContext.User.Identity is null)
                {
                    return NotFound();
                }

                if (string.IsNullOrEmpty(HttpContext.User.Identity.Name) || string.IsNullOrWhiteSpace(HttpContext.User.Identity.Name))
                {
                    return NotFound();
                }

                var user = await manager.FindByNameAsync(HttpContext.User.Identity.Name ?? string.Empty);
                if (user is null)
                {
                    return NotFound();
                }

                var activos = new List<CRMActividades>().AsQueryable();

                if (await manager.IsInRoleAsync(user, "Admin"))
                {
                    activos = context.CRMActividades
                       .Where(x => x.Activo && x.Estados.Valor != "Completada")
                       .Include(x => x.vendedor)
                       .Include(x => x.contacto)
                       .ThenInclude(x => x.Cliente)
                       .Include(x => x.asuntos)
                       .Include(x => x.Estados)
                       .Include(x => x.prioridades)
                       .OrderByDescending(x => x.Fecha_Creacion)
                       .AsQueryable();
                }
                else if (await manager.IsInRoleAsync(user, "LIDER_DE_EQUIPO"))
                {
                    var comercial = await context.CRMOriginadores.FirstOrDefaultAsync(x => x.UserId == user.Id);
                    if (comercial is null)
                    {
                        return NotFound();
                    }

                    var equipos = await context.CRMEquipos.AsNoTracking()
                        .Where(x => x.Activo && x.LiderId == comercial.Id).Select(x => x.Id).ToListAsync();

                    var relacion = await context.CRMEquipoVendedores.AsNoTracking()
                       .Where(x => equipos.Contains(x.EquipoId)).GroupBy(x => x.VendedorId).Select(x => x.Key).ToListAsync();

                    activos = context.CRMActividades
                        .AsNoTracking()
                        .Where(x => x.Activo && x.Estados.Valor != "Completada" && relacion.Contains((int)x.Asignado) && equipos.Contains((int)x.EquipoId))
                         .Include(x => x.vendedor)
                        .Include(x => x.contacto)
                         .ThenInclude(x => x.Cliente)
                        .Include(x => x.asuntos)
                        .Include(x => x.Estados)
                        .Include(x => x.prioridades)
                        .OrderByDescending(x => x.Fecha_Creacion)
                        .AsQueryable();
                }
                else if (await manager.IsInRoleAsync(user, "VER_DETALLE_ACTIVIDAD"))
                {
                    var vendedor = await context.CRMVendedores.FirstOrDefaultAsync(x => x.UserId == user.Id);
                    if (vendedor is null) { return NotFound(); }

                    activos = context.CRMActividades
                        .AsNoTracking()
                        .Where(x => x.Activo && x.Estados.Valor != "Completada" && x.Asignado == vendedor.Id)
                        .Include(x => x.vendedor)
                        .Include(x => x.contacto)
                         .ThenInclude(x => x.Cliente)
                        .Include(x => x.asuntos)
                        .Include(x => x.Estados)
                        .Include(x => x.prioridades)
                        .OrderByDescending(x => x.Fecha_Creacion)
                        .AsQueryable();
                }

                if (!string.IsNullOrEmpty(activo.Asunto) && !string.IsNullOrWhiteSpace(activo.Asunto))
                    activos = activos.Where(x => x.asuntos != null && x.asuntos.Valor.ToLower().Contains(activo.Asunto.ToLower()));

                if (!string.IsNullOrEmpty(activo.Prioridad) && !string.IsNullOrWhiteSpace(activo.Prioridad))
                    activos = activos.Where(x => x.prioridades != null && x.prioridades.Valor.ToLower().Contains(activo.Prioridad.ToLower()));

                if (!string.IsNullOrEmpty(activo.Estatus) && !string.IsNullOrWhiteSpace(activo.Estatus))
                    activos = activos.Where(x => x.Estados != null && x.Estados.Valor.ToLower().Contains(activo.Estatus.ToLower()));

                if (!string.IsNullOrEmpty(activo.VendedorId) && !string.IsNullOrWhiteSpace(activo.VendedorId))
                    activos = activos.Where(x => x.vendedor != null && x.vendedor.Nombre.ToLower().Contains(activo.VendedorId.ToLower()));

                if (!activo.Asignado.IsZero())
                    activos = activos.Where(x => x.Asignado.Equals(activo.Asignado));

                if (activo.Excel)
                {
                    ExcelPackage.LicenseContext = LicenseContext.Commercial;
                    ExcelPackage excelPackage = new();
                    ExcelWorksheet ws = excelPackage.Workbook.Worksheets.Add("Actividades");
                    var actividadesexcel = activos
                     .Include(x => x.vendedor)
                     .Include(x => x.contacto)
                     .Include(x => x.asuntos)
                     .Include(x => x.Estados)
                     .Include(x => x.prioridades)
                     .OrderByDescending(x => x.Fecha_Creacion)
                     .Select(x => mapper.Map<CRMActividades, CRMActividadesExcelDTO>(x)).ToList();
                    ws.Cells["A1"].LoadFromCollection(actividadesexcel, opt =>
                    {
                        opt.PrintHeaders = true;
                        opt.TableStyle = OfficeOpenXml.Table.TableStyles.Medium12;
                    });

                    // Formato de fecha para la columna E (Columna 5)
                    ws.Cells[1, 5, ws.Dimension.End.Row, 5].Style.Numberformat.Format = "dd/MM/yyyy";

                    // Formato de fecha para la columna F (Columna 6)
                    ws.Cells[1, 6, ws.Dimension.End.Row, 6].Style.Numberformat.Format = "dd/MM/yyyy";


                    ws.Cells[1, 1, ws.Dimension.End.Row, ws.Dimension.End.Column].AutoFitColumns();

                    return Ok(excelPackage.GetAsByteArray());
                }


                await HttpContext.InsertarParametrosPaginacion(activos, activo.Registros_por_pagina, activo.Pagina);

                if (HttpContext.Response.Headers.TryGetValue("pagina", out StringValues value))
                    if (!string.IsNullOrEmpty(value) || !string.IsNullOrWhiteSpace(value) && value != activo.Pagina)
                        activo.Pagina = int.Parse(value!);

                activos = activos.Skip((activo.Pagina - 1) * activo.Registros_por_pagina).Take(activo.Registros_por_pagina);

                var actividadesdto = activos.Select(x => mapper.Map<CRMActividadDTO>(x));

                return Ok(actividadesdto);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("historial")]
        public async Task<ActionResult> Historial_Actividades([FromQuery] CRMActividadDTO actividadDTO)
        {
            try
            {
                if (HttpContext.User.Identity is null)
                {
                    return NotFound();
                }

                if (string.IsNullOrEmpty(HttpContext.User.Identity.Name) || string.IsNullOrWhiteSpace(HttpContext.User.Identity.Name))
                {
                    return NotFound();
                }

                var user = await manager.FindByNameAsync(HttpContext.User.Identity.Name);
                if (user is null)
                {
                    return NotFound();
                }

                var actividades = new List<CRMActividades>().AsQueryable();

                if (await manager.IsInRoleAsync(user, "Admin"))
                {
                    //Consulta a la entidad CRMActividades
                    actividades = context.CRMActividades
                       .Where(x => x.Fecha_Creacion >= actividadDTO.Fecha_Creacion && x.Fecha_Creacion <= actividadDTO.Fecha_Ven && x.Estados.Valor.Equals("Completada"))
                       .Include(x => x.asuntos)
                       .Include(x => x.Estados)
                       .Include(x => x.contacto)
                       .Include(x => x.prioridades)
                   .AsQueryable();
                }
                else if (await manager.IsInRoleAsync(user, "LIDER_DE_EQUIPO"))
                {
                    var comercial = await context.CRMOriginadores.FirstOrDefaultAsync(x => x.UserId == user.Id);
                    if (comercial is null)
                    {
                        return NotFound();
                    }

                    var equipos = await context.CRMEquipos.AsNoTracking()
                        .Where(x => x.Activo && x.LiderId == comercial.Id).Select(x => x.Id).ToListAsync();

                    var relacion = await context.CRMEquipoVendedores.AsNoTracking()
                       .Where(x => equipos.Contains(x.EquipoId)).GroupBy(x => x.VendedorId).Select(x => x.Key).ToListAsync();

                    actividades = context.CRMActividades
                        .AsNoTracking()
                        .Where(x => x.Activo && x.Estados.Valor == "Completada" && relacion.Contains((int)x.Asignado) && equipos.Contains((int)x.EquipoId))
                        .Include(x => x.asuntos)
                        .Include(x => x.Estados)
                        .Include(x => x.contacto)
                        .Include(x => x.prioridades)
                        .AsQueryable();
                }
                else if (await manager.IsInRoleAsync(user, "VER_MODULO_HISTORIAL_ACTIVIDADES"))
                {
                    var vendedor = await context.CRMVendedores.FirstOrDefaultAsync(x => x.UserId == user.Id);
                    if (vendedor is null) { return NotFound(); }

                    //Consulta a la entidad CRMActividades
                    actividades = context.CRMActividades
                       .Where(x => x.Fecha_Creacion >= actividadDTO.Fecha_Creacion && x.Fecha_Creacion <= actividadDTO.Fecha_Ven && x.Asignado == vendedor.Id && x.Estados.Valor.Equals("Completada"))
                       .Include(x => x.asuntos)
                       .Include(x => x.Estados)
                       .Include(x => x.contacto)
                       .Include(x => x.prioridades)
                   .AsQueryable();

                }

                //Filtros
                if (!string.IsNullOrEmpty(actividadDTO.Asunto) && !string.IsNullOrWhiteSpace(actividadDTO.Asunto))
                    actividades = actividades.Where(x => x.asuntos != null && x.asuntos.Valor.ToLower().Contains(actividadDTO.Asunto.ToLower()));

                if (!string.IsNullOrEmpty(actividadDTO.Prioridad) && !string.IsNullOrWhiteSpace(actividadDTO.Prioridad))
                    actividades = actividades.Where(x => x.prioridades != null && x.prioridades.Valor.ToLower().Contains(actividadDTO.Prioridad.ToLower()));

                //Paginacion
                await HttpContext.InsertarParametrosPaginacion(actividades, actividadDTO.Registros_por_pagina, actividadDTO.Pagina);

                if (HttpContext.Response.Headers.TryGetValue("pagina", out StringValues value))
                    if (!string.IsNullOrEmpty(value) || !string.IsNullOrWhiteSpace(value) && value != actividadDTO.Pagina)
                        actividadDTO.Pagina = int.Parse(value!);
                actividades = actividades.Skip((actividadDTO.Pagina - 1) * actividadDTO.Registros_por_pagina).Take(actividadDTO.Registros_por_pagina);
                var actividadesdto = actividades.Select(x => mapper.Map<CRMActividadDTO>(x));

                return Ok(actividadesdto);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("historialExcel")]
        public async Task<ActionResult> Historial_ActividadesExcel([FromBody] CRMActividadDTO actividadDTO)
        {
            try
            {
                if (HttpContext.User.Identity is null)
                {
                    return NotFound();
                }

                if (string.IsNullOrEmpty(HttpContext.User.Identity.Name) || string.IsNullOrWhiteSpace(HttpContext.User.Identity.Name))
                {
                    return NotFound();
                }

                var user = await manager.FindByNameAsync(HttpContext.User.Identity.Name);
                if (user is null)
                {
                    return NotFound();
                }


                if (await manager.IsInRoleAsync(user, "Admin"))
                {
                    //Consulta a la entidad CRMActividades
                    var actividades = context.CRMActividades
                       .Where(x => x.Fecha_Creacion >= actividadDTO.Fecha_Creacion && x.Fecha_Creacion <= actividadDTO.Fecha_Ven && x.Estados.Valor.Equals("Completada"))
                       .Include(x => x.asuntos)
                       .Include(x => x.Estados)
                       .Include(x => x.contacto)
                       .Include(x => x.prioridades)
                       .Include(x => x.vendedor)
                       .Select(x => x.Asignacion_Datos())
                   .ToList();

                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    //Generacion de Excel
                    var excel = new ExcelPackage();
                    var worksheet = excel.Workbook.Worksheets.Add("Historial_Actividades");
                    //Formación de Excel
                    var tablebody = worksheet.Cells["A1"].LoadFromCollection(
                        actividades, c =>
                        {
                            c.PrintHeaders = true;
                            c.TableStyle = TableStyles.Medium2;
                        });

                    worksheet.Cells[1, 1, worksheet.Dimension.End.Row, worksheet.Dimension.End.Column].AutoFitColumns();
                    // Formato de fecha para la columna E (Columna 5)
                    worksheet.Cells[1, 5, worksheet.Dimension.End.Row, 9].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";

                    // Formato de fecha para la columna F (Columna 6)
                    worksheet.Cells[1, 2, worksheet.Dimension.End.Row, 6].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";
                    // Formato de fecha para la columna F (Columna 6)
                    worksheet.Cells[1, 3, worksheet.Dimension.End.Row, 7].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";
                    // Formato de fecha para la columna F (Columna 6)
                    worksheet.Cells[1, 4, worksheet.Dimension.End.Row, 8].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";
                    return Ok(excel.GetAsByteArray());

                }
                else if (await manager.IsInRoleAsync(user, "LIDER_DE_EQUIPO"))
                {
                    var comercial = await context.CRMOriginadores.FirstOrDefaultAsync(x => x.UserId == user.Id);
                    if (comercial is null)
                    {
                        return NotFound();
                    }

                    var equipos = await context.CRMEquipos.AsNoTracking()
                        .Where(x => x.Activo && x.LiderId == comercial.Id).Select(x => x.Id).ToListAsync();

                    var relacion = await context.CRMEquipoVendedores.AsNoTracking()
                       .Where(x => equipos.Contains(x.EquipoId)).GroupBy(x => x.VendedorId).Select(x => x.Key).ToListAsync();

                    var actividades = context.CRMActividades
                        .AsNoTracking()
                        .Where(x => x.Activo && x.Estados.Valor == "Completada" && relacion.Contains((int)x.Asignado) && equipos.Contains((int)x.EquipoId))
                        .Include(x => x.asuntos)
                        .Include(x => x.Estados)
                        .Include(x => x.contacto)
                        .Include(x => x.prioridades)
                        .Select(x => x.Asignacion_Datos())
                        .ToList();

                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    //Generacion de Excel
                    var excel = new ExcelPackage();
                    var worksheet = excel.Workbook.Worksheets.Add("Historial_Actividades");
                    //Formación de Excel
                    var tablebody = worksheet.Cells["A1"].LoadFromCollection(
                        actividades, c =>
                        {
                            c.PrintHeaders = true;
                            c.TableStyle = TableStyles.Medium2;
                        });

                    worksheet.Cells[1, 1, worksheet.Dimension.End.Row, worksheet.Dimension.End.Column].AutoFitColumns();
                    // Formato de fecha para la columna E (Columna 5)
                    worksheet.Cells[1, 5, worksheet.Dimension.End.Row, 6].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";

                    // Formato de fecha para la columna F (Columna 6)
                    worksheet.Cells[1, 2, worksheet.Dimension.End.Row, 7].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";
                    // Formato de fecha para la columna F (Columna 6)
                    worksheet.Cells[1, 3, worksheet.Dimension.End.Row, 8].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";
                    // Formato de fecha para la columna F (Columna 6)
                    worksheet.Cells[1, 4, worksheet.Dimension.End.Row, 9].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";

                    return Ok(excel.GetAsByteArray());
                }
                else if (await manager.IsInRoleAsync(user, "VER_MODULO_HISTORIAL_ACTIVIDADES"))
                {
                    var vendedor = await context.CRMVendedores.FirstOrDefaultAsync(x => x.UserId == user.Id);
                    if (vendedor is null) { return NotFound(); }

                    //Consulta a la entidad CRMActividades
                    var actividades = context.CRMActividades
                        .Where(x => x.Fecha_Creacion >= actividadDTO.Fecha_Creacion && x.Fecha_Creacion <= actividadDTO.Fecha_Ven && x.Asignado == vendedor.Id && x.Estados.Valor.Equals("Completada"))
                        .Include(x => x.asuntos)
                        .Include(x => x.Estados)
                        .Include(x => x.contacto)
                        .Include(x => x.prioridades)
                        .Select(x => x.Asignacion_Datos())
                    .ToList();

                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    //Generacion de Excel
                    var excel = new ExcelPackage();
                    var worksheet = excel.Workbook.Worksheets.Add("Historial_Actividades");
                    //Formación de Excel
                    var tablebody = worksheet.Cells["A1"].LoadFromCollection(
                        actividades, c =>
                        {
                            c.PrintHeaders = true;
                            c.TableStyle = TableStyles.Medium2;
                        });

                    worksheet.Cells[1, 1, worksheet.Dimension.End.Row, worksheet.Dimension.End.Column].AutoFitColumns();
                    // Formato de fecha para la columna E (Columna 5)
                    worksheet.Cells[1, 5, worksheet.Dimension.End.Row, 6].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";

                    // Formato de fecha para la columna F (Columna 6)
                    worksheet.Cells[1, 2, worksheet.Dimension.End.Row, 7].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";
                    // Formato de fecha para la columna F (Columna 6)
                    worksheet.Cells[1, 3, worksheet.Dimension.End.Row, 8].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";
                    // Formato de fecha para la columna F (Columna 6)
                    worksheet.Cells[1, 4, worksheet.Dimension.End.Row, 9].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";

                    return Ok(excel.GetAsByteArray());
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

        [HttpGet("{Id:int}")]
        public async Task<ActionResult> ObtenerCatalogoStatus([FromRoute] int Id)
        {
            try
            {
                var actividad = await context.CRMActividades.AsNoTracking()
                    .Where(x => x.Id == Id)
                    .Include(x => x.Documentos.OrderByDescending(y => y.FechaCreacion))
                    .Select(x => mapper.Map<CRMActividadPostDTO>(x))
                    .FirstOrDefaultAsync();

                if (actividad is null)
                {
                    return NotFound();
                }

                return Ok(actividad);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

    }
}

