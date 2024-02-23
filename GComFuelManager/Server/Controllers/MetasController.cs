using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,Roles = "Admin, Administrador, Reportes De Venta")]

    public class MetasController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUsuario> userManager;
        private readonly VerifyUserId verifyUser;

        public MetasController(ApplicationDbContext context, UserManager<IdentityUsuario> userManager, VerifyUserId verifyUser)
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
                var Fechas_Diponibles = context.Orden.Where(x => x.Fch != null).Select(x => x.Fch.Value.Year);
                var años_ordenados = Fechas_Diponibles.Order();
                var años = Fechas_Diponibles.Distinct();
                return Ok(años);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        public ActionResult Obtener_Metas([FromQuery] Metas_Vendedor metas_)
        {
            try
            {
                if (metas_ is null)
                    return NotFound();

                if (metas_.Ano_reporte == 0)
                    return BadRequest("Año no valido");

                if (metas_.VendedorId == 0)
                    return BadRequest("Vendedor no valido");

                var año_actual = metas_.Ano_reporte;

                var vendedor = context.Vendedores.Include(x => x.Clientes).FirstOrDefault(x => x.Id == metas_.VendedorId);
                if (vendedor is null)
                    return NotFound();

                if (vendedor.Clientes is not null)
                {

                    List<Metas_Vendedor> metas = new();

                    if (año_actual < DateTime.Today.Year)
                    {
                        metas = context.Metas_Vendedor.Where(x => x.VendedorId == metas_.VendedorId && x.Mes.Year == año_actual).ToList();
                    }
                    else
                    {
                        var fecha = new DateTime(año_actual, DateTime.Today.Month, 1);
                        metas = context.Metas_Vendedor.Where(x => x.VendedorId == metas_.VendedorId && x.Mes.Month <= DateTime.Today.Month && x.Mes.Year == año_actual).ToList();
                    }

                    foreach (var meta in metas)
                    {
                        meta.Referencia = 0;
                        meta.Venta_Real = 0;

                        foreach (var cliente in vendedor.Clientes)
                        {
                            var ordenes_mes_actual = context.Orden.Where(x => x.Codest != 14 && x.Destino != null && x.Destino.Codcte == cliente.Cod
                                            && x.Fchcar != null && x.Fchcar.Value.Month == meta.Mes.Month && x.Fchcar.Value.Year == año_actual).ToList();

                            List<Orden> ordenes_a_sumar = new();

                            foreach (var orden in ordenes_mes_actual)
                                if (!ordenes_a_sumar.Any(x => x.Ref == orden.Ref && x.Bolguiid != orden.Bolguiid))
                                    ordenes_a_sumar.Add(orden);


                            //var ordenes_mes_actual_distinguidas = ordenes_mes_actual.DistinctBy(x => x.Liniteid);

                            //meta.Venta_Real += ordenes_mes_actual_distinguidas.Sum(x => x.Vol);
                            meta.Venta_Real += ordenes_a_sumar.Sum(x => x.Vol);

                            if (meta.Mes.Month == 1)
                            {
                                var ordenes_mes_pasado = context.Orden.Where(x => x.Codest != 14 && x.Destino != null && x.Destino.Codcte == cliente.Cod
                                && x.Fchcar != null && x.Fchcar.Value.Month == new DateTime(año_actual, meta.Mes.Month, 1).AddMonths(-1).Month
                                && x.Fchcar.Value.Year == new DateTime(año_actual, meta.Mes.Month, 1).AddMonths(-1).Year).ToList();

                                List<Orden> ordenes_a_sumar_mes_pasado = new();

                                foreach (var orden in ordenes_mes_pasado)
                                    if (!ordenes_a_sumar_mes_pasado.Any(x => x.Ref == orden.Ref && x.Bolguiid != orden.Bolguiid))
                                        ordenes_a_sumar_mes_pasado.Add(orden);

                                //var ordenes_mes_pasado_distinguidas = ordenes_mes_pasado.DistinctBy(x => x.Liniteid);

                                //meta.Referencia += ordenes_mes_pasado_distinguidas.Sum(x => x.Vol);
                                meta.Referencia += ordenes_a_sumar_mes_pasado.Sum(x => x.Vol);
                            }

                        }

                        if (meta.Mes.Month != 1)
                        {
                            var m = meta.Mes.AddMonths(-1).Month;
                            var y = meta.Mes.AddMonths(-1).Year;

                            var meta_anterior = metas.FirstOrDefault(x => x.VendedorId == vendedor.Id && x.Mes.Month == meta.Mes_De_Referencia);

                            if (meta_anterior is not null)
                            {
                                meta.Referencia = meta_anterior.Meta_Acumulada;
                            }
                        }

                        meta.Meta_Acumulada = (meta.Meta + meta.Referencia);
                        meta.Resultado_Mes = (meta.Venta_Real - meta.Meta_Acumulada);

                        if (meta.Venta_Real != 0 && meta.Meta_Acumulada != 0)
                        {
                            var result_cumplimiento = (meta.Venta_Real / meta.Meta_Acumulada);
                            if (result_cumplimiento is not null)
                                meta.Cumplimiento_Mes = Math.Round((double)result_cumplimiento, 2);
                        }

                        if (meta.Mes.Month != 1)
                        {
                            var meta_anterior = metas.FirstOrDefault(x => x.VendedorId == vendedor.Id && x.Mes.Month == meta.Mes_De_Referencia);

                            if (meta_anterior is not null)
                            {
                                meta.Resultado_Acumulado = meta_anterior.Resultado_Acumulado + meta.Resultado_Mes;
                            }
                        }
                        else
                        {
                            meta.Resultado_Acumulado = meta.Resultado_Mes;
                        }

                        if (meta.Resultado_Acumulado != 0 && meta.Meta_Acumulada != 0)
                        {
                            var resultado = (meta.Resultado_Acumulado / meta.Meta_Acumulada);
                            if (resultado is not null)
                                meta.Porciento_Cumplimiento = Math.Round((double)resultado, 2);
                        }

                    }
                    return Ok(metas);
                }

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("{id:int}/{año:int}")]
        public async Task<ActionResult> Guardar_Meta_Vendedor([FromRoute] int año, [FromRoute] int id)
        {
            try
            {
                var vendedor = context.Vendedores.Find(id);
                if (vendedor is not null)
                {
                    if (!context.Metas_Vendedor.Any(x => x.VendedorId == vendedor.Id && x.Mes.Year == año))
                    {
                        for (int i = 1; i <= 12; i++)
                        {
                            Metas_Vendedor metas_Vendedor = new()
                            {
                                VendedorId = vendedor.Id,
                                Mes = new DateTime(año, i, 1)
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

        [HttpPost]
        public async Task<ActionResult> Guardar_Meta_Vendedor([FromBody] Metas_Vendedor metas_)
        {
            try
            {
                if (metas_ is null)
                    return NotFound();

                metas_.Vendedor = null!;

                context.Update(metas_);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("{mes:int}")]
        public async Task<ActionResult> Actualizar_Metas_De_Mes([FromRoute] int mes)
        {
            try
            {
                //var año_actual = DateTime.Today.Year;
                var año_actual = 2023;

                var vendedores = context.Vendedores.Where(x => x.Activo).Include(x => x.Clientes).IgnoreAutoIncludes().ToList();
                if (vendedores is not null)
                {
                    foreach (var vendedor in vendedores)
                    {

                        if (!context.Metas_Vendedor.Any(x => x.VendedorId == vendedor.Id && x.Mes.Year == año_actual))
                        {
                            for (int i = 1; i <= 12; i++)
                            {
                                Metas_Vendedor metas_Vendedor = new()
                                {
                                    VendedorId = vendedor.Id,
                                    Mes = new DateTime(año_actual, i, 1)
                                };

                                if (!context.Metas_Vendedor.Any(x => x.Mes.Month == metas_Vendedor.Mes.Month && x.VendedorId == vendedor.Id && x.Mes.Year == metas_Vendedor.Mes.Year))
                                {
                                    context.Add(metas_Vendedor);
                                    await context.SaveChangesAsync();
                                }
                            }
                        }

                        if (vendedor.Clientes is not null)
                        {
                            var meta = context.Metas_Vendedor.FirstOrDefault(x => x.Mes.Month == mes && x.Mes.Year == año_actual && x.Activa && x.VendedorId == vendedor.Id);
                            if (meta is not null)
                            {
                                meta.Referencia = 0;
                                meta.Venta_Real = 0;
                                foreach (var cliente in vendedor.Clientes)
                                {


                                    var ordenes_mes_actual = context.Orden.Where(x => x.Codest != 14 && x.Destino != null && x.Destino.Codcte == cliente.Cod
                                    && x.Fchcar != null && x.Fchcar.Value.Month == mes && x.Fchcar.Value.Year == año_actual).ToList();

                                    var ordenes_mes_actual_distinguidas = ordenes_mes_actual.DistinctBy(x => x.Liniteid);

                                    meta.Venta_Real += ordenes_mes_actual_distinguidas.Sum(x => x.Vol);

                                    //var ordenes_mes_pasado = context.Orden.Where(x => x.Codest != 14 && x.Destino != null && x.Destino.Codcte == cliente.Cod
                                    //&& x.Fchcar != null && x.Fchcar.Value.Month == new DateTime(año_actual, mes, 1).AddMonths(-1).Month
                                    //&& x.Fchcar.Value.Year == new DateTime(año_actual, mes, 1).AddMonths(-1).Year).ToList();

                                    //var ordenes_mes_pasado_distinguidas = ordenes_mes_pasado.DistinctBy(x => x.Liniteid);

                                    //meta.Referencia = ordenes_mes_pasado_distinguidas.Sum(x => x.Vol);

                                    //meta.Meta_Acumulada = meta.Meta + meta.Referencia;
                                    //meta.Resultado_Mes = meta.Venta_Real - meta.Meta_Acumulada;
                                    //meta.Cumplimiento_Mes = meta.Venta_Real / meta.Meta_Acumulada;

                                    //if (mes != 1)
                                    //{
                                    //    var meta_anterior = context.Metas_Vendedor.FirstOrDefault(x => x.VendedorId == vendedor.Id && x.Mes.Month == new DateTime(año_actual, mes, 1).AddMonths(-1).Month
                                    //    && x.Mes.Year == new DateTime(año_actual, mes, 1).AddMonths(-1).Year);

                                    //    if (meta_anterior is not null)
                                    //    {
                                    //        meta.Resultado_Acumulado = meta_anterior.Resultado_Acumulado + meta.Resultado_Mes;
                                    //    }
                                    //}
                                    //else
                                    //{
                                    //    meta.Resultado_Acumulado = meta.Resultado_Mes;
                                    //}

                                    //if (meta.Resultado_Acumulado > 0 && meta.Meta_Acumulada > 0)
                                    //{
                                    //    meta.Porciento_Cumplimiento = meta.Resultado_Acumulado / meta.Meta_Acumulada;
                                    //}
                                }

                                meta.Meta_Acumulada = (meta.Meta + meta.Referencia);
                                meta.Resultado_Mes = (meta.Venta_Real - meta.Meta_Acumulada);

                                context.Update(meta);
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
    }
}
