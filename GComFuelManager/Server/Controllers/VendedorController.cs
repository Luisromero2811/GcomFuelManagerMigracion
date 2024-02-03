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
                var vendedores = context.Vendedores.Include(x => x.Originadores).IgnoreAutoIncludes().OrderBy(x => x.Nombre).AsQueryable();

                if (!string.IsNullOrEmpty(vendedor.Nombre))
                    vendedores = vendedores.Where(x => x.Nombre.ToLower().Contains(vendedor.Nombre.ToLower())).OrderBy(x => x.Nombre);

                if (!string.IsNullOrEmpty(vendedor.Nombre_Originador) && vendedor.Originadores is not null)
                    vendedores = vendedores.Where(x => x.Originadores.Any(x => x.Nombre.ToLower().Contains(vendedor.Nombre_Originador.ToLower())));

                return Ok(vendedores);
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
                var vendedores = context.Vendedores.Where(x => x.Activo).OrderBy(x => x.Nombre).IgnoreAutoIncludes().AsQueryable();

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
                    //vendedor.Vendedor_Originador = null!;
                    vendedor.Originadores = null!;

                    context.Update(vendedor);
                    await context.SaveChangesAsync(id, 38);
                }
                else
                {
                    context.Add(vendedor);
                    await context.SaveChangesAsync(id, 37);

                    if (vendedor.Id_Originador != 0)
                    {
                        Vendedor_Originador vendedor_Originador = new()
                        {
                            VendedorId = vendedor.Id,
                            OriginadorId = vendedor.Id_Originador
                        };

                        context.Add(vendedor_Originador);
                        await context.SaveChangesAsync(id, 41);
                    }

                    if (!context.Metas_Vendedor.Any(x => x.VendedorId == vendedor.Id && x.Mes.Year == DateTime.Today.Year))
                    {
                        for (int i = 1; i <= 12; i++)
                        {
                            Metas_Vendedor metas_Vendedor = new()
                            {
                                VendedorId = vendedor.Id,
                                Mes = new DateTime(DateTime.Today.Year, i, 1)
                            };

                            if (!context.Metas_Vendedor.Any(x => x.Mes.Month == metas_Vendedor.Mes.Month && x.VendedorId == vendedor.Id && x.Mes.Year == metas_Vendedor.Mes.Year))
                            {
                                context.Add(metas_Vendedor);
                                await context.SaveChangesAsync();
                            }
                        }
                    }
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

        [HttpPost("borrar/relacion/cliente")]
        public async Task<ActionResult> Guardar_Relacion_Vendeor_Originador([FromQuery] Cliente cliente)
        {
            try
            {
                if (cliente is null)
                    return NotFound();

                var id = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                var cliente_encontrado = context.Cliente.Find(cliente.Cod);

                if (cliente_encontrado is not null)
                {
                    if (cliente_encontrado.Id_Vendedor == cliente.Id_Vendedor)
                    {
                        cliente_encontrado.Id_Vendedor = null!;
                        context.Update(cliente_encontrado);
                        await context.SaveChangesAsync(id, 42);
                        return Ok();
                    }
                }

                return NotFound();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("relacionar/originador")]
        public async Task<ActionResult> Guardar_Relacion_Vendeor_Originador([FromQuery] Vendedor_Originador vendedor_Originador)
        {
            try
            {
                if (vendedor_Originador is null)
                    return NotFound();

                var id = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                if (vendedor_Originador.Borrar)
                    context.Remove(vendedor_Originador);
                else
                    context.Add(vendedor_Originador);


                await context.SaveChangesAsync(id, 41);

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
                var vendedores = context.Vendedores.Where(x => x.Activo).Include(x => x.Clientes).OrderBy(x => x.Nombre).AsQueryable();

                if (!string.IsNullOrEmpty(vendedor.Nombre))
                    vendedores = vendedores.Where(x => x.Nombre.ToLower().Contains(vendedor.Nombre.ToLower())).OrderBy(x => x.Nombre);

                List<Vendedor> Vendedores_Validos = vendedores.ToList();

                var meses = CultureInfo.CurrentCulture.Calendar.GetMonthsInYear(DateTime.Today.Year);

                foreach (var vendedor_valido in Vendedores_Validos)
                {
                    for (int mes = 1; mes <= meses; mes++)
                    {
                        Mes_Venta mes_Venta = new()
                        {
                            Nro_Mes = mes,
                            Nombre_Mes = new DateTime(DateTime.Today.Year, mes, 1).ToString("MMM")
                        };

                        if (vendedor_valido.Clientes is not null)
                        {
                            foreach (var cliente in vendedor_valido.Clientes)
                            {
                                List<Orden> Ordenes = context.Orden.Where(x => x.Destino != null && x.Destino.Codcte == cliente.Cod
                                && x.Fchcar != null && x.Fchcar.Value.Month == mes && x.Fchcar.Value.Year == 2023 && x.Codest != 14)
                                .Include(x => x.Producto)
                                .Include(x => x.Destino)
                                .Include(x => x.OrdenEmbarque)
                                .IgnoreAutoIncludes()
                                .ToList();

                                var ordenes_dintinguidas = Ordenes.DistinctBy(x => x.Liniteid);

                                foreach (var orden in ordenes_dintinguidas)
                                {
                                    //Mes_Venta_Producto mes_Venta_Producto = new()
                                    //{
                                    //    Producto = orden.Obtener_Nombre_Producto,
                                    //    Litros_Vendidos = orden.Obtener_Volumen,
                                    //    Venta = orden.Obtener_Precio_Orden_Embarque * orden.Obtener_Volumen
                                    //};

                                    //mes_Venta.Litros_Vendidos += mes_Venta_Producto.Litros_Vendidos;
                                    //mes_Venta.Venta += mes_Venta_Producto.Venta;

                                    mes_Venta.Litros_Vendidos += orden.Obtener_Volumen;
                                    mes_Venta.Venta += orden.Obtener_Precio_Orden_Embarque * orden.Obtener_Volumen;

                                    //mes_Venta.Mes_Venta_Productos.Add(mes_Venta_Producto);
                                }
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
    }
}
