using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedimentoController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public PedimentoController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public ActionResult Index([FromQuery] CierreFiltroDTO filtroDTO)
        {
            try
            {
                if (filtroDTO is null)
                    return BadRequest("valores no permitidos");

                List<Pedimento> pedimentos = context.Pedimentos.Where(x => x.Fecha_Actual >= filtroDTO.Fecha_Inicio && x.Fecha_Actual <= filtroDTO.Fecha_Fin)
                    .Include(x => x.Producto)
                    .OrderByDescending(x => x.Numero_Pedimento)
                    .ToList();

                return Ok(pedimentos);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("{Id:int}/detalle")]
        public ActionResult Obtener_Pedimento([FromRoute] int Id)
        {
            try
            {
                Pedimento? pedimento = context.Pedimentos.Find(Id);

                if (pedimento is null)
                    return BadRequest("No se encontro el pedimento");

                List<OrdenEmbarque> Ordenes_Cargadas = context.OrdenEmbarque.Where(x => x.Fchcar == pedimento.Fecha_Actual && x.Codprd == pedimento.ID_Producto && x.Codest == 20).Include(x => x.Orden).ToList();
                List<OrdenEmbarque> Ordenes_Programadas = context.OrdenEmbarque.Where(x => x.Fchcar == pedimento.Fecha_Actual && x.Codprd == pedimento.ID_Producto && x.Codest == 22).Include(x => x.Orden).ToList();
                List<OrdenEmbarque> Ordenes_En_Trayecto = context.OrdenEmbarque.Where(x => x.Fchcar == pedimento.Fecha_Actual && x.Codprd == pedimento.ID_Producto && x.Codest == 26).Include(x => x.Orden).ToList();

                pedimento.Ordens.AddRange(Ordenes_Programadas);
                pedimento.Ordens.AddRange(Ordenes_Cargadas);
                pedimento.Ordens.AddRange(Ordenes_En_Trayecto);

                return Ok(pedimento);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Crear_Pedimento([FromBody] Pedimento pedimento)
        {
            try
            {
                if (pedimento is null)
                    return BadRequest("Los valores no pueden estar vacios");

                if (pedimento.Id == 0)
                    context.Add(pedimento);
                else
                {
                    pedimento.Producto = null;
                    context.Update(pedimento);
                }

                await context.SaveChangesAsync();

                var Nuevo_Pedimento = context.Pedimentos.Where(x => x.Id == pedimento.Id).Include(x => x.Producto).FirstOrDefault();

                return Ok(Nuevo_Pedimento);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
