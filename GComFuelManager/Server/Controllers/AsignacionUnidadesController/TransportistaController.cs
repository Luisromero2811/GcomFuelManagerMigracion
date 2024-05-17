using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
//using ServiceReference6;
//using ServiceReference3;
using ServiceReference10;
using ServiceReference8;

namespace GComFuelManager.Server.Controllers.AsignacionUnidadesController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TransportistaController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly RequestToFile toFile;
        private readonly VerifyUserId verifyUser;
        private readonly UserManager<IdentityUsuario> UserManager;
        private readonly User_Terminal _terminal;

        public TransportistaController(ApplicationDbContext context, RequestToFile toFile, VerifyUserId verifyUser, UserManager<IdentityUsuario> UserManager, User_Terminal _Terminal)
        {
            this.context = context;
            this.toFile = toFile;
            this.verifyUser = verifyUser;
            this.UserManager = UserManager;
            this._terminal = _Terminal;
        }

        private async Task SaveErrors(Exception e)
        {
            context.Add(new Errors()
            {
                Error = JsonConvert.SerializeObject(new Error()
                {
                    Inner = JsonConvert.SerializeObject(e.InnerException),
                    Message = JsonConvert.SerializeObject(e.Message)
                }),
                Accion = "Obtener cargadas"
            });
            await context.SaveChangesAsync();
        }

        [HttpPost("save")]
        public async Task<ActionResult> PostGroups([FromBody] GrupoTransportista grupoTransportista)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                if (grupoTransportista is null)
                {
                    return NotFound();
                }
                if (grupoTransportista.cod == 0)
                {
                    grupoTransportista.Id_Tad = id_terminal;
                    context.Add(grupoTransportista);
                    await context.SaveChangesAsync();
                    if (!context.GrupoTransportista_Tad.Any(x => x.Id_Terminal == id_terminal && x.Id_GrupoTransportista == grupoTransportista.cod))
                    {
                        GrupoTransportista_Tad grupotransportetad = new()
                        {
                            Id_GrupoTransportista = grupoTransportista.cod,
                            Id_Terminal = id_terminal
                        };
                        context.Add(grupotransportetad);
                        await context.SaveChangesAsync();
                    }
                }
                else
                {
                    grupoTransportista.Terminales = null!;
                    grupoTransportista.GrupoTransportista_Tads = null!;
                    grupoTransportista.Tad = null!;
                    grupoTransportista.Id_Tad = id_terminal;

                    context.Update(grupoTransportista);
                    await context.SaveChangesAsync();
                }
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("desactivargrupo/{cod:int}")]
        public async Task<ActionResult> ChangeStatus([FromRoute] int cod, [FromBody] bool status)
        {
            try
            {
                if (cod == 0)
                    return BadRequest();

                var destino = context.GrupoTransportista.Where(x => x.cod == cod).FirstOrDefault();
                if (destino == null)
                {
                    return NotFound();
                }
                destino.Activo = status;

                context.Update(destino);
                var acc = destino.Activo ? 26 : 27;
                var id = await verifyUser.GetId(HttpContext, UserManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("relacion")]
        public async Task<ActionResult> PostClienteTerminal([FromBody] ClienteTadDTO clienteTadDTO)
        {
            try
            {
                //Si el cliente es nulo, retornamos un notfound
                if (clienteTadDTO is null)
                    return NotFound();

                foreach (var terminal in clienteTadDTO.Tads)
                {
                    foreach (var grupotransportes in clienteTadDTO.GrupoTransportistas)
                    {
                        if (!context.GrupoTransportista_Tad.Any(x => x.Id_Terminal == terminal.Cod && x.Id_GrupoTransportista == grupotransportes.cod))
                        {
                            GrupoTransportista_Tad grupotransportetad = new()
                            {
                                Id_GrupoTransportista = grupotransportes.cod,
                                Id_Terminal = terminal.Cod
                            };
                            context.Add(grupotransportetad);
                        }
                    }
                }
                await context.SaveChangesAsync();

                return Ok();

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("crearTransportista")]
        public async Task<ActionResult> PostTransportista([FromBody] Transportista transportista)
        {
            try
            {
                Random random = new Random();
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                if (transportista is null)
                    return BadRequest();
                //Si el cod del transportista viene en ceros se procede a la creación de una empresa transportista con el Id_Tad con la terminal que estamos logueados, el codgru del transportista al grupo y el carrid y busentid random para nuevos registros 
                if (transportista.Cod == 0)
                {
                    //transportista.Codgru = transportista.GrupoTransportista!.cod!;
                    //Genero el número aleatorio para el Carrid
                    transportista.CarrId = Convert.ToString(random.Next(1, 50000));
                    //Con Any compruebo si el número aleatorio existe en la BD
                    var exist = context.Transportista.Any(x => x.CarrId == transportista.CarrId);
                    //Si ya existe, genera un nuevo número Random
                    if (exist)
                    {
                        transportista.CarrId = Convert.ToString(random.Next(1, 50000));
                    }

                    //Genero el número aleatorio para busentid
                    transportista.Busentid = Convert.ToString(random.Next(1, 50000));
                    //Con Any compruebo si el número aleatorio existe en la BD
                    var existBusentid = context.Transportista.Any(x => x.Busentid == transportista.Busentid);
                    //Si ya existe, genera un nuevo número random
                    if (existBusentid)
                    {
                        transportista.Busentid = Convert.ToString(random.Next(1, 50000));
                    }
                    transportista.Id_Tad = id_terminal;
                    context.Add(transportista);
                    await context.SaveChangesAsync();
                    if (!context.Transportista_Tad.Any(x => x.Id_Terminal == id_terminal && x.Id_Transportista == transportista.Cod))
                    {
                        Transportista_Tad transportista_Tad = new()
                        {
                            Id_Transportista = transportista.Cod,
                            Id_Terminal = id_terminal
                        };
                        context.Add(transportista_Tad);
                        await context.SaveChangesAsync();
                    }
                }
                else
                {
                    transportista.Id_Tad = id_terminal;
                    transportista.Terminales = null!;
                    context.Update(transportista);
                    await context.SaveChangesAsync();
                }

                return Ok();

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("desactivarempresas/{cod:int}")]
        public async Task<ActionResult> ChangeStatusTransportistas([FromRoute] int cod, [FromBody] bool status)
        {
            try
            {
                if (cod == 0)
                    return BadRequest();

                var destino = context.Transportista.Where(x => x.Cod == cod).FirstOrDefault();
                if (destino == null)
                {
                    return NotFound();
                }
                destino.Activo = status;

                context.Update(destino);
                var acc = (bool)destino.Activo ? 26 : 27;
                var id = await verifyUser.GetId(HttpContext, UserManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("relacionempresa")]
        public async Task<ActionResult> PostTransportistaTerminal([FromBody] ClienteTadDTO clienteTadDTO)
        {
            try
            {
                //Si el cliente es nulo, retornamos un notfound
                if (clienteTadDTO is null)
                    return NotFound();

                foreach (var terminal in clienteTadDTO.Tads)
                {
                    foreach (var transportista in clienteTadDTO.Transportistas)
                    {
                        if (!context.Transportista_Tad.Any(x => x.Id_Terminal == terminal.Cod && x.Id_Transportista == transportista.Cod))
                        {
                            Transportista_Tad transportista_Tad = new()
                            {
                                Id_Transportista = transportista.Cod,
                                Id_Terminal = terminal.Cod
                            };
                            context.Add(transportista_Tad);
                        }
                    }
                }
                await context.SaveChangesAsync();

                return Ok();

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("gruposactivos")]
        public async Task<ActionResult> GetGrupos()
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var grupostransporte = await context.GrupoTransportista
                    .Where(x => x.Terminales.Any(x => x.Cod == id_terminal))
                     .Include(x => x.Terminales)
                    .OrderBy(x => x.den)
                    .ToListAsync();
                return Ok(grupostransporte);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("allGT")]
        public ActionResult GetAllGT()
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var GrupoTransportes = context.GrupoTransportista.IgnoreAutoIncludes().Where(x => x.Terminales.Any(x => x.Cod == id_terminal)).Include(x => x.Terminales).IgnoreAutoIncludes().OrderBy(x => x.den);
                return Ok(GrupoTransportes);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("filtraractivos")]
        public ActionResult Obtener_Grupos_Activos([FromQuery] GrupoTransportista grupo)
        {
            try
            {
                var grupos = context.GrupoTransportista
                     .Include(x => x.Terminales)
                    .IgnoreAutoIncludes().AsQueryable();

                if (!string.IsNullOrEmpty(grupo.den))
                    grupos = grupos.Where(x => x.den!.ToLower().Contains(grupo.den.ToLower()));

                return Ok(grupos);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("filtrarempresa")]
        public ActionResult Obtener_Empresa_Activa([FromQuery] Transportista transportista)
        {
            try
            {
                var transportistas = context.Transportista.IgnoreAutoIncludes().AsQueryable();

                if (!string.IsNullOrEmpty(transportista.Den))
                    transportistas = transportistas.Where(x => x.Den!.ToLower().Contains(transportista.Den.ToLower()) && x.Activo == true);

                return Ok(transportistas);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("Grupo/{grupo:int}")]
        public async Task<ActionResult> GetTransportistas(int grupo)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var transportistas = await context.Transportista
                    .Include(x => x.Terminales)
                    .Where(x => x.Codgru == grupo && x.Terminales.Any(y => y.Cod == id_terminal))
                    .OrderBy(x => x.Den)
                    .ToListAsync();
                return Ok(transportistas);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("lista")]
        public async Task<ActionResult> GetList()
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var transportistas = await context.Transportista.Where(x => x.Activo == true && x.Terminales.Any(y => y.Cod == id_terminal))
                    .OrderBy(x => x.Den)
                    .ToListAsync();
                return Ok(transportistas);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        public ActionResult Get()
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var transportistas = context.Transportista.IgnoreAutoIncludes().Where(x => x.Activo == true && x.Terminales.Any(y => y.Cod == id_terminal))
                    .Include(x => x.Terminales).IgnoreAutoIncludes()
                    .OrderBy(x => x.Den)
                    .ToList();

                return Ok(transportistas);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("all")]
        public ActionResult GetAll()
        {
            try
            {
                var transportistas = context.Transportista
                    .OrderBy(x => x.Den)
                    .ToList();
                return Ok(transportistas);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("borrar/relacion")]
        public async Task<ActionResult> Borrar_Relacion([FromBody] Transportista_Tad transportista_Tad)
        {
            try
            {
                if (transportista_Tad is null)
                    return NotFound();

                if (context.OrdenEmbarque.Include(x => x.Tonel).Any(x => x.Codtad == transportista_Tad.Id_Terminal && x.Tonel.Transportista.Cod == transportista_Tad.Id_Transportista) ||
                    context.OrdenCierre.Include(x => x.OrdenEmbarque).ThenInclude(x => x.Tonel).ThenInclude(x => x.Transportista).Any(x => x.Id_Tad == transportista_Tad.Id_Terminal && x.OrdenEmbarque!.Tonel.Transportista!.Cod == transportista_Tad.Id_Transportista)
                    || context.Orden.Include(x => x.Tonel).ThenInclude(x => x.Transportista).Any(x => x.Id_Tad == transportista_Tad.Id_Terminal && x.Tonel!.Transportista!.Cod == transportista_Tad.Id_Transportista))
                {
                    return BadRequest("Error, no puede eliminar la relación debido a pedidos u órdenes activas ligadas a esta empresa transportista y Unidad de negocio");
                }

                var id = await verifyUser.GetId(HttpContext, UserManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                context.Remove(transportista_Tad);
                await context.SaveChangesAsync();

                return Ok(transportista_Tad);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("borrar/relaciones")]
        public async Task<ActionResult> Borrar_Relaciones([FromBody] GrupoTransportista_Tad transportista_Tad)
        {
            try
            {
                if (transportista_Tad is null)
                    return NotFound();

                if (context.OrdenEmbarque.Any(x => x.Codtad == transportista_Tad.Id_Terminal) ||
                    context.OrdenCierre.Any(x => x.Id_Tad == transportista_Tad.Id_Terminal) ||
                    context.Orden.Any(x => x.Id_Tad == transportista_Tad.Id_Terminal))
                {
                    return BadRequest("Error, no puede eliminar la relación debido a pedidos u órdenes activas ligadas a este Grupo transportista y Unidad de negocio");
                }

                var id = await verifyUser.GetId(HttpContext, UserManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                context.Remove(transportista_Tad);
                await context.SaveChangesAsync();

                return Ok(transportista_Tad);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Route("service")]
        [HttpGet]
        public async Task<ActionResult> GetTransportistasService()
        {
            try
            {
                if (HttpContext.User.Identity is null)
                    return NotFound();

                if (string.IsNullOrEmpty(HttpContext.User.Identity.Name))
                    return NotFound();

                var user = await UserManager.FindByNameAsync(HttpContext.User.Identity.Name);
                if (user is null)
                    return NotFound();
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0 && !await UserManager.IsInRoleAsync(user, "Obtencion de Ordenes"))
                    return BadRequest();

                if (id_terminal != 1 && !await UserManager.IsInRoleAsync(user, "Obtencion de Ordenes"))
                    return BadRequest("No esta permitida esta accion en esta terminal");


                ServiceReference8.BusinessEntityServiceClient client = new(BusinessEntityServiceClient.EndpointConfiguration.BasicHttpBinding_BusinessEntityService);
                client.ClientCredentials.UserName.UserName = "energasws";
                client.ClientCredentials.UserName.Password = "Energas23!";
                client.Endpoint.Binding.SendTimeout = TimeSpan.FromMinutes(5);
                client.Endpoint.Binding.ReceiveTimeout = TimeSpan.FromMinutes(10);

                TruckCarrierServiceClient truck = new(TruckCarrierServiceClient.EndpointConfiguration.BasicHttpBinding_TruckCarrierService);
                truck.ClientCredentials.UserName.UserName = "energasws";
                truck.ClientCredentials.UserName.Password = "Energas23!";
                client.Endpoint.Binding.SendTimeout = TimeSpan.FromMinutes(5);
                truck.Endpoint.Binding.ReceiveTimeout = TimeSpan.FromMinutes(10);

                try
                {

                    List<Transportista> transportistas = new();
                    //ServiceReference6.BusinessEntityServiceChannel svc = svcTruck.CreateChannel();
                    var svc = client.ChannelFactory.CreateChannel();
                    //Conexión a WebService para obtener el transportista
                    WsGetBusinessEntityAssociationsRequest getReq = new();

                    getReq.IncludeChildObjects = new ServiceReference8.NBool();
                    getReq.IncludeChildObjects.Value = true;

                    getReq.BusinessEntityType = new ServiceReference8.NInt();
                    getReq.BusinessEntityType.Value = 8;//truck carrier

                    getReq.AssociatedBusinessEntityId = new ServiceReference8.Identifier();
                    getReq.AssociatedBusinessEntityId.Id = new ServiceReference8.NLong();
                    getReq.AssociatedBusinessEntityId.Id.Value = 51004;//energas

                    getReq.AssociatedBusinessEntityType = new ServiceReference8.NInt();
                    getReq.AssociatedBusinessEntityType.Value = 1;

                    //toFile.GenerateFile(JsonConvert.SerializeObject(getReq), $"Request_Transportistas_{DateTime.Now.ToString("ddMMyyyyHHmmss")}", $"{DateTime.Now.ToString("ddMMyyyy")}");

                    var respuesta = await svc.GetBusinessEntityAssociationsAsync(getReq);

                    //toFile.GenerateFile(JsonConvert.SerializeObject(respuesta), $"Response_Transportistas_{DateTime.Now.ToString("ddMMyyyyHHmmss")}", $"{DateTime.Now.ToString("ddMMyyyy")}");

                    //Conexion a WebService para obtener carrId del transportista 
                    WsGetTruckCarriersRequest truckRequest = new();

                    //toFile.GenerateFile(JsonConvert.SerializeObject(truckRequest), $"Request_Transportistas_ID_{DateTime.Now.ToString("ddMMyyyyHHmmss")}", $"{DateTime.Now.ToString("ddMMyyyy")}");

                    var truckResponse = await truck.GetTruckCarriersAsync(truckRequest);

                    //toFile.GenerateFile(JsonConvert.SerializeObject(truckResponse), $"Response_Transportistas_ID_{DateTime.Now.ToString("ddMMyyyyHHmmss")}", $"{DateTime.Now.ToString("ddMMyyyy")}");


                    foreach (var item in respuesta.BusinessEntityAssociations)
                    {
                        if (!string.IsNullOrEmpty(item.BusinessEntity.BusinessEntityName) && !string.IsNullOrWhiteSpace(item.BusinessEntity.BusinessEntityName))
                        {
                            var carrid = truckResponse.TruckCarriers.FirstOrDefault(x => x.BusinessEntityId.Id.Value == item.BusinessEntity.BusinessEntityId.Id.Value);
                            //Creacion del objeto del transportista
                            Transportista transportista = new()
                            {
                                Den = item.BusinessEntity.BusinessEntityName,
                                Busentid = item.BusinessEntity.BusinessEntityId.Id.Value.ToString(),
                                Activo = item.BusinessEntity.ActiveIndicator.Value == ServiceReference8.ActiveIndicatorEnum.ACTIVE ? true : false,
                                CarrId = carrid == null ? string.Empty : carrid.CarrierId.Id.Value.ToString(),
                                Id_Tad = 1
                            };
                            //Si el transportista esta activo 
                            if (transportista.Activo == true)
                            {
                                Transportista? t = context.Transportista.Where(x => x.Busentid == transportista.Busentid && x.Id_Tad == 1)
                                    .DefaultIfEmpty()
                                    .FirstOrDefault();
                                //Si el transportista no es nulo 
                                if (t != null)
                                {
                                    //Lo actualiza
                                    t.Den = transportista.Den;
                                    t.Activo = transportista.Activo;
                                    t.CarrId = string.IsNullOrEmpty(transportista.CarrId) ? string.Empty : transportista.CarrId;
                                    context.Update(t);

                                    if (!context.Transportista_Tad.Any(x => x.Id_Transportista == t.Cod))
                                    {
                                        Transportista_Tad _Tad = new()
                                        {
                                            Id_Terminal = 1,
                                            Id_Transportista = t.Cod,
                                            Terminal = null,
                                            Transportista = null
                                        };
                                        context.Add(_Tad);
                                    }
                                }
                                else
                                {
                                    //Agrega un nuevo transportista 
                                    // context.Add(transportista);

                                    Transportista_Tad _Tad = new()
                                    {
                                        Id_Terminal = 1,
                                        Terminal = null,
                                        Transportista = transportista
                                    };

                                    context.Add(_Tad);
                                }
                            }
                            else
                            {
                                //Actualiza el campo de activo 
                                var cod = context.Transportista.Where(x => x.Busentid == transportista.Busentid && string.IsNullOrEmpty(x.CarrId) && x.Id_Tad == 1).DefaultIfEmpty().FirstOrDefault();
                                if (cod != null)
                                {
                                    var tinactivo = context.Transportista.FirstOrDefault(x => x.Cod == cod.Cod && x.Id_Tad == 1);
                                    if (tinactivo != null)
                                    {
                                        tinactivo.Activo = false;
                                        context.Update(tinactivo);
                                    }
                                }
                            }
                        }
                    }

                    //Guarda los cambios
                    await context.SaveChangesAsync();

                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }
                finally
                {
                    //svcTruck.Close();
                    client.Close();
                }

                return Ok(true);
            }
            catch (Exception e)
            {
                await SaveErrors(e);
                return BadRequest(e.Message);
            }
        }
    }

}