using AutoMapper;
using FluentValidation;
using GComFuelManager.Server.Helpers;
using GComFuelManager.Shared.Extensiones;
using GComFuelManager.Shared.ModelDTOs;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class InventarioController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IValidator<InventarioPostDTO> validator;
        private readonly IMapper mapper;

        public InventarioController(ApplicationDbContext context, IValidator<InventarioPostDTO> validator, IMapper mapper)
        {
            this.context = context;
            this.validator = validator;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] InventarioDTO inventario)
        {
            try
            {
                var inventarios = context.Inventario
                    .AsNoTracking()
                    .Where(x => x.Activo && x.FechaRegistro.Date >= inventario.Fecha_Inicio.Date && x.FechaRegistro.Date <= inventario.Fecha_Fin.Date)
                    .Include(x => x.Producto)
                    .Include(x => x.Sitio)
                    .Include(x => x.Almacen)
                    .Include(x => x.Localidad)
                    .Include(x => x.UnidadMedida)
                    .Include(x => x.TipoMovimiento)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(inventario.Producto) && !string.IsNullOrWhiteSpace(inventario.Producto))
                    inventarios = inventarios.Where(x => !string.IsNullOrEmpty(x.Producto.Den) && x.Producto.Den.ToLower().Contains(inventario.Producto.ToLower()));

                if (!string.IsNullOrEmpty(inventario.Sitio) && !string.IsNullOrWhiteSpace(inventario.Sitio))
                    inventarios = inventarios.Where(x => x.Sitio.Valor.ToLower().Contains(inventario.Sitio.ToLower()));

                if (!string.IsNullOrEmpty(inventario.Almacen) && !string.IsNullOrWhiteSpace(inventario.Almacen))
                    inventarios = inventarios.Where(x => x.Almacen.Valor.ToLower().Contains(inventario.Almacen.ToLower()));

                if (!string.IsNullOrEmpty(inventario.Localidad) && !string.IsNullOrWhiteSpace(inventario.Localidad))
                    inventarios = inventarios.Where(x => x.Localidad.Valor.ToLower().Contains(inventario.Localidad.ToLower()));

                if (!string.IsNullOrEmpty(inventario.UnidadMedida) && !string.IsNullOrWhiteSpace(inventario.UnidadMedida))
                    inventarios = inventarios.Where(x => x.UnidadMedida.Valor.ToLower().Contains(inventario.UnidadMedida.ToLower()));

                if (!string.IsNullOrEmpty(inventario.TipoMovimiento) && !string.IsNullOrWhiteSpace(inventario.TipoMovimiento))
                    inventarios = inventarios.Where(x => x.TipoMovimiento.Valor.ToLower().Contains(inventario.TipoMovimiento.ToLower()));

                await HttpContext.InsertarParametrosPaginacion(inventarios, inventario.Registros_por_pagina, inventario.Pagina);
                inventario.Pagina = HttpContext.ObtenerPagina();

                var inventariodto = inventarios
                    .Select(x => mapper.Map<InventarioDTO>(x))
                    .Skip((inventario.Pagina - 1) * inventario.Registros_por_pagina)
                    .Take(inventario.Registros_por_pagina);

                return Ok(inventariodto);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] InventarioPostDTO post)
        {
            try
            {
                var result = validator.Validate(post);
                if (!result.IsValid) { return BadRequest(result.Errors.Select(x => x.ErrorMessage)); }

                var inventariodto = mapper.Map<Inventario>(post);
                if (!inventariodto.Id.IsZero())
                {
                    var inventariodb = await context.Inventario.FindAsync(inventariodto.Id);
                    if (inventariodb is null) { return NotFound(); }

                    var inventario = mapper.Map(inventariodto, inventariodb);
                    context.Update(inventario);
                }
                else
                    await context.AddAsync(inventariodto);

                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete]
        public async Task<ActionResult> Delete([FromRoute] int Id)
        {
            try
            {
                var inventario = await context.Inventario.FindAsync(Id);
                if (inventario is null) { return NotFound(); }

                inventario.Activo = false;

                context.Update(inventario);
                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        #region Catalogos

        [HttpGet("catalogo/sitio")]
        public ActionResult CatalogoSitio()
        {
            try
            {
                var catalogo = context.Accion.FirstOrDefault(x => x.Nombre.Equals("Catalogo_Inventario_Sitios"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para sitios"); }

                var catalogo_fijo = context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo.Cod)).ToList();

                return Ok(catalogo_fijo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/almacen")]
        public ActionResult CatalogoAlmacen()
        {
            try
            {
                var catalogo = context.Accion.FirstOrDefault(x => x.Nombre.Equals("Catalogo_Inventario_Almacenes"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para almacenes"); }

                var catalogo_fijo = context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo.Cod)).ToList();

                return Ok(catalogo_fijo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/localidad")]
        public ActionResult CatalogoLocalidad()
        {
            try
            {
                var catalogo = context.Accion.FirstOrDefault(x => x.Nombre.Equals("Catalogo_Inventario_Localidades"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para localidades"); }

                var catalogo_fijo = context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo.Cod)).ToList();

                return Ok(catalogo_fijo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/unidadmedida")]
        public ActionResult CatalogoUnidadMedida()
        {
            try
            {
                var catalogo = context.Accion.FirstOrDefault(x => x.Nombre.Equals("Catalogo_Inventario_Unidad_Medida"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para unidad de medida"); }

                var catalogo_fijo = context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo.Cod)).ToList();

                return Ok(catalogo_fijo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/tipomovimiento")]
        public ActionResult CatalogoTipoMovimiento()
        {
            try
            {
                var catalogo = context.Accion.FirstOrDefault(x => x.Nombre.Equals("Catalogo_Inventario_Tipo_Movimientos"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para sitios"); }

                var catalogo_fijo = context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo.Cod)).ToList();

                return Ok(catalogo_fijo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        #endregion
    }
}
