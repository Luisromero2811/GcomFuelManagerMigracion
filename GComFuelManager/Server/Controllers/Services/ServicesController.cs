using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ServiceReference7;
//using ServiceReference2;
using System.Diagnostics;
using System.ServiceModel;

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

        public ServicesController(ApplicationDbContext context, VerifyUserToken verify, RequestToFile toFile, UserManager<IdentityUsuario> userManager)
        {
            this.context = context;
            this.verify = verify;
            this.toFile = toFile;
            this.userManager = userManager;
        }

        #region envio de synthesis

        [HttpPost("send"), Authorize(Roles = "Programador, Admin, Administrador Sistema")]
        public async Task<ActionResult> SendSynthesis([FromBody] List<OrdenEmbarque> ordens)
        {
            try
            {
                BillOfLadingServiceClient client = new BillOfLadingServiceClient(BillOfLadingServiceClient.EndpointConfiguration.BasicHttpBinding_BillOfLadingService1);
                client.ClientCredentials.UserName.UserName = "energasws";
                client.ClientCredentials.UserName.Password = "Energas23!";
                client.Endpoint.Binding.SendTimeout = TimeSpan.FromMinutes(5);
                client.Endpoint.Binding.ReceiveTimeout = TimeSpan.FromMinutes(10);

                if (ordens is null)
                {
                    return BadRequest();
                }
                else
                {

                    foreach (var item in ordens)
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

                        var bolguid = context.OrdenEmbarque.Find(item.Cod);
                        if (bolguid == null)
                            return BadRequest();

                        if (string.IsNullOrEmpty(bolguid.Bolguidid))
                        {
                            request.BillOfLading.Destination.DestinationId.Id.Value = long.Parse(item.Destino!.Codsyn!);
                            request.BillOfLading.Customer.BusinessEntityId.Id.Value = long.Parse(item.Destino!.Cliente!.Codsyn!);
                            request.BillOfLading.StartLoadTime.Value = item.Fchcar!.Value;
                            request.BillOfLading.EndLoadTime.Value = item.Fchcar.Value.AddDays(2);
                            var folio = context.OrdenEmbarque.OrderByDescending(X => X.Folio).FirstOrDefault()!.Folio;

                            if (folio == 0)
                                return BadRequest();
                            folio++;
                            request.BillOfLading.CustomerReference = $"ENER-{folio}";

                            request.BillOfLading.BolStatus.Value = 34; // 34=SCHEDULED;12 = DRAFT;24 = PENDING

                            request.BillOfLading.PurchaseOrderRef = request.BillOfLading.CustomerReference;

                            request.BillOfLading.Driver.DriverId.Id.Value = long.Parse(item.Chofer!.Dricod!);
                            request.BillOfLading.TruckCarrier.BusinessEntityId.Id.Value = long.Parse(item.Tonel!.Transportista!.CarrId!);

                            var pedidos = ordens.Where(x => x!.Codton == item.Codton
                            && x!.Codchf == item.Codchf && x.Fchcar == item.Fchcar 
                            && string.IsNullOrEmpty(x.Bolguidid) && x.Codest == 3).ToList();

                            request.BillOfLading.LineItems = new BillOfLadingLineItem[pedidos.Count];
                            List<BillOfLadingLineItem> billOfLadingLineItems = new List<BillOfLadingLineItem>();
                            foreach (var p in pedidos)
                            {
                                BillOfLadingLineItem lineItem = new BillOfLadingLineItem();

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
                            }

                            request.BillOfLading.LineItems = billOfLadingLineItems.ToArray();

                            WsBillOfLadingResponse response = new WsBillOfLadingResponse();

                            if (request.BillOfLading.LineItems != null)
                            {
                                toFile.GenerateFile(JsonConvert.SerializeObject(request), $"Request_Synthesis_{DateTime.Now.ToString("ddMMyyyyHHmmss")}", $"{DateTime.Now.ToString("ddMMyyyy")}");

                                response = await client.SaveBillOfLadingAsync(request);

                                toFile.GenerateFile(JsonConvert.SerializeObject(response), $"Response_Synthesis_{DateTime.Now.ToString("ddMMyyyyHHmmss")}", $"{DateTime.Now.ToString("ddMMyyyy")}");


                            }
                            else
                                return BadRequest();

                            if (response != null && response.BillOfLadings != null && response.BillOfLadings.Length > 0)
                            {
                                BillOfLading billOfLading = response.BillOfLadings[0];

                                //pedidos.ForEach(x =>
                                //{
                                //    x.Bolguidid = billOfLading.BolGuidId;
                                //    x.Folio = folio;
                                //});

                                //var ordenembarque = pedidos.ToList();

                                //context.UpdateRange(ordenembarque!);

                                item.Bolguidid = billOfLading.BolGuidId;
                                item.Folio = folio;

                                context.Update(item);

                                var id = await verify.GetId(HttpContext, userManager);
                                if (string.IsNullOrEmpty(id))
                                    return BadRequest();

                                await context.SaveChangesAsync(id,10);
                            }
                        }
                    }

                    return Ok(true);
                }

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        #endregion

        #region re-envio de synthesis

        [HttpPost("resend"), Authorize(Roles = "Programador, Admin, Administrador Sistema")]
        public async Task<ActionResult> ResendSynthesis([FromBody] List<OrdenEmbarque> ordens)
        {
            try
            {
                BillOfLadingServiceClient client = new BillOfLadingServiceClient(BillOfLadingServiceClient.EndpointConfiguration.BasicHttpBinding_BillOfLadingService);
                client.ClientCredentials.UserName.UserName = "energasws";
                client.ClientCredentials.UserName.Password = "Energas23!";
                client.Endpoint.Binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
                client.Endpoint.Binding.SendTimeout = TimeSpan.FromMinutes(5);

                WsSaveBillOfLadingRequest request = new WsSaveBillOfLadingRequest();

                if (ordens is null)
                {
                    return BadRequest();
                }
                else
                {

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

                    foreach (var item in ordens)
                    {
                        var bolguid = context.OrdenEmbarque.Find(item.Cod);
                        if (bolguid == null)
                            return BadRequest();

                        request.BillOfLading.Destination.DestinationId.Id.Value = long.Parse(item.Destino!.Codsyn!);
                        request.BillOfLading.Customer.BusinessEntityId.Id.Value = long.Parse(item.Destino!.Cliente!.Codsyn!);
                        request.BillOfLading.StartLoadTime.Value = item.Fchcar!.Value;
                        request.BillOfLading.EndLoadTime.Value = item.Fchcar.Value.AddDays(2);
                        var folio = context.OrdenEmbarque.OrderByDescending(X => X.Folio).FirstOrDefault()!.Folio;

                        if (folio == 0)
                            return BadRequest();
                        folio = folio + 1;
                        request.BillOfLading.CustomerReference = $"ENER-{folio}";

                        request.BillOfLading.BolStatus.Value = 34; // 34=SCHEDULED;12 = DRAFT;24 = PENDING

                        request.BillOfLading.PurchaseOrderRef = request.BillOfLading.CustomerReference;

                        request.BillOfLading.Driver.DriverId.Id.Value = long.Parse(item.Chofer!.Dricod!);
                        request.BillOfLading.TruckCarrier.BusinessEntityId.Id.Value = long.Parse(item.Tonel!.Transportista!.CarrId!);

                        var pedidos = ordens.Where(x => x!.Codton == item.Codton
                        && x!.Codchf == item.Codchf && x.Fchcar == item.Fchcar).ToList();

                        request.BillOfLading.LineItems = new BillOfLadingLineItem[pedidos.Count];
                        List<BillOfLadingLineItem> billOfLadingLineItems = new List<BillOfLadingLineItem>();
                        foreach (var p in pedidos)
                        {
                            BillOfLadingLineItem lineItem = new BillOfLadingLineItem();

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
                        }

                        request.BillOfLading.LineItems = billOfLadingLineItems.ToArray();

                        WsBillOfLadingResponse response = new WsBillOfLadingResponse();

                        if (request.BillOfLading.LineItems != null)
                        {
                            toFile.GenerateFile(JsonConvert.SerializeObject(request), $"Request_Synthesis_{DateTime.Now.ToString("ddMMyyyyHHmmss")}", $"{DateTime.Now.ToString("ddMMyyyy")}");

                            response = await client.SaveBillOfLadingAsync(request);

                            toFile.GenerateFile(JsonConvert.SerializeObject(response), $"Response_Synthesis_{DateTime.Now.ToString("ddMMyyyyHHmmss")}", $"{DateTime.Now.ToString("ddMMyyyy")}");


                        }
                        else
                            return BadRequest();

                        if (response != null && response.BillOfLadings != null && response.BillOfLadings.Length > 0)
                        {
                            BillOfLading billOfLading = response.BillOfLadings[0];

                            pedidos.ForEach(x =>
                            {
                                x.Bolguidid = billOfLading.BolGuidId;
                                x.Folio = folio;
                            });

                            var ordenembarque = pedidos.ToList();

                            context.UpdateRange(ordenembarque!);

                            var id = await verify.GetId(HttpContext, userManager);
                            if (string.IsNullOrEmpty(id))
                                return BadRequest();

                            await context.SaveChangesAsync(id,21);
                        }
                    }

                    return Ok(true);
                }

            }
            catch (Exception e)
            {
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
                ServiceReference7.BillOfLadingServiceClient client = new ServiceReference7.BillOfLadingServiceClient(ServiceReference7.BillOfLadingServiceClient.EndpointConfiguration.BasicHttpBinding_BillOfLadingService);
                client.ClientCredentials.UserName.UserName = "energasws";
                client.ClientCredentials.UserName.Password = "Energas23!";
                client.Endpoint.Binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
                client.Endpoint.Binding.SendTimeout = TimeSpan.FromMinutes(5);

                ServiceReference7.WsGetBillOfLadingsRequest request = new ServiceReference7.WsGetBillOfLadingsRequest();

                request.IncludeChildObjects = new ServiceReference7.NBool();
                request.IncludeChildObjects.Value = true;

                request.EndLoadDateFrom = new ServiceReference7.NDateTime();
                request.EndLoadDateFrom.Value = DateTime.Today.AddDays(-1);

                request.EndLoadDateTo = new ServiceReference7.NDateTime();
                request.EndLoadDateTo.Value = request.EndLoadDateFrom.Value.AddDays(2);

                request.ShipperId = new ServiceReference7.Identifier();
                request.ShipperId.Id = new ServiceReference7.NLong();
                request.ShipperId.Id.Value = 51004;

                request.DeliveryReceiptIndicator = new ServiceReference7.NInt();
                request.DeliveryReceiptIndicator.Value = 1;

                request.BolStatus = new ServiceReference7.NEnumOfOrderStatusEnum();
                request.BolStatus.Value = ServiceReference7.OrderStatusEnum.COMPLETED;

                request.IncludeCustomFields = new ServiceReference7.NBool();
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

                                    if (tonel is null) return BadRequest($"No existe el tonel. Codigo synthesis: {orden.Coduni}");

                                    orden.Coduni = tonel.Cod;

                                    var tran = context.Transportista.FirstOrDefault(x => x.CarrId == tonel.Carid);

                                    if (tran is null) return BadRequest($"No existe el transportista. Carid transportista: {tonel.Carid}");

                                    var cho = context.Chofer.FirstOrDefault(x => x.Dricod == orden.Codchfsyn.ToString() && x.Codtransport == Convert.ToInt32(tran.Busentid));

                                    if (cho is null) return BadRequest($"No existe el chofer. Dricod chofer: {orden.Codchfsyn}. transportista: {tran.Busentid}");

                                    orden.Codchf = cho.Cod;

                                    var prd = context.Producto.FirstOrDefault(x => x.Codsyn == orden.Codprdsyn.ToString());

                                    if (prd is null) return BadRequest($"No existe el producto. Codigo synthesis: {orden.Codprdsyn}");

                                    orden.Codprd = prd.Cod;

                                    var prd2 = context.Producto.FirstOrDefault(x => x.Codsyn == orden.Codprd2syn.ToString());

                                    if (prd2 is null) return BadRequest($"No existe el producto. Codigo synthesis: {orden.Codprd2syn}");

                                    orden.Codprd2 = prd2.Cod;

                                    orden.Fch = DateTime.Now;
                                    orden.Codest = 20;

                                    context.Add(orden);
                                }
                            }
                        }
                    }
                    await context.SaveChangesAsync();
                }
                return Ok();

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        #endregion

        #region obtencion de ordenes canceladas
        [HttpGet("canceladas")]
        public async Task<ActionResult> GetOrdenesCanceladas()
        {
            try
            {
                ServiceReference7.BillOfLadingServiceClient client = new ServiceReference7.BillOfLadingServiceClient(ServiceReference7.BillOfLadingServiceClient.EndpointConfiguration.BasicHttpBinding_BillOfLadingService);
                client.ClientCredentials.UserName.UserName = "energasws";
                client.ClientCredentials.UserName.Password = "Energas23!";
                client.Endpoint.Binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
                client.Endpoint.Binding.SendTimeout = TimeSpan.FromMinutes(5);

                ServiceReference7.WsGetBillOfLadingsRequest request = new ServiceReference7.WsGetBillOfLadingsRequest();

                request.IncludeChildObjects = new ServiceReference7.NBool();
                request.IncludeChildObjects.Value = true;

                request.EndLoadDateFrom = new ServiceReference7.NDateTime();
                request.EndLoadDateFrom.Value = DateTime.Today.AddDays(-1);

                request.EndLoadDateTo = new ServiceReference7.NDateTime();
                request.EndLoadDateTo.Value = request.EndLoadDateFrom.Value.AddDays(2);

                request.ShipperId = new ServiceReference7.Identifier();
                request.ShipperId.Id = new ServiceReference7.NLong();
                request.ShipperId.Id.Value = 51004;

                request.DeliveryReceiptIndicator = new ServiceReference7.NInt();
                request.DeliveryReceiptIndicator.Value = 1;

                request.BolStatus = new ServiceReference7.NEnumOfOrderStatusEnum();
                request.BolStatus.Value = ServiceReference7.OrderStatusEnum.REJECTED;

                request.IncludeCustomFields = new ServiceReference7.NBool();
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

                                    if (tonel is null) return BadRequest($"No existe el tonel. Codigo synthesis: {orden.Coduni}");

                                    orden.Coduni = tonel.Cod;

                                    var tran = context.Transportista.FirstOrDefault(x => x.CarrId == tonel.Carid);

                                    if (tran is null) return BadRequest($"No existe el transportista. Carid transportista: {tonel.Carid}");

                                    var cho = context.Chofer.FirstOrDefault(x => x.Dricod == orden.Codchfsyn.ToString() && x.Codtransport == Convert.ToInt32(tran.Busentid));

                                    if (cho is null) return BadRequest($"No existe el chofer. Dricod chofer: {orden.Codchfsyn}. transportista: {tran.Busentid}");

                                    orden.Codchf = cho.Cod;

                                    var prd = context.Producto.FirstOrDefault(x => x.Codsyn == orden.Codprdsyn.ToString());

                                    if (prd is null) return BadRequest($"No existe el producto. Codigo synthesis: {orden.Codprdsyn}");

                                    orden.Codprd = prd.Cod;

                                    var prd2 = context.Producto.FirstOrDefault(x => x.Codsyn == orden.Codprd2syn.ToString());

                                    if (prd2 is null) return BadRequest($"No existe el producto. Codigo synthesis: {orden.Codprd2syn}");

                                    orden.Codprd2 = prd2.Cod;

                                    orden.Fch = DateTime.Now;
                                    orden.Codest = 14;

                                    context.Add(orden);
                                }
                            }
                        }
                    }
                    await context.SaveChangesAsync();
                }
                return Ok();

            }
            catch (Exception e)
            {
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

                    x.Folio = randomNumber;
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

        #region Simulacion Cargadas#
        [HttpGet]
        [Route("simulacion/cargadas")]
        public async Task<ActionResult> SimulacionObtener()
        {
            try
            {
                var ordenes = await context.OrdenEmbarque.Where(x => x.FchOrd >= DateTime.Today.AddDays(-3) && x.FchOrd <= DateTime.Today && x.Folio != null && x.Bolguidid == null)
                    .Include(x => x.Destino)
                    .ToListAsync();

                if (ordenes is null)
                    return NoContent();

                ordenes.ForEach(x =>
                {
                    Guid guid = Guid.NewGuid();

                    x.Bolguidid = guid.ToString();

                    context.Update(x);
                });

                foreach (var item in ordenes)
                {
                    Random random = new Random();

                    Orden orden = new Orden()
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
                        Liniteid = random.Next(1, 100001)
                    };

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

        [HttpGet]
        public async Task<ActionResult> CerrarOrdenes()
        {
            try
            {

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
