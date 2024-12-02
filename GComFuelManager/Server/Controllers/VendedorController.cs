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
using System.Globalization;
using System.Linq;
using static GComFuelManager.Server.Controllers.Precios.PrecioController;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, Administrador, Reportes De Venta, Direccion, Gerencia, Ejecutivo de Cuenta Comercial")]
    public class VendedorController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUsuario> userManager;
        private readonly VerifyUserId verifyUser;
        private readonly User_Terminal _terminal;

        public VendedorController(ApplicationDbContext context, UserManager<IdentityUsuario> userManager, VerifyUserId verifyUser, User_Terminal _Terminal)
        {
            this.context = context;
            this.userManager = userManager;
            this.verifyUser = verifyUser;
            this._terminal = _Terminal;
        }

        [HttpGet("anos/reporte")]
        public async Task<ActionResult> Obtener_Años_Disponbles()
        {
            try
            {
                var Fechas_Diponibles = await context.Orden.Where(x => x.Fch != null).GroupBy(x => x.Fchcar!.Value.Year).Select(x => x.Key).ToListAsync();
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

        [HttpGet("clientes")]//TODO: checar utilidad
        public ActionResult Obtener_Clientes_De_Vendedores_Filtrados([FromQuery] Cliente cliente)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var vendedores = context.Cliente_Tad.IgnoreAutoIncludes().Where(x => x.Cliente != null && x.Cliente.Activo && x.Cliente.Id_Vendedor == cliente.Id_Vendedor && x.Id_Terminal == id_terminal)
                    .OrderBy(x => x.Cliente!.Den)
                    .Include(x => x.Cliente).ThenInclude(x => x.Originador).Select(x => x.Cliente).AsQueryable();

                //var vendedores = context.Cliente.IgnoreAutoIncludes().Where(x => x.Activo && x.Id_Vendedor == cliente.Id_Vendedor).Include(x => x.Originador).OrderBy(x => x.Den).AsQueryable();

                if (!string.IsNullOrEmpty(cliente.Den) || !string.IsNullOrWhiteSpace(cliente.Den))
                    vendedores = vendedores.Where(x => x != null && (!string.IsNullOrEmpty(x.Den) || !string.IsNullOrWhiteSpace(x.Den)) && x.Den.ToLower().Contains(cliente.Den.ToLower())).OrderBy(x => x!.Den);

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

                if (string.IsNullOrEmpty(vendedor.Nombre) || string.IsNullOrWhiteSpace(vendedor.Nombre))
                    return BadRequest("Nombre de vendedor no valido");

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
        public async Task<ActionResult> Guardar_Relacion_Cliente_Vendedor([FromBody] List<int> clientesids, [FromQuery] Vendedor vendedor)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var id = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                var clientes = await context.Cliente
                    .Where(x => x.Codtad == id_terminal && clientesids.Contains(x.Cod))
                    .ToListAsync();

                var clienteseditados = clientes.Select(x =>
                {
                    x.Id_Vendedor = vendedor.Id;
                    x.Id_Originador = vendedor.Id_Originador;
                    return x;
                });

                context.UpdateRange();
                await context.SaveChangesAsync();

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
                        cliente_encontrado.Id_Vendedor = 0;
                        cliente_encontrado.Id_Originador = 0;
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

                if (vendedor_Originador.OriginadorId == 0)
                    return BadRequest("Originador no valido");

                if (vendedor_Originador.VendedorId == 0)
                    return BadRequest("Vendedor no valido");

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

        [HttpPost("reporte")]
        public ActionResult Obtener_Venta_De_Meses_Por_Vendedor([FromQuery] Vendedor vendedor, [FromBody] List<int> Meses_Venta)
        {
            try
            {
                Reporte_Venta reporte_Venta = new();

                var vendedores = context.Vendedores.IgnoreAutoIncludes().Where(x => x.Activo)
                    .Include(x => x.Vendedor_Originador).IgnoreAutoIncludes()
                    .OrderBy(x => x.Nombre).AsQueryable();

                if (!string.IsNullOrEmpty(vendedor.Nombre))
                    vendedores = vendedores.Where(x => x.Nombre.ToLower().Contains(vendedor.Nombre.ToLower())).OrderBy(x => x.Nombre);

                if (vendedor.Id != 0)
                    vendedores = vendedores.Where(x => x.Id == vendedor.Id).OrderBy(x => x.Nombre);

                if (vendedor.Id_Originador != 0)
                    vendedores = vendedores.Where(x => x.Vendedor_Originador != null && x.Vendedor_Originador.Any(x => x.OriginadorId == vendedor.Id_Originador)).OrderBy(x => x.Nombre);

                List<Vendedor> Vendedores_Validos = vendedores.ToList();
                var ids_vendedores = Vendedores_Validos.Select(x => x.Id).ToList();

                var meses_seleccionados_ordenados = Meses_Venta.Where(x => x != 0).Order().ToList();
                List<int> meses_validos = new();

                foreach (var mes in meses_seleccionados_ordenados)
                    if (DateTime.Today.Year <= vendedor.Fecha_Registro)
                    {
                        if (DateTime.Today.Month >= mes)
                            meses_validos.Add(mes);
                    }
                    else
                        meses_validos.Add(mes);

                foreach (var mes in meses_validos)
                {
                    if (mes > 0 && mes <= 12)
                    {
                        Mes_Venta total_mes = new()
                        {
                            Nro_Mes = mes
                        };

                        List<string?> clientes_validos = new();

                        foreach (var vendedor_valido in Vendedores_Validos)
                        {
                            if (context.Cliente.Any(x => x.Id_Vendedor == vendedor_valido.Id))
                            {
                                if (vendedor.Id_Originador != 0)
                                    clientes_validos = context.Cliente.Where(x => x.Id_Vendedor == vendedor_valido.Id && x.Id_Originador == vendedor.Id_Originador)
                                        .GroupBy(x => x.Den).Select(x => x.Key).ToList();
                                else
                                    clientes_validos = context.Cliente.Where(x => x.Id_Vendedor == vendedor_valido.Id)
                                        .GroupBy(x => x.Den).Select(x => x.Key).ToList();

                                Mes_Venta mes_Venta = new()
                                {
                                    Nro_Mes = mes,
                                    //Nombre_Mes = new DateTime(DateTime.Today.Year, mes, 1).ToString("MMM")
                                };

                                var Ordenes = context.Orden.IgnoreAutoIncludes().Where(x => x.Destino != null && x.Destino.Cliente != null
                                && !string.IsNullOrEmpty(x.Destino.Cliente.Den) && clientes_validos.Contains(x.Destino.Cliente.Den)
                                && x.Fchcar != null && x.Fchcar.Value.Month == mes && x.Fchcar.Value.Year == vendedor.Fecha_Registro && x.Codest != 14)
                                .Include(x => x.Destino)
                                .ThenInclude(x => x.Cliente)
                                .Include(x => x.OrdenEmbarque).IgnoreAutoIncludes()
                                .Select(x => new { x.Obtener_Volumen, x.Obtener_Precio_Orden_Embarque, vendedor_valido.Nombre })
                                .ToList();

                                var suma_ordenes = Ordenes.GroupBy(x => x.Nombre).Select(x => new Mes_Venta
                                {
                                    Nro_Mes = mes,
                                    Litros_Vendidos = x.Sum(y => y.Obtener_Volumen),
                                    Venta = x.Sum(y => (y.Obtener_Volumen * y.Obtener_Precio_Orden_Embarque))
                                });

                                vendedor_valido.Venta_Por_Meses.AddRange(suma_ordenes);
                            }

                        }

                        var total = context.Orden.IgnoreAutoIncludes().Where(x => x.Destino != null && x.Destino.Cliente != null
                                && ids_vendedores.Contains(x.Destino.Cliente.Id_Vendedor)
                                && x.Fchcar != null && x.Fchcar.Value.Month == mes && x.Fchcar.Value.Year == vendedor.Fecha_Registro && x.Codest != 14)
                                .Include(x => x.Destino)
                                .ThenInclude(x => x.Cliente)
                                .Include(x => x.OrdenEmbarque).IgnoreAutoIncludes()
                                .Select(x => new { x.Obtener_Volumen, x.Obtener_Precio_Orden_Embarque })
                                .ToList();

                        total_mes.Litros_Vendidos = total.Sum(x => x.Obtener_Volumen);
                        total_mes.Venta = total.Sum(x => (x.Obtener_Volumen * x.Obtener_Precio_Orden_Embarque));

                        reporte_Venta.Totales.Add(total_mes);
                    }
                }

                reporte_Venta.Vendedores = Vendedores_Validos;
                reporte_Venta.Meses_Venta = meses_validos;

                return Ok(reporte_Venta);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("reporte/clientes")]
        public async Task<ActionResult> Obtener_Venta_De_Meses_Por_Vendedor_Cliente([FromQuery] Vendedor vendedor, [FromBody] List<int> meses)
        {
            try
            {
                var ctes = await context.Cliente.Where(x => x.Id_Vendedor == vendedor.Id).GroupBy(x => x.Den).Select(x => new Venta_Cliente(x.Key ?? string.Empty, new())).ToListAsync();

                var mo = meses.Where(x => x != 0).Order().ToList();
                List<Venta_Cliente> Clientes = new();
                List<int> Meses = new();

                for (int i = 0; i < mo.Count; i++)
                {
                    Meses.Add(mo[i]);

                    for (int j = 0; j < ctes.Count; j++)
                    {

                        var Ordenes = await context.Orden.IgnoreAutoIncludes().Where(x => x.Destino != null && x.Destino.Cliente != null
                        && !string.IsNullOrEmpty(x.Destino.Cliente.Den) && x.Destino.Cliente.Den == ctes[j].Cliente
                        && x.Fchcar != null && x.Fchcar.Value.Month == mo[i] && x.Fchcar.Value.Year == vendedor.Fecha_Registro && x.Codest != 14)
                            .Include(x => x.Destino)
                            .ThenInclude(x => x.Cliente)
                            .Include(x => x.OrdenEmbarque).IgnoreAutoIncludes()
                            .Select(x => new { x.Obtener_Volumen, x.Obtener_Precio_Orden_Embarque, x.Obtener_Cliente_De_Orden })
                            .ToListAsync();

                        var suma_ordenes = Ordenes
                            .GroupBy(x => x.Obtener_Cliente_De_Orden)
                            .Select(x => new Monto_Venta_Cliente(x.Sum(y => y.Obtener_Volumen), x.Sum(y => (y.Obtener_Volumen * y.Obtener_Precio_Orden_Embarque)), mo[i]))
                            .ToList();

                        //Clientes.Add(new(ctes[j] ?? string.Empty, suma_ordenes));
                        ctes[j].Montos.AddRange(suma_ordenes);
                    }
                }

                return Ok(new Detalle_Venta_Cliente(Meses, ctes));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("reporte/originador")]
        public ActionResult Obtener_Venta_De_Meses_Por_Originador([FromQuery] Vendedor originador, [FromBody] List<int> Meses_Venta)
        {
            try
            {
                Reporte_Venta reporte_Venta = new();

                var originadores = context.Originadores.IgnoreAutoIncludes().Where(x => x.Activo)
                    .Select(x => new Vendedor
                    { Nombre = x.Nombre, Id = x.Id })
                    .OrderBy(x => x.Nombre).AsQueryable();

                if (!string.IsNullOrEmpty(originador.Nombre))
                    originadores = originadores.Where(x => x.Nombre.ToLower().Contains(originador.Nombre.ToLower())).OrderBy(x => x.Nombre);

                if (originador.Id_Originador != 0)
                    originadores = originadores.Where(x => x.Id == originador.Id_Originador).OrderBy(x => x.Nombre);

                var originadores_validos = originadores.ToList();
                var ids_originadores = originadores_validos.Select(x => x.Id);

                var meses_seleccionados_ordenados = Meses_Venta.Where(x => x != 0).Order().ToList();
                List<int> meses_validos = new();

                foreach (var mes in meses_seleccionados_ordenados)
                    if (DateTime.Today.Year <= originador.Fecha_Registro)
                    {
                        if (DateTime.Today.Month >= mes)
                            meses_validos.Add(mes);
                    }
                    else
                        meses_validos.Add(mes);

                foreach (var mes in meses_validos)
                {
                    if (mes > 0 && mes <= 12)
                    {
                        Mes_Venta total_mes = new()
                        {
                            Nro_Mes = mes
                        };

                        foreach (var item in originadores_validos)
                        {
                            if (context.Cliente.Any(x => x.Id_Originador == item.Id))
                            {
                                Mes_Venta mes_Venta = new()
                                {
                                    Nro_Mes = mes,
                                    //Nombre_Mes = new DateTime(DateTime.Today.Year, mes, 1).ToString("MMM")
                                };

                                List<string?> clientes_validos = context.Cliente.Where(x => x.Id_Originador == item.Id)
                                    .IgnoreAutoIncludes().Select(x => (string?)x.Den).ToList();

                                var Ordenes = context.Orden.IgnoreAutoIncludes().Where(x => x.Destino != null && x.Destino.Cliente != null
                                && !string.IsNullOrEmpty(x.Destino.Cliente.Den) && clientes_validos.Contains(x.Destino.Cliente.Den)
                                && x.Fchcar != null && x.Fchcar.Value.Month == mes && x.Fchcar.Value.Year == originador.Fecha_Registro && x.Codest != 14)
                                    .Include(x => x.Destino)
                                    .ThenInclude(x => x.Cliente)
                                    .Include(x => x.OrdenEmbarque).IgnoreAutoIncludes()
                                    .Select(x => new { x.Obtener_Volumen, x.Obtener_Precio_Orden_Embarque, Vendedor = item.Nombre })
                                    .ToList();


                                var suma_ordenes = Ordenes.GroupBy(x => x.Vendedor).Select(x => new Mes_Venta
                                {
                                    Nro_Mes = mes,
                                    Litros_Vendidos = x.Sum(y => y.Obtener_Volumen),
                                    Venta = x.Sum(y => (y.Obtener_Volumen * y.Obtener_Precio_Orden_Embarque))
                                });

                                //List<Orden> ordenes_a_sumar = new();
                                //List<dynamic> ordenes_seleccionadas_a_sumar = new();

                                //foreach (var orden in Ordenes)
                                //    if (!ordenes_seleccionadas_a_sumar.Any(x => x.Ref == orden.Ref && x.Bolguiid != orden.Bolguiid))
                                //    {
                                //        ordenes_seleccionadas_a_sumar.Add(orden);
                                //        mes_Venta.Litros_Vendidos += orden.Obtener_Volumen;
                                //        mes_Venta.Venta += (orden.Obtener_Precio_Orden_Embarque * orden.Obtener_Volumen);

                                //        if (total_mes.Mes_Venta_Productos.Any(x => x.Producto == orden.Obtener_Nombre_Producto))
                                //        {
                                //            total_mes.Mes_Venta_Productos.First(x => x.Producto == orden.Obtener_Nombre_Producto).Venta +=
                                //                (orden.Obtener_Precio_Orden_Embarque * orden.Obtener_Volumen);
                                //            total_mes.Mes_Venta_Productos.First(x => x.Producto == orden.Obtener_Nombre_Producto).Litros_Vendidos += orden.Obtener_Volumen;
                                //        }
                                //        else
                                //        {
                                //            Mes_Venta_Producto mes_Venta_Producto = new()
                                //            {
                                //                Litros_Vendidos = orden.Obtener_Volumen,
                                //                Venta = (orden.Obtener_Precio_Orden_Embarque * orden.Obtener_Volumen),
                                //                Producto = orden.Obtener_Nombre_Producto
                                //            };
                                //            total_mes.Mes_Venta_Productos.Add(mes_Venta_Producto);
                                //        }
                                //    }

                                //total_mes.Litros_Vendidos += mes_Venta.Litros_Vendidos;
                                //total_mes.Venta += mes_Venta.Venta;

                                item.Venta_Por_Meses.AddRange(suma_ordenes);
                            }
                        }

                        var total = context.Orden.IgnoreAutoIncludes().Where(x => x.Destino != null && x.Destino.Cliente != null
                                && ids_originadores.Contains(x.Destino.Cliente.Id_Vendedor)
                                && x.Fchcar != null && x.Fchcar.Value.Month == mes && x.Fchcar.Value.Year == originador.Fecha_Registro && x.Codest != 14)
                                .Include(x => x.Destino)
                                .ThenInclude(x => x.Cliente)
                                .Include(x => x.OrdenEmbarque).IgnoreAutoIncludes()
                                .Select(x => new { x.Obtener_Volumen, x.Obtener_Precio_Orden_Embarque })
                                .ToList();

                        total_mes.Litros_Vendidos = total.Sum(x => x.Obtener_Volumen);
                        total_mes.Venta = total.Sum(x => (x.Obtener_Volumen * x.Obtener_Precio_Orden_Embarque));

                        reporte_Venta.Totales.Add(total_mes);
                    }
                }

                reporte_Venta.Vendedores = originadores_validos;
                reporte_Venta.Meses_Venta = meses_seleccionados_ordenados;

                return Ok(reporte_Venta);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("reporte/originador/clientes")]
        public async Task<ActionResult> Obtener_Venta_De_Meses_Por_Originador_Cliente([FromQuery] Vendedor vendedor, [FromBody] List<int> meses)
        {
            try
            {
                var ctes = await context.Cliente.Where(x => x.Id_Originador == vendedor.Id).GroupBy(x => x.Den).Select(x => new Venta_Cliente(x.Key ?? string.Empty, new())).ToListAsync();

                var mo = meses.Where(x => x != 0).Order().ToList();
                List<Venta_Cliente> Clientes = new();
                List<int> Meses = new();

                for (int i = 0; i < mo.Count; i++)
                {
                    Meses.Add(mo[i]);

                    for (int j = 0; j < ctes.Count; j++)
                    {

                        var Ordenes = await context.Orden.IgnoreAutoIncludes().Where(x => x.Destino != null && x.Destino.Cliente != null
                        && !string.IsNullOrEmpty(x.Destino.Cliente.Den) && x.Destino.Cliente.Den == ctes[j].Cliente
                        && x.Fchcar != null && x.Fchcar.Value.Month == mo[i] && x.Fchcar.Value.Year == vendedor.Fecha_Registro && x.Codest != 14)
                            .Include(x => x.Destino)
                            .ThenInclude(x => x.Cliente)
                            .Include(x => x.OrdenEmbarque).IgnoreAutoIncludes()
                            .Select(x => new { x.Obtener_Volumen, x.Obtener_Precio_Orden_Embarque, x.Obtener_Cliente_De_Orden })
                            .ToListAsync();

                        var suma_ordenes = Ordenes
                            .GroupBy(x => x.Obtener_Cliente_De_Orden)
                            .Select(x => new Monto_Venta_Cliente(x.Sum(y => y.Obtener_Volumen), x.Sum(y => (y.Obtener_Volumen * y.Obtener_Precio_Orden_Embarque)), mo[i]))
                            .ToList();

                        //Clientes.Add(new(ctes[j] ?? string.Empty, suma_ordenes));
                        ctes[j].Montos.AddRange(suma_ordenes);
                    }
                }

                return Ok(new Detalle_Venta_Cliente(Meses, ctes));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
