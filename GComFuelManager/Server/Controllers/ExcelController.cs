using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
                            Precio = item.Precio,
                            Tipo_Venta = item.TipoPago,
                            Activa = item.Activa,
                            Estatus = item.Estatus
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
                        Volumen_Cargado = item.Obtener_Volumen_De_Orden(),
                        Estado = item.Obtener_Estado_De_Orden,
                        Fecha_Carga = item?.Orden?.Fchcar?.ToString("d"),
                        Cliente = item?.Obtener_Cliente_De_Orden,
                        Destino = item?.Obtener_Destino_De_Orden,
                        Producto = item?.Obtener_Producto_De_Orden,
                        Fecha_Programa = item?.Fchcar?.ToString("d"),
                        Utilidad = item?.Obtener_Utilidad_Coste(),
                        Utilidad_Sobre_Volumen = item?.Obtener_Utilidad_Sobre_Volumen()
                    });
                }

                return Ok(Datos_Excel);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("reporte/ventas/desempeno")]
        public ActionResult Obtener_Reporte_Desempeño_De_Ventas([FromBody] List<Vendedor> vendedores)
        {
            try
            {
                List<Vendedor_Reporte_Desemeño> vendedor_Reporte_Desemeño = new();
                Reporte_Completo_Vendedor_Desempeño reporte_completo = new();

                foreach (var vendedor in vendedores)
                {
                    Vendedor_Reporte_Desemeño vendedor_litros = new() { Vendedor = vendedor.Nombre };
                    Vendedor_Reporte_Desemeño vendedor_venta = new() { Vendedor = vendedor.Nombre };

                    foreach (var mes in vendedor.Venta_Por_Meses)
                    {
                        switch (mes.Nro_Mes)
                        {
                            case 1: vendedor_litros.Enero = mes.Litros_Vendidos; break;
                            case 2: vendedor_litros.Febrero = mes.Litros_Vendidos; break;
                            case 3: vendedor_litros.Marzo = mes.Litros_Vendidos; break;
                            case 4: vendedor_litros.Abril = mes.Litros_Vendidos; break;
                            case 5: vendedor_litros.Mayo = mes.Litros_Vendidos; break;
                            case 6: vendedor_litros.Junio = mes.Litros_Vendidos; break;
                            case 7: vendedor_litros.Julio = mes.Litros_Vendidos; break;
                            case 8: vendedor_litros.Agosto = mes.Litros_Vendidos; break;
                            case 9: vendedor_litros.Septiembre = mes.Litros_Vendidos; break;
                            case 10: vendedor_litros.Octubre = mes.Litros_Vendidos; break;
                            case 11: vendedor_litros.Noviembre = mes.Litros_Vendidos; break;
                            case 12: vendedor_litros.Diciembre = mes.Litros_Vendidos; break;
                            default: break;
                        }

                        switch (mes.Nro_Mes)
                        {
                            case 1: vendedor_venta.Enero = mes.Venta; break;
                            case 2: vendedor_venta.Febrero = mes.Venta; break;
                            case 3: vendedor_venta.Marzo = mes.Venta; break;
                            case 4: vendedor_venta.Abril = mes.Venta; break;
                            case 5: vendedor_venta.Mayo = mes.Venta; break;
                            case 6: vendedor_venta.Junio = mes.Venta; break;
                            case 7: vendedor_venta.Julio = mes.Venta; break;
                            case 8: vendedor_venta.Agosto = mes.Venta; break;
                            case 9: vendedor_venta.Septiembre = mes.Venta; break;
                            case 10: vendedor_venta.Octubre = mes.Venta; break;
                            case 11: vendedor_venta.Noviembre = mes.Venta; break;
                            case 12: vendedor_venta.Diciembre = mes.Venta; break;
                            default: break;
                        }
                    }

                    reporte_completo.Litros.Add(vendedor_litros);
                    reporte_completo.Venta.Add(vendedor_venta);

                    vendedor_litros = new();
                    vendedor_venta = new();
                }

                return Ok(reporte_completo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
