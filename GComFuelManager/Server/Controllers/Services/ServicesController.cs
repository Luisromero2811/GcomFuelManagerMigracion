using GComFuelManager.Server.Helpers;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ServiceReference2;
using System.Diagnostics;
using System.ServiceModel;

namespace GComFuelManager.Server.Controllers.Services
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly VerifyUserToken verify;
        private readonly RequestToFile toFile;

        public ServicesController(ApplicationDbContext context, VerifyUserToken verify, RequestToFile toFile)
        {
            this.context = context;
            this.verify = verify;
            this.toFile = toFile;
        }

        [HttpPost("send")]
        public async Task<ActionResult> SendSynthesis([FromBody] List<OrdenCierre> ordens)
        {
            try
            {
                BillOfLadingServiceClient client = new BillOfLadingServiceClient(BillOfLadingServiceClient.EndpointConfiguration.BasicHttpBinding_BillOfLadingService);
                client.ClientCredentials.UserName.UserName = "energasws";
                client.ClientCredentials.UserName.Password = "Energas23!";
                client.Endpoint.Binding.ReceiveTimeout = TimeSpan.FromMinutes(10);

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
                        var bolguid = context.OrdenEmbarque.Find(item.CodPed);
                        if (bolguid == null)
                            return BadRequest();

                        if (string.IsNullOrEmpty(bolguid.Bolguidid))
                        {
                            request.BillOfLading.Destination.DestinationId.Id.Value = long.Parse(item.Destino!.Codsyn!);
                            request.BillOfLading.Customer.BusinessEntityId.Id.Value = long.Parse(item.Cliente!.Codsyn!);
                            request.BillOfLading.StartLoadTime.Value = item.OrdenEmbarque!.Fchcar!.Value;
                            request.BillOfLading.EndLoadTime.Value = item.OrdenEmbarque!.Fchcar.Value.AddDays(2);
                            var folio = context.OrdenEmbarque.OrderByDescending(X => X.Folio).FirstOrDefault()!.Folio;

                            if (folio == 0)
                                return BadRequest();
                            folio = folio + 1;
                            request.BillOfLading.CustomerReference = $"ENER-{folio}";

                            request.BillOfLading.BolStatus.Value = 34; // 34=SCHEDULED;12 = DRAFT;24 = PENDING

                            request.BillOfLading.PurchaseOrderRef = request.BillOfLading.CustomerReference;

                            request.BillOfLading.Driver.DriverId.Id.Value = long.Parse(item.OrdenEmbarque!.Chofer!.Dricod!);
                            request.BillOfLading.TruckCarrier.BusinessEntityId.Id.Value = long.Parse(item.OrdenEmbarque.Tonel!.Transportista!.CarrId!);

                            var pedidos = ordens.Where(x => x.OrdenEmbarque!.Codton == item.OrdenEmbarque.Codton
                            && x.OrdenEmbarque!.Codchf == item.OrdenEmbarque!.Codchf && x.OrdenEmbarque!.Fchcar == item.OrdenEmbarque!.Fchcar).ToList();

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

                                lineItem.TrailerId = item.OrdenEmbarque!.Tonel!.Codsyn.ToString();
                                lineItem.CompartmentId.Value = long.Parse(p.OrdenEmbarque!.CompartmentId.ToString()!);

                                lineItem.BaseNetQuantity.Value = 0M;
                                var vol = p.OrdenEmbarque!.Compartment == 1 ? p.OrdenEmbarque!.Tonel!.Capcom
                                    : p.OrdenEmbarque!.Compartment == 2 ? p.OrdenEmbarque!.Tonel!.Capcom2
                                    : p.OrdenEmbarque!.Compartment == 3 ? p.OrdenEmbarque!.Tonel!.Capcom3
                                    : p.OrdenEmbarque!.Tonel!.Capcom4;
                                lineItem.CustomerOrderQuantity.Value = decimal.Parse(vol.ToString()!);

                                lineItem.Customer.BusinessEntityId.Id.Value = long.Parse(p.Cliente!.Codsyn!);
                                lineItem.BaseProduct.ProductId.Id.Value = long.Parse(p.Producto!.Codsyn!);
                                lineItem.OrderedProduct.ProductId.Id.Value = long.Parse(p.Producto!.Codsyn!);
                                lineItem.EndLoadTime.Value = item.OrdenEmbarque!.Fchcar.Value.AddDays(2);

                                cfm.EntityKey = p.OrdenEmbarque!.Cod.ToString();
                                cfm.Name = p.OrdenEmbarque!.Cod.ToString();
                                cfm.EntityName = "BILL_OF_LADING_LINE_ITEM";
                                cfm.CustomFieldMetaDataId = new NLong();
                                cfm.CustomFieldMetaDataId.Value = 46;

                                lineItem.CustomFieldInstances[0].CustomFieldMetaData = cfm;
                                lineItem.CustomFieldInstances[0].FieldStringValue = $"{request.BillOfLading.CustomerReference}_{p.OrdenEmbarque!.Compartment}";
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
                                    x.OrdenEmbarque!.Bolguidid = billOfLading.BolGuidId;
                                    x.OrdenEmbarque!.Folio = folio;
                                });

                                var ordenembarque = pedidos.Select(x => x.OrdenEmbarque).ToList();

                                context.UpdateRange(ordenembarque!);
                                await context.SaveChangesAsync();
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
    }
}
