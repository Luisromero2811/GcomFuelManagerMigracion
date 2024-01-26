using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RedireccionController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUsuario> userManager;
        private readonly VerifyUserToken verifyUser;

        public RedireccionController(ApplicationDbContext context, UserManager<IdentityUsuario> userManager, VerifyUserToken verifyUser)
        {
            this.context = context;
            this.userManager = userManager;
            this.verifyUser = verifyUser;
        }

        [HttpGet]
        public ActionResult Obtener_Redireccionamientos([FromQuery] Folio_Activo_Vigente filtro_)
        {
            try
            {
                var redireccionamientos = context.Redireccionamientos.Where(x => x.Fecha_Red >= filtro_.Fecha_Inicio && x.Fecha_Red <= filtro_.Fecha_Fin)
                    .Include(x => x.Grupo)
                    .Include(x => x.Cliente)
                    .Include(x => x.Destino)
                    .Include(x => x.Orden)
                    .ThenInclude(x => x.Producto)
                    .Include(x => x.Orden)
                    .ThenInclude(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .IgnoreAutoIncludes()
                    .AsQueryable();

                if (!string.IsNullOrEmpty(filtro_.Cliente_Filtrado))
                    redireccionamientos = redireccionamientos.Where(x => x.Cliente != null && !string.IsNullOrEmpty(x.Cliente.Den) && x.Cliente.Den.ToLower().Contains(filtro_.Cliente_Filtrado.ToLower()));

                if (!string.IsNullOrEmpty(filtro_.Cliente_Original))
                    redireccionamientos = redireccionamientos.Where(x => x.Orden != null && x.Orden.Destino != null && x.Orden.Destino.Cliente != null
                    && !string.IsNullOrEmpty(x.Orden.Destino.Cliente.Den) && x.Orden.Destino.Cliente.Den.ToLower().Contains(filtro_.Cliente_Original.ToLower()));

                if (!string.IsNullOrEmpty(filtro_.Destino_Filtrado))
                    redireccionamientos = redireccionamientos.Where(x => x.Destino != null && !string.IsNullOrEmpty(x.Destino.Den) && x.Destino.Den.ToLower().Contains(filtro_.Destino_Filtrado.ToLower()));

                if (!string.IsNullOrEmpty(filtro_.Destino_Original))
                    redireccionamientos = redireccionamientos.Where(x => x.Orden != null && x.Orden.Destino != null && !string.IsNullOrEmpty(x.Orden.Destino.Den)
                    && x.Orden.Destino.Den.ToLower().Contains(filtro_.Destino_Original.ToLower()));

                if (!string.IsNullOrEmpty(filtro_.Producto_Filtrado))
                    redireccionamientos = redireccionamientos.Where(x => x.Orden != null && x.Orden.Producto != null && !string.IsNullOrEmpty(x.Orden.Producto.Den)
                    && x.Orden.Producto.Den.ToLower().Contains(filtro_.Producto_Filtrado.ToLower()));

                if (filtro_.BOL is not null)
                    redireccionamientos = redireccionamientos.Where(x => x.Orden != null && x.Orden.BatchId != null && x.Orden.BatchId.ToString().Contains(filtro_.BOL.ToString()));

                return Ok(redireccionamientos);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Guardar_Redireccionamientos(Redireccionamiento redireccionamiento)
        {
            try
            {
                if (redireccionamiento is null)
                    return BadRequest();

                var id = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                var orden_synthesis = context.Orden.Find(redireccionamiento.Id_Orden);

                if(orden_synthesis is not null)
                {
                    var orden_enviada = context.OrdenEmbarque.FirstOrDefault(x => x.FolioSyn == orden_synthesis.Ref);
                    if(orden_enviada is not null)
                    {
                        var pertenece_a_cierre = context.OrdenPedido.Any(x => x.CodPed == orden_enviada.Cod && x.Pedido_Original == 0 && string.IsNullOrEmpty(x.Folio_Cierre_Copia));
                        if (pertenece_a_cierre)
                        {
                            var orden_pedido = context.OrdenPedido.FirstOrDefault(x => x.CodPed == orden_enviada.Cod);

                            if (orden_pedido is not null && !string.IsNullOrEmpty(orden_pedido.Folio))
                            {

                            }
                        }
                    }
                }

                if (redireccionamiento.Id != 0)
                {
                    context.Update(redireccionamiento);
                    await context.SaveChangesAsync(id,36);

                }
                else
                {
                    context.Add(redireccionamiento);
                    await context.SaveChangesAsync(id,35);
                }

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("checar")]
        public ActionResult Checar_Redireccion([FromQuery] Folio_Activo_Vigente redireccionamiento)
        {
            try
            {
                if (redireccionamiento is null)
                    return NotFound();

                bool redireccion = context.Redireccionamientos.Any(x => x.Id_Orden == redireccionamiento.ID_Orden);

                return Ok(redireccion);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
