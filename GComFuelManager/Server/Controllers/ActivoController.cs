using GComFuelManager.Server.Helpers;
using GComFuelManager.Shared.Filtro;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, Administrador Sistema, Activos Fijos")]
    [ApiController]
    public class ActivoController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public ActivoController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult> Obtener_Activos([FromQuery] Activo_Fijo activo)
        {
            try
            {
                if (activo is null) { return BadRequest(); }

                var activos = context.Activo_Fijo
                    .Include(x => x.Conjunto)
                    .Include(x => x.Condicion)
                    .Include(x => x.Tipo)
                    .Include(x => x.Unidad)
                    .Include(x => x.Origen)
                    .Include(x => x.Etiqueta)
                    .Where(x => x.Activo).AsQueryable();

                if (!string.IsNullOrEmpty(activo.Nombre) || !string.IsNullOrWhiteSpace(activo.Nombre))
                    activos = activos.Where(x => x.Nombre.ToLower().Contains(activo.Nombre.ToLower()));

                if (!string.IsNullOrEmpty(activo.Nro_Activo) || !string.IsNullOrWhiteSpace(activo.Nro_Activo))
                    activos = activos.Where(x => x.Nro_Activo.ToLower().Contains(activo.Nro_Activo.ToLower()));

                if (!string.IsNullOrEmpty(activo.Nro_Etiqueta) || !string.IsNullOrWhiteSpace(activo.Nro_Etiqueta))
                    activos = activos.Where(x => x.Nro_Etiqueta.ToLower().Contains(activo.Nro_Etiqueta.ToLower()));

                if (activo.Conjunto_Activo != 0)
                    activos = activos.Where(x => x.Conjunto_Activo == activo.Conjunto_Activo);

                if (activo.Condicion_Activo != 0)
                    activos = activos.Where(x => x.Condicion_Activo == activo.Condicion_Activo);

                if (activo.Tipo_Activo != 0)
                    activos = activos.Where(x => x.Tipo_Activo == activo.Tipo_Activo);

                if (activo.Unidad_Medida != 0)
                    activos = activos.Where(x => x.Unidad_Medida == activo.Unidad_Medida);

                if (activo.Origen_Activo != 0)
                    activos = activos.Where(x => x.Origen_Activo == activo.Origen_Activo);

                if (activo.Etiquetado_Activo != 0)
                    activos = activos.Where(x => x.Etiquetado_Activo == activo.Etiquetado_Activo);

                await HttpContext.InsertarParametrosPaginacion(activos, activo.Registros_por_pagina, activo.Pagina);

                if (HttpContext.Response.Headers.TryGetValue("pagina", out Microsoft.Extensions.Primitives.StringValues value))
                {
                    if (!string.IsNullOrEmpty(value) && !string.IsNullOrWhiteSpace(value) && value != activo.Pagina)
                    {
                        activo.Pagina = int.Parse(value!);
                    }
                }

                activos = activos.Skip((activo.Pagina - 1) * activo.Registros_por_pagina).Take(activo.Registros_por_pagina);

                return Ok(activos);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Guardar_Activo([FromBody] Activo_Fijo activo_Fijo)
        {
            try
            {
                if (activo_Fijo is null) { return BadRequest(); }

                activo_Fijo.Conjunto = null!;
                activo_Fijo.Condicion = null!;
                activo_Fijo.Tipo = null!;
                activo_Fijo.Unidad = null!;
                activo_Fijo.Origen = null!;
                activo_Fijo.Etiqueta = null!;

                if (activo_Fijo.Id == 0)
                {

                    var consecutivo = context.Consecutivo.FirstOrDefault(x => x.Nombre.Equals("Activo_Fijo"));
                    if (consecutivo is null)
                    {
                        Consecutivo Nuevo_Consecutivo = new() { Numeracion = 1, Nombre = "Activo_Fijo" };
                        context.Add(Nuevo_Consecutivo);
                        await context.SaveChangesAsync();
                        consecutivo = Nuevo_Consecutivo;
                    }
                    else
                    {
                        consecutivo.Numeracion++;
                        context.Update(consecutivo);
                        await context.SaveChangesAsync();
                    }

                    var conjunto = context.Catalogo_Fijo.FirstOrDefault(x => x.Id == activo_Fijo.Conjunto_Activo);
                    if (conjunto is null) { return BadRequest(); }

                    activo_Fijo.Nro_Activo = $"{conjunto.Valor.Trim()}{consecutivo.Numeracion:00000}";
                    activo_Fijo.Numeracion = consecutivo.Numeracion;

                    context.Add(activo_Fijo);
                }
                else
                {
                    var conjunto = context.Catalogo_Fijo.FirstOrDefault(x => x.Id == activo_Fijo.Conjunto_Activo);
                    if (conjunto is null) { return BadRequest(); }

                    activo_Fijo.Nro_Activo = $"{conjunto.Valor.Trim()}{activo_Fijo.Numeracion:00000}";

                    context.Update(activo_Fijo);
                }
                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/conjunto")]
        public ActionResult Obtener_Catalogo_Conjunto()
        {
            try
            {
                var catalogo = context.Accion.FirstOrDefault(x => x.Nombre.Equals("Catalogo_Conjunto"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para conjuntos"); }

                var catalogo_fijo = context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo.Cod)).ToList();

                return Ok(catalogo_fijo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/condicion")]
        public ActionResult Obtener_Catalogo_Condicion()
        {
            try
            {
                var catalogo = context.Accion.FirstOrDefault(x => x.Nombre.Equals("Catalogo_Condicion"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para condiciones de activos"); }

                var catalogo_fijo = context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo.Cod)).ToList();

                return Ok(catalogo_fijo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/tipo")]
        public ActionResult Obtener_Catalogo_Tipo()
        {
            try
            {
                var catalogo = context.Accion.FirstOrDefault(x => x.Nombre.Equals("Catalogo_Tipo"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para tipo"); }

                var catalogo_fijo = context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo.Cod)).ToList();

                return Ok(catalogo_fijo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/unidad")]
        public ActionResult Obtener_Catalogo_Unidad()
        {
            try
            {
                var catalogo = context.Accion.FirstOrDefault(x => x.Nombre.Equals("Catalogo_Unidad_Medida"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para unidades de medida"); }

                var catalogo_fijo = context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo.Cod)).ToList();

                return Ok(catalogo_fijo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/origen")]
        public ActionResult Obtener_Catalogo_Origen()
        {
            try
            {
                var catalogo = context.Accion.FirstOrDefault(x => x.Nombre.Equals("Catalogo_Origen"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para origen"); }

                var catalogo_fijo = context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo.Cod)).ToList();

                return Ok(catalogo_fijo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/etiqueta")]
        public ActionResult Obtener_Catalogo_Etiqueta()
        {
            try
            {
                var catalogo = context.Accion.FirstOrDefault(x => x.Nombre.Equals("Catalogo_Etiqueta"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para etiqueta"); }

                var catalogo_fijo = context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo.Cod)).ToList();

                return Ok(catalogo_fijo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("{Id:int}")]
        public async Task<ActionResult> Eliminar_Activo([FromRoute] int Id)
        {
            try
            {
                var activo = context.Activo_Fijo.Find(Id);
                if (activo is null) { return NotFound(); }

                activo.Activo = false;

                context.Update(activo);
                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("formato")]

    }
}
