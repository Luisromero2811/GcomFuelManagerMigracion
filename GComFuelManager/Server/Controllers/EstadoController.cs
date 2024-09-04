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
using Radzen.Blazor.Rendering;
using System.Security.Claims;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class EstadoController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUsuario> userManager;
        private readonly VerifyUserToken verifyUser;
        private readonly VerifyUserId verifyUserId;
        private readonly User_Terminal _terminal;

        public EstadoController(ApplicationDbContext context, UserManager<IdentityUsuario> userManager, VerifyUserToken verifyUser, VerifyUserId verifyUserId, User_Terminal _Terminal)
        {
            this.context = context;
            this.userManager = userManager;
            this.verifyUser = verifyUser;
            this.verifyUserId = verifyUserId;
            _terminal = _Terminal;
        }

        [HttpGet("{id_tipo}")]
        public ActionResult Get([FromRoute] short id_tipo)
        {
            try
            {
                var estados = context.Estado.Where(x => x.Id_Tipo == id_tipo).ToList();
                return Ok(estados);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("cambio/estado")]
        public async Task<ActionResult> Post([FromBody] OrdenEmbarque ordenEmbarque)
        {
            try
            {
                var id = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                if (ordenEmbarque.Estatus is null) { return NotFound(); }

                ordenEmbarque.Destino = null!;
                ordenEmbarque.Tad = null!;
                ordenEmbarque.Producto = null!;
                ordenEmbarque.Tonel = null!;
                ordenEmbarque.Chofer = null!;
                ordenEmbarque.Estado = null!;
                ordenEmbarque.Orden = null!;
                ordenEmbarque.HistorialEstados = null!;
                ordenEmbarque.Estatus_Orden = null!;

                HistorialEstados historialEstados = new()
                {
                    Id_Orden = ordenEmbarque.Cod,
                    Id_Estado = (byte)ordenEmbarque.Estatus,
                    Id_Usuario = id
                };

                historialEstados.Estado = null!;
                historialEstados.OrdenEmbarque = null!;

                context.Add(historialEstados);
                context.Update(ordenEmbarque);

                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("estados/ordenes")]
        public async Task<ActionResult> ObtenerOrdenes([FromBody] Gestión_EstadosDTO gestión_Estados)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();
                if (gestión_Estados.Estado == 1)
                {
                    List<Gestión_EstadosDTO> Ordenes = new List<Gestión_EstadosDTO>();
                    //x.Destino!.Cliente!.Tipven!.Contains("terno") DELIVERY'S
                    var ordenesTotales = await context.OrdenEmbarque.IgnoreAutoIncludes()
                        .Where(x => x.Fchcar >= gestión_Estados.DateInicio && x.Fchcar <= gestión_Estados.DateFin  && x.Modelo_Venta_Orden == Shared.Enums.Tipo_Venta.Delivery && x.Codtad == id_terminal)
                        .Include(x => x.Datos_Facturas)
                          .Include(x => x.Orden)
                        .Include(x => x.Chofer)
                        .Include(x => x.Destino)
                        .ThenInclude(x => x.Cliente)
                        .Include(x => x.Estado)
                        .Include(x => x.OrdenCompra)
                        .Include(x => x.Tad)
                        .Include(x => x.Producto)
                        .Include(x => x.Tonel)
                        .ThenInclude(x => x.Transportista)
                        .Include(x => x.OrdenCierre)
                         .Include(x => x.OrdenPedido)
                         .Include(x => x.HistorialEstados)
                         .ThenInclude(x => x.Estado)
                        .OrderBy(x => x.Fchpet)
                        .Select(x => x.Obtener_Orden_Gestion_Estado())
                        .Take(10000)
                        .ToListAsync();
                    Ordenes.AddRange(ordenesTotales);


                    return Ok(Ordenes);
                }
                else if (gestión_Estados.Estado == 2)
                {
                    List<Gestión_EstadosDTO> Ordenes = new List<Gestión_EstadosDTO>();

                    var ordenesTotales = await context.OrdenEmbarque.IgnoreAutoIncludes()
                        .Where(x => x.Fchcar >= gestión_Estados.DateInicio && x.Fchcar <= gestión_Estados.DateFin && x.Modelo_Venta_Orden == Shared.Enums.Tipo_Venta.Rack && x.Codtad == id_terminal)
                        .Include(x => x.Datos_Facturas)
                          .Include(x => x.Orden)
                        .Include(x => x.Chofer)
                        .Include(x => x.Destino)
                        .ThenInclude(x => x.Cliente)
                        .Include(x => x.Estado)
                        .Include(x => x.OrdenCompra)
                        .Include(x => x.Tad)
                        .Include(x => x.Producto)
                        .Include(x => x.Tonel)
                        .ThenInclude(x => x.Transportista)
                        .Include(x => x.OrdenCierre)
                         .Include(x => x.OrdenPedido)
                         .Include(x => x.HistorialEstados)
                         .ThenInclude(x => x.Estado)
                        .OrderBy(x => x.Fchpet)
                        .Select(x => x.Obtener_Orden_Gestion_Estado())
                        .Take(10000)
                        .ToListAsync();
                    Ordenes.AddRange(ordenesTotales);


                    return Ok(Ordenes);
                }
                //else if (gestión_Estados.Estado == 3)
                //{
                //    List<Gestión_EstadosDTO> Ordenes = new List<Gestión_EstadosDTO>();

                //    var ordenesTotales = await context.OrdenEmbarque.IgnoreAutoIncludes()
                //        .Where(x => x.Fchcar >= gestión_Estados.DateInicio && x.Fchcar <= gestión_Estados.DateFin && gestión_Estados.Estado.Equals(3) == x.Destino!.Cliente!.Tipven!.Contains("Externo") && x.Codtad == id_terminal)
                //        .Include(x => x.Datos_Facturas)
                //          .Include(x => x.Orden)
                //        .Include(x => x.Chofer)
                //        .Include(x => x.Destino)
                //        .ThenInclude(x => x.Cliente)
                //        .Include(x => x.Estado)
                //        .Include(x => x.OrdenCompra)
                //        .Include(x => x.Tad)
                //        .Include(x => x.Producto)
                //        .Include(x => x.Tonel)
                //        .ThenInclude(x => x.Transportista)
                //        .Include(x => x.OrdenCierre)
                //         .Include(x => x.OrdenPedido)
                //         .Include(x => x.HistorialEstados)
                //         .ThenInclude(x => x.Estado)
                //        .OrderBy(x => x.Fchpet)
                //        .Select(x => x.Obtener_Orden_Gestion_Estado())
                //        .Take(10000)
                //        .ToListAsync();
                //    Ordenes.AddRange(ordenesTotales);


                //    return Ok(Ordenes);
                //}
                else if (gestión_Estados.Estado == 3)
                {
                    List<Gestión_EstadosDTO> Ordenes = new List<Gestión_EstadosDTO>();

                    var ordenesTotales = await context.OrdenEmbarque.IgnoreAutoIncludes()
                        .Where(x => x.Fchcar >= gestión_Estados.DateInicio && x.Fchcar <= gestión_Estados.DateFin && x.Codtad == id_terminal)
                        .Include(x => x.Orden)
                        .ThenInclude(x => x.OrdEmbDet)
                        .Include(x => x.Orden)
                        .Include(x => x.Destino)
                        .ThenInclude(x => x.Cliente)
                        .Include(x => x.Estado)
                        .Include(x => x.Orden)
                        .ThenInclude(x => x.Estado)
                        .Include(x => x.Tad)
                        .Include(x => x.Producto)
                        .Include(x => x.Tonel)
                        .ThenInclude(x => x.Transportista)
                        .Include(x => x.Chofer)
                        .Include(x => x.OrdenCierre)
                        .Include(x => x.Datos_Facturas)
                        .Include(x => x.OrdenCompra) 
                         .Include(x => x.OrdenPedido)
                         .Include(x => x.HistorialEstados)
                         .ThenInclude(x => x.Estado)
                        .OrderBy(x => x.Fchpet)
                        .Select(x => x.Obtener_Orden_Gestion_Estado())
                        .Take(10000)
                        .ToListAsync();
                    Ordenes.AddRange(ordenesTotales);


                    return Ok(Ordenes);
                }
                else
                {
                    return BadRequest();
                }

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

    }
}