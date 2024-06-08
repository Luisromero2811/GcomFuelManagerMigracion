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
using OfficeOpenXml;

using ServiceReference7; //prod 
//using ServiceReference2; //qa
using System;
using System.Diagnostics;
using System.Transactions;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;

namespace GComFuelManager.Server.Controllers.Services
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Obtencion de Ordenes, Admin, Administrador Sistema, Programador")]
    public class ServicesController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly VerifyUserToken verify;
        private readonly RequestToFile toFile;
        private readonly UserManager<IdentityUsuario> userManager;
        private readonly User_Terminal _terminal;

        public ServicesController(ApplicationDbContext context, VerifyUserToken verify, RequestToFile toFile, UserManager<IdentityUsuario> userManager, User_Terminal _Terminal)
        {
            this.context = context;
            this.verify = verify;
            this.toFile = toFile;
            this.userManager = userManager;
            _terminal = _Terminal;
        }

        private async Task SaveErrors(Exception e, string accion)
        {
            context.Add(new Errors()
            {
                Error = JsonConvert.SerializeObject(new Error()
                {
                    Inner = JsonConvert.SerializeObject(e.InnerException),
                    Message = JsonConvert.SerializeObject(e.Message),
                    StackTrace = JsonConvert.SerializeObject(e.StackTrace)
                }),
                Accion = accion
            });
            await context.SaveChangesAsync();
        }

        #region envio de synthesis

        [HttpPost("send"), Authorize(Roles = "Programador, Admin, Administrador Sistema")]
        public async Task<ActionResult> SendSynthesis([FromBody] List<OrdenEmbarque> ordens)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();
                if (id_terminal == 1)
                {
                    List<OrdenEmbarque> OrdenesEnviadas = new List<OrdenEmbarque>();
                    List<OrdenEmbarque> ordenesEmbarque = new List<OrdenEmbarque>();
                    BillOfLadingServiceClient client = new BillOfLadingServiceClient(BillOfLadingServiceClient.EndpointConfiguration.BasicHttpBinding_BillOfLadingService2);
                    client.ClientCredentials.UserName.UserName = "energasws";
                    client.ClientCredentials.UserName.Password = "Energas23!";
                    client.Endpoint.Binding.SendTimeout = TimeSpan.FromMinutes(15);
                    client.Endpoint.Binding.ReceiveTimeout = TimeSpan.FromMinutes(15);

                    if (ordens is null)
                    {
                        return BadRequest();
                    }
                    else
                    {

                        foreach (var item in ordens)
                        {

                            var bolguid = context.OrdenEmbarque.Find(item.Cod);

                            bolguid ??= ordens.First(x => x.Cod == item.Cod);
                            //    return BadRequest();

                            if (string.IsNullOrEmpty(bolguid.Bolguidid))
                            {

                                WsSaveBillOfLadingRequest request = new WsSaveBillOfLadingRequest();

                                request.BillOfLading = new BillOfLading();

                                request.EnableExplicitRevisioning = new NBool();
                                request.EnableExplicitRevisioning.Value = true;

                                request.GenerateBolFromOrder = new NBool();
                                request.GenerateBolFromOrder.Value = false;

                                request.BillOfLading.DeliveryReceiptIndicator = new NInt();
                                request.BillOfLading.DeliveryReceiptIndicator.Value = 1;

                                request.BillOfLading.LoadType = new NEnumOfLoadTypeEnum();
                                request.BillOfLading.LoadType.Value = LoadTypeEnum.SCHED;

                                request.BillOfLading.LoadSystem = new NEnumOfLoadSystemEnum();
                                request.BillOfLading.LoadSystem.Value = LoadSystemEnum.UNKNOWN;

                                request.BillOfLading.Terminal = new Location();
                                request.BillOfLading.Terminal.LocationId = new Identifier();
                                request.BillOfLading.Terminal.LocationId.Id = new NLong();
                                request.BillOfLading.Terminal.LocationId.Id.Value = 1100;

                                request.BillOfLading.Shipper = new BusinessEntity();
                                request.BillOfLading.Shipper.BusinessEntityId = new Identifier();
                                request.BillOfLading.Shipper.BusinessEntityId.Id = new NLong();
                                request.BillOfLading.Shipper.BusinessEntityId.Id.Value = 51004;

                                request.BillOfLading.Stockholder = new BusinessEntity();
                                request.BillOfLading.Stockholder.BusinessEntityId = new Identifier();
                                request.BillOfLading.Stockholder.BusinessEntityId.Id = new NLong();
                                request.BillOfLading.Stockholder.BusinessEntityId.Id.Value = 51004;

                                request.BillOfLading.UnitOfMeasure = new NEnumOfUnitOfMeasureEnum();
                                request.BillOfLading.UnitOfMeasure.Value = UnitOfMeasureEnum.L;

                                request.BillOfLading.Comments = new FlexComment[1];
                                request.BillOfLading.Comments[0] = new FlexComment();
                                request.BillOfLading.Comments[0].CommentText = "Synthesis Web Service Innovacion";

                                request.BillOfLading.ModeOfTransport = new NEnumOfModeOfTransportEnum();
                                request.BillOfLading.ModeOfTransport.Value = ModeOfTransportEnum.TRUCK;

                                var username = verify.GetName(HttpContext);
                                if (string.IsNullOrEmpty(username))
                                    return BadRequest();

                                request.BillOfLading.CreatedBy = $"energasws {username}";

                                request.BillOfLading.HeaderId = new NLong();
                                request.BillOfLading.HeaderId.Value = 50383;

                                request.BillOfLading.ActivityId = new NLong();
                                request.BillOfLading.ActivityId.Value = 1009;

                                request.BillOfLading.RevisionNumber = new NInt();
                                request.BillOfLading.RevisionNumber.Value = 1;

                                request.BillOfLading.SeqNum = new NInt();
                                request.BillOfLading.SeqNum.Value = 10;

                                request.BillOfLading.Destination = new Destination();
                                request.BillOfLading.Destination.DestinationId = new Identifier();
                                request.BillOfLading.Destination.DestinationId.Id = new NLong();

                                request.BillOfLading.Customer = new BusinessEntity();
                                request.BillOfLading.Customer.BusinessEntityId = new Identifier();
                                request.BillOfLading.Customer.BusinessEntityId.Id = new NLong();

                                request.BillOfLading.StartLoadTime = new NDateTime();
                                request.BillOfLading.EndLoadTime = new NDateTime();
                                request.BillOfLading.BolStatus = new NInt();

                                request.BillOfLading.TruckCarrier = new TruckCarrier();
                                request.BillOfLading.TruckCarrier.BusinessEntityId = new Identifier();
                                request.BillOfLading.TruckCarrier.BusinessEntityId.Id = new NLong();

                                request.BillOfLading.Driver = new Driver();
                                request.BillOfLading.Driver.DriverId = new Identifier();
                                request.BillOfLading.Driver.DriverId.Id = new NLong();

                                request.BillOfLading.Destination.DestinationId.Id.Value = long.Parse(item.Destino!.Codsyn!);
                                request.BillOfLading.Customer.BusinessEntityId.Id.Value = long.Parse(item.Destino!.Cliente!.Codsyn!);
                                request.BillOfLading.StartLoadTime.Value = item.Fchcar!.Value;
                                request.BillOfLading.EndLoadTime.Value = item.Fchcar.Value.AddDays(2);
                                var folio = context.OrdenEmbarque.Where(x => x.Codtad == id_terminal).OrderByDescending(X => X.Folio).Select(x => x.Folio).FirstOrDefault();

                                folio ??= 0;

                                folio++;
                                request.BillOfLading.CustomerReference = $"ENER-{folio}";

                                request.BillOfLading.BolStatus.Value = 34; // 34=SCHEDULED;12 = DRAFT;24 = PENDING

                                request.BillOfLading.PurchaseOrderRef = request.BillOfLading.CustomerReference;

                                request.BillOfLading.Driver.DriverId.Id.Value = long.Parse(item.Chofer!.Dricod!);
                                request.BillOfLading.TruckCarrier.BusinessEntityId.Id.Value = long.Parse(item.Tonel!.Transportista!.Busentid!);

                                //var pedidos = ordens.Where(x => x!.Codton == item.Codton
                                //&& x!.Codchf == item.Codchf && x.Fchcar == item.Fchcar
                                //&& string.IsNullOrEmpty(x.Bolguidid) && x.Codest == 3).ToList();

                                List<OrdenEmbarque> ordenEmbarques = new List<OrdenEmbarque>();

                                ordenEmbarques = context.OrdenEmbarque.Where(x => x.Codchf == item.Codchf && x.Codton == item.Codton && x.Fchcar == item.Fchcar
                                && x.Codest == 3 && string.IsNullOrEmpty(x.Bolguidid))
                                    .Include(x => x.Chofer)
                                    .Include(x => x.Estado)
                                    .Include(x => x.OrdenCierre)
                                    .Include(x => x.Destino)
                                    .ThenInclude(x => x.Cliente)
                                    .Include(x => x.Tonel)
                                    .ThenInclude(x => x.Transportista)
                                    .Include(x => x.Producto)
                                    .ToList();

                                request.BillOfLading.LineItems = new BillOfLadingLineItem[ordenEmbarques.Count];
                                List<BillOfLadingLineItem> billOfLadingLineItems = new List<BillOfLadingLineItem>();

                                foreach (var p in ordenEmbarques.DistinctBy(x => x.Cod).ToList())
                                {
                                    //var ord = OrdenesEnviadas.FirstOrDefault(x => x.Cod == p.Cod);
                                    //if (ord is null)
                                    //{

                                    BillOfLadingLineItem lineItem = new BillOfLadingLineItem();
                                    OrdenesEnviadas.Add(p);
                                    lineItem.Tank = new Device();
                                    lineItem.Tank.DeviceId = new Identifier();
                                    lineItem.Tank.DeviceId.Id = new NLong();
                                    lineItem.Tank.DeviceId.Id.Value = 16023;

                                    lineItem.Supplier = new BusinessEntity();
                                    lineItem.Supplier.BusinessEntityId = new Identifier();
                                    lineItem.Supplier.BusinessEntityId.Id = new NLong();
                                    lineItem.Supplier.BusinessEntityId.Id.Value = 51004;

                                    lineItem.CompartmentId = new NLong();
                                    lineItem.BaseNetQuantity = new NDecimal();
                                    lineItem.CustomerOrderQuantity = new NDecimal();

                                    lineItem.Customer = new BusinessEntity();
                                    lineItem.Customer.BusinessEntityId = new Identifier();
                                    lineItem.Customer.BusinessEntityId.Id = new NLong();

                                    lineItem.BaseProduct = new Product();
                                    lineItem.BaseProduct.ProductId = new Identifier();
                                    lineItem.BaseProduct.ProductId.Id = new NLong();

                                    lineItem.OrderedProduct = new Product();
                                    lineItem.OrderedProduct.ProductId = new Identifier();
                                    lineItem.OrderedProduct.ProductId.Id = new NLong();

                                    lineItem.EndLoadTime = new NDateTime();

                                    CustomFieldMetaData cfm = new CustomFieldMetaData();

                                    lineItem.CustomFieldInstances = new CustomFieldInstance[1];
                                    lineItem.CustomFieldInstances[0] = new CustomFieldInstance();
                                    lineItem.CustomFieldInstances[0].CustomFieldMetaData = new CustomFieldMetaData();

                                    lineItem.Destination = new Destination();
                                    lineItem.Destination.DestinationId = new Identifier();
                                    lineItem.Destination.DestinationId.Id = new NLong();

                                    lineItem.TrailerId = item.Tonel!.Codsyn.ToString();
                                    lineItem.CompartmentId.Value = long.Parse(p.CompartmentId.ToString()!);

                                    lineItem.BaseNetQuantity.Value = 0M;
                                    var vol = p.Compartment == 1 ? p.Tonel!.Capcom
                                        : p.Compartment == 2 ? p.Tonel!.Capcom2
                                        : p.Compartment == 3 ? p.Tonel!.Capcom3
                                        : p.Tonel!.Capcom4;
                                    lineItem.CustomerOrderQuantity.Value = decimal.Parse(vol.ToString()!);

                                    lineItem.Customer.BusinessEntityId.Id.Value = long.Parse(p.Destino!.Cliente!.Codsyn!);
                                    lineItem.BaseProduct.ProductId.Id.Value = long.Parse(p.Producto!.Codsyn!);
                                    lineItem.OrderedProduct.ProductId.Id.Value = long.Parse(p.Producto!.Codsyn!);
                                    lineItem.EndLoadTime.Value = item.Fchcar.Value.AddDays(2);

                                    cfm.EntityKey = p.Cod.ToString();
                                    cfm.Name = p.Cod.ToString();
                                    cfm.EntityName = "BILL_OF_LADING_LINE_ITEM";
                                    cfm.CustomFieldMetaDataId = new NLong();
                                    cfm.CustomFieldMetaDataId.Value = 46;

                                    lineItem.CustomFieldInstances[0].CustomFieldMetaData = cfm;
                                    lineItem.CustomFieldInstances[0].FieldStringValue = $"{request.BillOfLading.CustomerReference}_{p.Compartment}";
                                    lineItem.Destination.DestinationId.Id.Value = long.Parse(p.Destino!.Codsyn!);

                                    billOfLadingLineItems.Add(lineItem);
                                    //}
                                }

                                request.BillOfLading.LineItems = billOfLadingLineItems.ToArray();

                                WsBillOfLadingResponse response = new WsBillOfLadingResponse();

                                if (request.BillOfLading.LineItems != null)
                                {
                                    toFile.GenerateFile(JsonConvert.SerializeObject(request), $"Request_Synthesis_{DateTime.Now.ToString("ddMMyyyyHHmmss")}_{folio}.json", $"{DateTime.Now.ToString("ddMMyyyy")}");
                                    toFile.GenerateFileXML($"Request_Synthesis_{DateTime.Now.ToString("ddMMyyyyHHmmss")}_{folio}.xml", $"{DateTime.Now.ToString("ddMMyyyy")}", request);

                                    response = await client.SaveBillOfLadingAsync(request);

                                    toFile.GenerateFile(JsonConvert.SerializeObject(response), $"Response_Synthesis_{DateTime.Now.ToString("ddMMyyyyHHmmss")}_{folio}.json", $"{DateTime.Now.ToString("ddMMyyyy")}");
                                    toFile.GenerateFileXMLResponse($"Response_Synthesis_{DateTime.Now.ToString("ddMMyyyyHHmmss")}_{folio}.xml", $"{DateTime.Now.ToString("ddMMyyyy")}", response);
                                }
                                else
                                    return BadRequest();

                                if (response != null && response.BillOfLadings != null && response.BillOfLadings.Length > 0)
                                {
                                    BillOfLading billOfLading = response.BillOfLadings[0];

                                    ordenEmbarques.ForEach(x =>
                                    {
                                        x.Bolguidid = billOfLading.BolGuidId;
                                        x.Folio = folio;
                                        x.Codest = 22;
                                        foreach (var line in billOfLading.LineItems)
                                            if (line.CompartmentId != null && line.CompartmentId.Value.ToString() == x.CompartmentId.ToString())
                                                if (line.CustomFieldInstances != null)
                                                    x.FolioSyn = line.CustomFieldInstances.FirstOrDefault(y => y.CustomFieldMetaData != null && y.CustomFieldMetaData.Name.Equals(x.Cod.ToString()))?.FieldStringValue ?? string.Empty;
                                        //x.Chofer = null!;
                                        //x.Destino = null!;
                                        //x.Estado = null!;
                                        //x.Orden = null!;
                                        //x.OrdenCierre = null!;
                                        //x.OrdenCompra = null!;
                                        //x.OrdenPedido = null!;
                                        //x.Producto = null!;
                                        //x.Tad = null!;
                                        //x.Tonel = null!;
                                    });

                                    context.UpdateRange(ordenEmbarques);
                                    var id = await verify.GetId(HttpContext, userManager);
                                    if (string.IsNullOrEmpty(id))
                                        return BadRequest();

                                    await context.SaveChangesAsync(id, 10);
                                }

                                if (response != null && response.Errors != null && response.Errors.Length > 0)
                                {
                                    context.Add(new Errors()
                                    {
                                        Error = JsonConvert.SerializeObject(new Error()
                                        {
                                            Message = JsonConvert.SerializeObject(response.Errors),
                                        }),
                                        Accion = "Envio de orden a synthesis: Response"
                                    });

                                    await context.SaveChangesAsync();
                                }
                            }
                        }

                        //context.UpdateRange(ordenesEmbarque);

                        //var id = await verify.GetId(HttpContext, userManager);
                        //if (string.IsNullOrEmpty(id))
                        //    return BadRequest();

                        //await context.SaveChangesAsync(id, 10);

                        return Ok(true);
                    }
                }

                return Ok();
            }
            catch (NullReferenceException e)
            {
                await SaveErrors(e, "Envio a synthesis: Null reference");
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                await SaveErrors(e, "Envio a synthesis: exception");
                return BadRequest(e.Message);
            }
        }

        #endregion

        #region obtencion de ordenes cargadas
        [HttpGet("cargadas")]
        public async Task<ActionResult> GetOrdenesCargadas()
        {
            try
            {
                BillOfLadingServiceClient client = new BillOfLadingServiceClient(BillOfLadingServiceClient.EndpointConfiguration.BasicHttpBinding_BillOfLadingService2);
                client.ClientCredentials.UserName.UserName = "energasws";
                client.ClientCredentials.UserName.Password = "Energas23!";
                client.Endpoint.Binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
                client.Endpoint.Binding.SendTimeout = TimeSpan.FromMinutes(5);

                WsGetBillOfLadingsRequest request = new WsGetBillOfLadingsRequest();

                request.IncludeChildObjects = new NBool();
                request.IncludeChildObjects.Value = true;

                request.EndLoadDateFrom = new NDateTime();
                request.EndLoadDateFrom.Value = DateTime.Today.AddDays(-1);

                request.EndLoadDateTo = new NDateTime();
                request.EndLoadDateTo.Value = request.EndLoadDateFrom.Value.AddDays(2);

                request.ShipperId = new Identifier();
                request.ShipperId.Id = new NLong();
                request.ShipperId.Id.Value = 51004;

                request.DeliveryReceiptIndicator = new NInt();
                request.DeliveryReceiptIndicator.Value = 1;

                request.BolStatus = new NEnumOfOrderStatusEnum();
                request.BolStatus.Value = OrderStatusEnum.COMPLETED;

                request.IncludeCustomFields = new NBool();
                request.IncludeCustomFields.Value = true;

                var respuesta = await client.GetBillOfLadingsAsync(request);

                if (respuesta.BillOfLadings == null)
                    return BadRequest("Sin ordenes");

                if (respuesta.BillOfLadings.Length > 0)
                {
                    var BOL = respuesta.BillOfLadings;
                    foreach (var item in BOL)
                    {
                        Orden orden = new Orden();

                        orden.Ref = item.CustomerReference;
                        orden.Codchfsyn = item.Driver.DriverId.Id.Value;
                        orden.Bolguiid = item.BolGuidId;
                        orden.Dendes = item.Destination.DestinationName;
                        orden.Coddes = Convert.ToInt32(item.Destination.DestinationId.Id.Value);

                        if (item.SealNumber is not null)
                        {
                            foreach (var seal in item.SealNumber)
                            {
                                orden.SealNumber += seal + ",";
                                orden.SealNumber = orden.SealNumber.Trim();
                            }
                            orden.SealNumber = orden.SealNumber?.Replace("\t", "");
                            orden.SealNumber = orden.SealNumber?.Trim(',');
                        }

                        foreach (var line in item.LineItems)
                        {
                            if (line.BaseGravity is not null)
                            {
                                orden.Liniteid = line.BolLineItemId.Id.Value;
                                if (!context.Orden.Any(x => x.Liniteid == orden.Liniteid))
                                {
                                    orden.CompartmentId = Convert.ToInt32(line.CompartmentId.Value);
                                    orden.Codprdsyn = line.OrderedProduct.ProductId.Id.Value;
                                    orden.Vol = Convert.ToDouble(line.BaseNetQuantity.Value);
                                    orden.Fchcar = line.EndLoadTime.Value;
                                    orden.Coduni = Convert.ToInt32(line.TrailerId);
                                    orden.Codprd2syn = line.BaseProduct.ProductId.Id.Value;
                                    orden.Vol2 = Convert.ToDouble(line.BaseGrossQuantity.Value);

                                    var des = context.Destino.FirstOrDefault(x => x.Codsyn == line.Destination.DestinationId.Id.Value.ToString());
                                    if (des is not null)
                                    {
                                        orden.Coddes = des.Cod;
                                        orden.Dendes = des.Den?.Replace("'", "");
                                    }
                                    else
                                    {
                                        des = context.Destino.FirstOrDefault(x => x.Codsyn == item.Destination.DestinationId.Id.Value.ToString());
                                        orden.Coddes = des.Cod;
                                        orden.Dendes = des.Den?.Replace("'", "");
                                    }

                                    foreach (var cfi in line.CustomFieldInstances)
                                    {
                                        if (cfi.CustomFieldMetaData.Name.Equals("tm_batch_id"))
                                            orden.BatchId = Convert.ToInt32(cfi.FieldStringValue);
                                        else if (cfi.CustomFieldMetaData.Name.Equals(".ExternalOrderId"))
                                            orden.Ref = cfi.FieldStringValue;
                                    }

                                    var tonel = context.Tonel.FirstOrDefault(x => x.Codsyn == orden.Coduni && x.Activo == true);
                                    if (tonel is null)
                                    {
                                        orden.Coduni = 0;
                                        tonel = new Tonel() { Carid = string.Empty };
                                    }
                                    else
                                        //if (tonel is null) return BadRequest($"No existe el tonel. Codigo synthesis: {orden.Coduni}");

                                        orden.Coduni = tonel.Cod;

                                    var tran = context.Transportista.FirstOrDefault(x => x.CarrId == tonel.Carid);
                                    if (tran is null)
                                        tran = new Transportista() { Busentid = "0" };

                                    //if (tran is null) return BadRequest($"No existe el transportista. Carid transportista: {tonel.Carid}");

                                    var cho = context.Chofer.FirstOrDefault(x => x.Dricod == orden.Codchfsyn.ToString() && x.Codtransport == Convert.ToInt32(tran.Busentid));
                                    if (cho is null)
                                        orden.Codchf = 0;
                                    else
                                        //if (cho is null) return BadRequest($"No existe el chofer. Dricod chofer: {orden.Codchfsyn}. transportista: {tran.Busentid}");

                                        orden.Codchf = cho.Cod;

                                    var prd = context.Producto.FirstOrDefault(x => x.Codsyn == orden.Codprdsyn.ToString());
                                    if (prd is null)
                                        orden.Codprd = 0;
                                    else
                                        //if (prd is null) return BadRequest($"No existe el producto. Codigo synthesis: {orden.Codprdsyn}");

                                        orden.Codprd = prd.Cod;

                                    var prd2 = context.Producto.FirstOrDefault(x => x.Codsyn == orden.Codprd2syn.ToString());
                                    if (prd2 is null)
                                        orden.Codprd2 = 0;
                                    else
                                        //if (prd2 is null) return BadRequest($"No existe el producto. Codigo synthesis: {orden.Codprd2syn}");

                                        orden.Codprd2 = prd2.Cod;

                                    orden.Fch = DateTime.Now;
                                    orden.Codest = 20;

                                    string[] refs = orden.Ref.Split("-");
                                    string[] folio = refs[1].Split("_");
                                    if (!string.IsNullOrEmpty(folio[0]))
                                        orden.Folio = int.Parse(folio[0]);
                                    else
                                        orden.Folio = 0;

                                    if (orden.Codchf != 0 && orden.Codprd != 0 && orden.Codprd2 != 0 && orden.Coduni != 0)
                                        context.Add(orden);
                                }
                            }
                        }
                    }
                    await context.SaveChangesAsync();
                }
                return Ok();

            }
            catch (NullReferenceException e)
            {
                await SaveErrors(e, "Obtencion de ordenes cargadas: Null reference");
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                await SaveErrors(e, "Obtencion de ordenes cargadas");
                return BadRequest(e.Message);
            }
        }
        #endregion

        #region Simulacion Synthesis#
        [HttpPost]
        [Route("simulacion/synthesis")]
        public async Task<ActionResult> Siumulacion([FromBody] List<OrdenEmbarque> ordenes)
        {
            try
            {
                ordenes.ForEach(x =>
                {
                    x.Producto = null;
                    x.Chofer = null;
                    x.Destino = null;
                    x.Tonel = null;
                    x.Tad = null;
                    x.OrdenCompra = null;
                    x.Estado = null;
                    x.Cliente = null!;
                    x.OrdenCierre = null!;

                    Random rand = new Random();
                    int randomNumber = rand.Next(1, 100001);
                    Guid guid = Guid.NewGuid();
                    x.Bolguidid = guid.ToString();
                    x.Folio = randomNumber;
                    x.Codest = 22;
                });

                context.UpdateRange(ordenes);
                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        #endregion#

        #region Simulacion Cargadas
        [HttpGet]
        [Route("simulacion/cargadas")]
        public async Task<ActionResult> SimulacionObtener()
        {
            try
            {
                var ordenes = await context.OrdenEmbarque.Where(x => x.FchOrd >= DateTime.Today.AddDays(-3) && x.FchOrd <= DateTime.Today && x.Folio != null && x.Bolguidid != null)
                    .Include(x => x.Destino)
                    .ToListAsync();

                if (ordenes is null)
                    return NoContent();

                foreach (var item in ordenes)
                {
                    Random random = new Random();
                    Orden orden = new Orden();
                    if (!context.Orden.Any(x => x.Folio == item.Folio && x.CompartmentId == item.CompartmentId))
                        orden = new Orden()
                        {
                            Ref = "ENER-" + item.Folio,
                            Codchf = item.Codchf,
                            Coddes = item.Coddes,
                            Codest = 20,
                            Codprd = item.Codprd,
                            Coduni = item.Codton,
                            Bolguiid = item.Bolguidid,
                            BatchId = random.Next(1, 100001),
                            Fch = DateTime.Now,
                            Fchcar = item.Fchcar,
                            Codprd2 = item.Codprd,
                            Vol = item.Vol,
                            Vol2 = item.Vol,
                            Dendes = item.Destino?.Den,
                            CompartmentId = item.CompartmentId,
                            Liniteid = random.Next(1, 100001),
                            Folio = item.Folio
                        };
                    //orden.Estado = null!;
                    //orden.Destino = null!;
                    //orden.Producto = null!;
                    //orden.Tonel = null!;
                    //orden.OrdenEmbarque = null!;
                    //orden.OrdEmbDet = null!;
                    if (orden.Folio != null && orden.CompartmentId != null)
                        context.Add(orden);
                }

                await context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        #endregion#

        #region Abrir Ordenes cerradas
        [HttpGet("abrir/canceladas")]
        public async Task<ActionResult> ReabrirCierres()
        {
            try
            {
                List<OrdenEmbarque> ordenEmbarques = new();
                ordenEmbarques = context.OrdenEmbarque.Where(x => ((x.Codest == 14) || (x.Orden != null && x.Orden.Codest == 14)) && x.Fchpet >= DateTime.Today.AddDays(-8) && x.Fchpet <= DateTime.Today)
                    .Include(x => x.Orden)
                    .IgnoreAutoIncludes()
                    .ToList();

                foreach (var item in ordenEmbarques)
                {
                    if (context.OrdenPedido.Any(x => x.CodPed == item.Cod))
                    {
                        OrdenPedido? ordenPedido = new();
                        ordenPedido = context.OrdenPedido.Where(x => x.CodPed == item.Cod).IgnoreAutoIncludes().FirstOrDefault();
                        if (ordenPedido is not null)
                        {
                            OrdenCierre? ordenCierre = new();
                            ordenCierre = context.OrdenCierre.Where(x => x.Cod == ordenPedido.CodCierre && x.Activa == false).IgnoreAutoIncludes().FirstOrDefault();
                            if (ordenCierre is not null)
                            {
                                ordenCierre.Activa = true;
                                context.Update(ordenCierre);
                            }
                        }

                    }
                }

                await context.SaveChangesAsync();
                return NoContent();
            }
            catch (NullReferenceException e)
            {
                await SaveErrors(e, "Abrir pedidos cerrados: Null reference");
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                await SaveErrors(e, "Abrir pedidos cerrardos");
                return BadRequest(e.Message);
            }
        }
        #endregion

        #region actualizar ordenes
        [HttpGet("cancelar/{folio:int}")]
        public async Task<ActionResult> GetOrdenesModificadas([FromRoute] int folio)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                List<OrdenEmbarque> ordenEmbarques = context.OrdenEmbarque.Where(x => x.Folio == folio && x.Codtad == id_terminal).Include(x => x.Orden).IgnoreAutoIncludes().ToList();
                foreach (var orden in ordenEmbarques)
                {
                    orden.Codest = 14;

                    if (orden.Orden is not null)
                    {
                        orden.Orden.Codest = 14;
                        context.Update(orden.Orden);
                    }

                    orden.Orden = null!;
                    context.Update(orden);
                }

                var id = await verify.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                await context.SaveChangesAsync(id, 33);

                List<OrdenEmbarque> NewOrden = context.OrdenEmbarque.Where(x => x.Folio == folio && x.Codtad == id_terminal)
                    .Include(x => x.Destino)
                    .Include(x => x.Producto)
                    .Include(x => x.Chofer)
                    .Include(x => x.Tad)
                    .Include(x => x.Tonel)
                    .Include(x => x.OrdenCierre)
                    .ThenInclude(x => x.Cliente)
                    .Include(x => x.Estado)
                    .Include(x => x.Orden)
                    .ThenInclude(x => x.Estado)
                    .ToList();

                if (NewOrden is null)
                    return Ok(new List<OrdenEmbarque>());

                return Ok(NewOrden);

            }
            catch (NullReferenceException e)
            {
                await SaveErrors(e, "Actualizar ordenes de synthesis: Null reference");
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                await SaveErrors(e, "Actualizar ordenes de synthesis");
                return BadRequest(e.Message);
            }
        }
        #endregion

        #region Generar codigo relacion
        [HttpGet("generar/relacion")]
        public async Task<ActionResult> GenerarRelacion()
        {
            try
            {
                List<OrdenEmbarque> cierres = new List<OrdenEmbarque>();
                cierres = context.OrdenEmbarque.Where(x => x.Fchpet >= DateTime.Today.AddDays(-20) && x.Fchpet <= DateTime.Now && string.IsNullOrEmpty(x.FolioSyn)).ToList();
                foreach (var item in cierres.DistinctBy(x => x.Cod))
                {
                    if (item.Folio != null)
                        item.FolioSyn = $"ENER-{item.Folio}_{item.Compartment}";
                    else
                        item.FolioSyn = string.Empty;
                    context.Update(item);
                }
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (NullReferenceException e)
            {
                await SaveErrors(e, "Actualizar ordenes de synthesis: Null reference");
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                await SaveErrors(e, "Actualizar ordenes de synthesis");
                return BadRequest(e.Message);
            }
        }
        #endregion

        #region Copia de catalogos

        [HttpGet("default/data/{id_terminal}/{id_terminal_catalogo}")]
        public async Task<ActionResult> Fijar_Datos_Por_Defecto([FromRoute] short id_terminal, [FromRoute] short id_terminal_catalogo)
        {
            try
            {
                //var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                //if (id_terminal == 0)
                //    return BadRequest();

                if (!context.Tad.Any(x => x.Cod == id_terminal))
                    return NotFound();

                var clientes = context.Cliente.IgnoreAutoIncludes().Where(x => x.Id_Tad == id_terminal_catalogo).Select(x => x.Cod).ToList();
                List<Cliente_Tad> cliente_Tads = new();

                foreach (var cliente in clientes)
                    if (!context.Cliente_Tad.Any(x => x.Id_Cliente == cliente && x.Id_Terminal == id_terminal))
                        cliente_Tads.Add(new() { Id_Cliente = cliente, Id_Terminal = id_terminal });

                var destinos = context.Destino.IgnoreAutoIncludes().Where(x => x.Id_Tad == id_terminal_catalogo).Select(x => x.Cod).ToList();
                List<Destino_Tad> destino_Tads = new();

                foreach (var destino in destinos)
                    if (!context.Destino_Tad.Any(x => x.Id_Destino == destino && x.Id_Terminal == id_terminal))
                        destino_Tads.Add(new() { Id_Destino = destino, Id_Terminal = id_terminal });

                var transportistas = context.Transportista.IgnoreAutoIncludes().Where(x => x.Id_Tad == id_terminal_catalogo).Select(x => x.Cod).ToList();
                List<Transportista_Tad> transportista_s = new();

                foreach (var trans in transportistas)
                    if (!context.Transportista_Tad.Any(x => x.Id_Transportista == trans && x.Id_Terminal == id_terminal))
                        transportista_s.Add(new() { Id_Terminal = id_terminal, Id_Transportista = trans });


                var choferes = context.Chofer.IgnoreAutoIncludes().Where(x => x.Id_Tad == id_terminal_catalogo).Select(x => x.Cod).ToList();
                List<Chofer_Tad> choferes_tad = new();

                foreach (var chofer in choferes)
                    if (!context.Chofer_Tad.Any(x => x.Id_Chofer == chofer && x.Id_Terminal == id_terminal))
                        choferes_tad.Add(new() { Id_Chofer = chofer, Id_Terminal = id_terminal });

                var unidades = context.Tonel.IgnoreAutoIncludes().Where(x => x.Id_Tad == id_terminal_catalogo).Select(x => x.Cod).ToList();
                List<Unidad_Tad> unidad_Tads = new();

                foreach (var unidad in unidades)
                    if (!context.Unidad_Tad.Any(x => x.Id_Unidad == unidad && x.Id_Terminal == id_terminal))
                        unidad_Tads.Add(new() { Id_Unidad = unidad, Id_Terminal = id_terminal });

                var usuarios = context.Users.Select(x => new { x.Id, x.UserName }).ToList();
                List<Usuario_Tad> usuario_Tads = new();

                foreach (var usuario in usuarios)
                    if (!context.Usuario.Any(x => x.Usu == usuario.UserName))
                        if (!context.Usuario_Tad.Any(x => x.Id_Usuario == usuario.Id && x.Id_Terminal == id_terminal))
                            usuario_Tads.Add(new() { Id_Usuario = usuario.Id, Id_Terminal = id_terminal });

                context.AddRange(cliente_Tads);
                context.AddRange(destino_Tads);
                context.AddRange(transportista_s);
                context.AddRange(choferes_tad);
                context.AddRange(unidad_Tads);
                context.AddRange(usuario_Tads);

                await context.SaveChangesAsync();

                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("copiar/cliente/{terminal}/{terminal_destino}")]
        public async Task<ActionResult> Copiar_Clientes([FromRoute] short terminal, [FromRoute] short terminal_destino)
        {
            try
            {
                var clientes = context.Cliente.IgnoreAutoIncludes().Where(x => x.Id_Tad == terminal && x.Codgru != null
                && !string.IsNullOrEmpty(x.Codsyn)).ToList();

                List<Cliente_Tad> Clientes_validos = new();
                List<Cliente> clientes_editados = new();

                for (int i = 0; i < clientes.Count; i++)
                {
                    if (!context.Cliente.Any(x => !string.IsNullOrEmpty(x.Codsyn) && x.Codsyn.Equals(clientes[i].Codsyn)
                        && x.Id_Tad == terminal_destino && x.Codgru != null))
                    {
                        var new_cliente = clientes[i].HardCopy();
                        new_cliente.Cod = 0;
                        new_cliente.Id_Tad = terminal_destino;

                        Cliente_Tad cliente_Tad = new()
                        {
                            Id_Terminal = terminal_destino,
                            Cliente = new_cliente,
                            Terminal = null!
                        };

                        Clientes_validos.Add(cliente_Tad);
                    }
                    else
                    {
                        if (context.Cliente.Any(x => !string.IsNullOrEmpty(x.Codsyn) && x.Codsyn.Equals(clientes[i].Codsyn) && x.Id_Tad == terminal_destino && x.Codgru != null
                        && (x.Den != clientes[i].Den || x.CodCte != clientes[i].CodCte || x.Activo != clientes[i].Activo) && !string.IsNullOrEmpty(clientes[i].Codsyn)))
                        {
                            var cliente_existente = context.Cliente
                                .IgnoreAutoIncludes()
                                .FirstOrDefault(x => !string.IsNullOrEmpty(x.Codsyn) && x.Codsyn.Equals(clientes[i].Codsyn) && x.Id_Tad == terminal_destino && x.Codgru != null);
                            if (cliente_existente is not null)
                            {
                                cliente_existente.Den = clientes[i].Den;
                                cliente_existente.CodCte = clientes[i].CodCte;
                                cliente_existente.Activo = clientes[i].Activo;
                                clientes_editados.Add(cliente_existente);
                            }
                        }
                    }
                }

                context.AddRange(Clientes_validos);
                context.UpdateRange(clientes_editados);
                await context.SaveChangesAsync();

                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("copiar/destino/{terminal}/{terminal_destino}")]
        public async Task<ActionResult> Copiar_Destinos([FromRoute] short terminal, [FromRoute] short terminal_destino)
        {
            try
            {
                var clientes = await context.Cliente.IgnoreAutoIncludes().Where(x => x.Id_Tad == terminal && !string.IsNullOrEmpty(x.Codsyn)).Select(x => new { x.Cod, x.Codsyn }).ToListAsync();
                var destinos = await context.Destino.IgnoreAutoIncludes().Where(x => x.Id_Tad == terminal && !string.IsNullOrEmpty(x.Codsyn)).ToListAsync();

                List<Destino_Tad> destinos_validos = new();
                List<Destino> destinos_editados = new();

                for (int i = 0; i < clientes.Count; i++)
                {
                    if (context.Cliente.Any(x => !string.IsNullOrEmpty(x.Codsyn) && x.Codsyn == clientes[i].Codsyn && x.Id_Tad == terminal_destino))
                    {
                        var destinos_cliente = destinos.Where(x => x.Codcte == clientes[i].Cod).ToList();
                        int? cliente = await context.Cliente.IgnoreAutoIncludes().Where(x => x.Codsyn == clientes[i].Codsyn && x.Id_Tad == terminal_destino).Select(x => x.Cod).FirstOrDefaultAsync();

                        for (int j = 0; j < destinos_cliente.Count; j++)
                        {
                            if (!context.Destino.Any(x => !string.IsNullOrEmpty(x.Codsyn) && x.Codsyn == destinos_cliente[j].Codsyn && x.Id_Tad == terminal_destino))
                            {
                                if (cliente != null && cliente != 0)
                                {
                                    var new_destino = destinos_cliente[j].HardCopy();
                                    new_destino.Cod = 0;
                                    new_destino.Id_Tad = terminal_destino;
                                    new_destino.Codcte = cliente;

                                    Destino_Tad destino_Tad = new()
                                    {
                                        Id_Terminal = terminal_destino,
                                        Destino = new_destino,
                                        Terminal = null!
                                    };

                                    destinos_validos.Add(destino_Tad);
                                }
                            }
                            else
                            {
                                if (context.Destino.Any(x => !string.IsNullOrEmpty(x.Codsyn) && x.Codsyn == destinos_cliente[j].Codsyn && x.Id_Tad == terminal_destino
                                && (x.Den != destinos_cliente[j].Den || x.Activo != destinos_cliente[j].Activo)))
                                {
                                    var destino_existente = await context.Destino
                                        .IgnoreAutoIncludes()
                                        .FirstOrDefaultAsync(x => !string.IsNullOrEmpty(x.Codsyn) && x.Codsyn == destinos_cliente[j].Codsyn && x.Id_Tad == terminal_destino);

                                    if (destino_existente is not null)
                                    {
                                        destino_existente.Den = destinos_cliente[j].Den;
                                        destino_existente.Activo = destinos_cliente[j].Activo;
                                        destinos_editados.Add(destino_existente);
                                    }
                                }
                            }
                        }
                    }
                }

                context.AddRange(destinos_validos);
                context.UpdateRange(destinos_editados);
                await context.SaveChangesAsync();

                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("copiar/contacto/{terminal}/{terminal_destino}")]
        public async Task<ActionResult> Copiar_Contacto([FromRoute] short terminal, [FromRoute] short terminal_destino)
        {
            try
            {
                var clientes = context.Cliente.Where(x => x.Id_Tad == terminal && !string.IsNullOrEmpty(x.Codsyn)).Select(x => new { x.Cod, x.Codsyn }).ToList();
                //var contactos = context.Destino.Where(x => x.Id_Tad == terminal && x.Activo).ToList();

                List<Contacto> contactos_validos = new();

                for (int i = 0; i < clientes.Count; i++)
                {
                    if (context.Cliente.Any(x => !string.IsNullOrEmpty(x.Codsyn) && x.Den == clientes[i].Codsyn && x.Id_Tad == terminal_destino))
                    {
                        var contactos_cliente = context.Contacto.IgnoreAutoIncludes().Where(x => x.CodCte == clientes[i].Cod).Include(x => x.AccionCorreos).IgnoreAutoIncludes().ToList();
                        var cliente = context.Cliente.FirstOrDefault(x => x.Den == clientes[i].Codsyn && x.Id_Tad == terminal_destino);

                        for (int j = 0; j < contactos_cliente.Count; j++)
                        {
                            if (cliente is not null)
                            {
                                if (!context.Contacto.Any(x => !string.IsNullOrEmpty(x.Correo) && x.Correo == contactos_cliente[j].Correo && x.CodCte == cliente.Cod))
                                {


                                    var new_contacto = contactos_cliente[j].HardCopy();
                                    new_contacto.Cod = 0;
                                    new_contacto.CodCte = cliente.Cod;
                                    new_contacto.Cliente = null!;

                                    var acciones = new_contacto.AccionCorreos;

                                    new_contacto.AccionCorreos = new();

                                    if (acciones is not null)
                                    {
                                        foreach (var accion in acciones)
                                        {
                                            new_contacto.AccionCorreos.Add(new() { CodAccion = accion.CodAccion });
                                        }
                                    }

                                    contactos_validos.Add(new_contacto);
                                }
                            }
                        }
                    }
                }

                context.AddRange(contactos_validos);
                await context.SaveChangesAsync();

                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("copiar/transportista/{terminal}/{terminal_destino}")]
        public async Task<ActionResult> Copiar_Transportista([FromRoute] short terminal, [FromRoute] short terminal_destino)
        {
            try
            {
                var transportistas = await context.Transportista
                    .Where(x => x.Id_Tad == terminal && x.Activo == true)
                    .ToListAsync();

                List<Transportista_Tad> transportistas_validos = new();
                List<Transportista> transportistas_editados = new();

                for (int i = 0; i < transportistas.Count; i++)
                {
                    if (!context.Transportista.Any(x => x.Identificacion != null && x.Identificacion == transportistas[i].Identificacion && x.Id_Tad == terminal_destino))
                    {
                        var numero = GetRandomCarid(transportistas_validos);
                        var new_transpor = transportistas[i].HardCopy();


                        new_transpor.Cod = 0;
                        new_transpor.Id_Tad = terminal_destino;

                        new_transpor.BusentId_Original = new_transpor.Busentid;
                        new_transpor.CarId_Original = new_transpor.CarrId;

                        new_transpor.CarrId = numero;
                        new_transpor.Busentid = numero;
                        new_transpor.Identificacion = transportistas[i].Identificacion;

                        Transportista_Tad transportista_Tad = new()
                        {
                            Id_Terminal = terminal_destino,
                            Transportista = new_transpor,
                            Terminal = null!
                        };

                        transportistas_validos.Add(transportista_Tad);
                    }
                    else
                    {
                        if (context.Transportista.Any(x => x.Identificacion == transportistas[i].Identificacion && x.Id_Tad == terminal_destino
                        && (x.Den != transportistas[i].Den || x.RFC != transportistas[i].RFC || x.Activo != transportistas[i].Activo)))
                        {
                            var transportista = await context.Transportista
                                .IgnoreAutoIncludes()
                                .FirstOrDefaultAsync(x => x.Identificacion == transportistas[i].Identificacion && x.Id_Tad == terminal_destino);

                            if (transportista is not null)
                            {
                                transportista.Den = transportistas[i].Den;
                                transportista.RFC = transportistas[i].RFC;
                                transportista.Activo = transportistas[i].Activo;
                                transportistas_editados.Add(transportista);
                            }
                        }
                    }
                }

                context.AddRange(transportistas_validos);
                context.UpdateRange(transportistas_editados);
                await context.SaveChangesAsync();

                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("copiar/chofer/{terminal}/{terminal_destino}")]
        public async Task<ActionResult> Copiar_Choferes([FromRoute] short terminal, [FromRoute] short terminal_destino)
        {
            try
            {
                var transportistas = await context.Transportista.Where(x => x.Id_Tad == terminal).Select(x => new { x.Cod, x.Identificacion, x.Busentid }).ToListAsync();
                var choferes = await context.Chofer.Where(x => x.Id_Tad == terminal).ToListAsync();

                List<Chofer_Tad> chofer_Tads = new();
                List<Chofer> Choferes_editados = new();

                for (int i = 0; i < transportistas.Count; i++)
                {
                    if (await context.Transportista.AnyAsync(x => x.Identificacion != null && x.Identificacion == transportistas[i].Identificacion && x.Id_Tad == terminal_destino))
                    {
                        var transportista = await context.Transportista
                            .Where(x => x.Identificacion != null && x.Identificacion == transportistas[i].Identificacion && x.Id_Tad == terminal_destino)
                            .Select(x => x.Busentid)
                            .FirstOrDefaultAsync();

                        if (!string.IsNullOrEmpty(transportista))
                        {
                            if (int.TryParse(transportista, out int busentid))
                            {
                                var choferes_transpor = choferes.Where(x => x.Codtransport.ToString() == transportistas[i].Busentid).ToList();

                                for (int j = 0; j < choferes_transpor.Count; j++)
                                {
                                    if (!await context.Chofer.AnyAsync(x => x.Id_Tad == terminal_destino && x.Dricod == choferes_transpor[j].Dricod && x.Codtransport == busentid))
                                    {
                                        var new_chofer = choferes_transpor[j].HardCopy();
                                        new_chofer.Cod = 0;
                                        new_chofer.Id_Tad = terminal_destino;
                                        new_chofer.Codtransport = busentid;

                                        Chofer_Tad chofer_Tad = new()
                                        {
                                            Id_Terminal = terminal_destino,
                                            Chofer = new_chofer,
                                            Terminal = null!
                                        };

                                        chofer_Tads.Add(chofer_Tad);
                                    }
                                    else
                                    {
                                        if (context.Chofer.Any(x => x.Dricod == choferes_transpor[j].Dricod && x.Id_Tad == terminal_destino && x.Codtransport == busentid
                                        && (x.Den != choferes_transpor[j].Den || x.Shortden != choferes_transpor[j].Shortden
                                        || x.RFC != choferes_transpor[j].RFC || x.Activo != choferes_transpor[j].Activo) && x.Identificador == choferes_transpor[j].Identificador))
                                        {
                                            var chofer = await context.Chofer.IgnoreAutoIncludes().FirstOrDefaultAsync(x => x.Dricod == choferes_transpor[j].Dricod && x.Id_Tad == terminal_destino
                                                                                        && x.Codtransport == busentid && x.Identificador == choferes_transpor[j].Identificador);
                                            if (chofer is not null)
                                            {
                                                chofer.Den = choferes_transpor[j].Den;
                                                chofer.Shortden = choferes_transpor[j].Shortden;
                                                chofer.RFC = choferes_transpor[j].RFC;
                                                chofer.Activo = choferes_transpor[j].Activo;

                                                Choferes_editados.Add(chofer);
                                            }
                                        }

                                    }
                                }
                            }
                        }
                    }
                }

                context.AddRange(chofer_Tads);
                context.UpdateRange(Choferes_editados);
                await context.SaveChangesAsync();

                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("copiar/tonel/{terminal}/{terminal_destino}")]
        public async Task<ActionResult> Copiar_Toneles([FromRoute] short terminal, [FromRoute] short terminal_destino)
        {
            try
            {
                var transportistas = await context.Transportista.IgnoreAutoIncludes().Where(x => x.Id_Tad == terminal && !string.IsNullOrEmpty(x.CarrId)).ToListAsync();
                var toneles = await context.Tonel.IgnoreAutoIncludes().Where(x => x.Id_Tad == terminal && x.Codsyn != 0 && x.Codsyn != null).ToListAsync();

                List<Unidad_Tad> unidad_Tads = new();
                List<Tonel> unidad_editada = new();

                for (int i = 0; i < transportistas.Count; i++)
                {
                    if (context.Transportista.Any(x => x.Identificacion != null && x.Identificacion == transportistas[i].Identificacion && x.Id_Tad == terminal_destino))
                    {
                        var transportista = await context.Transportista.IgnoreAutoIncludes()
                            .Where(x => x.Identificacion != null && x.Identificacion == transportistas[i].Identificacion && x.Id_Tad == terminal_destino)
                            .Select(x => x.CarrId)
                            .FirstOrDefaultAsync();

                        if (!string.IsNullOrEmpty(transportista))
                        {
                            var toneles_transpor = toneles.Where(x => x.Carid == transportistas[i].CarrId).ToList();

                            for (int j = 0; j < toneles_transpor.Count; j++)
                            {
                                if (!context.Tonel.Any(x => x.Id_Tad == terminal_destino && x.Codsyn == toneles_transpor[j].Codsyn && x.Carid == transportista))
                                {
                                    var new_unidad = toneles_transpor[j].HardCopy();
                                    new_unidad.Cod = 0;
                                    new_unidad.Id_Tad = terminal_destino;
                                    new_unidad.Carid = transportista;

                                    Unidad_Tad unidad_Tad = new()
                                    {
                                        Id_Terminal = terminal_destino,
                                        Tonel = new_unidad,
                                        Terminal = null!
                                    };

                                    unidad_Tads.Add(unidad_Tad);
                                }
                                else
                                {
                                    if (context.Tonel.Any(x => x.Id_Tad == terminal_destino && x.Codsyn == toneles_transpor[j].Codsyn && x.Carid == transportista
                                    && (x.Placa != toneles_transpor[j].Placa || x.Tracto != toneles_transpor[j].Tracto
                                    || x.Placatracto != toneles_transpor[j].Placatracto || x.Certificado_Calibracion != toneles_transpor[j].Certificado_Calibracion
                                    || x.Activo != toneles_transpor[j].Activo || x.Capcom != toneles_transpor[j].Capcom || x.Capcom2 != toneles_transpor[j].Capcom2
                                    || x.Capcom3 != toneles_transpor[j].Capcom3 || x.Capcom4 != toneles_transpor[j].Capcom4)))
                                    {
                                        var tonel = await context.Tonel.FirstOrDefaultAsync(x => x.Id_Tad == terminal_destino && x.Codsyn == toneles_transpor[j].Codsyn && x.Carid == transportista);
                                        if (tonel is not null)
                                        {
                                            tonel.Placa = toneles_transpor[j].Placa;
                                            tonel.Tracto = toneles_transpor[j].Tracto;
                                            tonel.Placatracto = toneles_transpor[j].Placatracto;
                                            tonel.Certificado_Calibracion = toneles_transpor[j].Certificado_Calibracion;
                                            tonel.Activo = toneles_transpor[j].Activo;
                                            tonel.Capcom = toneles_transpor[j].Capcom;
                                            tonel.Capcom2 = toneles_transpor[j].Capcom2;
                                            tonel.Capcom3 = toneles_transpor[j].Capcom3;
                                            tonel.Capcom4 = toneles_transpor[j].Capcom4;

                                            unidad_editada.Add(tonel);
                                        }
                                    }
                                }
                            }
                        }

                    }
                }

                context.AddRange(unidad_Tads);
                context.UpdateRange(unidad_editada);
                await context.SaveChangesAsync();

                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("copiar/producto/{terminal}/{terminal_destino}")]
        public async Task<ActionResult> Copiar_Productos([FromRoute] short terminal, [FromRoute] short terminal_destino)
        {
            try
            {
                var productos = context.Producto.Where(x => x.Id_Tad == terminal && !string.IsNullOrEmpty(x.Codsyn)).ToList();

                List<Producto> productos_validos = new();
                List<Producto> productos_editados = new();

                for (int i = 0; i < productos.Count; i++)
                {
                    if (!context.Producto.Any(x => !string.IsNullOrEmpty(x.Codsyn) && x.Codsyn == productos[i].Codsyn && x.Id_Tad == terminal_destino))
                    {
                        var new_producto = productos[i].HardCopy();
                        new_producto.Cod = 0;
                        new_producto.Id_Tad = terminal_destino;

                        productos_validos.Add(new_producto);
                    }
                    else
                    {
                        if (context.Producto.Any(x => !string.IsNullOrEmpty(x.Codsyn) && x.Codsyn == productos[i].Codsyn && x.Id_Tad == terminal_destino
                        && (x.Den != productos[i].Den || x.Activo != productos[i].Activo || x.Id_Tipo != productos[i].Id_Tipo)))
                        {
                            var producto = await context.Producto.FirstOrDefaultAsync(x => !string.IsNullOrEmpty(x.Codsyn) && x.Codsyn == productos[i].Codsyn && x.Id_Tad == terminal_destino);
                            if (producto is not null)
                            {
                                producto.Den = productos[i].Den;
                                producto.Activo = productos[i].Activo;
                                producto.Id_Tipo = productos[i].Id_Tipo;

                                productos_editados.Add(producto);
                            }
                        }
                    }
                }

                context.AddRange(productos_validos);
                context.UpdateRange(productos_editados);
                await context.SaveChangesAsync();

                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        #endregion

        #region Igualar identificacion al codigo id
        [HttpGet("identificador"), Authorize(Roles = "Admin")]
        public async Task<ActionResult> Set_Identificador()
        {
            try
            {
                var transportistas = await context.Transportista.IgnoreAutoIncludes().Where(x => x.Id_Tad == 1 && x.Activo == true).Select(x => new { x.Den, x.Identificacion }).ToListAsync();
                for (int i = 0; i < transportistas.Count; i++)
                {
                    var ts = await context.Transportista.IgnoreAutoIncludes()
                        .Where(x => !string.IsNullOrEmpty(x.Den) && x.Den.Equals(transportistas[i].Den) && x.Identificacion == null && x.Id_Tad != 1)
                        .ToListAsync();

                    for (int j = 0; j < ts.Count; j++)
                    {
                        ts[j].Identificacion = transportistas[i].Identificacion;
                        context.Update(ts[j]);
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

        [HttpGet("identificador/chofer"), Authorize(Roles = "Admin")]
        public async Task<ActionResult> Set_Identificador_Chofer()
        {
            try
            {
                var transportistas = await context.Transportista.IgnoreAutoIncludes().Where(x => x.Id_Tad == 1).Select(x => new { x.Den, x.Identificacion, x.Busentid }).ToListAsync();
                var choferes = await context.Chofer.IgnoreAutoIncludes().Where(x => x.Id_Tad == 1).Select(x => new { x.Cod, x.Den, x.Shortden, x.Codtransport, x.Dricod }).ToListAsync();
                List<Chofer> choferes_editados = new();
                for (int i = 0; i < transportistas.Count; i++)
                {
                    if (int.TryParse(transportistas[i].Busentid, out int busentid))
                    {
                        var choferes_validos = choferes.Where(x => x.Codtransport == busentid).ToList();

                        var ts = await context.Transportista.IgnoreAutoIncludes()
                                                .Where(x => x.Identificacion == transportistas[i].Identificacion && x.Id_Tad != 1)
                                                .Select(x => new {x.Busentid})
                                                .ToListAsync();
                        for (int k = 0; k < ts.Count; k++)
                        {
                            if (int.TryParse(ts[k].Busentid, out int bsid))
                            {
                                for (int j = 0; j < choferes_validos.Count; j++)
                                {
                                    var cs = await context.Chofer
                                        .Where(x =>x.Den == choferes_validos[j].Den && x.Shortden == choferes_validos[j].Shortden &&
                                        x.Codtransport == bsid && x.Dricod == choferes_validos[j].Dricod && x.Id_Tad != 1)
                                        .ToListAsync();
                                    for (int l = 0; l < cs.Count; l++)
                                    {
                                        cs[l].Identificador = choferes_validos[j].Cod;
                                        choferes_editados.Add(cs[l]);
                                    }
                                }
                            }

                        }
                    }
                }

                context.UpdateRange(choferes_editados);
                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        #endregion

        private string GetRandomCarid(List<Transportista_Tad> transportistas)
        {
            var random = new Random().Next(1, 999999).ToString();

            if (!context.Transportista.Any(x => !string.IsNullOrEmpty(x.CarrId) && x.CarrId.Equals(random)
                                             || !string.IsNullOrEmpty(x.Busentid) && x.Busentid.Equals(random)))
            {
                if (!transportistas.Any(x => !string.IsNullOrEmpty(x.Transportista?.CarrId) && x.Transportista.CarrId.Equals(random)))
                    return random;
                else
                    GetRandomCarid(transportistas);
            }
            else
                GetRandomCarid(transportistas);

            return string.Empty;
        }
    }
}
