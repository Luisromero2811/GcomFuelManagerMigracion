using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

        public RedireccionController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public ActionResult Obtener_Redireccionamientos([FromQuery] CierreFiltroDTO filtroDTO)
        {
            try
            {
                List<Redireccionamiento> redireccionamientos = context.Redireccionamientos.Where(x => x.Fecha_Red >= filtroDTO.Fecha_Inicio && x.Fecha_Red <= filtroDTO.Fecha_Fin)
                    .Include(x => x.Grupo)
                    .Include(x => x.Cliente)
                    .Include(x => x.Destino)
                    .Include(x => x.Orden)
                    .IgnoreAutoIncludes()
                    .ToList();

                redireccionamientos.ForEach(x =>
                {

                });

                return Ok();
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

                context.Add(redireccionamiento);

                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
