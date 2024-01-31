using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using static GComFuelManager.Server.Controllers.Precios.PrecioController;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendedorController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUsuario> userManager;
        private readonly VerifyUserId verifyUser;

        public VendedorController(ApplicationDbContext context, UserManager<IdentityUsuario> userManager, VerifyUserId verifyUser)
        {
            this.context = context;
            this.userManager = userManager;
            this.verifyUser = verifyUser;
        }

        [HttpGet("anos/reporte")]
        public ActionResult Obtener_Años_Disponbles()
        {
            try
            {
                var Fechas_Diponibles = context.OrdenCierre.Where(x => x.FchCierre != null).Select(x => new DateTime(x.FchCierre.Value.Year, 1, 1)).DistinctBy(x => x.Year).ToList();
                return Ok(Fechas_Diponibles);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        public ActionResult Obtener_Vendedores([FromQuery] Vendedor vendedor)
        {
            try
            {
                var vendedores = context.Vendedores.OrderBy(x => x.Nombre).AsQueryable();

                if (!string.IsNullOrEmpty(vendedor.Nombre))
                    vendedores = vendedores.Where(x => x.Nombre.ToLower().Contains(vendedor.Nombre.ToLower())).OrderBy(x => x.Nombre);

                return Ok(vendedores);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("filtrar")]
        public ActionResult Obtener_Vendedores_Filtrados([FromQuery] Vendedor vendedor)
        {
            try
            {
                var vendedores = context.Vendedores.Where(x => x.Activo).OrderBy(x => x.Nombre).AsQueryable();

                if (!string.IsNullOrEmpty(vendedor.Nombre))
                    vendedores = vendedores.Where(x => x.Nombre.ToLower().Contains(vendedor.Nombre.ToLower())).OrderBy(x => x.Nombre);

                return Ok(vendedores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Guardar_Vendedores([FromBody] Vendedor vendedor)
        {
            try
            {
                if (vendedor is null)
                    return NotFound();

                var id = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                if (vendedor.Id != 0)
                {
                    context.Update(vendedor);
                    await context.SaveChangesAsync(id, 38);
                }
                else
                {
                    context.Add(vendedor);
                    await context.SaveChangesAsync(id, 37);
                }
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("relacionar/cliente")]
        public async Task<ActionResult> Guardar_Relacion_Cliente_Vendedor([FromBody] List<Cliente> clientes, [FromQuery] Vendedor vendedor)
        {
            try
            {
                if (clientes is null)
                    return NotFound();

                var id = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                foreach (var cliente in clientes)
                {
                    var cliente_buscado = context.Cliente.FirstOrDefault(x => x.Cod == cliente.Cod);
                    if (cliente_buscado is not null)
                    {
                        cliente_buscado.Id_Vendedor = vendedor.Id;
                        context.Update(cliente_buscado);
                        await context.SaveChangesAsync(id, 38);
                    }
                }
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpGet("reporte")]
        public ActionResult Obtener_Venta_De_Meses_Por_Vendedor([FromQuery] Vendedor vendedor)
        {
            try
            {
                var vendedores = context.Vendedores.Where(x => x.Activo).OrderBy(x => x.Nombre).AsQueryable();

                if (!string.IsNullOrEmpty(vendedor.Nombre))
                    vendedores = vendedores.Where(x => x.Nombre.ToLower().Contains(vendedor.Nombre.ToLower())).OrderBy(x => x.Nombre);

                List<Vendedor> Vendedores_Validos = vendedores.ToList();

                var meses = CultureInfo.CurrentCulture.Calendar.GetMonthsInYear(DateTime.Today.Year);

                foreach (var vendedor_valido in Vendedores_Validos)
                {
                    List<Cliente> clientes = context.Cliente.Where(x => x.Id_Vendedor == vendedor_valido.Id && x.Activo).ToList();
                    for (int mes = 1; mes <= meses; mes++)
                    {
                        Mes_Venta mes_Venta = new()
                        {
                            Nro_Mes = mes,
                            Nombre_Mes = new DateTime(DateTime.Today.Year, mes, 1).ToString("MMMM")
                        };

                        foreach (var cliente in clientes)
                        {
                            List<Orden> Ordenes = context.Orden.Where(x => x.Destino != null && x.Destino.Codcte == cliente.Cod
                            && x.Fchcar != null && x.Fchcar.Value.Month == mes && x.Fchcar.Value.Year == 2023 && x.Codest != 14)
                            .Include(x => x.Producto)
                            .Include(x => x.Destino)
                            .IgnoreAutoIncludes()
                            .ToList();

                            foreach (var orden in Ordenes)
                            {
                                Mes_Venta_Producto mes_Venta_Producto = new()
                                {
                                    Producto = orden.Producto?.Den ?? string.Empty,
                                    Litros_Vendidos = orden.Vol ?? 0,
                                    Venta = (Obtener_Precio_Por_Bol(orden.BatchId).Precio * orden.Vol) ?? 0
                                };

                                mes_Venta.Litros_Vendidos += mes_Venta_Producto.Litros_Vendidos;
                                mes_Venta.Venta += mes_Venta_Producto.Venta;

                                mes_Venta.Mes_Venta_Productos.Add(mes_Venta_Producto);
                            }
                        }

                        vendedor_valido.Venta_Por_Meses.Add(mes_Venta);
                    }
                }

                return Ok(Vendedores_Validos);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        public PrecioBolDTO Obtener_Precio_Por_Bol(int? BOL)
        {
            try
            {
                PrecioBol precios = new();

                var ordenes = context.Orden.Where(x => x.BatchId == BOL)
                    .Include(x => x.Producto)
                    .Include(x => x.Destino)
                    .FirstOrDefault();

                if (ordenes is null)
                    return new PrecioBolDTO();

                PrecioBolDTO precio = new();

                OrdenEmbarque? orden = new OrdenEmbarque();
                orden = context.OrdenEmbarque.Where(x => x.FolioSyn == ordenes.Ref).Include(x => x.Producto).Include(x => x.Destino).ThenInclude(x => x.Cliente).Include(x => x.OrdenCierre).FirstOrDefault();

                precio.Fecha_De_Carga = ordenes.Fchcar;

                precio.Referencia = ordenes.Ref;

                if (orden is not null)
                {
                    if (orden.Producto is not null)
                        precio.Producto_Original = orden.Producto.Den;

                    if (orden.Destino is not null)
                    {
                        precio.Destino_Original = orden.Destino.Den;
                        if (orden.Destino.Cliente is not null)
                            if (!string.IsNullOrEmpty(orden.Destino.Cliente.Den))
                                precio.Cliente_Original = orden.Destino.Cliente.Den;

                    }
                }

                precio.BOL = ordenes.BatchId;
                precio.Volumen_Cargado = ordenes.Vol;

                var precioVig = context.Precio.Where(x => ordenes != null && x.codDes == ordenes.Coddes && x.codPrd == ordenes.Codprd)
                    .OrderByDescending(x => x.FchDia)
                    .FirstOrDefault();

                var precioPro = context.PrecioProgramado.Where(x => ordenes != null && x.codDes == ordenes.Coddes && x.codPrd == ordenes.Codprd)
                    .OrderByDescending(x => x.FchDia)
                    .FirstOrDefault();

                var precioHis = context.PreciosHistorico.Where(x => ordenes != null && x.codDes == ordenes.Coddes && x.codPrd == ordenes.Codprd
                    && ordenes.Fchcar != null && x.FchDia <= ordenes.Fchcar.Value.Date)
                    .OrderByDescending(x => x.FchDia)
                    .FirstOrDefault();

                if (precioHis is not null)
                {
                    precio.Precio = precioHis.pre;
                    precio.Fecha_De_Precio = precioHis.FchDia;
                    precio.Precio_Encontrado = true;
                    precio.Precio_Encontrado_En = "Historial";
                    precio.Moneda = precioHis?.Moneda?.Nombre ?? "MXN";
                    precio.Tipo_De_Cambio = precioHis?.Equibalencia ?? 1;
                    precio.Tipo_Moneda = precioHis?.ID_Moneda;
                }

                if (ordenes != null && precioVig is not null && ordenes.Fchcar is not null && ordenes.Fchcar.Value.Date == DateTime.Today)
                {
                    precio.Precio = precioVig.Pre;
                    precio.Fecha_De_Precio = precioVig.FchDia;
                    precio.Precio_Encontrado = true;
                    precio.Precio_Encontrado_En = "Vigente";
                    precio.Moneda = precioVig?.Moneda?.Nombre ?? "MXN";
                    precio.Tipo_De_Cambio = precioVig?.Equibalencia ?? 1;
                    precio.Tipo_Moneda = precioVig?.ID_Moneda;
                }

                if (ordenes != null && precioPro is not null && ordenes.Fchcar is not null && ordenes.Fchcar.Value.Date == DateTime.Today && context.PrecioProgramado.Any())
                {
                    precio.Precio = precioPro.Pre;
                    precio.Fecha_De_Precio = precioPro.FchDia;
                    precio.Precio_Encontrado = true;
                    precio.Precio_Encontrado_En = "Programado";
                    precio.Moneda = precioPro?.Moneda?.Nombre ?? "MXN";
                    precio.Tipo_De_Cambio = precioPro?.Equibalencia ?? 1;
                    precio.Tipo_Moneda = precioPro?.ID_Moneda;
                }

                if (orden != null && context.OrdenPedido.Any(x => x.CodPed == orden.Cod))
                {
                    var ordenepedido = context.OrdenPedido.Where(x => x.CodPed == orden.Cod && !string.IsNullOrEmpty(x.Folio)).FirstOrDefault();

                    if (ordenepedido is not null)
                    {
                        var cierre = context.OrdenCierre.Where(x => x.Folio == ordenepedido.Folio
                         && x.CodPrd == orden.Codprd).FirstOrDefault();

                        if (cierre is not null)
                        {
                            precio.Precio = cierre.Precio;
                            precio.Fecha_De_Precio = cierre.fchPrecio;
                            precio.Es_Cierre = true;
                            precio.Precio_Encontrado = true;
                            precio.Precio_Encontrado_En = "Cierre";
                            precio.Moneda = cierre?.Moneda?.Nombre ?? "MXN";
                            precio.Tipo_De_Cambio = cierre?.Equibalencia ?? 1;
                            precio.Tipo_Moneda = cierre?.ID_Moneda;
                        }
                    }
                }

                if (orden is not null && precioHis is null && precioPro is null && precioVig is null && !precio.Es_Cierre)
                {
                    precio.Precio = orden.Pre;

                    if (orden.OrdenCierre is not null)
                        precio.Fecha_De_Precio = orden.OrdenCierre.fchPrecio;

                    precio.Es_Precio_De_Creacion = true;
                    precio.Precio_Encontrado_En = "Creacion";
                }

                precio.Moneda = !string.IsNullOrEmpty(precio.Moneda) ? precio.Moneda : "MXN";

                return precio;
            }
            catch (Exception e)
            {
                return new PrecioBolDTO();
            }
        }
    }
}
