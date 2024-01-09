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
    public class VolumenController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly VerifyUserId verifyUser;

        public VolumenController(ApplicationDbContext context, UserManager<IdentityUsuario> userManager, VerifyUserId verifyUser)
        {
            this.context = context;
            this.verifyUser = verifyUser;
        }

        [HttpGet]
        public ActionResult Obtener_Volumen_De_Pedido([FromQuery] OrdenCierre ordenCierre)
        {
            try
            {
                if (ordenCierre is null)
                    return BadRequest("No se recibio ningun orden");

                List<ProductoVolumen> Producto_Volumen = new List<ProductoVolumen>();

                if (ordenCierre.Cod != 0)
                {
                    var cierre = context.OrdenCierre.IgnoreAutoIncludes().FirstOrDefault(x => x.Cod == ordenCierre.Cod && x.Folio == ordenCierre.Folio && x.Estatus == true);
                    if (cierre is not null)
                    {
                        var volumen = ObtenerVolumenDisponibleDeProducto(cierre);
                        if (volumen is not null)
                            Producto_Volumen.Add(volumen);
                    }
                }
                else
                {
                    var cierres = context.OrdenCierre.Where(x => x.Folio == ordenCierre.Folio && x.Estatus == true).IgnoreAutoIncludes().ToList();
                    if (cierres is not null)
                    {
                        foreach (var item in cierres)
                        {
                            var volumen = ObtenerVolumenDisponibleDeProducto(item);
                            if (volumen is not null)
                                Producto_Volumen.Add(volumen);
                        }
                    }
                }
                return Ok(Producto_Volumen);
            }
            catch (ArgumentNullException)
            {
                return BadRequest("No se puede enviar la orden vacia");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("reporte")]
        public ActionResult Obtener_Volumen_De_Pedido_De_Reporte([FromQuery] Folio_Activo_Vigente parametros)
        {
            try
            {
                if (parametros is null)
                    return BadRequest("No se recibio ningun orden");

                List<ProductoVolumen> Producto_Volumen = new List<ProductoVolumen>();

                List<OrdenCierre> ordenCierres = new List<OrdenCierre>();

                Folio_Activo_Vigente folio = new Folio_Activo_Vigente();
                //&& x.Activa == true
                var cierres = context.OrdenCierre.Where(x => x.CodPed == 0)
                    .Include(x => x.Grupo)
                    .Include(x => x.Cliente)
                    .Include(x => x.Destino)
                    .Include(x => x.Producto)
                    .IgnoreAutoIncludes()
                    .OrderByDescending(x => x.FchCierre)
                    .AsQueryable();

                if (parametros.ID_Grupo is not null && parametros.ID_Grupo != 0)
                    cierres = cierres.Where(x => x.CodGru == parametros.ID_Grupo);

                if (parametros.ID_Cliente is not null && parametros.ID_Cliente != 0)
                    cierres = cierres.Where(x => !string.IsNullOrEmpty(x.Folio) && (x.CodCte == parametros.ID_Cliente || x.Folio.StartsWith("G")));

                if (parametros.ID_Destino is not null && parametros.ID_Destino != 0)
                    cierres = cierres.Where(x => !string.IsNullOrEmpty(x.Folio) && (x.CodDes == parametros.ID_Destino || x.Folio.StartsWith("G")));

                if (parametros.ID_Producto is not null && parametros.ID_Producto != 0)
                    cierres = cierres.Where(x => x.CodPrd == parametros.ID_Producto);

                //if (parametros.ID_FchIni != null && parametros.ID_FchFin != null)
                //    cierres = cierres.Where(x => x.FchCierre >= parametros.ID_FchIni && x.FchCierre <= parametros.ID_FchFin);

                if (cierres is not null)
                {
                    ordenCierres = cierres.ToList();

                    folio.OrdenCierres = ordenCierres;

                    foreach (var item in ordenCierres)
                    {
                        var volumen = ObtenerVolumenDisponibleDeProducto(item);
                        if (volumen is not null)
                        {
                            ordenCierres.First(x => x.Cod == item.Cod).Volumen_Disponible = volumen.Disponible;

                            if (volumen.Disponible >= volumen.PromedioCarga)
                            {
                                if (Producto_Volumen.Any(x => x.ID_Producto == item.CodPrd))
                                {
                                    Producto_Volumen.First(x => x.ID_Producto == item.CodPrd).Total += volumen.Total;
                                    Producto_Volumen.First(x => x.ID_Producto == item.CodPrd).Consumido += volumen.Consumido;
                                    Producto_Volumen.First(x => x.ID_Producto == item.CodPrd).Reservado += volumen.Reservado;
                                    Producto_Volumen.First(x => x.ID_Producto == item.CodPrd).Solicitud += volumen.Solicitud;
                                    Producto_Volumen.First(x => x.ID_Producto == item.CodPrd).Congelado += volumen.Congelado;
                                    Producto_Volumen.First(x => x.ID_Producto == item.CodPrd).Programado += volumen.Programado;
                                    Producto_Volumen.First(x => x.ID_Producto == item.CodPrd).Disponible += volumen.Disponible;
                                }
                                else
                                    Producto_Volumen.Add(volumen);
                            }

                            folio.ProductoVolumenes = Producto_Volumen;
                        }
                    }
                }

                return Ok(folio);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("reportefecha")]
        public ActionResult Obtener_Volumen_De_Pedido_De_ReporteFecha([FromQuery] Folio_Activo_Vigente parametros)
        {
            try
            {
                if (parametros is null)
                    return BadRequest("No se recibio ningun orden");

                List<ProductoVolumen> Producto_Volumen = new List<ProductoVolumen>();

                List<OrdenCierre> ordenCierres = new List<OrdenCierre>();

                Folio_Activo_Vigente folio = new Folio_Activo_Vigente();
                //&& x.Activa == true
                var cierres = context.OrdenCierre.Where(x => x.CodPed == 0)
                    .Include(x => x.Grupo)
                    .Include(x => x.Cliente)
                    .Include(x => x.Destino)
                    .Include(x => x.Producto)
                    .IgnoreAutoIncludes()
                    .OrderByDescending(x => x.FchCierre)
                    .AsQueryable();

                if (parametros.ID_FchIni != null && parametros.ID_FchFin != null)
                    cierres = cierres.Where(x => x.FchCierre >= parametros.ID_FchIni && x.FchCierre <= parametros.ID_FchFin);

                if (cierres is not null)
                {
                    ordenCierres = cierres.ToList();

                    folio.OrdenCierres = ordenCierres;

                    foreach (var item in ordenCierres)
                    {
                        var volumen = ObtenerVolumenDisponibleDeProducto(item);
                        if (volumen is not null)
                        {
                            ordenCierres.First(x => x.Cod == item.Cod).Volumen_Disponible = volumen.Disponible;

                            if (volumen.Disponible >= volumen.PromedioCarga)
                            {
                                if (Producto_Volumen.Any(x => x.ID_Producto == item.CodPrd))
                                {
                                    Producto_Volumen.First(x => x.ID_Producto == item.CodPrd).Total += volumen.Total;
                                    Producto_Volumen.First(x => x.ID_Producto == item.CodPrd).Consumido += volumen.Consumido;
                                    Producto_Volumen.First(x => x.ID_Producto == item.CodPrd).Reservado += volumen.Reservado;
                                    Producto_Volumen.First(x => x.ID_Producto == item.CodPrd).Solicitud += volumen.Solicitud;
                                    Producto_Volumen.First(x => x.ID_Producto == item.CodPrd).Congelado += volumen.Congelado;
                                    Producto_Volumen.First(x => x.ID_Producto == item.CodPrd).Programado += volumen.Programado;
                                    Producto_Volumen.First(x => x.ID_Producto == item.CodPrd).Disponible += volumen.Disponible;
                                }
                                else
                                    Producto_Volumen.Add(volumen);
                            }

                            folio.ProductoVolumenes = Producto_Volumen;
                        }
                    }
                }

                return Ok(folio);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        private ProductoVolumen ObtenerVolumenDisponibleDeProducto(OrdenCierre ordenCierre)
        {
            var VolumenDisponible = ordenCierre.Volumen;

            var listConsumido = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(ordenCierre.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Tonel != null && x.OrdenEmbarque.Codprd == ordenCierre.CodPrd
                && x.OrdenEmbarque.Codest == 22
                && x.OrdenEmbarque.Folio != null
                && x.OrdenEmbarque.Bolguidid != null)
                .Include(x => x.OrdenEmbarque)
                .ThenInclude(x => x.Tonel).ToList();

            var VolumenCongelado = listConsumido.Sum(item => item.OrdenEmbarque!.Compartment == 1 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom!.ToString())
                            : item.OrdenEmbarque!.Compartment == 2 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom2!.ToString())
                            : item.OrdenEmbarque!.Compartment == 3 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom3!.ToString())
                            : item.OrdenEmbarque!.Compartment == 4 && item.OrdenEmbarque.Tonel != null ? double.Parse(item!.OrdenEmbarque!.Tonel!.Capcom4!.ToString())
                            : item.OrdenEmbarque!.Vol);

            var countCongelado = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(ordenCierre.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Codprd == ordenCierre.CodPrd
            && x.OrdenEmbarque.Codest == 22
            && x.OrdenEmbarque.Folio != null)
                .Include(x => x.OrdenEmbarque)
                .Count();

            var VolumenConsumido = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(ordenCierre.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Orden != null && x.OrdenEmbarque.Folio != null
                && x.OrdenEmbarque.Orden.Codest != 14
                && x.OrdenEmbarque.Codest != 14
            && x.OrdenEmbarque.Codprd == ordenCierre.CodPrd
            && x.OrdenEmbarque.Orden.BatchId != null)
                .Include(x => x.OrdenEmbarque)
                .ThenInclude(x => x.Orden)
                .Sum(x => x.OrdenEmbarque!.Orden!.Vol);

            var countConsumido = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(ordenCierre.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Orden != null && x.OrdenEmbarque.Folio != null
                && x.OrdenEmbarque.Orden.Codest != 14
                && x.OrdenEmbarque.Codest != 14
                && x.OrdenEmbarque.Codprd == ordenCierre.CodPrd
                && x.OrdenEmbarque.Orden.BatchId != null)
                .Include(x => x.OrdenEmbarque)
                .ThenInclude(x => x.Orden)
                .Count();

            var VolumenProgramado = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(ordenCierre.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Folio == null
                && x.OrdenEmbarque.Codprd == ordenCierre.CodPrd
                && x.OrdenEmbarque.Bolguidid == null && x.OrdenEmbarque.Codest == 3
                && x.OrdenEmbarque.FchOrd != null)
                .Include(x => x.OrdenEmbarque)
                    .Sum(x => x.OrdenEmbarque!.Vol);

            var CountVolumenProgramado = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(ordenCierre.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Folio == null
                && x.OrdenEmbarque.Codprd == ordenCierre.CodPrd
                && x.OrdenEmbarque.Bolguidid == null && x.OrdenEmbarque.Codest == 3
                && x.OrdenEmbarque.FchOrd != null)
                .Include(x => x.OrdenEmbarque).Count();

            var VolumenSolicitado = context.OrdenPedido.Where(x => !string.IsNullOrEmpty(x.Folio) && x.Folio.Equals(ordenCierre.Folio) && x.OrdenEmbarque != null && x.OrdenEmbarque.Folio == null
            && x.OrdenEmbarque.Codprd == ordenCierre.CodPrd
            && x.OrdenEmbarque.Bolguidid == null && x.OrdenEmbarque.Codest == 9
            && x.OrdenEmbarque.FchOrd == null)
                .Include(x => x.OrdenEmbarque)
                .Sum(x => x.OrdenEmbarque!.Vol);

            var Volumen_Resevado = VolumenSolicitado + VolumenProgramado + VolumenCongelado + VolumenConsumido;
            var VolumenTotalDisponible = VolumenDisponible - (Volumen_Resevado);
            double? PromedioCargas = 0;
            var sumVolumen = VolumenConsumido + VolumenCongelado + VolumenProgramado;
            var sumCount = countCongelado + countConsumido + CountVolumenProgramado;

            if (sumVolumen != 0 && sumCount != 0)
                PromedioCargas = sumVolumen / sumCount;

            ProductoVolumen productoVolumen = new ProductoVolumen();

            productoVolumen.Nombre = context.Producto.FirstOrDefault(x => x.Cod == ordenCierre.CodPrd)?.Den ?? string.Empty;
            productoVolumen.Reservado = Volumen_Resevado;
            productoVolumen.Disponible = VolumenTotalDisponible;
            productoVolumen.Congelado = VolumenCongelado;
            productoVolumen.Consumido = VolumenConsumido;
            productoVolumen.Total = VolumenDisponible;
            productoVolumen.PromedioCarga = PromedioCargas;
            productoVolumen.Solicitud = VolumenSolicitado;
            productoVolumen.Programado = VolumenProgramado;
            productoVolumen.ID_Producto = ordenCierre.CodPrd;

            return productoVolumen;
        }
    }
}