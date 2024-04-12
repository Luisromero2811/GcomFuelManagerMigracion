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

                List<Gestión_EstadosDTO> Ordenes = new List<Gestión_EstadosDTO>();

                var ordenesTotales = await context.OrdenEmbarque
                    .Where(x => x.Fchcar >= gestión_Estados.DateInicio && x.Fchcar <= gestión_Estados.DateFin && x.Codtad == id_terminal)
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
                    .OrderBy(x => x.Fchpet)
                           .Select(e => new Gestión_EstadosDTO()
                           {
                               Referencia = e.FolioSyn ?? "",
                               FechaPrograma = e.Fchcar.Value.ToString("yyyy-MM-dd") ?? "",
                               Unidad_Negocio = e.Tad.Den ?? "",
                               EstatusOrden = e.Estado.den,
                               FechaCarga = e.Orden.Fchcar.Value.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                               Bol = e.Orden.BatchId,
                               DeliveryRack = e.Destino.Cliente.Tipven ?? "",
                               Cliente = e.Destino.Cliente.Den ?? "",
                               Destino = e.Destino.Den ?? "",
                               Producto = e.Producto.Den ?? "",
                               VolNat = e.Compartment == 1 ? Convert.ToDouble(e.Tonel.Capcom) :
                            e.Compartment == 2 ? Convert.ToDouble(e.Tonel.Capcom2) :
                            e.Compartment == 3 ? Convert.ToDouble(e.Tonel.Capcom3) :
                            e.Compartment == 4 ? e.Tonel.Capcom4 : e.Vol,
                               VolCar = e.Orden.Vol,
                               Transportista = e.Tonel.Transportista.Den ?? "",
                               Unidad = e.Tonel.Veh ?? "",
                               Operador = e.Chofer.Den ?? "",
                               Por_Asignar = e.HistorialEstados.Where(x => x.Estado.den == "1_Por asignar").FirstOrDefault().Fecha_Actualizacion.ToString() ?? "",
                               Asignado = e.HistorialEstados.Where(x => x.Estado.den == "2_Asignado").FirstOrDefault().Fecha_Actualizacion.ToString() ?? "",
                               Por_Cargar = e.HistorialEstados.Where(x => x.Estado.den == "3_Por cargar").FirstOrDefault().Fecha_Actualizacion.ToString() ?? "",
                               Cargado = e.HistorialEstados.Where(x => x.Estado.den == "4_Cargado").FirstOrDefault().Fecha_Actualizacion.ToString() ?? "",
                               Ruta_Tas = e.HistorialEstados.Where(x => x.Estado.den == "5_En ruta a TAS").FirstOrDefault().Fecha_Actualizacion.ToString() ?? "",
                               Fuera_Tas = e.HistorialEstados.Where(x => x.Estado.den == "6_Fuera de TAS").FirstOrDefault().Fecha_Actualizacion.ToString() ?? "",
                               Espera_dentro_TAS = e.HistorialEstados.Where(x => x.Estado.den == "7_En espera dentro de TAS").FirstOrDefault().Fecha_Actualizacion.ToString() ?? "",
                               Proceso_descarga = e.HistorialEstados.Where(x => x.Estado.den == "8_En proceso de descarga").FirstOrDefault().Fecha_Actualizacion.ToString() ?? "",
                               Descargado = e.HistorialEstados.Where(x => x.Estado.den == "9_Descargado").FirstOrDefault().Fecha_Actualizacion.ToString() ?? "",
                               Orden_Cancelada = e.HistorialEstados.Where(x => x.Estado.den == "10_Orden cancelada").FirstOrDefault().Fecha_Actualizacion.ToString() ?? "",
                           })
                    .Take(10000)
                    .ToListAsync();
                Ordenes.AddRange(ordenesTotales);


                return Ok(Ordenes);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

    }
}