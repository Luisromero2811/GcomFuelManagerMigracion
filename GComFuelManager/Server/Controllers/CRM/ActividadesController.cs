using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GComFuelManager.Shared.Modelos;
using Microsoft.EntityFrameworkCore;
using GComFuelManager.Shared.DTOs.CRM;
using FluentValidation;
using AutoMapper;
using GComFuelManager.Server.Helpers;
using Microsoft.Extensions.Primitives;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActividadesController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IValidator<CRMActividadPostDTO> validator;
        private readonly IMapper mapper;

        public ActividadesController(ApplicationDbContext context, IValidator<CRMActividadPostDTO> validator, IMapper mapper)
        {
            this.context = context;
            this.validator = validator;
            this.mapper = mapper;
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
                if (actividad.Id == 0)
                {
                   await context.AddAsync(actividad);
                }
                else
                {
                    actividad.Fecha_Mod = DateTime.Now;

                    context.Update(actividad);
                }
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
                var catalogo = context.Accion.FirstOrDefault(x => x.Nombre.Equals("Catalogo_Actividad_Asunto"));
                if (catalogo is null)
                {
                    return BadRequest("No existe el catalogo para asunto");
                }

                var catalogo_fijo = context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo.Cod)).ToList();
                return Ok(catalogo_fijo);

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
                var catalogo = context.Accion.FirstOrDefault(x => x.Nombre.Equals("Catalogo_Actividad_Prioridad"));
                if (catalogo is null)
                {
                    return BadRequest("No existe el catalogo para la prioridad");
                }

                var catalogo_fijo = context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo.Cod)).ToList();
                return Ok(catalogo_fijo);

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
                var catalogo = context.Accion.FirstOrDefault(x => x.Nombre.Equals("Catalogo_Actividad_Estatus"));
                if (catalogo is null)
                {
                    return BadRequest("No existe el catalogo para los estatus");
                }

                var catalogo_fijo = context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo.Cod)).ToList();
                return Ok(catalogo_fijo);

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("contactlist")]
        public ActionResult Obtener_Catalogo_Contacto()
        {
            try
            {
                var contactocrm = context.CRMContactos
                    //.Where(x => x.Activo == true)
                    .ToList();
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
                var activos = context.CRMActividades
                    .Where(x => x.Activo)
                    .Include(x => x.vendedor)
                    .Include(x => x.contacto)
                    .Include(x => x.asuntos)
                    .Include(x => x.Estados)
                    .Include(x => x.prioridades)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(activo.Asunto) && !string.IsNullOrWhiteSpace(activo.Asunto))
                    activos = activos.Where(x => x.asuntos != null && x.asuntos.Valor.ToLower().Contains(activo.Asunto.ToLower()));

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

        [HttpGet("{Id:int}")]
        public async Task<ActionResult> ObtenerCatalogoStatus([FromRoute] int Id)
        {
            try
            {
                var actividad = await context.CRMActividades.Where(x => x.Id == Id).Select(x => mapper.Map<CRMActividadPostDTO>(x)).FirstOrDefaultAsync();
                return Ok(actividad);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

    }
}

