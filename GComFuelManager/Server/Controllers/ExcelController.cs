using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExcelController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly VerifyUserId verifyUser;

        public ExcelController(ApplicationDbContext context, UserManager<IdentityUsuario> userManager, VerifyUserId verifyUser)
        {
            this.context = context;
            this.verifyUser = verifyUser;
        }

        [HttpPost("reporte/volumen/cierre")]
        public ActionResult Obtener_Reporte_Volumen_Cierres([FromBody] Folio_Activo_Vigente datos)
        {
            try
            {
                if (datos is null)
                    return BadRequest("No se admiten parametros vacios");

                List<FolioCierreDTO> DTO = new List<FolioCierreDTO>();
                if (datos.OrdenCierres is not null)
                {
                    foreach (var item in datos.OrdenCierres)
                    {
                        DTO.Add(new FolioCierreDTO()
                        {
                            Grupo = item.Grupo,
                            cliente = item.Cliente,
                            destino = item.Destino,
                            Producto = item.Producto,
                            FchCierre = item.FchCierre,
                            FchCierre_Vencimiento = item.FchVencimiento,
                            Volumen = item.Volumen,
                            Volumen_Disponible = item.Volumen_Disponible,
                            Folio = item.Folio,
                            Observaciones = item.Observaciones,
                            Precio = item.Precio 
                        });
                    }
                }

                return Ok(DTO);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
