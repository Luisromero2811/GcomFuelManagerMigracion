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

namespace GComFuelManager.Server.Controllers.AsignacionUnidadesController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class VehiculoController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public VehiculoController(ApplicationDbContext context)
        {
            this.context = context;
        }
        [HttpGet("{transportista:int}")]
        public async Task<ActionResult> Get(int transportista)
        {
            try
            {
                var vehiculos = await context.Tonel
                    .Where(x => Convert.ToInt32(x.Carid) == transportista && x.Activo == true)
                    //.Select(x => new CodDenDTO { Cod = x.Cod, Den =  $"{x.Tracto} {x.Placatracto} {x.Placa} {x.Capcom!} {x.Capcom2!} {x.Capcom3!} {x.Capcom4!} {x.Codsyn!}" })
                    .OrderBy(x => x.Placa)
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

                try
                {
                    var svc = client.ChannelFactory.CreateChannel();

                    WsGetVehiclesRequest getReq = new WsGetVehiclesRequest();

                    //Pendientes

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

                    Debug.WriteLine(JsonConvert.SerializeObject(respuesta));
                    //long carrId;

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
                                Debug.WriteLine($"activo: {tonel.Carid}");
                                    //Si el tonel no es nulo lo actualimos
                                    if (t != null)
                                    {
                                    Debug.WriteLine($"activo: {t.Cod}");
                                        t.Placa = item.RegistrationNumber.Trim();
                                        t.Tracto = item.VehicleCode.Trim();
                                        //t.Placatracto = item.RfiTagId.Trim();
                                        t.Placatracto = item.RfiTagId != null ? item.RfiTagId.Trim() : "";
                                        t.Codsyn = Convert.ToInt32(item.VehicleId.Id.Value);
                                        t.Carid = carrId.ToString();
                                        context.Update(t);
                                    }
                                    //Sino le agregamos un nuevo tonel
                                    else
                                    {
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
    }
}
//BusinessEntityService.WsGetBusinessEntityAssociationsRequest be = new BusinessEntityService.WsGetBusinessEntityAssociationsRequest();