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

        [HttpPost("reporte/detalle/cierre")]
        public ActionResult Obtener_Reporte_Detalle_Cierre([FromBody] List<OrdenCierre> datos)
        {
            try
            {
                if (datos is null)
                    return BadRequest("No se admiten valores vacios");

                List<DetalleCierreDTO> detalleCierres = new List<DetalleCierreDTO>();

                foreach (var item in datos)
                {
                    detalleCierres.Add(new DetalleCierreDTO
                    {
                        OC = item.Folio,
                        FolioReferencia = item?.OrdenEmbarque?.Folio.ToString(),
                        Precio = item.Precio.ToString(),
                        Volumen = item?.OrdenEmbarque?.Orden is not null ? item?.OrdenEmbarque?.Orden?.Vol
                                    : item?.OrdenEmbarque?.Compartment == 1 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom?.ToString())
                                    : item?.OrdenEmbarque?.Compartment == 2 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom2?.ToString())
                                    : item?.OrdenEmbarque?.Compartment == 3 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom3?.ToString())
                                    : item?.OrdenEmbarque?.Compartment == 4 && item.OrdenEmbarque?.Tonel is not null ? double.Parse(item?.OrdenEmbarque?.Tonel?.Capcom4?.ToString())
                                    : item?.OrdenEmbarque?.Vol,
                        Observaciones = item?.Observaciones,
                        FchCierre = item?.FchCierre?.ToString("dd/MM/yyyy"),
                        Destino = item?.Destino?.Den,
                        Producto = item?.Producto?.Den,
                        BOL = item?.OrdenEmbarque?.Orden?.BatchId.ToString(),
                        Unidad = item?.OrdenEmbarque?.Tonel?.Veh,
                        Estatus = item?.OrdenEmbarque?.Orden != null ? item.OrdenEmbarque?.Orden?.Estado?.den : item?.OrdenEmbarque?.Estado?.den,
                        FchLlegada = item?.FechaLlegada
                    });
                }

                return Ok(detalleCierres);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("reporte/ordenes/pedimento")]
        public ActionResult Obtener_Reporte_Ordenes_Pedimento([FromBody] List<OrdenEmbarque> datos)
        {
            try
            {
                if (datos is null)
                    return BadRequest("No se aceptan valores vacios");

                List<Excel_Ordenes_Pedimento> Datos_Excel = new();

                foreach (var item in datos)
                {
                    Datos_Excel.Add(new Excel_Ordenes_Pedimento()
                    {
                        Referencia = item.FolioSyn ?? string.Empty,
                        BOL = item.Orden?.BatchId,
                        Precio = item.Pre ?? 0,
                        Costo = item.Costo,
                        Volumen_Cargado = item.Compartment == 1 && item.Tonel != null ? double.Parse(item!.Tonel!.Capcom!.ToString() ?? "0")
                                        : item.Compartment == 2 && item.Tonel != null ? double.Parse(item!.Tonel!.Capcom2!.ToString() ?? "0")
                                        : item.Compartment == 3 && item.Tonel != null ? double.Parse(item!.Tonel!.Capcom3!.ToString() ?? "0")
                                        : item.Compartment == 4 && item.Tonel != null ? double.Parse(item!.Tonel!.Capcom4!.ToString() ?? "0")
                                        : item.Vol ?? 0,
                        Estado = item.Orden != null ? item.Orden.Estado?.den ?? "" : item.Estado?.den ?? "",
                        Fecha_Carga = item?.Orden?.Fchcar?.ToString("d"),
                        Cliente = item?.OrdenCierre?.Cliente?.Den ?? string.Empty,
                        Destino = item?.Orden?.Destino?.Den ?? item?.Destino?.Den ?? string.Empty,
                        Producto = item?.Orden?.Producto?.Den ?? item?.Producto?.Den ?? string.Empty,
                        Fecha_Programa = item?.Fchcar?.ToString("d")
                    });
                }

                return Ok(Datos_Excel);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
