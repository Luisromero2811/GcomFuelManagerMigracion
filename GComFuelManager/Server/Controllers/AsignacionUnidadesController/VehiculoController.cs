using System;
using Microsoft.AspNetCore.Mvc;
using GComFuelManager.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Diagnostics;
using System.ServiceModel;
using ServiceReference1;
using Newtonsoft.Json;
using GComFuelManager.Shared.Modelos;
using System.Numerics;
using Microsoft.AspNetCore.Identity;
using GComFuelManager.Server.Identity;
using GComFuelManager.Server.Helpers;

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

        public VehiculoController(ApplicationDbContext context,UserManager<IdentityUsuario> UserManager, VerifyUserId verifyUser)
        {
            this.context = context;
            this.UserManager = UserManager;
            this.verifyUser = verifyUser;
        }
        [HttpGet("{transportista:int}")]
        public async Task<ActionResult> Get(int transportista)
        {
            try
            {
                var vehiculos = await context.Tonel
                    .Where(x => Convert.ToInt32(x.Carid) == transportista && x.Activo == true)
                    //.Select(x => new CodDenDTO { Cod = x.Cod, Den =  $"{x.Tracto} {x.Placatracto} {x.Placa} {x.Capcom!} {x.Capcom2!} {x.Capcom3!} {x.Capcom4!} {x.Codsyn!}" })
                    .OrderBy(x => x.Tracto)
                    .ToListAsync();
                return Ok(vehiculos);
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

                VehicleServiceClient client = new VehicleServiceClient(VehicleServiceClient.EndpointConfiguration.BasicHttpBinding_VehicleService);
                client.ClientCredentials.UserName.UserName = "energasws";
                client.ClientCredentials.UserName.Password = "Energas23!";
                client.Endpoint.Binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
                client.Endpoint.Binding.SendTimeout = TimeSpan.FromMinutes(5);

                try
                {
                    var svc = client.ChannelFactory.CreateChannel();

                    WsGetVehiclesRequest getReq = new WsGetVehiclesRequest();

                    getReq.IncludeChildObjects = new ServiceReference1.NBool();
                    getReq.IncludeChildObjects.Value = true;

                    getReq.SystemId = new ServiceReference1.Identifier();
                    getReq.SystemId.Id = new ServiceReference1.NLong();
                    getReq.SystemId.Id.Value = 100;

                    getReq.VehicleType = new ServiceReference1.NInt();
                    getReq.VehicleType.Value = 4;

                    getReq.ActiveInd = new ServiceReference1.NEnumOfActiveIndicatorEnum();
                    getReq.ActiveInd.Value = ServiceReference1.ActiveIndicatorEnum.ACTIVE;

                    getReq.IncludeVehicleCompartments = new ServiceReference1.NBool();
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
                                Tonel tonel = new Tonel()
                                {
                                    Placa = item.RegistrationNumber.Trim(),
                                    Tracto = item.VehicleCode.Trim(),
                                    //Placatracto = item.RfiTagId.Trim(),
                                    Placatracto = item.RfiTagId != null ? item.RfiTagId.Trim() : "",
                                    Codsyn = Convert.ToInt32(item.VehicleId.Id.Value),
                                    Carid = item.CarrierId.Id.Value.ToString()
                                };
                                //Obtenemos el code del tonel
                                Tonel? t = context.Tonel.Where(x => x.Placa == tonel.Placa && x.Tracto == tonel.Tracto && x.Placatracto == tonel.Placatracto && x.Codsyn == tonel.Codsyn && x.Carid == tonel.Carid)
                                    .DefaultIfEmpty()
                                    .FirstOrDefault();
                                //Si el tonel esta activo
                                if (tonel.Activo == true)
                                {
                                    //Si el tonel no es nulo lo actualimos
                                    if (t != null)
                                    {
                                        t.Placa = item.RegistrationNumber.Trim();
                                        t.Tracto = item.VehicleCode.Trim();
                                        //t.Placatracto = item.RfiTagId.Trim();
                                        t.Placatracto = item.RfiTagId != null ? item.RfiTagId.Trim() : "";
                                        t.Codsyn = Convert.ToInt32(item.VehicleId.Id.Value);
                                        t.Carid = carrId.ToString();

                                        foreach (var com in item.VehicleCompartments)
                                        {
                                            if (com.CompartmentId.Id.Value == 1)
                                            {
                                                t.Idcom = Convert.ToInt32(com.CompartmentId.Id.Value);
                                                t.Nrocom = Convert.ToInt32(com.CompartmentNumber.Value);
                                                t.Capcom = com.Capacity.Value;
                                            }
                                            if (com.CompartmentId.Id.Value == 2)
                                            {
                                                t.Idcom2 = Convert.ToInt32(com.CompartmentId.Id.Value);
                                                t.Nrocom2 = Convert.ToInt32(com.CompartmentNumber.Value);
                                                t.Capcom2 = com.Capacity.Value;
                                            }
                                            if (com.CompartmentId.Id.Value == 3)
                                            {
                                                t.Idcom3 = Convert.ToInt32(com.CompartmentId.Id.Value);
                                                t.Nrocom3 = Convert.ToInt32(com.CompartmentNumber.Value);
                                                t.Capcom3 = com.Capacity.Value;
                                            }
                                            if (com.CompartmentId.Id.Value == 4)
                                            {
                                                t.Idcom4 = Convert.ToInt32(com.CompartmentId.Id.Value);
                                                t.Nrocom4 = Convert.ToInt32(com.CompartmentNumber.Value);
                                                t.Capcom4 = Convert.ToInt32(com.Capacity.Value);
                                            }
                                        }

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
                                else
                                {
                                    //Actualizamos el campo activo del tonel
                                    var cod = context.Tonel.Where(x => x.Codsyn == tonel.Codsyn)
                                        .DefaultIfEmpty()
                                        .FirstOrDefault();
                                    if (cod != null)
                                    {
                                        cod.Activo = false;
                                        context.Update(cod);
                                    }
                                }
                            }

                        }

                        var id = await verifyUser.GetId(HttpContext, UserManager);
                        if (string.IsNullOrEmpty(id))
                            return BadRequest();

                        await context.SaveChangesAsync(id, 14);
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

                await context.SaveChangesAsync(id,7);

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

                if(tonel.Capacidades.Count > 0)
                {
                    if(tonel.Capacidades.Count >= 1)
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
