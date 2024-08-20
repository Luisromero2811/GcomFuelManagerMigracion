using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GComFuelManager.Shared.Modelos;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActividadesController : Controller
    {
        private readonly ApplicationDbContext context;

        public ActividadesController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpPost("create")]
        public async Task<ActionResult> Create_Actividad([FromBody] CRMActividades cRMActividades)
        {
            try
            {
                //Si la actividad viene nula del front regresamos un badrequest
                if (cRMActividades is null)
                {
                    return BadRequest();
                }
                //Propiedades de navegación por defecto se dejan en nulo


                //Si el ID de la actividad viene en 0 se agrega un nuevo registro de lo contrario se edita el registro
                if (cRMActividades.Id == 0)
                {
                    cRMActividades.prioridades = null!;
                    cRMActividades.vendedor = null!;
                    cRMActividades.contacto = null!;
                    cRMActividades.Estados = null!;
                    cRMActividades.asuntos = null!;

                    context.Add(cRMActividades);
                }
                else
                {
                    cRMActividades.Fecha_Mod = DateTime.Now;

                    context.Update(cRMActividades);
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

        [HttpGet("listadoActividades")]
        public async Task<ActionResult> GetListActivities()
        {
            try
            {
                var activos = context.CRMActividades
                    .Include(x => x.vendedor)
                    .Include(x => x.contacto)
                    .Include(x => x.asuntos)
                    .Include(x => x.Estados)
                    .ToList();

                return Ok(activos);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}

