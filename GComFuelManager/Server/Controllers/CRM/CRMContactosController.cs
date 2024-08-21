﻿using AutoMapper;
using FluentValidation;
using GComFuelManager.Server.Helpers;
using GComFuelManager.Shared.DTOs.CRM;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;

namespace GComFuelManager.Server.Controllers.CRM
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, Administrador Sistema, CRM, CRM_Lider")]
    public class CRMContactosController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IValidator<CRMContactoPostDTO> validator;
        private readonly IMapper mapper;

        public CRMContactosController(ApplicationDbContext context, IValidator<CRMContactoPostDTO> validator, IMapper mapper)
        {
            this.context = context;
            this.validator = validator;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] CRMContactoDTO contacto)
        {
            try
            {
                var contactos = context.CRMContactos.AsNoTracking().Where(x => x.Activo)
                    .Include(x => x.Estatus)
                    .Include(x => x.Origen)
                    .Include(x => x.Cliente)
                    .Include(x => x.Vendedor)
                    .AsQueryable();

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

                await HttpContext.InsertarParametrosPaginacion(contactos, contacto.Registros_por_pagina, contacto.Pagina);

                if (HttpContext.Response.Headers.TryGetValue("pagina", out StringValues value))
                    if (!string.IsNullOrEmpty(value) || !string.IsNullOrWhiteSpace(value) && value != contacto.Pagina)
                        contacto.Pagina = int.Parse(value!);

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
        public async Task<ActionResult> ObtenerCatalogoStatus([FromRoute] int Id)
        {
            try
            {
                var contacto = await context.CRMContactos.AsNoTracking().Where(x => x.Id == Id).Select(x => mapper.Map<CRMContactoPostDTO>(x)).FirstOrDefaultAsync();
                return Ok(contacto);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CRMContactoPostDTO contactodto)
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
                var catalogo = await context.Accion.FirstOrDefaultAsync(x => x.Nombre.Equals("Catalogo_Contacto_Status"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para estatus"); }

                var catalogo_fijo = await context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo.Cod)).ToListAsync();

                return Ok(catalogo_fijo);
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
                var catalogo = await context.Accion.FirstOrDefaultAsync(x => x.Nombre.Equals("Catalogo_Contacto_Origen"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para origenes"); }

                var catalogo_fijo = await context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo.Cod)).ToListAsync();

                return Ok(catalogo_fijo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}