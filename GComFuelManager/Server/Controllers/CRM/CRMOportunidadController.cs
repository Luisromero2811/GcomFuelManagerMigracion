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

namespace GComFuelManager.Server.Controllers.CRM
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, Administrador Sistema, CRM, LIDER_DE_EQUIPO")]

    public class CRMOportunidadController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IValidator<CRMOportunidadPostDTO> validator;
        private readonly UserManager<IdentityUsuario> userManager;

        public CRMOportunidadController(ApplicationDbContext context, IMapper mapper, IValidator<CRMOportunidadPostDTO> validator, UserManager<IdentityUsuario> userManager)
        {
            this.context = context;
            this.mapper = mapper;
            this.validator = validator;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll([FromQuery] CRMOportunidadDTO dTO)
        {
            try
            {
                var user = await userManager.FindByNameAsync(HttpContext.User.Identity?.Name ?? string.Empty);
                if (user is null) return NotFound();

                var oportunidades = new List<CRMOportunidad>().AsQueryable();
                if (await userManager.IsInRoleAsync(user, "Admin"))
                {
                    oportunidades = context.CRMOportunidades.AsNoTracking().Where(x => x.Activo)
                        .Include(x => x.UnidadMedida)
                        .Include(x => x.Tipo)
                        .Include(x => x.CRMCliente)
                        .Include(x => x.EtapaVenta)
                        .Include(x => x.Vendedor)
                        .Include(x => x.Contacto)
                        .Include(x => x.Equipo)
                        .ThenInclude(x => x.Division)
                        .Include(x => x.ConoceClienteOportunidad)
                        .OrderByDescending(x => x.FechaCreacion)
                        .AsQueryable();
                }
                else if (await userManager.IsInRoleAsync(user, "LIDER_DE_EQUIPO"))
                {
                    var comercial = await context.CRMOriginadores.FirstOrDefaultAsync(x => x.UserId == user.Id);
                    if (comercial is null) return NotFound();

                    var equipos = await context.CRMEquipos.AsNoTracking()
                        .Where(x => x.Activo && x.EquipoOriginadores.Any(e => e.OriginadorId == comercial.Id)).Select(x => x.Id).ToListAsync();

                    var relacion = await context.CRMEquipoVendedores.AsNoTracking()
                        .Where(x => equipos.Contains(x.EquipoId)).GroupBy(x => x.VendedorId).Select(x => x.Key).ToListAsync();

                    oportunidades = context.CRMOportunidades.AsNoTracking().Where(x => x.Activo && relacion.Contains(x.VendedorId) && equipos.Contains(x.EquipoId))
                        .Include(x => x.UnidadMedida)
                        .Include(x => x.Tipo)
                        .Include(x => x.CRMCliente)
                        .Include(x => x.EtapaVenta)
                        .Include(x => x.Vendedor)
                        .Include(x => x.Contacto)
                        .Include(x => x.Equipo)
                        .ThenInclude(x => x.Division)
                        .Include(x => x.ConoceClienteOportunidad)
                        .OrderByDescending(x => x.FechaCreacion)
                        .AsQueryable();
                }
                else if (await userManager.IsInRoleAsync(user, "VER_DOCUMENTOS_JURIDICO"))
                {

                    oportunidades = context.CRMOportunidades.AsNoTracking().Where(x => x.Activo && x.EtapaVentaId == 74 || x.Activo && x.EtapaVentaId == 132)
                       .Include(x => x.UnidadMedida)
                       .Include(x => x.Tipo)
                       .Include(x => x.CRMCliente)
                       .Include(x => x.EtapaVenta)
                       .Include(x => x.Vendedor)
                       .Include(x => x.Contacto)
                       .Include(x => x.Equipo)
                       .ThenInclude(x => x.Division)
                       .Include(x => x.ConoceClienteOportunidad)
                       .OrderByDescending(x => x.FechaCreacion)
                       .AsQueryable();

                }
                else
                {
                    var vendedor = await context.CRMVendedores.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == user.Id);
                    if (vendedor is null) return NotFound();

                    oportunidades = context.CRMOportunidades.AsNoTracking().Where(x => x.Activo && x.VendedorId == vendedor.Id)
                        .Include(x => x.UnidadMedida)
                        .Include(x => x.Tipo)
                        .Include(x => x.CRMCliente)
                        .Include(x => x.EtapaVenta)
                        .Include(x => x.Vendedor)
                        .Include(x => x.Contacto)
                        .Include(x => x.Equipo)
                        .ThenInclude(x => x.Division)
                        .Include(x => x.ConoceClienteOportunidad)
                        .OrderByDescending(x => x.FechaCreacion)
                        .AsQueryable();
                }

                if (!string.IsNullOrEmpty(dTO.Nombre_Opor) || !string.IsNullOrWhiteSpace(dTO.Nombre_Opor))
                    oportunidades = oportunidades.Where(x => x.Nombre_Opor.ToLower().Contains(dTO.Nombre_Opor.ToLower()));

                if (!string.IsNullOrEmpty(dTO.Vendedor) || !string.IsNullOrWhiteSpace(dTO.Vendedor))
                    oportunidades = oportunidades.Where(x => x.Vendedor != null && x.Vendedor.Nombre.ToLower().Contains(dTO.Vendedor.ToLower()));

                if (!string.IsNullOrEmpty(dTO.Contacto) || !string.IsNullOrWhiteSpace(dTO.Contacto))
                    oportunidades = oportunidades.Where(x => x.Contacto != null && x.Contacto.Nombre.ToLower().Contains(dTO.Contacto.ToLower()));

                if (!string.IsNullOrEmpty(dTO.Cuenta) || !string.IsNullOrWhiteSpace(dTO.Cuenta))
                    oportunidades = oportunidades.Where(x => x.CRMCliente != null && x.CRMCliente.Nombre.ToLower().Contains(dTO.Cuenta.ToLower()));

                if (!string.IsNullOrEmpty(dTO.EtapaVenta) || !string.IsNullOrWhiteSpace(dTO.EtapaVenta))
                    oportunidades = oportunidades.Where(x => x.EtapaVenta != null && x.EtapaVenta.Valor.ToLower().Contains(dTO.EtapaVenta.ToLower()));

                if (!string.IsNullOrEmpty(dTO.Equipo) || !string.IsNullOrWhiteSpace(dTO.Equipo))
                    oportunidades = oportunidades.Where(x => x.Equipo != null && x.Equipo.Nombre.ToLower().Contains(dTO.Equipo.ToLower()));

                if (!string.IsNullOrEmpty(dTO.Division) || !string.IsNullOrWhiteSpace(dTO.Division))
                    oportunidades = oportunidades.Where(x => x.Equipo.Division != null && x.Equipo.Division.Nombre.ToLower().Contains(dTO.Division.ToLower()));

                if (!dTO.EquipoId.IsZero())
                    oportunidades = oportunidades.Where(x => x.EquipoId.Equals(dTO.EquipoId));

                if (!dTO.CuentaId.IsZero())
                    oportunidades = oportunidades.Where(x => x.CuentaId.Equals(dTO.CuentaId));

                if (!dTO.VendedorId.IsZero())
                    oportunidades = oportunidades.Where(x => x.VendedorId.Equals(dTO.VendedorId));

                if (dTO.Excel)
                {
                    ExcelPackage.LicenseContext = LicenseContext.Commercial;
                    ExcelPackage excelPackage = new();
                    ExcelWorksheet ws = excelPackage.Workbook.Worksheets.Add("Oportunidades");
                    var oportunidadesexcel = oportunidades
                        .Include(x => x.Periodo)
                        .Include(x => x.CRMCliente)
                        .Include(x => x.Tipo)
                        .Include(x => x.OrigenProducto)
                        .Include(x => x.TipoProducto)
                        .Include(x => x.ModeloVenta)
                        .Include(x => x.Volumen)
                        .Include(x => x.FormaPago)
                        .Include(x => x.DiasCredito)
                        .Include(x => x.Equipo)
                        .ThenInclude(x => x.Division)
                        .Select(x => mapper.Map<CRMOportunidad, CRMOportunidadExcelDTO>(x)).ToList();
                    ws.Cells["A1"].LoadFromCollection(oportunidadesexcel, opt =>
                    {
                        opt.PrintHeaders = true;
                        opt.TableStyle = OfficeOpenXml.Table.TableStyles.Medium12;
                    });

                    ws.Cells[1, 4, ws.Dimension.End.Row, 4].Style.Numberformat.Format = "$#,##0.00";
                    ws.Cells[1, 14, ws.Dimension.End.Row, 16].Style.Numberformat.Format = "#,##0.00";

                    ws.Cells[1, 1, ws.Dimension.End.Row, ws.Dimension.End.Column].AutoFitColumns();

                    return Ok(excelPackage.GetAsByteArray());
                }

                await HttpContext.InsertarParametrosPaginacion(oportunidades, dTO.Registros_por_pagina, dTO.Pagina);

                if (HttpContext.Response.Headers.TryGetValue("pagina", out StringValues value))
                    if (!string.IsNullOrEmpty(value) || !string.IsNullOrWhiteSpace(value) && value != dTO.Pagina)
                        dTO.Pagina = int.Parse(value!);

                oportunidades = oportunidades.Skip((dTO.Pagina - 1) * dTO.Registros_por_pagina).Take(dTO.Registros_por_pagina);

                var dtoportunidades = oportunidades.Select(x => mapper.Map<CRMOportunidadDTO>(x));

                return Ok(dtoportunidades);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult> GetById([FromRoute] int id)
        {
            try
            {
                var oportunidad = await context.CRMOportunidades.AsNoTracking()
                    .Where(x => x.Id == id)
                    .Include(x => x.Documentos.OrderByDescending(y => y.FechaCreacion))
                        .ThenInclude(x => x.DocumentoTipoDocumentos)
                        .ThenInclude(x => x.TipoDocumento)
                   .Select(x => new CRMOportunidadPostDTO
                   {
                       // Mapear propiedades principales
                       Id = x.Id,
                       Nombre_Opor = x.Nombre_Opor,
                       ValorOportunidad = x.ValorOportunidad,
                       UnidadMedidaId = x.UnidadMedidaId,
                       Prox_Paso = x.Prox_Paso,
                       VendedorId = x.VendedorId,
                       CuentaId = x.CuentaId,
                       ContactoId = x.ContactoId,
                       PeriodoId = x.PeriodoId,
                       TipoId = x.TipoId,
                       FechaCreacion = x.FechaCreacion,
                       FechaCierre = x.FechaCierre,
                       EtapaVentaId = x.EtapaVentaId,
                       Probabilidad = x.Probabilidad,
                       OrigenPrductoId = x.OrigenPrductoId,
                       TipoProductoId = x.TipoProductoId,
                       ModeloVentaId = x.ModeloVentaId,
                       VolumenId = x.VolumenId,
                       FormaPagoId = x.FormaPagoId,
                       DiasPagoId = x.DiasPagoId,
                       CantidadEstaciones = x.CantidadEstaciones,
                       CantidadLts = x.CantidadLts,
                       PrecioLts = x.PrecioLts,
                       TotalLts = x.TotalLts,
                       EquipoId = x.EquipoId,
                       DocumentoId = x.Documentos.OrderByDescending(x => x.FechaCreacion).Select(x => x.Id).FirstOrDefault(),

                       // Mapeo directo de propiedades del último documento
                       NombreDocumento = x.Documentos
                    .OrderByDescending(doc => doc.FechaCreacion)
                    .Select(doc => doc.NombreDocumento)
                    .FirstOrDefault() ?? string.Empty,
                       FechaCaducidad = x.Documentos
                    .OrderByDescending(doc => doc.FechaCreacion)
                    .Select(doc => doc.FechaCaducidad)
                    .FirstOrDefault(),
                       Version = x.Documentos
                    .OrderByDescending(doc => doc.FechaCreacion)
                    .Select(doc => doc.Version)
                    .FirstOrDefault() ?? string.Empty,
                       Descripcion = x.Documentos
                    .OrderByDescending(doc => doc.FechaCreacion)
                    .Select(doc => doc.Descripcion)
                    .FirstOrDefault() ?? string.Empty,

                       DocumentoReciente = x.Documentos
                          .OrderByDescending(doc => doc.FechaCreacion)
                          .Select(doc => new CRMDocumentoDTO
                          {
                              Id = doc.Id,
                              NombreDocumento = doc.NombreDocumento ?? string.Empty,
                              FechaCaducidad = doc.FechaCaducidad,
                              Version = doc.Version ?? string.Empty,
                              Descripcion = doc.Descripcion ?? string.Empty,

                          })
                                .FirstOrDefault(),


                       // IDs de tipos de documentos relacionados
                       TiposDocumentoIds = x.Documentos
                            .SelectMany(doc => doc.DocumentoTipoDocumentos.Select(dtd => dtd.TipoDocumento.Id))
                            .Distinct()
                            .ToList()
                   })
                    .FirstOrDefaultAsync();
                if (oportunidad is null) { return NotFound(); }

                return Ok(oportunidad);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("{Id:int}/detalle")]
        public async Task<ActionResult> GetByIdDetail([FromRoute] int Id)
        {
            try
            {
                var contacto = await context.CRMOportunidades
                    .AsNoTracking()
                    .Where(x => x.Id == Id)
                    .Include(x => x.Vendedor)
                    .Include(x => x.Contacto)
                    .Include(x => x.UnidadMedida)
                    .Include(x => x.EtapaVenta)
                    .Include(x => x.Periodo)
                    .Include(x => x.CRMCliente)
                    .Include(x => x.Tipo)
                    .Include(x => x.OrigenProducto)
                    .Include(x => x.TipoProducto)
                    .Include(x => x.ModeloVenta)
                    .Include(x => x.Volumen)
                    .Include(x => x.FormaPago)
                    .Include(x => x.DiasCredito)
                    .Include(x => x.Equipo)
                    .ThenInclude(x => x.Division)
                    .Include(x => x.Documentos.OrderByDescending(x => x.FechaCaducidad))
                    .Select(x => mapper.Map<CRMOportunidadDetalleDTO>(x))
                    .FirstOrDefaultAsync();

                return Ok(contacto);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CRMOportunidadPostDTO dto)
        {
            try
            {
                var user = await userManager.FindByNameAsync(HttpContext.User.Identity?.Name ?? string.Empty);
                if (user is null) return NotFound();

                var result = validator.Validate(dto);
                if (!result.IsValid) { return BadRequest(result.Errors); }

                var na = await context.CRMCatalogoValores.AsNoTracking().FirstOrDefaultAsync(x => x.Valor.ToUpper().Equals("N/A"));

                var oportunidad = mapper.Map<CRMOportunidadPostDTO, CRMOportunidad>(dto);

                if (dto.EsConclusion)
                {
                    var estatusConcluida = await context.CRMCatalogoValores
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Valor.ToUpper().Equals("CONCLUIDA"));

                    if (estatusConcluida is null) return NotFound("Estatus 'Concluida' no encontrado.");

                    oportunidad.EtapaVentaId = estatusConcluida.Id;
                    oportunidad.FechaConclusión = DateTime.Now; 

                    context.Update(oportunidad);
                    await context.SaveChangesAsync();

                    return Ok("Oportunidad concluida exitosamente.");
                }

                if (oportunidad.DiasPagoId.IsZero())
                {
                    if (na is null) return NotFound();
                    oportunidad.DiasPagoId = na.Id;
                }

                var estatus = await context.CRMCatalogoValores.AsNoTracking().FirstOrDefaultAsync(x => x.Valor.ToUpper().Equals("GANADO"));
                if (estatus is not null)
                {
                    if (estatus.Id == oportunidad.EtapaVentaId)
                    {

                        if (oportunidad.FechaGanada == null)
                        {
                            oportunidad.FechaGanada = DateTime.Now;
                        }

                        if (!await context.CRMOportunidades.AnyAsync(x => x.ContactoId == oportunidad.ContactoId && x.EtapaVentaId == estatus.Id))
                        {
                            var contacto = await context.CRMContactos.FirstOrDefaultAsync(x => x.Id == oportunidad.ContactoId);
                            if (contacto is not null)
                            {
                                var estatuscontacto = await context.CRMCatalogoValores.AsNoTracking().FirstOrDefaultAsync(x => x.Valor.ToUpper().Equals("CONVERTIDO"));
                                if (estatuscontacto is not null)
                                {
                                    contacto.EstatusId = estatuscontacto.Id;
                                    context.Update(contacto);
                                }
                            }
                        }
                    }
                }

                if (oportunidad.Id != 0)
                {
                    var oportunidaddb = await context.CRMOportunidades.AsNoTracking().FirstOrDefaultAsync(x => x.Id == oportunidad.Id);
                    if (oportunidaddb is not null)
                    {
                        if (oportunidaddb.EtapaVentaId != oportunidad.EtapaVentaId)
                        {
                            var historial = new CRMOportunidadEstadoHistorial
                            {
                                UserId = user.Id,
                                Oportunidad = oportunidad,
                                EtapaVentaId = oportunidad.EtapaVentaId
                            };
                            await context.AddAsync(historial);
                        }
                    }

                    context.Update(oportunidad);
                }
                else
                {
                    var historial = new CRMOportunidadEstadoHistorial
                    {
                        UserId = user.Id,
                        Oportunidad = oportunidad,
                        EtapaVentaId = oportunidad.EtapaVentaId
                    };

                    await context.AddAsync(oportunidad);
                    await context.AddAsync(historial);
                }

                if (!dto.DocumentoId.IsZero())
                {
                    var doc = await context.CRMDocumentos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == dto.DocumentoId);
                    if (doc is not null)
                    {
                        var documento = mapper.Map<CRMOportunidadPostDTO, CRMDocumento>(dto);
                        if (documento is not null)
                        {
                            var docupdate = mapper.Map(documento, doc);

                            var relacionesExistentes = context.DocumentoTipoDocumento.Where(x => x.DocumentoId == documento.Id);
                            context.DocumentoTipoDocumento.RemoveRange(relacionesExistentes);

                            // Registrar nuevas relaciones
                            if (dto.TiposDocumentoIds?.Count > 0)
                            {
                                foreach (var tipoId in dto.TiposDocumentoIds)
                                {
                                    var nuevaRelacion = new DocumentoTipoDocumento
                                    {
                                        DocumentoId = documento.Id,
                                        TipoDocumentoId = tipoId
                                    };
                                    await context.DocumentoTipoDocumento.AddAsync(nuevaRelacion);
                                }
                            }

                            if (!dto.DocumentoRelacionado.IsZero())
                            {
                                var docrelacionad = await context.CRMDocumentos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == dto.DocumentoRelacionado);
                                if (docrelacionad is not null)
                                {
                                    var docrelacionadorelacion = new CRMDocumentoRelacionado() { DocumentoId = documento.Id, DocumentoRelacionadoId = docrelacionad.Id };
                                    await context.AddAsync(docrelacionadorelacion);
                                }
                            }

                            if (!dto.DocumentoRevision.IsZero())
                            {
                                var docrelacionad = await context.CRMDocumentos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == dto.DocumentoRevision);
                                if (docrelacionad is not null)
                                {
                                    var docrelacionadorelacion = new CRMDocumentoRevision() { DocumentoId = documento.Id, RevisionId = docrelacionad.Id };
                                    await context.AddAsync(docrelacionadorelacion);
                                }
                            }

                            context.Update(docupdate);
                            if (!await context.CRMOportunidadDocumentos.AnyAsync(x => x.DocumentoId == documento.Id))
                            {
                                var docopo = new CRMOportunidadDocumento() { DocumentoId = documento.Id, Oportunidad = oportunidad, Documento = null! };
                                await context.AddAsync(docopo);
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

        [HttpDelete("{Id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int Id)
        {
            try
            {
                var oportunidad = await context.CRMOportunidades.FindAsync(Id);
                if (oportunidad is null) { return NotFound(); }

                oportunidad.Activo = false;

                context.Update(oportunidad);
                await context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/medida")]
        public async Task<ActionResult> ObtenerCatalogoMedida()
        {
            try
            {
                var catalogo = await context.CRMCatalogos.AsNoTracking().Include(x => x.Valores.Where(y => y.Activo)).FirstOrDefaultAsync(x => x.Nombre.Equals("Catalogo_Oportunidad_Medida"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para unidades de medida"); }
                return Ok(catalogo.Valores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/tipo")]
        public async Task<ActionResult> ObtenerCatalogoTipo()
        {
            try
            {
                var catalogo = await context.CRMCatalogos.AsNoTracking().Include(x => x.Valores.Where(y => y.Activo)).FirstOrDefaultAsync(x => x.Nombre.Equals("Catalogo_Oportunidad_Tipo"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para tipo"); }
                return Ok(catalogo.Valores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/etapa")]
        public async Task<ActionResult> ObtenerCatalogoEtapa()
        {
            try
            {
                var catalogo = await context.CRMCatalogos.AsNoTracking().Include(x => x.Valores.Where(y => y.Activo && !x.Nombre.Equals("CONCLUIDA"))).FirstOrDefaultAsync(x => x.Nombre.Equals("Catalogo_Oportunidad_Etapa"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para etapa de venta"); }
                return Ok(catalogo.Valores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/periodo")]
        public async Task<ActionResult> ObtenerPeriodo()
        {
            try
            {
                var catalogo = await context.CRMCatalogos.AsNoTracking().Include(x => x.Valores.Where(y => y.Activo)).FirstOrDefaultAsync(x => x.Nombre.Equals("Catalogo_Oportunidad_Periodo"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para el periodo"); }
                return Ok(catalogo.Valores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/origen/producto")]
        public async Task<ActionResult> ObtenerOrigenProducto()
        {
            try
            {
                var catalogo = await context.CRMCatalogos.AsNoTracking().Include(x => x.Valores.Where(y => y.Activo)).FirstOrDefaultAsync(x => x.Nombre.Equals("Catalogo_Oportunidad_Origen_Producto"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para el origen del producto"); }
                return Ok(catalogo.Valores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/tipo/producto")]
        public async Task<ActionResult> ObtenerTipoProducto()
        {
            try
            {
                var catalogo = await context.CRMCatalogos.AsNoTracking().Include(x => x.Valores.Where(y => y.Activo)).FirstOrDefaultAsync(x => x.Nombre.Equals("Catalogo_Oportunidad_Productos"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para los productos"); }
                return Ok(catalogo.Valores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/modelo/venta")]
        public async Task<ActionResult> ObtenerModeloVenta()
        {
            try
            {
                var catalogo = await context.CRMCatalogos.AsNoTracking().Include(x => x.Valores.Where(y => y.Activo)).FirstOrDefaultAsync(x => x.Nombre.Equals("Catalogo_Oportunidad_Modelo_Venta"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para los productos"); }
                return Ok(catalogo.Valores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/volumen")]
        public async Task<ActionResult> ObtenerVolumen()
        {
            try
            {
                var catalogo = await context.CRMCatalogos.AsNoTracking().Include(x => x.Valores.Where(y => y.Activo)).FirstOrDefaultAsync(x => x.Nombre.Equals("Catalogo_Oportunidad_Volumen"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para los productos"); }
                return Ok(catalogo.Valores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/forma/pago")]
        public async Task<ActionResult> ObtenerFormaPago()
        {
            try
            {
                var catalogo = await context.CRMCatalogos.AsNoTracking().Include(x => x.Valores.Where(y => y.Activo)).FirstOrDefaultAsync(x => x.Nombre.Equals("Catalogo_Oportunidad_Forma_Pago"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para los productos"); }
                return Ok(catalogo.Valores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/dias/credito")]
        public async Task<ActionResult> ObtenerDiasCredito()
        {
            try
            {
                var catalogo = await context.CRMCatalogos.AsNoTracking().Include(x => x.Valores.Where(y => y.Activo)).FirstOrDefaultAsync(x => x.Nombre.Equals("Catalogo_Oportunidad_Dias_Credito"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para los productos"); }
                return Ok(catalogo.Valores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
