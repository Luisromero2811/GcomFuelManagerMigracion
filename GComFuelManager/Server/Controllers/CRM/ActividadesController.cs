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
using iText.Commons.Utils;

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
                    .Where(x => x.Activo == true)
                    .Include(x => x.Vendedor)
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
                    .Where(x => x.Activo && x.Estados.Valor != "Completada")
                    .Include(x => x.vendedor)
                    .Include(x => x.contacto)
                    .Include(x => x.asuntos)
                    .Include(x => x.Estados)
                    .Include(x => x.prioridades)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(activo.Asunto) && !string.IsNullOrWhiteSpace(activo.Asunto))
                    activos = activos.Where(x => x.asuntos != null && x.asuntos.Valor.ToLower().Contains(activo.Asunto.ToLower()));

                if (!string.IsNullOrEmpty(activo.Prioridad) && !string.IsNullOrWhiteSpace(activo.Prioridad))
                    activos = activos.Where(x => x.prioridades != null && x.prioridades.Valor.ToLower().Contains(activo.Prioridad.ToLower()));

                if (!string.IsNullOrEmpty(activo.Estatus) && !string.IsNullOrWhiteSpace(activo.Estatus))
                    activos = activos.Where(x => x.Estados != null && x.Estados.Valor.ToLower().Contains(activo.Estatus.ToLower()));

                if (!string.IsNullOrEmpty(activo.Asignado) && !string.IsNullOrWhiteSpace(activo.Asignado))
                    activos = activos.Where(x => x.vendedor != null && x.vendedor.Nombre.ToLower().Contains(activo.Asignado.ToLower()));

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
                //Consulta a la entidad CRMActividades
                var actividades = context.CRMActividades
                    .Where(x => x.Fecha_Creacion >= actividadDTO.Fecha_Creacion && x.Fecha_Creacion <= actividadDTO.Fecha_Ven && x.Estados.Valor.Equals("Completada"))
                    .Include(x => x.asuntos)
                    .Include(x => x.Estados)
                    .Include(x => x.contacto)
                    .Include(x => x.prioridades)
                .AsQueryable();

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

