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
using OfficeOpenXml;

namespace GComFuelManager.Server.Controllers.CRM
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CRMContactosController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IValidator<CRMContactoPostDTO> validator;
        private readonly IMapper mapper;
        private readonly UserManager<IdentityUsuario> manager;

        public CRMContactosController(ApplicationDbContext context, IValidator<CRMContactoPostDTO> validator, IMapper mapper, UserManager<IdentityUsuario> manager)
        {
            this.context = context;
            this.validator = validator;
            this.mapper = mapper;
            this.manager = manager;
        }

        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] CRMContactoDTO contacto)
        {
            try
            {
                if (HttpContext.User.Identity is null) { return NotFound(); }
                if (string.IsNullOrEmpty(HttpContext.User.Identity.Name) || string.IsNullOrWhiteSpace(HttpContext.User.Identity.Name)) { return NotFound(); }

                var user = await manager.FindByNameAsync(HttpContext.User.Identity.Name);
                if (user is null) { return NotFound(); }

                var contactos = new List<CRMContacto>().AsQueryable();

                if (await manager.IsInRoleAsync(user, "Admin"))
                {
                    contactos = context.CRMContactos.AsNoTracking().Where(x => x.Activo)
                        .Include(x => x.Estatus)
                        .Include(x => x.Origen)
                        .Include(x => x.Cliente)
                        .Include(x => x.Vendedor)
                        .Include(x => x.Division)
                        .AsQueryable();
                }
                else if (await manager.IsInRoleAsync(user, "LIDER_DE_EQUIPO"))
                {
                    var comercial = await context.CRMOriginadores.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == user.Id);
                    if (comercial is not null)
                    {
                        List<int> equipos = await context.CRMEquipos
                            .AsNoTracking()
                            .Where(x => x.Activo && x.EquipoOriginadores.Any(e => e.OriginadorId == comercial.Id))
                            .Select(x => x.Id)
                            .ToListAsync();

                        List<int?> vendedoresequipo = await context.CRMEquipoVendedores
                            .AsNoTracking()
                            .Where(x => equipos.Contains(x.EquipoId))
                            .GroupBy(x => x.VendedorId)
                            .Select(x => (int?)x.Key)
                            .ToListAsync();

                        contactos = context.CRMContactos.AsNoTracking().Where(x => x.Activo
                        && vendedoresequipo.Contains(x.VendedorId))
                            .Include(x => x.Estatus)
                            .Include(x => x.Origen)
                            .Include(x => x.Cliente)
                            .Include(x => x.Vendedor)
                            .Include(x => x.Division)
                            .AsQueryable();
                    }
                }
                else
                {
                    var vendedor = await context.CRMVendedores.FirstOrDefaultAsync(x => x.UserId == user.Id);
                    if (vendedor is null) { return NotFound(); }

                    contactos = context.CRMContactos.AsNoTracking().Where(x => x.Activo && x.VendedorId == vendedor.Id)
                        .Include(x => x.Estatus)
                        .Include(x => x.Origen)
                        .Include(x => x.Cliente)
                        .Include(x => x.Vendedor)
                        .Include(x => x.Division)
                        .AsQueryable();
                }

                if (!string.IsNullOrEmpty(contacto.Nombre) && !string.IsNullOrWhiteSpace(contacto.Nombre))
                    contactos = contactos.Where(x => x.Nombre.ToLower().Contains(contacto.Nombre.ToLower()) || x.Apellidos.ToLower().Contains(contacto.Nombre.ToLower()));

                if (!string.IsNullOrEmpty(contacto.Cuenta) && !string.IsNullOrWhiteSpace(contacto.Cuenta))
                    contactos = contactos.Where(x => x.Cliente != null && x.Cliente.Nombre.ToLower().Contains(contacto.Cuenta.ToLower()));

                if (!string.IsNullOrEmpty(contacto.Vendedor) && !string.IsNullOrWhiteSpace(contacto.Vendedor))
                    contactos = contactos.Where(x => x.Vendedor != null && x.Vendedor.Nombre.ToLower().Contains(contacto.Vendedor.ToLower()));

                if (!contacto.VendedorId.IsZero())
                    contactos = contactos.Where(x => x.VendedorId.Equals(contacto.VendedorId));

                if (!string.IsNullOrEmpty(contacto.Correo) && !string.IsNullOrWhiteSpace(contacto.Correo))
                    contactos = contactos.Where(x => x.Correo.ToLower().Contains(contacto.Correo.ToLower()));

                if (!string.IsNullOrEmpty(contacto.Tel_Movil) && !string.IsNullOrWhiteSpace(contacto.Tel_Movil))
                    contactos = contactos.Where(x => x.Tel_Movil.ToLower().Contains(contacto.Tel_Movil.ToLower()));

                if (!string.IsNullOrEmpty(contacto.Tel_Oficina) && !string.IsNullOrWhiteSpace(contacto.Tel_Oficina))
                    contactos = contactos.Where(x => x.Tel_Oficina.ToLower().Contains(contacto.Tel_Oficina.ToLower()));

                if (!string.IsNullOrEmpty(contacto.Estatus) && !string.IsNullOrWhiteSpace(contacto.Estatus))
                    contactos = contactos.Where(x => x.Estatus != null && x.Estatus.Valor.ToLower().Contains(contacto.Estatus.ToLower()));

                if (!string.IsNullOrEmpty(contacto.Division) && !string.IsNullOrWhiteSpace(contacto.Division))
                    contactos = contactos.Where(x => x.Division != null && x.Division.Nombre.ToLower().Contains(contacto.Division.ToLower()));

                if (contacto.CuentaId != 0)
                    contactos = contactos.Where(x => x.CuentaId.Equals(contacto.CuentaId));

                if (contacto.Excel)
                {
                    if (await manager.IsInRoleAsync(user, "Admin"))
                    {
                        ExcelPackage.LicenseContext = LicenseContext.Commercial;
                        ExcelPackage excelPackage = new();
                        ExcelWorksheet ws = excelPackage.Workbook.Worksheets.Add("Contactos");

                        var contactosExcel = context.CRMContactos.AsNoTracking().Where(x => x.Activo)
                            .Include(x => x.Estatus)
                            .Include(x => x.Origen)
                            .Include(x => x.Cliente)
                            .Include(x => x.Vendedor)
                            .Include(x => x.Division)
                           .OrderByDescending(x => x.Fecha_Creacion)
                           .Select(x => mapper.Map<CRMContacto, CRMContactosExcelDTO>(x))
                           .ToList();


                        ws.Cells["A1"].LoadFromCollection(contactosExcel, opt =>
                        {
                            opt.PrintHeaders = true;
                            opt.TableStyle = OfficeOpenXml.Table.TableStyles.Medium12;
                        });

                        ws.Cells[1, 1, ws.Dimension.End.Row, ws.Dimension.End.Column].AutoFitColumns();

                        return Ok(excelPackage.GetAsByteArray());

                    }
                    else if (await manager.IsInRoleAsync(user, "LIDER_DE_EQUIPO"))
                    {
                        ExcelPackage.LicenseContext = LicenseContext.Commercial;
                        ExcelPackage excelPackage = new();
                        ExcelWorksheet ws = excelPackage.Workbook.Worksheets.Add("Contactos");

                        var comercial = await context.CRMOriginadores.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == user.Id);
                        if (comercial is not null)
                        {
                            List<int> equipos = await context.CRMEquipos
                                .AsNoTracking()
                                .Where(x => x.Activo && x.EquipoOriginadores.Any(e => e.OriginadorId == comercial.Id))
                                .Select(x => x.Id)
                                .ToListAsync();

                            List<int?> vendedoresequipo = await context.CRMEquipoVendedores
                                .AsNoTracking()
                                .Where(x => equipos.Contains(x.EquipoId))
                                .GroupBy(x => x.VendedorId)
                                .Select(x => (int?)x.Key)
                                .ToListAsync();

                            var contactosExcel = context.CRMContactos.AsNoTracking().Where(x => x.Activo
                             && vendedoresequipo.Contains(x.VendedorId))
                                 .Include(x => x.Estatus)
                                 .Include(x => x.Origen)
                                 .Include(x => x.Cliente)
                                 .Include(x => x.Vendedor)
                                 .Include(x => x.Division)
                                 .OrderByDescending(x => x.Fecha_Creacion)
                                .Select(x => mapper.Map<CRMContacto, CRMContactosExcelDTO>(x))
                                .ToList();


                            ws.Cells["A1"].LoadFromCollection(contactosExcel, opt =>
                            {
                                opt.PrintHeaders = true;
                                opt.TableStyle = OfficeOpenXml.Table.TableStyles.Medium12;
                            });

                            ws.Cells[1, 1, ws.Dimension.End.Row, ws.Dimension.End.Column].AutoFitColumns();

                            return Ok(excelPackage.GetAsByteArray());

                        }
                    }
                    else
                    {
                        ExcelPackage.LicenseContext = LicenseContext.Commercial;
                        ExcelPackage excelPackage = new();
                        ExcelWorksheet ws = excelPackage.Workbook.Worksheets.Add("Contactos");

                        var vendedor = await context.CRMVendedores.FirstOrDefaultAsync(x => x.UserId == user.Id);
                        if (vendedor is null) { return NotFound(); }

                        var contactosExcel = context.CRMContactos.AsNoTracking().Where(x => x.Activo && x.VendedorId == vendedor.Id)
                             .Include(x => x.Estatus)
                             .Include(x => x.Origen)
                             .Include(x => x.Cliente)
                             .Include(x => x.Vendedor)
                             .Include(x => x.Division)
                             .OrderByDescending(x => x.Fecha_Creacion)
                             .Select(x => mapper.Map<CRMContacto, CRMContactosExcelDTO>(x))
                             .ToList();


                        ws.Cells["A1"].LoadFromCollection(contactosExcel, opt =>
                        {
                            opt.PrintHeaders = true;
                            opt.TableStyle = OfficeOpenXml.Table.TableStyles.Medium12;
                        });

                        ws.Cells[1, 1, ws.Dimension.End.Row, ws.Dimension.End.Column].AutoFitColumns();

                        return Ok(excelPackage.GetAsByteArray());
                    }
                }

                if (contacto.Paginacion)
                {
                    await HttpContext.InsertarParametrosPaginacion(contactos, contacto.Registros_por_pagina, contacto.Pagina);
                    contacto.Pagina = HttpContext.ObtenerPagina();
                    contactos = contactos.Skip((contacto.Pagina - 1) * contacto.Registros_por_pagina).Take(contacto.Registros_por_pagina);
                }

                var contactosdto = contactos.Select(x => mapper.Map<CRMContactoDTO>(x));

                return Ok(contactosdto);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("contactList")]
        public async Task<ActionResult> GetContact([FromQuery] CRMContactoDTO contacto)
        {
            try
            {
                var contactos = new List<CRMContacto>().AsQueryable();
                contactos = context.CRMContactos.AsNoTracking().Where(x => x.Activo)
                       .Include(x => x.Estatus)
                       .Include(x => x.Origen)
                       .Include(x => x.Cliente)
                       .Include(x => x.Vendedor)
                       .Include(x => x.Division)
                       .AsQueryable();
                if (!string.IsNullOrEmpty(contacto.Nombre) && !string.IsNullOrWhiteSpace(contacto.Nombre))
                    contactos = contactos.Where(x => x.Nombre.ToLower().Contains(contacto.Nombre.ToLower()) || x.Apellidos.ToLower().Contains(contacto.Nombre.ToLower()));

                var contactosdto = contactos.Select(x => mapper.Map<CRMContactoDTO>(x));

                return Ok(contactosdto);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("no/division")]
        public async Task<ActionResult> GetAnotherDivision([FromQuery] CRMContactoDTO contacto)
        {
            try
            {
                if (HttpContext.User.Identity is null) { return NotFound(); }
                if (string.IsNullOrEmpty(HttpContext.User.Identity.Name) || string.IsNullOrWhiteSpace(HttpContext.User.Identity.Name)) { return NotFound(); }

                var user = await manager.FindByNameAsync(HttpContext.User.Identity.Name);
                if (user is null) { return NotFound(); }

                var contactos = new List<CRMContacto>().AsQueryable();
                if (await manager.IsInRoleAsync(user, "Admin"))
                {

                    contactos = context.CRMContactos.AsNoTracking().Where(x => x.Activo)
                        .Include(x => x.Estatus)
                        .Include(x => x.Origen)
                        .Include(x => x.Cliente)
                        .Include(x => x.Vendedor)
                        .Include(x => x.Division)
                        .AsQueryable();
                }
                else if (await manager.IsInRoleAsync(user, "LIDER_DE_EQUIPO"))
                {
                    var originador = await context.CRMOriginadores.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == user.Id);
                    if (originador is not null)
                    {
                        var equipos = await context.CRMEquipos
                            .AsNoTracking()
                            .Where(x => x.Activo && x.EquipoOriginadores.Any(e => e.OriginadorId == originador.Id))
                            .Include(x => x.Vendedores)
                            .Select(x => x.Id)
                            .ToListAsync();

                        var equipovendedores = await context.CRMEquipoVendedores
                            .AsNoTracking()
                            .Where(x => equipos.Contains(x.EquipoId))
                            .GroupBy(x => x.VendedorId)
                            .Select(x => (int?)x.Key)
                            .ToListAsync();

                        contactos = context.CRMContactos.AsNoTracking().Where(x => x.Activo
                        && equipovendedores.Contains(x.VendedorId))
                                    .Include(x => x.Estatus)
                                    .Include(x => x.Origen)
                                    .Include(x => x.Cliente)
                                    .Include(x => x.Vendedor)
                                    .Include(x => x.Division)
                                    .AsQueryable();
                    }

                }
                else if (await manager.IsInRoleAsync(user, "VER_CONTACTOS"))
                {
                    var vendedor = await context.CRMVendedores.FirstOrDefaultAsync(x => x.UserId == user.Id);
                    if (vendedor is not null)
                    {
                        List<int?> equipos = await context.CRMEquipoVendedores.AsNoTracking()
                            .Where(x => x.VendedorId == vendedor.Id).Select(x => (int?)x.EquipoId)
                            .ToListAsync();

                        List<int?> vendedoresEnEquipo = await context.CRMEquipoVendedores
                            .AsNoTracking()
                            .Where(x => equipos.Any(y => y == x.EquipoId))
                            .Select(x => (int?)x.VendedorId)
                            .ToListAsync();

                        contactos = context.CRMContactos.AsNoTracking().Where(x => x.Vendedor != null && x.Activo && vendedoresEnEquipo.Any(y => y == x.VendedorId))
                                    .Include(x => x.Estatus)
                                    .Include(x => x.Origen)
                                    .Include(x => x.Cliente)
                                    .Include(x => x.Vendedor)
                                    .Include(x => x.Division)
                                    .AsQueryable();
                    }
                }

                if (!string.IsNullOrEmpty(contacto.Nombre) && !string.IsNullOrWhiteSpace(contacto.Nombre))
                    contactos = contactos.Where(x => x.Nombre.ToLower().Contains(contacto.Nombre.ToLower()) || x.Apellidos.ToLower().Contains(contacto.Nombre.ToLower()));

                if (!string.IsNullOrEmpty(contacto.Cuenta) && !string.IsNullOrWhiteSpace(contacto.Cuenta))
                    contactos = contactos.Where(x => x.Cliente != null && x.Cliente.Nombre.ToLower().Contains(contacto.Cuenta.ToLower()));

                if (!string.IsNullOrEmpty(contacto.Vendedor) && !string.IsNullOrWhiteSpace(contacto.Vendedor))
                    contactos = contactos.Where(x => x.Vendedor != null && x.Vendedor.Nombre.ToLower().Contains(contacto.Vendedor.ToLower()));

                if (!string.IsNullOrEmpty(contacto.Correo) && !string.IsNullOrWhiteSpace(contacto.Correo))
                    contactos = contactos.Where(x => x.Correo.ToLower().Contains(contacto.Correo.ToLower()));

                if (!string.IsNullOrEmpty(contacto.Tel_Movil) && !string.IsNullOrWhiteSpace(contacto.Tel_Movil))
                    contactos = contactos.Where(x => x.Tel_Movil.ToLower().Contains(contacto.Tel_Movil.ToLower()));

                if (!string.IsNullOrEmpty(contacto.Estatus) && !string.IsNullOrWhiteSpace(contacto.Estatus))
                    contactos = contactos.Where(x => x.Estatus != null && x.Estatus.Valor.ToLower().Contains(contacto.Estatus.ToLower()));

                if (!string.IsNullOrEmpty(contacto.Division) && !string.IsNullOrWhiteSpace(contacto.Division))
                    contactos = contactos.Where(x => x.Division != null && x.Division.Nombre.ToLower().Contains(contacto.Division.ToLower()));

                await HttpContext.InsertarParametrosPaginacion(contactos, contacto.Registros_por_pagina, contacto.Pagina);

                contacto.Pagina = HttpContext.ObtenerPagina();

                //var contactos_filtrados = await contactos.Select(x => mapper.Map<CRMContactoDTO>(x)).ToListAsync();
                contactos = contactos.Skip((contacto.Pagina - 1) * contacto.Registros_por_pagina).Take(contacto.Registros_por_pagina);

                var contactosdto = contactos.Select(x => mapper.Map<CRMContactoDTO>(x));

                return Ok(contactosdto);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("{Id:int}")]
        public async Task<ActionResult> GetById([FromRoute] int Id)
        {
            try
            {
                var contacto = await context.CRMContactos
                    .AsNoTracking()
                    .Where(x => x.Id == Id)
                    .Select(x => mapper.Map<CRMContactoPostDTO>(x)).FirstOrDefaultAsync();
                return Ok(contacto);
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
                var contacto = await context.CRMContactos
                    .AsNoTracking()
                    .Where(x => x.Id == Id)
                    .Include(x => x.Vendedor)
                    .Include(x => x.Cliente)
                    .Include(x => x.Estatus)
                    .Include(x => x.Origen)
                    .Include(x => x.Division)
                    .Select(x => mapper.Map<CRMContactoDetalleDTO>(x)).FirstOrDefaultAsync();
                return Ok(contacto);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CRMContactoPostDTO contactodto)
        {
            try
            {
                var result = await validator.ValidateAsync(contactodto);
                if (!result.IsValid) { return BadRequest(result.Errors); }
                var contacto = mapper.Map<CRMContactoPostDTO, CRMContacto>(contactodto);

                if (contacto.Id != 0)
                {
                    contacto.Fecha_Mod = DateTime.Now;
                    context.Update(contacto);
                }
                else
                    await context.AddAsync(contacto);

                await context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("{Id:int}")]
        public async Task<IActionResult> EliminarContacto([FromRoute] int Id)
        {
            try
            {
                var contacto = await context.CRMContactos.FindAsync(Id);
                if (contacto is null) { return NotFound(); }

                contacto.Activo = false;
                contacto.Fecha_Mod = DateTime.Now;
                context.Update(contacto);
                await context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/status")]
        public async Task<ActionResult> ObtenerCatalogoStatus()
        {
            try
            {
                var catalogo = await context.CRMCatalogos.AsNoTracking().Include(x => x.Valores).FirstOrDefaultAsync(x => x.Nombre.Equals("Catalogo_Contacto_Status"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para estatus"); }
                return Ok(catalogo.Valores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/origen")]
        public async Task<ActionResult> ObtenerCatalogoOrigen()
        {
            try
            {
                var catalogo = await context.CRMCatalogos.AsNoTracking().Include(x => x.Valores).FirstOrDefaultAsync(x => x.Nombre.Equals("Catalogo_Contacto_Origen"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para origenes"); }

                return Ok(catalogo.Valores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
