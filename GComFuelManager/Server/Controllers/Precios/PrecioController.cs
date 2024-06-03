
using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.Filtros;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IO;
using System.Xml;

namespace GComFuelManager.Server.Controllers.Precios
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PrecioController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUsuario> userManager;
        private readonly VerifyUserToken verifyUser;
        private readonly User_Terminal terminal;

        public PrecioController(ApplicationDbContext context, UserManager<IdentityUsuario> userManager, VerifyUserToken verifyUser, User_Terminal _Terminal)
        {
            this.context = context;
            this.userManager = userManager;
            this.verifyUser = verifyUser;
            terminal = _Terminal;
        }

        //[HttpGet]
        //public async Task<ActionResult> GetPrecios()
        //{
        //    try
        //    {
        //        var precios = await context.Precio
        //            .Include(x => x.Zona)
        //            .Include(x => x.Cliente)
        //            .Include(x => x.Producto)
        //            .Include(x => x.Destino)
        //            .ToListAsync();

        //        return Ok(precios);
        //    }
        //    catch (Exception e)
        //    {
        //        return BadRequest(e.Message);
        //    }
        //}

        [HttpPost("productos/{folio?}")]
        public async Task<ActionResult> GetPrecios([FromBody] ZonaCliente? zonaCliente, [FromRoute] string? folio)
        {
            try
            {
                var id_terminal = terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                List<Precio> precios = new();
                List<PrecioProgramado> preciosPro = new();
                var LimiteDate = DateTime.Today.AddHours(16);

                var user = await userManager.FindByNameAsync(HttpContext.User.FindFirstValue(ClaimTypes.Name)!);
                if (user == null)
                    return NotFound();

                var userSis = context.Usuario.FirstOrDefault(x => x.Usu == user.UserName);
                if (userSis == null)
                    return NotFound();

                if (!string.IsNullOrEmpty(folio))
                {
                    var ordenes = await context.OrdenCierre.Where(x => x.Folio == folio && x.Estatus == true && x.Id_Tad == id_terminal)
                            .Include(x => x.Cliente)
                            .ToListAsync();
                    var ordenesUnic = ordenes.DistinctBy(x => x.CodPrd).Select(x => x);

                    foreach (var item in ordenesUnic)
                    {
                        var zona = context.ZonaCliente.FirstOrDefault(x => x.CteCod == item.CodCte);
                        Precio precio = new()
                        {
                            Pre = item.Precio,
                            CodCte = item.CodCte,
                            CodDes = item.CodDes,
                            CodPrd = item.CodPrd,
                            CodGru = item.Cliente?.codgru,
                            CodZona = zona?.CteCod,
                            Producto = context.Producto.FirstOrDefault(x => x.Cod == item.CodPrd)
                        };
                        precios.Add(precio);
                    }
                    return Ok(precios);
                }

                precios = await context.Precio.Where(x => x.CodCte == userSis.CodCte && x.CodDes == zonaCliente.DesCod && x.Activo == true && x.Id_Tad == id_terminal)
                    //&& x.CodZona == zona.ZonaCod)
                    .Include(x => x.Producto)
                    .ToListAsync();

                precios.ForEach(x =>
                {
                    if (x.FchDia < DateTime.Today
                    && DateTime.Today.DayOfWeek != DayOfWeek.Saturday
                    && DateTime.Today.DayOfWeek != DayOfWeek.Sunday
                    && DateTime.Today.DayOfWeek != DayOfWeek.Monday)
                    {
                        var porcentaje = context.Porcentaje.FirstOrDefault(x => x.Accion == "cliente");
                        var aumento = (porcentaje.Porcen / 100) + 1;
                        x.Pre = x.FchDia < DateTime.Today ? Math.Round((x.Pre * aumento), 4) : Math.Round(x.Pre, 4);
                    }
                });

                if (DateTime.Now > LimiteDate &&
                    DateTime.Today.DayOfWeek != DayOfWeek.Saturday &&
                    DateTime.Today.DayOfWeek != DayOfWeek.Sunday)
                {
                    preciosPro = await context.PrecioProgramado.Where(x => x.CodCte == userSis.CodCte
                    && x.CodDes == zonaCliente.DesCod && x.Activo == true && x.Id_Tad == id_terminal)
                    //&& x.CodZona == zona.ZonaCod)
                    .Include(x => x.Producto)
                    .ToListAsync();

                    foreach (var item in preciosPro)
                    {
                        precios.FirstOrDefault(x => x.CodDes == item.CodDes && x.CodCte == item.CodCte && x.CodPrd == item.CodPrd && x.FchDia < item.FchDia).Pre = item.Pre;
                        precios.FirstOrDefault(x => x.CodDes == item.CodDes && x.CodCte == item.CodCte && x.CodPrd == item.CodPrd && x.FchDia < item.FchDia).FchDia = item.FchDia;
                        var pre = precios.FirstOrDefault(x => x.CodDes == item.CodDes && x.CodCte == item.CodCte && x.CodPrd == item.CodPrd);
                        if (pre is null)
                        {
                            precios.Add(new Precio()
                            {
                                Pre = item.Pre,
                                CodCte = item.CodCte,
                                CodDes = item.CodDes,
                                CodPrd = item.CodPrd,
                                CodGru = item.Cliente?.codgru,
                                Producto = context.Producto.FirstOrDefault(x => x.Cod == item.CodPrd)
                            });
                        }
                    }
                }

                return Ok(precios);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("{Orden_Compra}")]
        public ActionResult GetPrecioByEner([FromRoute] int Orden_Compra, Int16 Id_Terminal = 1)
        {
            try
            {
                List<PrecioBol> precios = new();

                var ordenes = context.OrdenEmbarque.IgnoreAutoIncludes().Where(x => x.Folio == Orden_Compra && x.Codtad == Id_Terminal)
                    .Include(x => x.Producto)
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Tad)
                    .ToList();

                if (ordenes is null)
                    return Ok(new List<PrecioBol>() { new PrecioBol() });

                foreach (var item in ordenes)
                {
                    PrecioBol precio = new();

                    Orden? orden = new();
                    orden = context.Orden.IgnoreAutoIncludes().Where(x => x.Ref == item.FolioSyn).Include(x => x.Producto).Include(x => x.Destino).ThenInclude(x => x.Cliente).Include(x => x.Terminal).FirstOrDefault();

                    precio.Fecha_De_Carga = orden?.Fchcar ?? item.Fchcar;

                    precio.Referencia = orden?.Ref ?? item.FolioSyn;

                    precio.Sellos = orden?.SealNumber ?? string.Empty;

                    precio.NOrden = orden?.NOrden ?? string.Empty;

                    precio.Factura = orden?.Factura ?? string.Empty;

                    if (orden is not null)
                    {
                        if (orden.Producto is not null)
                            precio.Producto_Synthesis = orden.Producto.Den ?? string.Empty;

                        if (orden.Destino is not null)
                            precio.Destino_Synthesis = orden.Destino.Den ?? string.Empty;

                        if (orden.Terminal is not null)
                            if (!string.IsNullOrEmpty(orden.Terminal.Den))
                            {
                                precio.Terminal_Final = orden.Terminal.Den;
                                if (!string.IsNullOrEmpty(orden.Terminal.Codigo))
                                    precio.Codigo_Terminal_Final = orden.Terminal.Codigo;
                            }

                        precio.BOL = orden.BatchId ?? 0;
                        precio.Volumen_Cargado = orden.Vol;
                    }

                    if (item is not null)
                    {
                        if (item.Destino is not null)
                        {
                            precio.Destino_Original = item.Destino.Den;
                            if (item.Destino.Cliente is not null)
                                if (!string.IsNullOrEmpty(item.Destino.Cliente.Den))
                                    precio.Cliente_Original = item.Destino.Cliente.Den;

                        }

                        if (item.Producto is not null)
                            if (!string.IsNullOrEmpty(item.Producto.Den))
                                precio.Producto_Original = item.Producto.Den;

                        if (item.Tad is not null)
                        {
                            if (!string.IsNullOrEmpty(item.Tad.Den))
                                precio.Terminal_Original = item.Tad.Den;

                            if (!string.IsNullOrEmpty(item.Tad.Codigo))
                                precio.Codigo_Terminal_Original = item.Tad.Codigo;
                        }
                    }

                    var precioVig = context.Precio.IgnoreAutoIncludes()
                        .Where(x => item != null && x.CodDes == item.Coddes && x.CodPrd == item.Codprd && x.Id_Tad == item.Codtad)
                        .OrderByDescending(x => x.FchActualizacion).FirstOrDefault();

                    if (orden is not null)
                        precioVig = context.Precio.IgnoreAutoIncludes()
                        .Where(x => x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd && x.Id_Tad == orden.Id_Tad)
                        .OrderByDescending(x => x.FchActualizacion).FirstOrDefault();

                    var precioPro = context.PrecioProgramado.IgnoreAutoIncludes()
                        .Where(x => item != null && x.CodDes == item.Coddes && x.CodPrd == item.Codprd && x.Id_Tad == item.Codtad)
                        .OrderByDescending(x => x.FchActualizacion).FirstOrDefault();

                    if (orden is not null)
                        precioPro = context.PrecioProgramado.IgnoreAutoIncludes()
                        .Where(x => x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd && x.Id_Tad == orden.Id_Tad)
                        .OrderByDescending(x => x.FchActualizacion).FirstOrDefault();

                    var precioHis = context.PreciosHistorico.IgnoreAutoIncludes()
                        .Where(x => item != null && x.CodDes == item.Coddes && x.CodPrd == item.Codprd && x.FchDia <= DateTime.Today && x.Id_Tad == item.Codtad)
                        .OrderByDescending(x => x.FchActualizacion)
                        .FirstOrDefault();

                    if (orden is not null)
                        precioHis = context.PreciosHistorico.IgnoreAutoIncludes()
                        .Where(x => x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd && orden.Fchcar != null && x.FchDia <= orden.Fchcar.Value.Date && x.Id_Tad == orden.Id_Tad)
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
                    }

                    if (item != null && precioVig is not null && orden is null || orden is not null && precioVig is not null)
                    {
                        if (precioVig.FchDia == DateTime.Today)
                        {
                            precio.Precio = precioVig.Pre;
                            precio.Fecha_De_Precio = precioVig.FchDia;
                            precio.Precio_Encontrado = true;
                            precio.Precio_Encontrado_En = "Vigente";
                            precio.Moneda = precioHis?.Moneda?.Nombre ?? "MXN";
                            precio.Tipo_De_Cambio = precioHis?.Equibalencia ?? 1;
                        }
                    }

                    if (item != null && precioPro is not null && context.PrecioProgramado.Any() || orden is not null && precioPro is not null && context.PrecioProgramado.Any())
                    {
                        if (precioPro.FchDia == DateTime.Today)
                        {
                            precio.Precio = precioPro.Pre;
                            precio.Fecha_De_Precio = precioPro.FchDia;
                            precio.Precio_Encontrado = true;
                            precio.Precio_Encontrado_En = "Programado";
                            precio.Moneda = precioHis?.Moneda?.Nombre ?? "MXN";
                            precio.Tipo_De_Cambio = precioHis?.Equibalencia ?? 1;
                        }
                    }

                    if (item != null && context.OrdenPedido.Any(x => x.CodPed == item.Cod && x.Pedido_Original == 0 && string.IsNullOrEmpty(x.Folio_Cierre_Copia)))
                    {
                        var ordenepedido = context.OrdenPedido.Where(x => x.CodPed == item.Cod && !string.IsNullOrEmpty(x.Folio) && x.Pedido_Original == 0 && string.IsNullOrEmpty(x.Folio_Cierre_Copia)).FirstOrDefault();

                        if (ordenepedido is not null)
                        {
                            var cierre = context.OrdenCierre.Where(x => x.Folio == ordenepedido.Folio
                             && x.CodPrd == item.Codprd).FirstOrDefault();

                            if (cierre is not null)
                            {
                                precio.Precio = cierre.Precio;
                                precio.Fecha_De_Precio = cierre.fchPrecio;
                                precio.Es_Cierre = true;
                                precio.Precio_Encontrado = true;
                                precio.Precio_Encontrado_En = "Cierre";
                                precio.Moneda = precioHis?.Moneda?.Nombre ?? "MXN";
                                precio.Tipo_De_Cambio = precioHis?.Equibalencia ?? 1;
                                precio.Folio_Cierre = cierre.Folio ?? string.Empty;
                            }
                        }
                    }

                    if (item is not null && precioHis is null && precioPro is null && precioVig is null && !precio.Es_Cierre)
                    {
                        precio.Precio = item.Pre;

                        if (item.OrdenCierre is not null)
                            precio.Fecha_De_Precio = item.OrdenCierre.fchPrecio;

                        precio.Es_Precio_De_Creacion = true;
                        precio.Precio_Encontrado_En = "Creacion";
                    }

                    precios.Add(precio);
                }

                return Ok(precios);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        public ActionResult GetPrecioByFechas([FromQuery] Parametros_Busqueda_Gen param)
        {
            try
            {
                List<Precio_Listado> precios = new();

                var ordenes = context.Orden.IgnoreAutoIncludes().Where(x => x.Fchcar >= param.Fecha_Inicio && x.Fchcar <= param.Fecha_Fin)
                    .Include(x => x.Producto)
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Terminal)
                    .Include(x => x.Tonel)
                    .ThenInclude(x => x.Transportista)
                    .Include(x => x.Chofer)
                    .ToList();

                if (ordenes is null)
                    return Ok(new List<Precio_Listado>() { new() });

                foreach (var item in ordenes)
                {
                    Precio_Listado precio = new()
                    {
                        Orden = item.Cod,
                        Fecha_De_Carga = item.Fchcar,
                        Sellos = item?.SealNumber,
                        Numero_Orden = item?.NOrden,
                        Factura = item?.Factura,
                        Producto = item?.Producto?.Den,
                        Destino = item?.Destino?.Den,
                        Terminal = item?.Terminal?.Den,
                        BOL = item?.BatchId,
                        Volumen_Cargado = item?.Vol,
                        RFC_Transportista = item?.Tonel?.Transportista?.RFC,
                        RFC_Operador = item?.Chofer?.RFC
                    };

                    var precioVig = context.Precio.IgnoreAutoIncludes()
                        .Where(x => item != null && x.CodDes == item.Coddes && x.CodPrd == item.Codprd && x.Id_Tad == item.Id_Tad)
                        .Select(x => new { x.Pre, x.FchActualizacion, x.FchDia })
                        .OrderByDescending(x => x.FchActualizacion)
                        .FirstOrDefault();

                    var precioPro = context.PrecioProgramado.IgnoreAutoIncludes()
                        .Where(x => item != null && x.CodDes == item.Coddes && x.CodPrd == item.Codprd && x.Id_Tad == item.Id_Tad)
                        .Select(x => new { x.Pre, x.FchActualizacion, x.FchDia })
                        .OrderByDescending(x => x.FchActualizacion)
                        .FirstOrDefault();

                    var precioHis = context.PreciosHistorico.IgnoreAutoIncludes()
                        .Where(x => item != null && x.CodDes == item.Coddes && x.CodPrd == item.Codprd && x.FchDia <= DateTime.Today && x.Id_Tad == item.Id_Tad)
                        .Select(x => new { x.pre, x.FchActualizacion, x.FchDia })
                        .OrderByDescending(x => x.FchActualizacion)
                        .FirstOrDefault();

                    if (precioHis is not null)
                    {
                        precio.Precio = precioHis.pre;
                    }

                    if (item != null && precioVig is not null && item.Fchcar is not null && item.Fchcar.Value.Date == DateTime.Today.Date)
                    {
                        if (precioVig.FchDia == DateTime.Today)
                        {
                            precio.Precio = precioVig.Pre;
                        }
                    }

                    if (item != null && precioPro is not null && context.PrecioProgramado.Any() && item.Fchcar is not null && item.Fchcar.Value.Date == DateTime.Today.Date)
                    {
                        if (precioPro.FchDia == DateTime.Today)
                        {
                            precio.Precio = precioPro.Pre;
                        }
                    }

                    if (item != null && context.OrdenPedido.Any(x => x.CodPed == item.Cod && x.Pedido_Original == 0 && string.IsNullOrEmpty(x.Folio_Cierre_Copia)))
                    {
                        var ordenepedido = context.OrdenPedido.Where(x => x.CodPed == item.Cod && !string.IsNullOrEmpty(x.Folio) && x.Pedido_Original == 0 && string.IsNullOrEmpty(x.Folio_Cierre_Copia)).FirstOrDefault();

                        if (ordenepedido is not null)
                        {
                            var cierre = context.OrdenCierre.Where(x => x.Folio == ordenepedido.Folio
                             && x.CodPrd == item.Codprd).FirstOrDefault();

                            if (cierre is not null)
                            {
                                precio.Precio = cierre.Precio;
                                precio.Es_Cierre = true;
                            }
                        }
                    }

                    if (item is not null && precioHis is null && precioPro is null && precioVig is null && !precio.Es_Cierre)
                    {
                        precio.Precio = item.OrdenEmbarque?.Pre;
                    }

                    precios.Add(precio);
                }

                return Ok(precios);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("orden/{id}")]
        public ActionResult Otener_Orden_Por_Cod([FromRoute] long id)
        {
            try
            {
                var orden = context.Orden
                    .Include(x => x.OrdenEmbarque)
                    .ThenInclude(x => x.Archivos)
                    .Include(x => x.Tonel)
                    .ThenInclude(x => x.Transportista)
                    .Include(x => x.Chofer)
                    .Include(x => x.Producto)
                    .Include(x => x.Destino)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Terminal)
                    .SingleOrDefault(x => x.Cod == id);

                if(orden is null) { return NotFound(); }

                Precio_Listado precio = new()
                {
                    Orden = orden.Cod,
                    Terminal = orden.Terminal?.Den,
                    BOL = orden.BatchId,
                    Numero_Orden = orden.NOrden,
                    Volumen_Cargado = orden.Vol,
                    Producto = orden.Producto?.Den,
                    Fecha_De_Carga = orden.Fchcar,
                    Destino = orden.Destino?.Den,
                    RFC_Transportista = orden.Tonel?.Transportista?.Den,
                    RFC_Operador = orden.Chofer?.RFC,
                    Sellos = orden.SealNumber,
                    Pedimento = orden.Pedimento,
                    Factura = orden.Factura
                };

                var precioVig = context.Precio.IgnoreAutoIncludes()
                        .Where(x => orden != null && x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd && x.Id_Tad == orden.Id_Tad)
                        .Select(x => new { x.Pre, x.FchActualizacion, x.FchDia })
                        .OrderByDescending(x => x.FchActualizacion)
                        .FirstOrDefault();

                var precioPro = context.PrecioProgramado.IgnoreAutoIncludes()
                    .Where(x => orden != null && x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd && x.Id_Tad == orden.Id_Tad)
                    .Select(x => new { x.Pre, x.FchActualizacion, x.FchDia })
                    .OrderByDescending(x => x.FchActualizacion)
                    .FirstOrDefault();

                var precioHis = context.PreciosHistorico.IgnoreAutoIncludes()
                    .Where(x => orden != null && x.CodDes == orden.Coddes && x.CodPrd == orden.Codprd && x.FchDia <= DateTime.Today && x.Id_Tad == orden.Id_Tad)
                    .Select(x => new { x.pre, x.FchActualizacion, x.FchDia })
                    .OrderByDescending(x => x.FchActualizacion)
                    .FirstOrDefault();

                if (precioHis is not null)
                {
                    precio.Precio = precioHis.pre;
                }

                if (orden != null && precioVig is not null && orden.Fchcar is not null && orden.Fchcar.Value.Date == DateTime.Today.Date)
                {
                    if (precioVig.FchDia == DateTime.Today)
                    {
                        precio.Precio = precioVig.Pre;
                    }
                }

                if (orden != null && precioPro is not null && context.PrecioProgramado.Any() && orden.Fchcar is not null && orden.Fchcar.Value.Date == DateTime.Today.Date)
                {
                    if (precioPro.FchDia == DateTime.Today)
                    {
                        precio.Precio = precioPro.Pre;
                    }
                }

                if (orden != null && context.OrdenPedido.Any(x => x.CodPed == orden.Cod && x.Pedido_Original == 0 && string.IsNullOrEmpty(x.Folio_Cierre_Copia)))
                {
                    var ordenepedido = context.OrdenPedido.Where(x => x.CodPed == orden.Cod && !string.IsNullOrEmpty(x.Folio) && x.Pedido_Original == 0 && string.IsNullOrEmpty(x.Folio_Cierre_Copia)).FirstOrDefault();

                    if (ordenepedido is not null)
                    {
                        var cierre = context.OrdenCierre.Where(x => x.Folio == ordenepedido.Folio
                         && x.CodPrd == orden.Codprd).FirstOrDefault();

                        if (cierre is not null)
                        {
                            precio.Precio = cierre.Precio;
                            precio.Es_Cierre = true;
                        }
                    }
                }

                if (orden is not null && precioHis is null && precioPro is null && precioVig is null && !precio.Es_Cierre)
                {
                    precio.Precio = orden.OrdenEmbarque?.Pre;
                }

                if(orden is not null && orden.OrdenEmbarque is not null && orden.OrdenEmbarque.Archivos is not null)
                {
                    var pdf = orden.OrdenEmbarque.Archivos.FirstOrDefault(x => x.Tipo_Archivo == Tipo_Archivo.PDF_FACTURA);
                    if(pdf is not null)
                    {
                        var pdf_bytes = System.IO.File.ReadAllBytes(pdf.Directorio);
                        string pdf_64 = Convert.ToBase64String(pdf_bytes);
                        precio.PDF = pdf_64;
                    }

                    var xml = orden.OrdenEmbarque.Archivos.FirstOrDefault(x => x.Tipo_Archivo == Tipo_Archivo.XML_FACTURA);
                    if(xml is not null)
                    {
                        XmlDocument doc = new();
                        doc.LoadXml(xml.Directorio);

                        precio.XML = doc.OuterXml;
                    }
                }

                return Ok(precio);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        #region precio por bol

        //[HttpGet("{BOL}")]
        //public ActionResult GetPrecioByBol([FromRoute] int BOL)
        //{
        //    try
        //    {
        //        PrecioBol precios = new PrecioBol();

        //        var ordenes = context.Orden.Where(x => x.BatchId == BOL)
        //            .Include(x => x.Producto)
        //            .Include(x => x.Destino)
        //            .FirstOrDefault();

        //        if (ordenes is null)
        //            return Ok(new PrecioBol());

        //        PrecioBol precio = new PrecioBol();

        //        OrdenEmbarque? orden = new OrdenEmbarque();
        //        orden = context.OrdenEmbarque.Where(x => x.FolioSyn == ordenes.Ref).Include(x => x.Producto).Include(x => x.Destino).ThenInclude(x => x.Cliente).Include(x => x.OrdenCierre).FirstOrDefault();

        //        precio.Fecha_De_Carga = ordenes.Fchcar;

        //        precio.Referencia = ordenes.Ref;

        //        if (orden is not null)
        //        {
        //            if (orden.Producto is not null)
        //                precio.Producto_Original = orden.Producto.Den;

        //            if (orden.Destino is not null)
        //            {
        //                precio.Destino_Original = orden.Destino.Den;
        //                if (orden.Destino.Cliente is not null)
        //                    if (!string.IsNullOrEmpty(orden.Destino.Cliente.Den))
        //                        precio.Cliente_Original = orden.Destino.Cliente.Den;

        //            }
        //        }

        //        precio.BOL = ordenes.BatchId;
        //        precio.Volumen_Cargado = ordenes.Vol;
        //        if (orden is not null)
        //        {
        //            if (orden.Destino is not null)
        //                precio.Destino_Original = orden.Destino.Den ?? "";

        //            if (orden.Producto is not null)
        //                precio.Producto_Original = orden.Producto.Den ?? "";
        //        }

        //        var precioVig = context.Precio.Where(x => ordenes != null && x.CodDes == ordenes.CodDes && x.CodPrd == ordenes.CodPrd)
        //            .OrderByDescending(x => x.FchDia)
        //            .FirstOrDefault();

        //        var precioPro = context.PrecioProgramado.Where(x => ordenes != null && x.CodDes == ordenes.CodDes && x.CodPrd == ordenes.CodPrd)
        //            .OrderByDescending(x => x.FchDia)
        //            .FirstOrDefault();

        //        var precioHis = context.PreciosHistorico.Where(x => ordenes != null && x.CodDes == ordenes.CodDes && x.CodPrd == ordenes.CodPrd
        //            && ordenes.Fchcar != null && x.FchDia <= ordenes.Fchcar.Value.Date)
        //            .OrderByDescending(x => x.FchDia)
        //            .FirstOrDefault();

        //        if (precioHis is not null)
        //        {
        //            precio.Precio = precioHis.pre;
        //            precio.Fecha_De_Precio = precioHis.FchDia;
        //            precio.Precio_Encontrado = true;
        //            precio.Precio_Encontrado_En = "Historial";
        //            precio.Moneda = precioHis?.Moneda?.Nombre;
        //            precio.Tipo_De_Cambio = precioHis?.Equibalencia ?? 1;
        //        }

        //        if (ordenes != null && precioVig is not null && ordenes.Fchcar is not null && ordenes.Fchcar.Value.Date == DateTime.Today)
        //        {
        //            precio.Precio = precioVig.Pre;
        //            precio.Fecha_De_Precio = precioVig.FchDia;
        //            precio.Precio_Encontrado = true;
        //            precio.Precio_Encontrado_En = "Vigente";
        //            precio.Moneda = precioVig?.Moneda?.Nombre;
        //            precio.Tipo_De_Cambio = precioVig?.Equibalencia ?? 1;
        //        }

        //        if (ordenes != null && precioPro is not null && ordenes.Fchcar is not null && ordenes.Fchcar.Value.Date == DateTime.Today && context.PrecioProgramado.Any())
        //        {
        //            precio.Precio = precioPro.Pre;
        //            precio.Fecha_De_Precio = precioPro.FchDia;
        //            precio.Precio_Encontrado = true;
        //            precio.Precio_Encontrado_En = "Programado";
        //            precio.Moneda = precioPro?.Moneda?.Nombre;
        //            precio.Tipo_De_Cambio = precioPro?.Equibalencia ?? 1;
        //        }

        //        if (orden != null && context.OrdenPedido.Any(x => x.CodPed == orden.Cod))
        //        {
        //            var ordenepedido = context.OrdenPedido.Where(x => x.CodPed == orden.Cod && !string.IsNullOrEmpty(x.Folio)).FirstOrDefault();

        //            if (ordenepedido is not null)
        //            {
        //                var cierre = context.OrdenCierre.Where(x => x.Folio == ordenepedido.Folio
        //                 && x.CodPrd == orden.CodPrd).FirstOrDefault();

        //                if (cierre is not null)
        //                {
        //                    precio.Precio = cierre.Precio;
        //                    precio.Fecha_De_Precio = cierre.fchPrecio;
        //                    precio.Es_Cierre = true;
        //                    precio.Precio_Encontrado = true;
        //                    precio.Precio_Encontrado_En = "Cierre";
        //                    precio.Moneda = cierre?.Moneda?.Nombre;
        //                    precio.Tipo_De_Cambio = cierre?.Equibalencia ?? 1;
        //                }
        //            }
        //        }

        //        if (orden is not null && precioHis is null && precioPro is null && precioVig is null && !precio.Es_Cierre)
        //        {
        //            precio.Precio = orden.Pre;

        //            if (orden.OrdenCierre is not null)
        //                precio.Fecha_De_Precio = orden.OrdenCierre.fchPrecio;

        //            precio.Es_Precio_De_Creacion = true;
        //            precio.Precio_Encontrado_En = "Creacion";
        //        }

        //        return Ok(precios);
        //    }
        //    catch (Exception e)
        //    {
        //        return BadRequest(e.Message);
        //    }
        //}
        #endregion

        public class PrecioBol
        {
            public double? Precio { get; set; } = 0;
            public string? Referencia { get; set; } = string.Empty;
            public int? BOL { get; set; } = 0;
            public DateTime? Fecha_De_Carga { get; set; } = DateTime.MinValue;
            public DateTime? Fecha_De_Precio { get; set; } = DateTime.MinValue;
            public string Destino_Synthesis { get; set; } = string.Empty;
            public string? Destino_Original { get; set; } = string.Empty;
            public string Producto_Synthesis { get; set; } = string.Empty;
            public string? Producto_Original { get; set; } = string.Empty;
            public bool Es_Cierre { get; set; } = false;
            public bool Es_Precio_De_Creacion { get; set; } = false;
            public bool Precio_Encontrado { get; set; } = false;
            public string Precio_Encontrado_En { get; set; } = string.Empty;
            public double Tipo_De_Cambio { get; set; } = 1;
            public string? Moneda { get; set; } = "MXN";
            public string? Cliente_Original { get; set; } = string.Empty;
            public double? Volumen_Cargado { get; set; } = 0;
            public string Folio_Cierre { get; set; } = string.Empty;
            public string Terminal_Original { get; set; } = string.Empty;
            public string Codigo_Terminal_Original { get; set; } = string.Empty;
            public string Terminal_Final { get; set; } = string.Empty;
            public string Codigo_Terminal_Final { get; set; } = string.Empty;
            public string Pedimento { get; set; } = string.Empty;
            public string NOrden { get; set; } = string.Empty;
            public string Factura { get; set; } = string.Empty;
            public string Sellos { get; set; } = string.Empty;
        }

        public class Precio_Listado
        {
            public long? Orden { get; set; }
            public string? Terminal { get; set; } = string.Empty;
            public int? BOL { get; set; } = 0;
            public string? Numero_Orden { get; set; } = string.Empty;
            public double? Volumen_Cargado { get; set; } = 0;
            public double? Volumen_Natural { get; set; } = 0;
            public string? Producto { get; set; } = string.Empty;
            public double? Precio { get; set; } = 0;
            public DateTime? Fecha_De_Carga { get; set; } = DateTime.MinValue;
            public double Flete_Compra { get; set; }
            public double Flete_Venta { get; set; }
            public string? Destino { get; set; } = string.Empty;
            public string? RFC_Transportista { get; set; } = string.Empty;
            public string? RFC_Operador { get; set; } = string.Empty;
            public string? Sellos { get; set; } = string.Empty;
            public string? Pedimento { get; set; } = string.Empty;
            public string? Factura { get; set; } = string.Empty;
            public bool Es_Cierre { get; set; } = false;
            public string PDF { get; set; } = string.Empty;
            public string XML { get; set; } = string.Empty;
        }
    }
}