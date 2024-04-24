using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Pkcs;
using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.Layout;
using PdfSharpCore.Pdf;
using System.Drawing;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class PdfController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUsuario> userManager;
        private readonly VerifyUserToken verifyUser;
        private readonly User_Terminal _terminal;

        public PdfController(ApplicationDbContext context, UserManager<IdentityUsuario> userManager, VerifyUserToken verifyUser, User_Terminal user_Terminal)
        {
            this.context = context;
            this.userManager = userManager;
            this.verifyUser = verifyUser;
            this._terminal = user_Terminal;
        }

        [HttpPost("vale")]
        public async Task<ActionResult> Obtener_Vale([FromBody] OrdenEmbarque orden)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                //x 540 - y 720 (con margen) x 612 - y 792 (completa) -- tamaño carta
                PdfDocument pdfDocument = new();

                PdfPage page = pdfDocument.AddPage();
                XGraphics graphics = XGraphics.FromPdfPage(page);
                XFont font = new("Arial", 11, XFontStyle.Regular);
                XFont font_valor = new("Arial", 10, XFontStyle.Regular);
                XFont font_bold = new("Arial", 11, XFontStyle.Bold);
                XTextFormatter tf = new(graphics);
                XPen pen = new(XColors.Black, .8);
                XRect rect = new();

                var ordenEmbarque = context.OrdenEmbarque.IgnoreAutoIncludes().FirstOrDefault(x => x.Cod == orden.Cod);
                if (ordenEmbarque is null) { return BadRequest("No se encontro la orden."); }

                ordenEmbarque.Id_Autorizador = orden.Id_Autorizador;
                ordenEmbarque.Id_Multidestino = orden.Id_Multidestino;

                if (ordenEmbarque.Folio_Vale is null || ordenEmbarque.Folio_Vale == 0)
                {
                    var consecutivo = context.Consecutivo.Include(x => x.Terminal).FirstOrDefault(x => x.Nombre == "Vale" && x.Id_Tad == id_terminal);
                    if (consecutivo is null)
                    {
                        Consecutivo Nuevo_Consecutivo = new() { Numeracion = 1, Nombre = "Vale", Id_Tad = id_terminal };
                        context.Add(Nuevo_Consecutivo);
                        await context.SaveChangesAsync();
                        consecutivo = Nuevo_Consecutivo;

                        ordenEmbarque.Folio_Vale = consecutivo.Numeracion;
                    }
                    else
                    {
                        consecutivo.Numeracion++;
                        context.Update(consecutivo);
                        await context.SaveChangesAsync();

                        ordenEmbarque.Folio_Vale = consecutivo.Numeracion;
                    }
                }

                //ordenEmbarque.Producto = null;
                //ordenEmbarque.Chofer = null;
                //ordenEmbarque.Destino = null;
                //ordenEmbarque.Tonel = null;
                //ordenEmbarque.Tad = null;
                //ordenEmbarque.OrdenCompra = null;
                //ordenEmbarque.Estado = null;
                //ordenEmbarque.Cliente = null!;
                //ordenEmbarque.OrdenCierre = null!;
                //ordenEmbarque.OrdenPedido = null!;

                context.Update(ordenEmbarque);
                await context.SaveChangesAsync();

                #region Consulta

                var terminal = context.Tad.FirstOrDefault(x => x.Cod == ordenEmbarque.Codtad);

                if (terminal is null) { return BadRequest("No se encontro la terminal"); }
                if (string.IsNullOrEmpty(terminal.Den) || string.IsNullOrWhiteSpace(terminal.Den)) { return BadRequest("No se encontro la terminal"); }

                var producto = context.Producto.FirstOrDefault(x => x.Cod == ordenEmbarque.Codprd);
                if (producto is null) { return BadRequest("No se encontro el producto"); }
                if (string.IsNullOrEmpty(producto.Den) || string.IsNullOrWhiteSpace(producto.Den)) { return BadRequest("No se encontro el producto"); }

                var tonel = context.Tonel.FirstOrDefault(x => x.Cod == ordenEmbarque.Codton);
                if (tonel is null) { return BadRequest("No se encontro el tonel"); }
                if (string.IsNullOrEmpty(tonel.Certificado_Calibracion) || string.IsNullOrWhiteSpace(tonel.Certificado_Calibracion))
                { return BadRequest($"El tonel {tonel.Tracto} no cuanta con un certificado de calibracion"); }

                var transportista = context.Transportista.FirstOrDefault(x => x.CarrId == tonel.Carid);
                if (transportista is null) { return BadRequest("No se encontro el transportista"); }
                if (string.IsNullOrEmpty(transportista.Den) || string.IsNullOrWhiteSpace(transportista.Den)) { return BadRequest("No se encontro el producto"); }

                var chofer = context.Chofer.FirstOrDefault(x => x.Cod == ordenEmbarque.Codchf);
                if (chofer is null) { return BadRequest("No se encontro un chofer"); }
                if (string.IsNullOrEmpty(chofer.RFC) || string.IsNullOrWhiteSpace(chofer.RFC)) { return BadRequest($"El chofer {chofer.FullName} no cuenta con el RFC"); }

                var destino = context.Destino.FirstOrDefault(x => x.Cod == ordenEmbarque.Id_Multidestino);
                if (destino is null) { return BadRequest("No se encontro el destino"); }

                var producto_anterior = producto.Den;
                //var producto_anterior = string.Empty;
                //var orden_anterior = context.OrdenEmbarque.Where(x => x.Codton == ordenEmbarque.Codton && x.Compartment == ordenEmbarque.Compartment && x.Cod != orden.Cod
                //&& x.Fchcar < orden.Fchcar).OrderByDescending(x => x.Fchpet).FirstOrDefault();
                //if (orden_anterior is not null)
                //{
                //    var prod_anterior = context.Producto.FirstOrDefault(x => x.Cod == orden_anterior.Codprd);
                //    if (prod_anterior is not null)
                //    {
                //        producto_anterior = prod_anterior.Den;
                //    }
                //}

                var autorizador = context.Autorizador.FirstOrDefault(x => x.Cod == ordenEmbarque.Id_Autorizador);
                if (autorizador is null) { return BadRequest("No se encontro el autorizador"); }

                #endregion

                #region imagen de mgc
                string path = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot/img/MGC_Icon.png");
                XImage xImage = XImage.FromFile(path);
                graphics.DrawImage(xImage, 36, 36);
                #endregion

                #region Folio de vale
                rect = new(465, 65, 40, 13);

                tf.DrawString("Folio:", font_bold, XBrushes.Red, rect, XStringFormats.TopLeft);

                rect = new(505, 65, 80, 13);
                tf.DrawString(ordenEmbarque.Folio_Vale.ToString(), font, XBrushes.Red, rect, XStringFormats.TopLeft);
                #endregion

                #region Datos terminal
                rect = new(36, 111, 220, 15);
                tf.DrawString("Pemex Transformacion Industrial", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new(36, 121, 220, 13);
                tf.DrawString(terminal.Den, font, XBrushes.Black, rect, XStringFormats.TopLeft);
                #endregion

                #region Datos inicio

                rect = new(31, 141, 120, 13);
                tf.DrawString("ATENCION", font_bold, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new(31, 154, 120, 13);
                tf.DrawString("ASESOR COMERCIAL", font_bold, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new(31, 167, 120, 13);
                tf.DrawString("PRESENTE", font_bold, XBrushes.Black, rect, XStringFormats.TopLeft);
                #endregion

                #region Datos de fecha y clave cliente
                XFont font_vale = new("Arial", 16, XFontStyle.Bold);

                rect = new(36, 195, 280, 25);
                tf.Alignment = XParagraphAlignment.Center;
                tf.DrawString("VALE DE RETIRO", font_vale, XBrushes.Black, rect, XStringFormats.TopLeft);

                tf.Alignment = XParagraphAlignment.Left;
                rect = new(318, 205, 38, 13);
                tf.DrawString("FECHA:", font_bold, XBrushes.Black, rect, XStringFormats.TopLeft);

                tf.Alignment = XParagraphAlignment.Center;
                rect = new(380, 205, 190, 13);
                tf.DrawString(DateTime.Today.ToString("D"), font_valor, XBrushes.Black, rect, XStringFormats.TopLeft);

                tf.Alignment = XParagraphAlignment.Left;

                graphics.DrawLine(pen, 380, 217, 540, 217);


                rect = new(318, 220, 135, 13);
                tf.DrawString("CLAVE DE CLIENTE SIIC:", font_bold, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new(455, 220, 122, 13);
                tf.DrawString("0000202967", font_bold, XBrushes.Black, rect, XStringFormats.TopLeft);

                graphics.DrawLine(pen, 455, 232, 540, 232);
                #endregion

                #region Solicitud de producto
                rect = new(36, 245, 280, 13);
                tf.DrawString("SOLICITO A USTED EL RETIRO DEL PRODUCTO:", font_bold, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new(36, 258, 280, 13);
                tf.DrawString("CONDICIONES DE ENTREGA L.A.B. LLENADERAS", font_bold, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new(316, 245, 260, 13);
                tf.Alignment = XParagraphAlignment.Center;
                tf.DrawString(producto.Den, font_valor, XBrushes.Black, rect, XStringFormats.TopLeft);
                tf.Alignment = XParagraphAlignment.Left;

                graphics.DrawLine(pen, 316, 257, 540, 257);
                #endregion

                #region Inicio detalle
                rect = new(36, 272, 540, 13);
                tf.DrawString("LINEA TRANSPORTISTA AUTORIZADA A RETIRAR EL PRODUCTO:", font, XBrushes.Black, rect, XStringFormats.TopLeft);
                #endregion

                #region Empresa transportista
                rect = new(36, 285, 160, 13);
                tf.DrawString("EMPRESA TRANSPORTISTA:", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new(210, 285, 360, 13);
                tf.DrawString(transportista.Den, font_valor, XBrushes.Black, rect, XStringFormats.TopLeft);

                graphics.DrawLine(pen, 210, 298, 540, 298);
                #endregion

                #region Nombre del operador
                rect = new(36, 300, 150, 13);
                tf.DrawString("NOMBRE DEL OPERADOR:", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new(210, 300, 360, 13);
                tf.DrawString(chofer.FullName, font_valor, XBrushes.Black, rect, XStringFormats.TopLeft);

                graphics.DrawLine(pen, 210, 313, 540, 313);
                #endregion

                #region RFC del operador
                rect = new(36, 316, 150, 13);
                tf.DrawString("RFC DEL OPERADOR:", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new(210, 316, 360, 13);
                tf.DrawString(chofer.RFC, font_valor, XBrushes.Black, rect, XStringFormats.TopLeft);

                graphics.DrawLine(pen, 210, 329, 540, 329);
                #endregion

                #region Numero del equipo
                rect = new(36, 332, 150, 13);
                tf.DrawString("NÚMERO DE EQUIPO:", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new(210, 332, 360, 13);
                tf.DrawString(tonel.Tracto, font_valor, XBrushes.Black, rect, XStringFormats.TopLeft);

                graphics.DrawLine(pen, 210, 345, 540, 345);
                #endregion

                #region Capacidad del equipo
                rect = new(36, 348, 150, 13);
                tf.DrawString("CAPACIDAD DEL EQUIPO:", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new(210, 348, 360, 13);
                tf.DrawString($"{tonel.Tanque} Compartimento {ordenEmbarque.Compartment} - {ordenEmbarque.Obtener_Volumen_De_Orden()}", font_valor, XBrushes.Black, rect, XStringFormats.TopLeft);

                graphics.DrawLine(pen, 210, 361, 540, 361);
                #endregion

                #region Numero de certificado
                rect = new(36, 364, 150, 13);
                tf.DrawString("NUM. DE CERTIFICADO:", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new(210, 364, 360, 13);
                tf.DrawString(tonel.Certificado_Calibracion, font_valor, XBrushes.Black, rect, XStringFormats.TopLeft);

                graphics.DrawLine(pen, 210, 377, 540, 377);
                #endregion

                #region Numero de placas
                rect = new(36, 380, 150, 13);
                tf.DrawString("NUM. DE PLACAS:", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new(210, 380, 360, 13);
                tf.DrawString(tonel.Placa, font_valor, XBrushes.Black, rect, XStringFormats.TopLeft);

                graphics.DrawLine(pen, 210, 393, 540, 393);
                #endregion

                #region Producto anterior
                rect = new(36, 396, 305, 13);
                tf.DrawString("PRODUCTO QUE TRANSPORTÓ EN EL VIAJE ANTERIOR:", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new(343, 396, 230, 13);
                tf.DrawString(producto_anterior, font_valor, XBrushes.Black, rect, XStringFormats.TopLeft);

                graphics.DrawLine(pen, 343, 409, 540, 409);
                #endregion

                #region Destino
                rect = new(36, 412, 70, 13);
                tf.DrawString("DESTINO:", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new(105, 412, 470, 26);
                tf.DrawString(destino.Den, font_valor, XBrushes.Black, rect, XStringFormats.TopLeft);

                graphics.DrawLine(pen, 105, 425, 540, 425);
                #endregion

                #region Firma de chofer
                rect = new(36, 440, 540, 26);
                tf.Alignment = XParagraphAlignment.Justify;
                tf.DrawString("NOMBRE Y FIRMA DEL OPERADOR AUTORIZADO PARA FIRMAR LA REMISIÓN POR LA RECEPCIÓN  DEL PRODUCTO:", font, XBrushes.Black, rect, XStringFormats.TopLeft);
                tf.Alignment = XParagraphAlignment.Left;

                rect = new(106, 503, 400, 13);
                tf.Alignment = XParagraphAlignment.Center;
                tf.DrawString(chofer.FullName, font_valor, XBrushes.Black, rect, XStringFormats.TopLeft);
                tf.Alignment = XParagraphAlignment.Left;

                graphics.DrawLine(pen, 106, 518, 506, 518);
                #endregion

                #region Firma de encargado
                tf.Alignment = XParagraphAlignment.Justify;
                rect = new(36, 540, 540, 26);
                tf.DrawString("PEMEX TRANSFORMACIÓN INDUSTRIAL QUEDA LIBERADO DE TODA RESPONSABILIDAD POR  CUALQUIER EVENTUALIDAD QUE PUDIERA OCURRIR DURANTE EL TRANSPORTE DEL PRODUCTO", font, XBrushes.Black, rect, XStringFormats.TopLeft);
                tf.Alignment = XParagraphAlignment.Center;

                rect = new(226, 588, 160, 13);
                tf.DrawString("ATENTAMENTE", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new(106, 660, 400, 13);
                tf.DrawString(autorizador.Den, font_valor, XBrushes.Black, rect, XStringFormats.TopLeft);
                tf.Alignment = XParagraphAlignment.Left;

                graphics.DrawLine(pen, 106, 675, 506, 675);

                rect = new(66, 685, 480, 13);
                tf.DrawString("NOMBRE Y FIRMA DE PERSONA AUTORIZADA PARA FIRMAR LOS VALES DE RETIRO", font, XBrushes.Black, rect, XStringFormats.TopLeft);
                #endregion

                #region acerca de
                rect = new(36, 723, 540, 26);
                tf.DrawString("MGC Mexico, S.A. de C.V. Lago Zurich 245 Piso 7, Col. Ampliacion Granada, Alcaldia Miguel Hidalgo, Ciudad de Mexico, CP  11529, Telefono (55) 4122 - 1532", font, XBrushes.Black, rect, XStringFormats.TopLeft);
                #endregion

                var ms = new System.IO.MemoryStream();
                pdfDocument.Save(ms);

                return Ok(ms.ToArray());
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("vale/multiple")]
        public async Task<ActionResult> Obtener_Multiples_Vales([FromBody] List<OrdenEmbarque> orden)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                //x 540 - y 720 (con margen) x 612 - y 792 (completa) -- tamaño carta
                PdfDocument pdfDocument = new();

                for (int i = 0; i < orden.Count; i++)
                {
                    PdfPage page = pdfDocument.AddPage();
                    XGraphics graphics = XGraphics.FromPdfPage(page);
                    XFont font = new("Arial", 11, XFontStyle.Regular);
                    XFont font_valor = new("Arial", 10, XFontStyle.Regular);
                    XFont font_bold = new("Arial", 11, XFontStyle.Bold);
                    XTextFormatter tf = new(graphics);
                    XPen pen = new(XColors.Black, .8);
                    XRect rect = new();

                    var ordenEmbarque = context.OrdenEmbarque.IgnoreAutoIncludes().FirstOrDefault(x => x.Cod == orden[i].Cod);
                    if (ordenEmbarque is null) { return BadRequest("No se encontro la orden."); }

                    ordenEmbarque.Id_Autorizador = orden[i].Id_Autorizador;
                    ordenEmbarque.Id_Multidestino = orden[i].Id_Multidestino;

                    if (ordenEmbarque.Folio_Vale is null || ordenEmbarque.Folio_Vale == 0)
                    {
                        var consecutivo = context.Consecutivo.Include(x => x.Terminal).FirstOrDefault(x => x.Nombre == "Vale" && x.Id_Tad == id_terminal);
                        if (consecutivo is null)
                        {
                            Consecutivo Nuevo_Consecutivo = new() { Numeracion = 1, Nombre = "Vale", Id_Tad = id_terminal };
                            context.Add(Nuevo_Consecutivo);
                            await context.SaveChangesAsync();
                            consecutivo = Nuevo_Consecutivo;

                            ordenEmbarque.Folio_Vale = consecutivo.Numeracion;
                        }
                        else
                        {
                            consecutivo.Numeracion++;
                            context.Update(consecutivo);
                            await context.SaveChangesAsync();

                            ordenEmbarque.Folio_Vale = consecutivo.Numeracion;
                        }
                    }

                    //ordenEmbarque.Producto = null;
                    //ordenEmbarque.Chofer = null;
                    //ordenEmbarque.Destino = null;
                    //ordenEmbarque.Tonel = null;
                    //ordenEmbarque.Tad = null;
                    //ordenEmbarque.OrdenCompra = null;
                    //ordenEmbarque.Estado = null;
                    //ordenEmbarque.Cliente = null!;
                    //ordenEmbarque.OrdenCierre = null!;
                    //ordenEmbarque.OrdenPedido = null!;

                    context.Update(ordenEmbarque);
                    await context.SaveChangesAsync();

                    #region Consulta

                    var terminal = context.Tad.FirstOrDefault(x => x.Cod == ordenEmbarque.Codtad);

                    if (terminal is null) { return BadRequest("No se encontro la terminal"); }
                    if (string.IsNullOrEmpty(terminal.Den) || string.IsNullOrWhiteSpace(terminal.Den)) { return BadRequest("No se encontro la terminal"); }

                    var producto = context.Producto.FirstOrDefault(x => x.Cod == ordenEmbarque.Codprd);
                    if (producto is null) { return BadRequest("No se encontro el producto"); }
                    if (string.IsNullOrEmpty(producto.Den) || string.IsNullOrWhiteSpace(producto.Den)) { return BadRequest("No se encontro el producto"); }

                    var tonel = context.Tonel.FirstOrDefault(x => x.Cod == ordenEmbarque.Codton);
                    if (tonel is null) { return BadRequest("No se encontro el tonel"); }
                    if (string.IsNullOrEmpty(tonel.Certificado_Calibracion) || string.IsNullOrWhiteSpace(tonel.Certificado_Calibracion))
                    { return BadRequest($"El tonel {tonel.Tracto} no cuanta con un certificado de calibracion"); }

                    var transportista = context.Transportista.FirstOrDefault(x => x.CarrId == tonel.Carid);
                    if (transportista is null) { return BadRequest("No se encontro el transportista"); }
                    if (string.IsNullOrEmpty(transportista.Den) || string.IsNullOrWhiteSpace(transportista.Den)) { return BadRequest("No se encontro el producto"); }

                    var chofer = context.Chofer.FirstOrDefault(x => x.Cod == ordenEmbarque.Codchf);
                    if (chofer is null) { return BadRequest("No se encontro un chofer"); }
                    if (string.IsNullOrEmpty(chofer.RFC) || string.IsNullOrWhiteSpace(chofer.RFC)) { return BadRequest($"El chofer {chofer.FullName} no cuenta con el RFC"); }

                    var destino = context.Destino.FirstOrDefault(x => x.Cod == ordenEmbarque.Id_Multidestino);
                    if (destino is null) { return BadRequest("No se encontro el destino"); }

                    var producto_anterior = string.Empty;
                    var orden_anterior = context.OrdenEmbarque.Where(x => x.Codton == ordenEmbarque.Codton && x.Compartment == ordenEmbarque.Compartment && x.Cod != orden[i].Cod
                    && x.Fchcar < orden[i].Fchcar).OrderByDescending(x => x.Fchpet).FirstOrDefault();
                    if (orden_anterior is not null)
                    {
                        var prod_anterior = context.Producto.FirstOrDefault(x => x.Cod == orden_anterior.Codprd);
                        if (prod_anterior is not null)
                        {
                            producto_anterior = prod_anterior.Den;
                        }
                    }

                    var autorizador = context.Autorizador.FirstOrDefault(x => x.Cod == ordenEmbarque.Id_Autorizador);
                    if (autorizador is null) { return BadRequest("No se encontro el autorizador"); }

                    #endregion

                    #region imagen de mgc
                    string path = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot/img/MGC_Icon.png");
                    XImage xImage = XImage.FromFile(path);
                    graphics.DrawImage(xImage, 36, 36);
                    #endregion

                    #region Folio de vale
                    rect = new(465, 65, 40, 13);

                    tf.DrawString("Folio:", font_bold, XBrushes.Red, rect, XStringFormats.TopLeft);

                    rect = new(505, 65, 80, 13);
                    tf.DrawString(ordenEmbarque.Folio_Vale.ToString(), font, XBrushes.Red, rect, XStringFormats.TopLeft);
                    #endregion

                    #region Datos terminal
                    rect = new(36, 111, 220, 15);
                    tf.DrawString("Pemex Transformacion Industrial", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                    rect = new(36, 121, 220, 13);
                    tf.DrawString(terminal.Den, font, XBrushes.Black, rect, XStringFormats.TopLeft);
                    #endregion

                    #region Datos inicio

                    rect = new(31, 141, 120, 13);
                    tf.DrawString("ATENCION", font_bold, XBrushes.Black, rect, XStringFormats.TopLeft);

                    rect = new(31, 154, 120, 13);
                    tf.DrawString("ASESOR COMERCIAL", font_bold, XBrushes.Black, rect, XStringFormats.TopLeft);

                    rect = new(31, 167, 120, 13);
                    tf.DrawString("PRESENTE", font_bold, XBrushes.Black, rect, XStringFormats.TopLeft);
                    #endregion

                    #region Datos de fecha y clave cliente
                    XFont font_vale = new("Arial", 16, XFontStyle.Bold);

                    rect = new(36, 195, 280, 25);
                    tf.Alignment = XParagraphAlignment.Center;
                    tf.DrawString("VALE DE RETIRO", font_vale, XBrushes.Black, rect, XStringFormats.TopLeft);

                    tf.Alignment = XParagraphAlignment.Left;
                    rect = new(318, 205, 38, 13);
                    tf.DrawString("FECHA:", font_bold, XBrushes.Black, rect, XStringFormats.TopLeft);

                    tf.Alignment = XParagraphAlignment.Center;
                    rect = new(380, 205, 190, 13);
                    tf.DrawString(DateTime.Today.ToString("D"), font_valor, XBrushes.Black, rect, XStringFormats.TopLeft);

                    tf.Alignment = XParagraphAlignment.Left;

                    graphics.DrawLine(pen, 380, 217, 540, 217);


                    rect = new(318, 220, 135, 13);
                    tf.DrawString("CLAVE DE CLIENTE SIIC:", font_bold, XBrushes.Black, rect, XStringFormats.TopLeft);

                    rect = new(455, 220, 122, 13);
                    tf.DrawString("0000202967", font_bold, XBrushes.Black, rect, XStringFormats.TopLeft);

                    graphics.DrawLine(pen, 455, 232, 540, 232);
                    #endregion

                    #region Solicitud de producto
                    rect = new(36, 245, 280, 13);
                    tf.DrawString("SOLICITO A USTED EL RETIRO DEL PRODUCTO:", font_bold, XBrushes.Black, rect, XStringFormats.TopLeft);

                    rect = new(36, 258, 280, 13);
                    tf.DrawString("CONDICIONES DE ENTREGA L.A.B. LLENADERAS", font_bold, XBrushes.Black, rect, XStringFormats.TopLeft);

                    rect = new(316, 245, 260, 13);
                    tf.Alignment = XParagraphAlignment.Center;
                    tf.DrawString(producto.Den, font_valor, XBrushes.Black, rect, XStringFormats.TopLeft);
                    tf.Alignment = XParagraphAlignment.Left;

                    graphics.DrawLine(pen, 316, 257, 540, 257);
                    #endregion

                    #region Inicio detalle
                    rect = new(36, 272, 540, 13);
                    tf.DrawString("LINEA TRANSPORTISTA AUTORIZADA A RETIRAR EL PRODUCTO:", font, XBrushes.Black, rect, XStringFormats.TopLeft);
                    #endregion

                    #region Empresa transportista
                    rect = new(36, 285, 160, 13);
                    tf.DrawString("EMPRESA TRANSPORTISTA:", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                    rect = new(210, 285, 360, 13);
                    tf.DrawString(transportista.Den, font_valor, XBrushes.Black, rect, XStringFormats.TopLeft);

                    graphics.DrawLine(pen, 210, 298, 540, 298);
                    #endregion

                    #region Nombre del operador
                    rect = new(36, 300, 150, 13);
                    tf.DrawString("NOMBRE DEL OPERADOR:", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                    rect = new(210, 300, 360, 13);
                    tf.DrawString(chofer.FullName, font_valor, XBrushes.Black, rect, XStringFormats.TopLeft);

                    graphics.DrawLine(pen, 210, 313, 540, 313);
                    #endregion

                    #region RFC del operador
                    rect = new(36, 316, 150, 13);
                    tf.DrawString("RFC DEL OPERADOR:", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                    rect = new(210, 316, 360, 13);
                    tf.DrawString(chofer.RFC, font_valor, XBrushes.Black, rect, XStringFormats.TopLeft);

                    graphics.DrawLine(pen, 210, 329, 540, 329);
                    #endregion

                    #region Numero del equipo
                    rect = new(36, 332, 150, 13);
                    tf.DrawString("NÚMERO DE EQUIPO:", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                    rect = new(210, 332, 360, 13);
                    tf.DrawString(tonel.Tracto, font_valor, XBrushes.Black, rect, XStringFormats.TopLeft);

                    graphics.DrawLine(pen, 210, 345, 540, 345);
                    #endregion

                    #region Capacidad del equipo
                    rect = new(36, 348, 150, 13);
                    tf.DrawString("CAPACIDAD DEL EQUIPO:", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                    rect = new(210, 348, 360, 13);
                    tf.DrawString($"{tonel.Tanque} Compartimento {ordenEmbarque.Compartment} - {ordenEmbarque.Obtener_Volumen_De_Orden()}", font_valor, XBrushes.Black, rect, XStringFormats.TopLeft);

                    graphics.DrawLine(pen, 210, 361, 540, 361);
                    #endregion

                    #region Numero de certificado
                    rect = new(36, 364, 150, 13);
                    tf.DrawString("NUM. DE CERTIFICADO:", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                    rect = new(210, 364, 360, 13);
                    tf.DrawString(tonel.Certificado_Calibracion, font_valor, XBrushes.Black, rect, XStringFormats.TopLeft);

                    graphics.DrawLine(pen, 210, 377, 540, 377);
                    #endregion

                    #region Numero de placas
                    rect = new(36, 380, 150, 13);
                    tf.DrawString("NUM. DE PLACAS:", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                    rect = new(210, 380, 360, 13);
                    tf.DrawString(tonel.Placa, font_valor, XBrushes.Black, rect, XStringFormats.TopLeft);

                    graphics.DrawLine(pen, 210, 393, 540, 393);
                    #endregion

                    #region Producto anterior
                    rect = new(36, 396, 305, 13);
                    tf.DrawString("PRODUCTO QUE TRANSPORTÓ EN EL VIAJE ANTERIOR:", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                    rect = new(343, 396, 230, 13);
                    tf.DrawString(producto_anterior, font_valor, XBrushes.Black, rect, XStringFormats.TopLeft);

                    graphics.DrawLine(pen, 343, 409, 540, 409);
                    #endregion

                    #region Destino
                    rect = new(36, 412, 70, 13);
                    tf.DrawString("DESTINO:", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                    rect = new(105, 412, 470, 26);
                    tf.DrawString(destino.Den, font_valor, XBrushes.Black, rect, XStringFormats.TopLeft);

                    graphics.DrawLine(pen, 105, 425, 540, 425);
                    #endregion

                    #region Firma de chofer
                    rect = new(36, 440, 540, 26);
                    tf.Alignment = XParagraphAlignment.Justify;
                    tf.DrawString("NOMBRE Y FIRMA DEL OPERADOR AUTORIZADO PARA FIRMAR LA REMISIÓN POR LA RECEPCIÓN  DEL PRODUCTO:", font, XBrushes.Black, rect, XStringFormats.TopLeft);
                    tf.Alignment = XParagraphAlignment.Left;

                    rect = new(106, 503, 400, 13);
                    tf.Alignment = XParagraphAlignment.Center;
                    tf.DrawString(chofer.FullName, font_valor, XBrushes.Black, rect, XStringFormats.TopLeft);
                    tf.Alignment = XParagraphAlignment.Left;

                    graphics.DrawLine(pen, 106, 518, 506, 518);
                    #endregion

                    #region Firma de encargado
                    tf.Alignment = XParagraphAlignment.Justify;
                    rect = new(36, 540, 540, 26);
                    tf.DrawString("PEMEX TRANSFORMACIÓN INDUSTRIAL QUEDA LIBERADO DE TODA RESPONSABILIDAD POR  CUALQUIER EVENTUALIDAD QUE PUDIERA OCURRIR DURANTE EL TRANSPORTE DEL PRODUCTO", font, XBrushes.Black, rect, XStringFormats.TopLeft);
                    tf.Alignment = XParagraphAlignment.Center;

                    rect = new(226, 588, 160, 13);
                    tf.DrawString("ATENTAMENTE", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                    rect = new(106, 660, 400, 13);
                    tf.DrawString(autorizador.Den, font_valor, XBrushes.Black, rect, XStringFormats.TopLeft);
                    tf.Alignment = XParagraphAlignment.Left;

                    graphics.DrawLine(pen, 106, 675, 506, 675);

                    rect = new(66, 685, 480, 13);
                    tf.DrawString("NOMBRE Y FIRMA DE PERSONA AUTORIZADA PARA FIRMAR LOS VALES DE RETIRO", font, XBrushes.Black, rect, XStringFormats.TopLeft);
                    #endregion

                    #region acerca de
                    rect = new(36, 723, 540, 26);
                    tf.DrawString("MGC Mexico, S.A. de C.V. Lago Zurich 245 Piso 7, Col. Ampliacion Granada, Alcaldia Miguel Hidalgo, Ciudad de Mexico, CP  11529, Telefono (55) 4122 - 1532", font, XBrushes.Black, rect, XStringFormats.TopLeft);
                    #endregion


                }

                var ms = new System.IO.MemoryStream();
                pdfDocument.Save(ms);

                return Ok(ms.ToArray());
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
