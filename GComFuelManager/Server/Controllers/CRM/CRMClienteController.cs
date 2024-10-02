using AutoMapper;
using FluentValidation;
using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.DTOs.CRM;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Server.Controllers.CRM
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CRMClienteController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUsuario> userManager;
        private readonly IMapper mapper;
        private readonly IValidator<CRMClientePostDTO> validator;

        public CRMClienteController(ApplicationDbContext context, UserManager<IdentityUsuario> userManager, IMapper mapper, IValidator<CRMClientePostDTO> validator)
        {
            this.context = context;
            this.userManager = userManager;
            this.mapper = mapper;
            this.validator = validator;
        }

        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] CRMClienteDTO cliente)
        {
            try
            {
                var clientes = context.CRMClientes.AsNoTracking().Where(x => x.Activo).AsQueryable();

                if (!string.IsNullOrEmpty(cliente.Nombre))
                    clientes = clientes.Where(x => x.Nombre.ToLower().Contains(cliente.Nombre.ToLower()));

                if (cliente.Paginacion)
                {
                    await HttpContext.InsertarParametrosPaginacion(clientes, cliente.Registros_por_pagina, cliente.Pagina);
                    cliente.Pagina_ACtual = HttpContext.ObtenerPagina();
                    clientes = clientes.Skip((cliente.Pagina - 1) * cliente.Registros_por_pagina).Take(cliente.Registros_por_pagina);
                }

                var clientesdto = clientes.Select(x => mapper.Map<CRMCliente, CRMClienteDTO>(x));
                return Ok(clientesdto);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CRMClientePostDTO dto)
        {
            try
            {
                var validate = validator.Validate(dto);
                if (!validate.IsValid) return BadRequest(validate.Errors);

                var cliente = mapper.Map<CRMClientePostDTO, CRMCliente>(dto);

                if (cliente.Id != 0)
                {
                    context.Update(cliente);
                }
                else
                    await context.AddAsync(cliente);

                await context.SaveChangesAsync();

                return NoContent();
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
                var cliente = await context.CRMClientes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id);
                if (cliente is null) return NotFound();

                var clientedto = mapper.Map<CRMCliente, CRMClientePostDTO>(cliente);
                return Ok(clientedto);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("{Id:int}/detalle")]
        public async Task<ActionResult> GetDetalle([FromRoute] int Id)
        {
            try
            {
                var cliente = await context.CRMClientes.AsNoTracking().Include(x => x.Contacto).FirstOrDefaultAsync(x => x.Id == Id);
                if (cliente is null) return NotFound();

                var clientedto = mapper.Map<CRMCliente, CRMClienteDetalleDTO>(cliente);
                return Ok(clientedto);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("{Id:int}")]
        public async Task<ActionResult> Delete([FromRoute] int Id)
        {
            try
            {
                var cliente = await context.CRMClientes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id);
                if (cliente is null) return NotFound();

                cliente.Activo = false;

                context.Update(cliente);
                await context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("contactos")]
        public async Task<ActionResult> GetClienteContacto([FromQuery] CRMCliente cliente)
        {
            try
            {
                var user = await userManager.FindByNameAsync(HttpContext.User.Identity?.Name ?? string.Empty);
                if (user is null) return NotFound();

                var clientes = new List<CRMCliente>().AsQueryable();

                if (await userManager.IsInRoleAsync(user, "Admin"))
                {
                    var contactos = await context.CRMContactos.AsNoTracking().Where(x => x.Activo).Select(x => x.CuentaId).ToListAsync();
                    clientes = context.CRMClientes.AsNoTracking().Where(x => x.Activo && contactos.Contains(x.Id)).AsQueryable();
                }
                else if (await userManager.IsInRoleAsync(user, "LIDER_DE_EQUIPO"))
                {
                    var comercial = await context.CRMOriginadores.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == user.Id);
                    if (comercial is null) return NotFound();

                    var equipos = await context.CRMEquipos.AsNoTracking().Where(x => x.Activo && x.LiderId == comercial.Id).Select(x => x.Id).ToListAsync();
                    var relacion = await context.CRMEquipoVendedores.AsNoTracking().Where(x => equipos.Contains(x.EquipoId))
                        .GroupBy(x => x.VendedorId)
                        .Select(x => (int?)x.Key).ToListAsync();
                    var contactos = await context.CRMContactos.AsNoTracking().Where(x => x.Activo && relacion.Contains(x.VendedorId)).Select(x => x.CuentaId).ToListAsync();
                    clientes = context.CRMClientes.AsNoTracking().Where(x => contactos.Contains(x.Id));
                }
                else
                {
                    var vendedor = await context.CRMVendedores.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == user.Id);
                    if (vendedor is null) return NotFound();

                    var contactos = await context.CRMContactos.AsNoTracking().Where(x => x.Activo && x.VendedorId == vendedor.Id).Select(x => x.CuentaId).ToListAsync();
                    clientes = context.CRMClientes.AsNoTracking().Where(x => contactos.Contains(x.Id));
                }

                if (!string.IsNullOrEmpty(cliente.Nombre))
                    clientes = clientes.Where(x => x.Nombre.Equals(cliente.Nombre));

                return Ok(clientes);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("default/data")]
        public async Task<IActionResult> SetClientesGcom()
        {
            try
            {
                var clientes = await context.Cliente.Where(x => !string.IsNullOrEmpty(x.Den) && x.Den.Length > 1)
                    .GroupBy(x => x.Den)
                    .Select(x => new CRMCliente { Nombre = (string)x.Key! })
                    .ToListAsync();
                foreach (var cliente in clientes)
                {
                    if (!context.CRMClientes.Any(x => x.Nombre == cliente.Nombre))
                    {
                        await context.AddAsync(cliente);
                        await context.SaveChangesAsync();
                    }
                }
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
