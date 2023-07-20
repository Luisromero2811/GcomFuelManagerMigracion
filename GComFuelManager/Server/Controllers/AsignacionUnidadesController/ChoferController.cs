using System;
using Microsoft.AspNetCore.Mvc;
using GComFuelManager.Shared.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Diagnostics;
using System.ServiceModel;
using ServiceReference6;
using ServiceReference3;
using GComFuelManager.Shared.Modelos;
using Newtonsoft.Json;

namespace GComFuelManager.Server.Controllers.AsignacionUnidadesController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ChoferController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public ChoferController(ApplicationDbContext context)
        {
            this.context = context;
        }
        [HttpGet("{transportista:int}")]
        public async Task<ActionResult> Get(int transportista)
        {
            try
            {
                var transportistas = await context.Chofer
                    .Where(x => x.Codtransport == transportista && x.Activo == true)
                    //.Select(x => new CodDenDTO { Cod = x.Cod, Den = x.Den! })
                    .OrderBy(x => x.Den)
                    .ToListAsync();
                return Ok(transportistas);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Route("service/{code:long}")]
        [HttpGet]
        public async Task<ActionResult> GetChofer([FromRoute] long code)
        {
            try
            {
                BusinessEntityServiceClient client = new BusinessEntityServiceClient(BusinessEntityServiceClient.EndpointConfiguration.BasicHttpBinding_BusinessEntityService);
                client.ClientCredentials.UserName.UserName = "energasws";
                client.ClientCredentials.UserName.Password = "Energas23!";
                client.Endpoint.Binding.SendTimeout = TimeSpan.FromMinutes(10);
                client.Endpoint.Binding.ReceiveTimeout = TimeSpan.FromMinutes(5);

                try
                {

                    var svc = client.ChannelFactory.CreateChannel();

                    WsGetBusinessEntityAssociationsRequest getReq = new WsGetBusinessEntityAssociationsRequest();

                    getReq.IncludeChildObjects = new ServiceReference6.NBool();
                    getReq.IncludeChildObjects.Value = false;

                    getReq.BusinessEntityType = new ServiceReference6.NInt();
                    getReq.BusinessEntityType.Value = 8192;

                    getReq.AssociatedBusinessEntityType = new ServiceReference6.NInt();
                    getReq.AssociatedBusinessEntityType.Value = 8;

                    getReq.ActiveIndicator = new ServiceReference6.NEnumOfActiveIndicatorEnum();
                    getReq.ActiveIndicator.Value = ServiceReference6.ActiveIndicatorEnum.ACTIVE;

                    getReq.AssociatedBusinessEntityId = new ServiceReference6.Identifier();
                    getReq.AssociatedBusinessEntityId.Id = new ServiceReference6.NLong();
                    getReq.AssociatedBusinessEntityId.Id.Value = code;

                    var respuesta = await svc.GetBusinessEntityAssociationsAsync(getReq);

                    string busEntTyp, busEntTyp2;

                    if (respuesta.BusinessEntityAssociations != null)
                    {
                        foreach (var item in respuesta.BusinessEntityAssociations)
                        {
                            busEntTyp = item.BusinessEntity.BusinessEntityType.Value.ToString();
                            busEntTyp2 = item.AssociatedBusinessEntity.BusinessEntityType.Value.ToString();

                            if (busEntTyp == "TRUCK_DRIVER")
                            {

                                //Creacion del objeto del chofer
                                Chofer chofer = new Chofer()
                                {
                                    Den = item.BusinessEntity.BusinessEntityName,
                                    Shortden = item.BusinessEntity.BusinessEntityShortName,
                                    Codtransport = Convert.ToInt32(code),
                                    Dricod = item.BusinessEntity.BusinessEntityId.Id.Value.ToString(),
                                    Activo = item.BusinessEntity.ActiveIndicator.Value == ServiceReference6.ActiveIndicatorEnum.ACTIVE ? true : false,
                                };
                                //Obtención del code del chofer
                                Chofer? c = context.Chofer.Where(x => x.Den == chofer.Den && x.Codtransport == chofer.Codtransport && x.Dricod == chofer.Dricod)
                                    .DefaultIfEmpty()
                                    .FirstOrDefault();
                                //Condicion de si el chofer esta activo dentro condicionando si el chofer existe sino activas campo de activo
                                if (chofer.Activo == true)
                                {
                                    if (c != null)
                                    {
                                        c.Den = chofer.Den;
                                        c.Shortden = chofer.Shortden;
                                        //c.Codtransport = chofer.Codtransport;
                                        c.Dricod = chofer.Dricod;
                                        c.Activo = chofer.Activo;
                                        context.Update(c);
                                    }
                                    else
                                    {
                                        //Agrega al nuevo chofer
                                        context.Add(chofer);
                                    }
                                }
                                else
                                {
                                    //Actualizamos el campo activo del chofer
                                    var cod = context.Chofer.Where(x => x.Cod == chofer.Cod)
                                        .DefaultIfEmpty()
                                        .FirstOrDefault();
                                    if (cod != null)
                                    {
                                        cod.Activo = false;
                                        context.Update(cod);
                                    }
                                }
                            }
                            if (busEntTyp2 == "TRUCK_DRIVER")
                            {

                                //Creacion del objeto del chofer
                                Chofer chofer = new Chofer()
                                {
                                    Den = item.AssociatedBusinessEntity.BusinessEntityName,
                                    Shortden = item.AssociatedBusinessEntity.BusinessEntityShortName,
                                    Codtransport = Convert.ToInt32(code),
                                    Dricod = item.AssociatedBusinessEntity.BusinessEntityId.Id.Value.ToString(),
                                    Activo = item.AssociatedBusinessEntity.ActiveIndicator.Value == ServiceReference6.ActiveIndicatorEnum.ACTIVE ? true : false,
                                };
                                //Obtención del code del chofer
                                Chofer? c = context.Chofer.Where(x => x.Den == chofer.Den && x.Codtransport == chofer.Codtransport && x.Dricod == chofer.Dricod)
                                    .DefaultIfEmpty()
                                    .FirstOrDefault();
                                //Condicion de si el chofer esta activo dentro condicionando si el chofer existe sino activas campo de activo
                                if (chofer.Activo == true)
                                {
                                    if (c != null)
                                    {
                                        c.Den = chofer.Den;
                                        c.Shortden = chofer.Shortden;
                                        //c.Codtransport = chofer.Codtransport;
                                        c.Dricod = chofer.Dricod;
                                        c.Activo = chofer.Activo;
                                        context.Update(c);
                                    }
                                    else
                                    {
                                        //Agrega al nuevo chofer
                                        context.Add(chofer);
                                    }
                                }
                                else
                                {
                                    //Actualizamos el campo activo del chofer
                                    var cod = context.Chofer.Where(x => x.Cod == chofer.Cod)
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
                        await context.SaveChangesAsync();
                        return Ok(true);
                    }
                    else
                        return BadRequest("No se encontraron operarios para este transportista");
                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }
                finally
                {
                    client.Close();
                }
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("service/all")]
        public async Task<ActionResult> GetAllChofers()
        {
            try
            {
                var i = 1;
                var transportistas = context.Transportista.Where(x=>x.Activo == true).ToList();
                foreach (var item in transportistas)
                {
                    long busentid;
                    if (long.TryParse(item.Busentid, out busentid))
                    {
                        Debug.WriteLine($"{i}:{item.Den}");
                        await GetChofer(busentid);
                        i++;
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

