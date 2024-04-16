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
//using ServiceReference7; //prod 
using ServiceReference2; //qa
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

        [HttpGet("default/data/{id_terminal}")]
        public async Task<ActionResult> Fijar_Datos_Por_Defecto([FromRoute] short id_terminal)
        {
            try
            {
                if (!context.Tad.Any(x => x.Cod == id_terminal))
                    return NotFound();

                var terminal = context.Tad.IgnoreAutoIncludes().FirstOrDefault(x => x.Cod == id_terminal);
                if (terminal is null) { return BadRequest("No se encontro la terminal"); }

                terminal.Ultima_Actualizacion_Catalogo = DateTime.Now;
                context.Update(terminal);
                await context.SaveChangesAsync();

                //copia de cliente ligada a tuxpan
                var clientes = context.Cliente.IgnoreAutoIncludes().Where(x => x.Activo && x.Id_Tad == 1).ToList();
                var destinos = context.Destino.IgnoreAutoIncludes().Where(x => x.Activo && x.Id_Tad == 1).ToList();
                //List<Cliente> clientes_nuevos = new();
                //List<Destino> destinos_nuevos = new();
                var clientes_en_tuxpa_no_terminal_destino = clientes.Except(context.Cliente.Where(x => x.Id_Tad == id_terminal && x.Activo).ToList());

                foreach (var cliente in clientes_en_tuxpa_no_terminal_destino)
                {
                    if (!context.Cliente.Any(x => x.Id_Tad == id_terminal && !string.IsNullOrEmpty(x.Den) && x.Den.Equals(cliente.Den)))
                    {
                        //if (!clientes_nuevos.Any(x => x.Id_Tad == id_terminal && !string.IsNullOrEmpty(x.Den) && x.Den.Equals(cliente.Den)))
                        //{
                        var new_cliente = cliente.HardCopy();
                        new_cliente.Cod = 0;
                        new_cliente.Id_Tad = id_terminal;
                        //clientes_nuevos.Add(new_cliente);
                        context.Add(new_cliente);
                        await context.SaveChangesAsync();

                        context.Add(new Cliente_Tad() { Id_Cliente = new_cliente.Cod, Id_Terminal = id_terminal });
                        await context.SaveChangesAsync();
                        //}
                    }

                    var destinos_validos = destinos.Where(x => x.Codcte == cliente.Cod).ToList();

                    var destinos_en_tuxpan_no_terminal_destino = destinos_validos.Except(context.Destino.Where(x => x.Codcte == cliente.Cod && x.Id_Tad == id_terminal).ToList());

                    var cliente_bd = context.Cliente.First(x => !string.IsNullOrEmpty(x.Den) && x.Den.Equals(cliente.Den) && x.Id_Tad == id_terminal && x.Activo);

                    foreach (var destino in destinos_en_tuxpan_no_terminal_destino)
                    {
                        if (!context.Destino.Any(x => x.Id_Tad == id_terminal && !string.IsNullOrEmpty(x.Den) && x.Den.Equals(destino.Den)))
                        {
                            //if (!destinos_nuevos.Any(x => x.Id_Tad == id_terminal && !string.IsNullOrEmpty(x.Den) && x.Den.Equals(destino.Den)))
                            //{
                            var new_destino = destino.HardCopy();
                            new_destino.Cod = 0;
                            new_destino.Id_Tad = id_terminal;
                            new_destino.Codcte = cliente_bd.Cod;
                            //destinos_nuevos.Add(new_destino);
                            context.Add(new_destino);
                            await context.SaveChangesAsync();

                            context.Add(new Destino_Tad() { Id_Destino = new_destino.Cod, Id_Terminal = id_terminal });
                            await context.SaveChangesAsync();
                            //}
                        }
                    }
                }
                //copia de destino ligado a tuxpan

                //foreach (var destino in destinos)
                //{
                //    if (!context.Destino.Any(x => x.Id_Tad == id_terminal && !string.IsNullOrEmpty(x.Den) && x.Den.Equals(destino.Den)))
                //    {
                //        //if (!destinos_nuevos.Any(x => x.Id_Tad == id_terminal && !string.IsNullOrEmpty(x.Den) && x.Den.Equals(destino.Den)))
                //        //{
                //        var new_destino = destino.HardCopy();
                //        new_destino.Cod = 0;
                //        new_destino.Id_Tad = id_terminal;
                //        //destinos_nuevos.Add(new_destino);
                //        context.Add(new_destino);
                //        await context.SaveChangesAsync();

                //        context.Add(new Destino_Tad() { Id_Destino = new_destino.Cod, Id_Terminal = id_terminal });
                //        await context.SaveChangesAsync();
                //        //}
                //    }
                //}
                //copia de transportista, tonel y chofer liagados a tuxpan
                var transportistas = context.Transportista.IgnoreAutoIncludes().Where(x => x.Activo == true && x.Id_Tad == 1).ToList();
                var choferes = context.Chofer.IgnoreAutoIncludes().Where(x => x.Activo == true && x.Id_Tad == 1).ToList();
                var unidades = context.Tonel.IgnoreAutoIncludes().Where(x => x.Activo == true && x.Id_Tad == 1).ToList();

                List<Transportista> transportistas_nuevos = new();
                //List<Chofer> choferes_nuevos = new();
                //List<Tonel> unidades_nuevas = new();

                var transportistas_tuspan_no_terminal_destino = transportistas.Except(context.Transportista.Where(x => x.Id_Tad == id_terminal && x.Activo == true).ToList());

                foreach (var trans in transportistas_tuspan_no_terminal_destino)
                {
                    //List<Chofer> choferes_nuevos = new();
                    //List<Tonel> unidades_nuevas = new();

                    if (!context.Transportista.Any(x => x.Id_Tad == id_terminal && !string.IsNullOrEmpty(x.Den) && x.Den.Equals(trans.Den))
                        && !string.IsNullOrEmpty(trans.Busentid) && !string.IsNullOrEmpty(trans.CarrId))
                    {
                        //if (!transportistas_nuevos.Any(x => x.Id_Tad == id_terminal && !string.IsNullOrEmpty(x.Den) && x.Den.Equals(trans.Den)))
                        //{
                        var numero = GetRandomCarid();
                        var new_transpor = trans.HardCopy();


                        new_transpor.Cod = 0;
                        new_transpor.Id_Tad = id_terminal;

                        new_transpor.BusentId_Original = new_transpor.Busentid;
                        new_transpor.CarId_Original = new_transpor.CarrId;

                        new_transpor.CarrId = numero;
                        new_transpor.Busentid = numero;

                        //transportistas_nuevos.Add(new_transpor);
                        Debug.WriteLine($"numero_transportista: {numero}");
                        context.Add(new_transpor);
                        await context.SaveChangesAsync();

                        context.Add(new Transportista_Tad() { Id_Transportista = new_transpor.Cod, Id_Terminal = id_terminal });
                        await context.SaveChangesAsync();
                        //}

                    }
                    //else
                    //{
                    var transportista_terminal = context.Transportista.First(x => !string.IsNullOrEmpty(x.Den) && x.Den.Equals(trans.Den) && x.Id_Tad == id_terminal
                    && !string.IsNullOrEmpty(x.Busentid) && !string.IsNullOrEmpty(x.CarrId) && !string.IsNullOrEmpty(x.BusentId_Original) && !string.IsNullOrEmpty(x.CarId_Original));

                    var choferes_validos = choferes.Where(x => x.Codtransport == Convert.ToInt32(transportista_terminal.BusentId_Original)).ToList();

                    var choferes_tuxpan_no_terminal_destino = choferes_validos.Except(context.Chofer.Where(x => x.Codtransport == Convert.ToInt32(transportista_terminal.Busentid)).ToList());

                    var unidades_validas = unidades.Where(x => x.Carid == transportista_terminal.CarId_Original).ToList();

                    var unidad_tuxpan_no_terminal_destino = unidades_validas.Except(context.Tonel.Where(x => !string.IsNullOrEmpty(x.Carid) && x.Carid.Equals(transportista_terminal.CarrId)).ToList());

                    foreach (var chofer in choferes_tuxpan_no_terminal_destino)
                    {
                        if (!context.Chofer.Any(x => x.Id_Tad == id_terminal && !string.IsNullOrEmpty(x.Den) && !string.IsNullOrEmpty(x.Shortden) && x.Den.Equals(chofer.Den) && x.Shortden.Equals(chofer.Shortden)
                        && x.Codtransport == chofer.Codtransport))
                        {
                            //if (!choferes_nuevos.Any(x => x.Id_Tad == id_terminal && !string.IsNullOrEmpty(x.Den) && !string.IsNullOrEmpty(x.Shortden) && x.Den.Equals(chofer.Den) && x.Shortden.Equals(chofer.Shortden)
                            //&& x.Codtransport == chofer.Codtransport))
                            //{
                            var new_chofer = chofer.HardCopy();
                            new_chofer.Cod = 0;
                            new_chofer.Id_Tad = id_terminal;
                            new_chofer.Codtransport = Convert.ToInt32(transportista_terminal.Busentid);
                            //choferes_nuevos.Add(new_chofer);
                            context.Add(new_chofer);
                            await context.SaveChangesAsync();

                            context.Add(new Chofer_Tad() { Id_Chofer = new_chofer.Cod, Id_Terminal = id_terminal });
                            await context.SaveChangesAsync();
                            //}
                        }
                    }

                    foreach (var unidad in unidad_tuxpan_no_terminal_destino)
                    {
                        if (!context.Tonel.Any(x => x.Id_Tad == id_terminal && !string.IsNullOrEmpty(x.Tracto) && x.Tracto.Equals(unidad.Tracto)))
                        {
                            //if (!unidades_nuevas.Any(x => x.Id_Tad == id_terminal && !string.IsNullOrEmpty(x.Tracto) && x.Tracto.Equals(unidad.Tracto)))
                            //{
                            var new_unidad = unidad.HardCopy();
                            new_unidad.Cod = 0;
                            new_unidad.Id_Tad = id_terminal;
                            new_unidad.Carid = transportista_terminal.CarrId;
                            //unidades_nuevas.Add(new_unidad);
                            context.Add(new_unidad);
                            await context.SaveChangesAsync();

                            context.Add(new Unidad_Tad() { Id_Unidad = new_unidad.Cod, Id_Terminal = id_terminal });
                            await context.SaveChangesAsync();
                            //}
                        }
                    }
                    //}
                }

                //context.AddRange(clientes_nuevos);
                //context.AddRange(destinos_nuevos);
                //context.AddRange(transportistas_nuevos);
                //context.AddRange(choferes_nuevos);
                //context.AddRange(unidades_nuevas);

                //await context.SaveChangesAsync();

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
                var clientes = context.Cliente.Where(x => x.Id_Tad == terminal && x.Activo).ToList();

                List<Cliente_Tad> Clientes_validos = new();

                for (int i = 0; i < clientes.Count; i++)
                {
                    if (!context.Cliente.Any(x => !string.IsNullOrEmpty(x.Den) && x.Den == clientes[i].Den && x.Id_Tad == terminal_destino))
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
                }

                context.AddRange(Clientes_validos);
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
                var clientes = context.Cliente.Where(x => x.Id_Tad == terminal && x.Activo).Select(x => new { x.Cod, x.Den }).ToList();
                var destinos = context.Destino.Where(x => x.Id_Tad == terminal && x.Activo).ToList();

                List<Destino_Tad> destinos_validos = new();

                for (int i = 0; i < clientes.Count; i++)
                {
                    if (context.Cliente.Any(x => !string.IsNullOrEmpty(x.Den) && x.Den == clientes[i].Den && x.Id_Tad == terminal_destino))
                    {
                        var destinos_cliente = destinos.Where(x => x.Codcte == clientes[i].Cod).ToList();
                        var cliente = context.Cliente.FirstOrDefault(x => x.Den == clientes[i].Den && x.Id_Tad == terminal_destino);

                        for (int j = 0; j < destinos_cliente.Count; j++)
                        {
                            if (!context.Destino.Any(x => !string.IsNullOrEmpty(x.Den) && x.Den == destinos_cliente[j].Den && x.Id_Tad == terminal_destino))
                            {

                                if (cliente is not null)
                                {
                                    var new_destino = destinos_cliente[j].HardCopy();
                                    new_destino.Cod = 0;
                                    new_destino.Id_Tad = terminal_destino;
                                    new_destino.Codcte = cliente.Cod;

                                    Destino_Tad destino_Tad = new()
                                    {
                                        Id_Terminal = terminal_destino,
                                        Destino = new_destino,
                                        Terminal = null!
                                    };

                                    destinos_validos.Add(destino_Tad);
                                }
                            }
                        }
                    }
                }

                context.AddRange(destinos_validos);
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
                var transportistas = context.Transportista.Where(x => x.Id_Tad == terminal && x.Activo == true).ToList();

                List<Transportista_Tad> transportistas_validos = new();

                for (int i = 0; i < transportistas.Count; i++)
                {
                    if (!context.Transportista.Any(x => !string.IsNullOrEmpty(x.Den) && x.Den == transportistas[i].Den && x.Id_Tad == terminal_destino
                    && x.Busentid == transportistas[i].Busentid && x.CarrId == transportistas[i].CarrId))
                    {
                        var numero = GetRandomCarid();
                        var new_transpor = transportistas[i].HardCopy();


                        new_transpor.Cod = 0;
                        new_transpor.Id_Tad = terminal_destino;

                        new_transpor.BusentId_Original = new_transpor.Busentid;
                        new_transpor.CarId_Original = new_transpor.CarrId;

                        new_transpor.CarrId = numero;
                        new_transpor.Busentid = numero;

                        Transportista_Tad transportista_Tad = new()
                        {
                            Id_Terminal = terminal_destino,
                            Transportista = new_transpor,
                            Terminal = null!
                        };

                        transportistas_validos.Add(transportista_Tad);
                    }
                }

                context.AddRange(transportistas_validos);
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
                var transportistas = context.Transportista.Where(x => x.Id_Tad == terminal && x.Activo == true).Select(x => new { x.Cod, x.Den, x.Busentid }).ToList();
                var choferes = context.Chofer.Where(x => x.Id_Tad == terminal && x.Activo == true).ToList();

                List<Chofer_Tad> chofer_Tads = new();

                for (int i = 0; i < transportistas.Count; i++)
                {
                    if (context.Transportista.Any(x => !string.IsNullOrEmpty(x.Den) && x.Den == transportistas[i].Den && x.Id_Tad == terminal_destino))
                    {
                        var transportista = context.Transportista.Where(x => x.Den == transportistas[i].Den && x.Id_Tad == terminal_destino).Select(x => x.Busentid).FirstOrDefault();
                        if (!string.IsNullOrEmpty(transportista))
                        {
                            var choferes_transpor = choferes.Where(x => x.Codtransport.ToString() == transportistas[i].Busentid).ToList();

                            for (int j = 0; j < choferes_transpor.Count; j++)
                            {
                                if (!context.Chofer.Any(x => x.Id_Tad == terminal_destino && x.Den == choferes_transpor[j].Den && x.Shortden == choferes_transpor[j].Shortden
                                && x.Codtransport.ToString() == transportista))
                                {
                                    var new_chofer = choferes_transpor[j].HardCopy();
                                    new_chofer.Cod = 0;
                                    new_chofer.Id_Tad = terminal_destino;
                                    new_chofer.Codtransport = Convert.ToInt32(transportista);

                                    Chofer_Tad chofer_Tad = new()
                                    {
                                        Id_Terminal = terminal_destino,
                                        Chofer = new_chofer,
                                        Terminal = null!
                                    };

                                    chofer_Tads.Add(chofer_Tad);
                                }
                            }
                        }

                    }
                }

                context.AddRange(chofer_Tads);
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
                var transportistas = context.Transportista.Where(x => x.Id_Tad == terminal && x.Activo == true).Select(x => new { x.Cod, x.Den, x.CarrId }).ToList();
                var toneles = context.Tonel.Where(x => x.Id_Tad == terminal && x.Activo == true).ToList();

                List<Unidad_Tad> unidad_Tads = new();

                for (int i = 0; i < transportistas.Count; i++)
                {
                    if (context.Transportista.Any(x => !string.IsNullOrEmpty(x.Den) && x.Den == transportistas[i].Den && x.Id_Tad == terminal_destino))
                    {
                        var transportista = context.Transportista.Where(x => x.Den == transportistas[i].Den && x.Id_Tad == terminal_destino).Select(x => x.CarrId).FirstOrDefault();
                        if (!string.IsNullOrEmpty(transportista))
                        {
                            var toneles_transpor = toneles.Where(x => x.Carid == transportistas[i].CarrId).ToList();

                            for (int j = 0; j < toneles_transpor.Count; j++)
                            {
                                if (!context.Tonel.Any(x => x.Id_Tad == terminal_destino && !string.IsNullOrEmpty(x.Tracto) && x.Tracto == toneles_transpor[j].Tracto
                                && !string.IsNullOrEmpty(x.Placa) && x.Placa == toneles_transpor[j].Placa && !string.IsNullOrEmpty(x.Placatracto) && x.Placatracto == toneles_transpor[j].Placatracto
                                && x.Carid == transportista))
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
                            }
                        }

                    }
                }

                context.AddRange(unidad_Tads);
                await context.SaveChangesAsync();

                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        private string GetRandomCarid()
        {
            var random = new Random().Next(1, 100000);

            if (!context.Transportista.Any(x => !string.IsNullOrEmpty(x.CarrId) && x.CarrId.Equals(random)
                                             || !string.IsNullOrEmpty(x.Busentid) && x.Busentid.Equals(random)))
            {
                //if (!transportistas.Any(x => !string.IsNullOrEmpty(x.CarrId) && x.CarrId.Equals(random.ToString())))
                return random.ToString();
                //else
                //    GetRandomCarid(transportistas);
            }
            else
                GetRandomCarid();

            return string.Empty;
        }
    }
}
