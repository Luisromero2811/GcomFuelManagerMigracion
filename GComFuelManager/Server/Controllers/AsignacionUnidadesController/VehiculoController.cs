using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
//using ServiceReference1;//qa
using ServiceReference9;//prod

namespace GComFuelManager.Server.Controllers.AsignacionUnidadesController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class VehiculoController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUsuario> UserManager;
        private readonly VerifyUserId verifyUser;
        private readonly User_Terminal _terminal;

        public VehiculoController(ApplicationDbContext context, UserManager<IdentityUsuario> UserManager, VerifyUserId verifyUser, User_Terminal _Terminal)
        {
            this.context = context;
            this.UserManager = UserManager;
            this.verifyUser = verifyUser;
            this._terminal = _Terminal;
        }
        [HttpGet("{transportista:int}")]
        public ActionResult Get(int transportista)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var vehiculos = context.Tonel.IgnoreAutoIncludes()
                    .Where(x => Convert.ToInt32(x.Carid) == transportista && x.Activo == true && x.Terminales.Any(y => y.Cod == id_terminal))
                    .Include(x => x.Terminales)
                    .OrderBy(x => x.Tracto)
                    .ToList();
                return Ok(vehiculos);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("getAll")]
        public async Task<ActionResult> GetUnidades()
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var vehiculos = context.Tonel.IgnoreAutoIncludes()
                    .Where(x => x.Activo == true && x.Terminales.Any(y => y.Cod == id_terminal))
                    .Include(x => x.Terminales)
                    .OrderBy(x => x.Tracto)
                    .ToList();
                return Ok(vehiculos);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("gestion")]
        public async Task<ActionResult> GetTonel([FromQuery] ParametrosBusquedaCatalogo transportista)
        {
            try
            {
                var vehiculos = context.Tonel.IgnoreAutoIncludes()
                    .Include(x => x.Terminales)
                    .Where(x => Convert.ToInt32(x.Carid) == transportista.carrid)
                    .Include(x => x.Terminales)
                    .OrderBy(x => x.Tracto)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(transportista.placatonel))
                    vehiculos = vehiculos.Where(x => x.Placa != null && !string.IsNullOrEmpty(x.Placa) && x.Placa.ToLower().Contains(transportista.placatonel.ToLower()));

                return Ok(vehiculos);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        [HttpGet("filtraractivos")]
        public ActionResult Obtener_Grupos_Activos([FromQuery] Tonel tonel)
        {
            try
            {
                var toneles = context.Tonel.IgnoreAutoIncludes().AsQueryable();

                if (!string.IsNullOrEmpty(tonel.Placa))
                    toneles = toneles.Where(x => x.Placa!.ToLower().Contains(tonel.Placa.ToLower()) && x.Activo == true);

                return Ok(toneles);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("crearUnidad")]
        public async Task<ActionResult> PostVehiculos([FromBody] Tonel tonel)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                if (tonel is null)
                    return BadRequest();

                if (tonel.Cod == 0)
                {
                    tonel.Id_Tad = id_terminal;
                    //tonel.Carid = Convert.ToString(tonel.Transportista!.CarrId);
                    tonel.Transportista = null!;
                    //var exist = context.Tonel.Any(x => x.Certificado_Calibracion == tonel.Certificado_Calibracion);
                    ////Si ya existe, genera un nuevo número Random
                    //if (exist)
                    //{
                    //    return BadRequest("El certificado de calibración ya existe, por favor ingrese otro identificador");
                    //}
                    //tonel.Carid = tonel.Transportista.CarrId;
                    context.Add(tonel);
                    await context.SaveChangesAsync();
                    if (!context.Unidad_Tad.Any(x => x.Id_Terminal == id_terminal && x.Id_Unidad == tonel.Cod))
                    {
                        Unidad_Tad tonelTad = new()
                        {
                            Id_Unidad = tonel.Cod,
                            Id_Terminal = id_terminal
                        };
                        context.Add(tonelTad);
                        await context.SaveChangesAsync();
                    }
                }
                else
                {
                    tonel.Id_Tad = id_terminal;
                    //if (context.Tonel.Any(x => x.Certificado_Calibracion != tonel.Certificado_Calibracion))
                    //{
                    //    //Con Any compruebo si el número aleatorio existe en la BD
                    //    var exist = context.Tonel.Any(x => x.Certificado_Calibracion == tonel.Certificado_Calibracion && x.Gps != tonel.Gps);
                    //    //Si ya existe, genera un nuevo número Random
                    //    if (exist)
                    //    {
                    //        return BadRequest("El certificado de calibración ya existe, por favor ingrese otro identificador");
                    //    }
                    //}
                    //else
                    //{
                    //    return BadRequest("El certificado de calibración ya existe, por favor ingrese otro identificador");
                    //}
                    tonel.Terminales = null!;
                    context.Update(tonel);
                    await context.SaveChangesAsync();
                }

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("{cod:int}")]
        public async Task<ActionResult> ChangeStatus([FromRoute] int cod, [FromBody] bool status)
        {
            try
            {
                if (cod == 0)
                    return BadRequest();

                var destino = context.Tonel.Where(x => x.Cod == cod).FirstOrDefault();
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
                    foreach (var tonel in clienteTadDTO.Toneles)
                    {
                        if (!context.Unidad_Tad.Any(x => x.Id_Terminal == terminal.Cod && x.Id_Unidad == tonel.Cod))
                        {
                            Unidad_Tad tonelTad = new()
                            {
                                Id_Unidad = tonel.Cod,
                                Id_Terminal = terminal.Cod
                            };
                            context.Add(tonelTad);
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

        [HttpPost("borrar/relacion")]
        public async Task<ActionResult> Borrar_Relacion([FromBody] Unidad_Tad tonel_tad)
        {
            try
            {
                if (tonel_tad is null)
                    return NotFound();

                if (context.OrdenEmbarque.Any(x => x.Codtad == tonel_tad.Id_Terminal && x.Codton == tonel_tad.Id_Unidad) ||
                    context.Orden.Any(x => x.Id_Tad == tonel_tad.Id_Terminal && x.Coduni == tonel_tad.Id_Unidad)
                    || context.OrdenCierre.Include(x => x.OrdenEmbarque).Any(x => x.Id_Tad == tonel_tad.Id_Terminal && x.OrdenEmbarque.Tonel.Cod == tonel_tad.Id_Unidad))
                {
                    return BadRequest("Error, no puede borrar esta relación debido a pedidos u órdenes activas relacionadas a este Vehículo y Unidad de Negocio");
                }

                var id = await verifyUser.GetId(HttpContext, UserManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                context.Remove(tonel_tad);
                await context.SaveChangesAsync();

                return Ok(tonel_tad);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Route("service/{carrId:long}")]
        [HttpGet]
        public async Task<ActionResult> GetVehiculo([FromRoute] long carrId)
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


                List<Tonel> TonelesActivos = new();
                VehicleServiceClient client = new(VehicleServiceClient.EndpointConfiguration.BasicHttpBinding_VehicleService);
                client.ClientCredentials.UserName.UserName = "energasws";
                client.ClientCredentials.UserName.Password = "Energas23!";
                client.Endpoint.Binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
                client.Endpoint.Binding.SendTimeout = TimeSpan.FromMinutes(10);

                try
                {
                    var svc = client.ChannelFactory.CreateChannel();

                    WsGetVehiclesRequest getReq = new();

                    getReq.IncludeChildObjects = new NBool();
                    getReq.IncludeChildObjects.Value = true;

                    getReq.SystemId = new Identifier();
                    getReq.SystemId.Id = new NLong();
                    getReq.SystemId.Id.Value = 100;

                    getReq.VehicleType = new NInt();
                    getReq.VehicleType.Value = 4;

                    getReq.ActiveInd = new NEnumOfActiveIndicatorEnum();
                    getReq.ActiveInd.Value = ActiveIndicatorEnum.ACTIVE;

                    getReq.IncludeVehicleCompartments = new NBool();
                    getReq.IncludeVehicleCompartments.Value = true;

                    var respuesta = await svc.GetVehiclesAsync(getReq);

                    //Si la respuesta no es nula, si existe respuesta
                    if (respuesta.Vehicles != null)
                    {
                        foreach (var item in respuesta.Vehicles)
                        {
                            //carrId = item.CarrierId.Id.Value;
                            if (carrId == item.CarrierId.Id.Value)
                            {
                                //Creamos el objeto del Tonel
                                Tonel tonel = new()
                                {
                                    Placa = item.RegistrationNumber.Trim(),
                                    Tracto = item.VehicleCode.Trim(),
                                    //Placatracto = item.RfiTagId.Trim(),
                                    Placatracto = item.RfiTagId != null ? item.RfiTagId.Trim() : "",
                                    Codsyn = Convert.ToInt32(item.VehicleId.Id.Value),
                                    Carid = item.CarrierId.Id.Value.ToString(),
                                    Id_Tad = 1
                                };
                                //Obtenemos el code del tonel
                                Tonel? t = context.Tonel.Where(x => x.Placa == tonel.Placa && x.Tracto == tonel.Tracto && x.Placatracto == tonel.Placatracto && x.Codsyn == tonel.Codsyn && x.Carid == tonel.Carid
                                && x.Id_Tad == 1)
                                    .DefaultIfEmpty()
                                    .FirstOrDefault();

                                //Si el tonel no es nulo lo actualimos
                                if (t != null)
                                {
                                    t.Placa = item.RegistrationNumber.Trim();
                                    t.Tracto = item.VehicleCode.Trim();
                                    //t.Placatracto = item.RfiTagId.Trim();
                                    t.Placatracto = item.RfiTagId != null ? item.RfiTagId.Trim() : "";
                                    t.Codsyn = Convert.ToInt32(item.VehicleId.Id.Value);
                                    t.Carid = carrId.ToString();
                                    t.Activo = true;

                                    foreach (var com in item.VehicleCompartments)
                                    {
                                        if (com.CompartmentNumber.Value == 1)
                                        {
                                            t.Idcom = Convert.ToInt32(com.CompartmentId.Id.Value);
                                            t.Nrocom = Convert.ToInt32(com.CompartmentNumber.Value);
                                            t.Capcom = com.Capacity.Value;
                                        }
                                        if (com.CompartmentNumber.Value == 2)
                                        {
                                            t.Idcom2 = Convert.ToInt32(com.CompartmentId.Id.Value);
                                            t.Nrocom2 = Convert.ToInt32(com.CompartmentNumber.Value);
                                            t.Capcom2 = com.Capacity.Value;
                                        }
                                        if (com.CompartmentNumber.Value == 3)
                                        {
                                            t.Idcom3 = Convert.ToInt32(com.CompartmentId.Id.Value);
                                            t.Nrocom3 = Convert.ToInt32(com.CompartmentNumber.Value);
                                            t.Capcom3 = com.Capacity.Value;
                                        }
                                        if (com.CompartmentNumber.Value == 4)
                                        {
                                            t.Idcom4 = Convert.ToInt32(com.CompartmentId.Id.Value);
                                            t.Nrocom4 = Convert.ToInt32(com.CompartmentNumber.Value);
                                            t.Capcom4 = Convert.ToInt32(com.Capacity.Value);
                                        }
                                    }
                                    TonelesActivos.Add(t);
                                    context.Update(t);
                                }
                                //Sino le agregamos un nuevo tonel
                                else
                                {
                                    foreach (var com in item.VehicleCompartments)
                                    {
                                        if (com.CompartmentNumber.Value == 1)
                                        {
                                            tonel.Idcom = Convert.ToInt32(com.CompartmentId.Id.Value);
                                            tonel.Nrocom = Convert.ToInt32(com.CompartmentNumber.Value);
                                            tonel.Capcom = com.Capacity.Value;
                                        }
                                        if (com.CompartmentNumber.Value == 2)
                                        {
                                            tonel.Idcom2 = Convert.ToInt32(com.CompartmentId.Id.Value);
                                            tonel.Nrocom2 = Convert.ToInt32(com.CompartmentNumber.Value);
                                            tonel.Capcom2 = com.Capacity.Value;
                                        }
                                        if (com.CompartmentNumber.Value == 3)
                                        {
                                            tonel.Idcom3 = Convert.ToInt32(com.CompartmentId.Id.Value);
                                            tonel.Nrocom3 = Convert.ToInt32(com.CompartmentNumber.Value);
                                            tonel.Capcom3 = com.Capacity.Value;
                                        }
                                        if (com.CompartmentNumber.Value == 4)
                                        {
                                            tonel.Idcom4 = Convert.ToInt32(com.CompartmentId.Id.Value);
                                            tonel.Nrocom4 = Convert.ToInt32(com.CompartmentNumber.Value);
                                            tonel.Capcom4 = Convert.ToInt32(com.Capacity.Value);
                                        }
                                    }
                                    context.Add(tonel);
                                }
                            }

                        }

                        List<Tonel> toneles = context.Tonel.Where(x => !string.IsNullOrEmpty(x.Carid) && x.Carid.Equals(carrId.ToString()) && x.Id_Tad == 1).ToList();

                        toneles.ForEach(x =>
                        {
                            if (!TonelesActivos.Contains(x))
                                x.Activo = false;
                        });

                        context.UpdateRange(toneles);

                        await context.SaveChangesAsync();

                        return Ok(true);
                    }
                    else
                    {
                        return BadRequest("No se encontraron vehiculos para este transportista");
                    }
                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }
                finally
                {
                    client.Close();
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Put([FromBody] Tonel tonel)
        {
            try
            {
                if (tonel is null)
                    return BadRequest();

                context.Update(tonel);

                var id = await verifyUser.GetId(HttpContext, UserManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                await context.SaveChangesAsync(id, 7);

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("compartimientos/{cod:int}")]
        public async Task<ActionResult> PutCompartimientos([FromRoute] int cod, [FromBody] List<CapTonel> capTonels)
        {
            try
            {
                var tonel = context.Tonel.Find(cod);
                if (tonel is null)
                    return BadRequest();

                if (tonel.Capacidades.Count > 0)
                {
                    if (tonel.Capacidades.Count >= 1)
                    {
                        tonel.Capcom = capTonels[0].CapCom;
                        tonel.Nrocom = capTonels[0].NroCom;
                        tonel.Idcom = capTonels[0].IdCom;
                    }

                    if (tonel.Capacidades.Count >= 2)
                    {
                        tonel.Capcom2 = capTonels[1].CapCom;
                        tonel.Nrocom2 = capTonels[1].NroCom;
                        tonel.Idcom2 = capTonels[1].IdCom;
                    }

                    if (tonel.Capacidades.Count >= 3)
                    {
                        tonel.Capcom3 = capTonels[2].CapCom;
                        tonel.Nrocom3 = capTonels[2].NroCom;
                        tonel.Idcom3 = capTonels[2].IdCom;
                    }

                    if (tonel.Capacidades.Count >= 4)
                    {
                        tonel.Capcom4 = Convert.ToInt32(capTonels[3].CapCom);
                        tonel.Nrocom4 = capTonels[3].NroCom;
                        tonel.Idcom4 = capTonels[3].IdCom;
                    }
                }
                context.Update(tonel);

                var id = await verifyUser.GetId(HttpContext, UserManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                await context.SaveChangesAsync(id, 7);

                return Ok(tonel);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
