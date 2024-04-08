using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Pkcs;
using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.Layout;
using PdfSharpCore.Pdf;

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

        public PdfController(ApplicationDbContext context, UserManager<IdentityUsuario> userManager, VerifyUserToken verifyUser)
        {
            this.context = context;
            this.userManager = userManager;
            this.verifyUser = verifyUser;
        }

        [HttpGet("vale")]
        public ActionResult Obtener_Vale()
        {
            try
            {
                //x 540 - y 720 (con margen) x 612 - y 792 (completa) -- tamaño carta
                PdfDocument pdfDocument = new();

                PdfPage page = pdfDocument.AddPage();
                XGraphics graphics = XGraphics.FromPdfPage(page);
                XFont font = new("Arial", 11, XFontStyle.Regular);
                XFont font_bold = new("Arial", 11, XFontStyle.Bold);
                XTextFormatter tf = new(graphics);
                XPen pen = new(XColors.Black, .8);
                XRect rect = new();

                #region imagen de mgc
                string path = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot/img/MGC_Icon.png");
                XImage xImage = XImage.FromFile(path);
                graphics.DrawImage(xImage, 36,36);
                #endregion

                #region Folio de vale
                rect = new(465, 65, 40, 13);

                tf.DrawString("Folio:", font_bold, XBrushes.Red, rect, XStringFormats.TopLeft);

                rect = new(505, 65, 80, 13);
                tf.DrawString("NUMERO_DE_VALE", font, XBrushes.Red, rect, XStringFormats.TopLeft);
                #endregion

                #region Datos terminal
                rect = new(36, 111, 220, 15);
                tf.DrawString("Pemex Transformacion Industrial", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new(36, 121, 220, 13);
                tf.DrawString("NOMBRE_DE_TERMINAL", font, XBrushes.Black, rect, XStringFormats.TopLeft);

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
                tf.DrawString("FECHA_DOCUMENTO_COMPLETA", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                tf.Alignment = XParagraphAlignment.Left;

                graphics.DrawLine(pen, 380, 217, 540, 217);


                rect = new(318, 220, 135, 13);
                tf.DrawString("CLAVE DE CLIENTE SIIC:", font_bold, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new(455, 220, 122, 13);
                tf.DrawString("CLAVE_CLIENTE", font_bold, XBrushes.Black, rect, XStringFormats.TopLeft);

                graphics.DrawLine(pen, 455, 232, 540, 232);
                #endregion

                #region Solicitud de producto
                rect = new(36, 245, 280, 13);
                tf.DrawString("SOLICITO A USTED EL RETIRO DEL PRODUCTO:", font_bold, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new(36, 258, 280, 13);
                tf.DrawString("CONDICIONES DE ENTREGA L.A.B. LLENADERAS", font_bold, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new(316, 245, 260, 13);
                tf.Alignment = XParagraphAlignment.Center;
                tf.DrawString("NOMBRE_PRODUCTO", font, XBrushes.Black, rect, XStringFormats.TopLeft);
                tf.Alignment = XParagraphAlignment.Left;

                graphics.DrawLine(pen, 316, 257, 540, 257);
                #endregion

                #region Inicio detalle
                rect = new(36, 272, 540, 13);
                tf.DrawString("LINEA TRANSPORTISTA AUTORIZADA A RETIRAR EL PRODUCTO:", font, XBrushes.Black, rect, XStringFormats.TopLeft);
                #endregion

                #region Empresa transportista
                rect = new(36, 285, 150, 13);
                tf.DrawString("EMPRESA TRANSPORTISTA:", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new(210, 285, 360, 13);
                tf.DrawString("NOMBRE_TRANSPORTISTA", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                graphics.DrawLine(pen, 210, 298, 540, 298);
                #endregion

                #region Nombre del operador
                rect = new(36, 300, 150, 13);
                tf.DrawString("NOMBRE DEL OPERADOR:", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new(210, 300, 360, 13);
                tf.DrawString("NOMBRE_DEL_OPERADOR", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                graphics.DrawLine(pen, 210, 313, 540, 313);
                #endregion

                #region RFC del operador
                rect = new(36, 316, 150, 13);
                tf.DrawString("NOMBRE DEL OPERADOR:", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new(210, 316, 360, 13);
                tf.DrawString("NOMBRE_DEL_OPERADOR", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                graphics.DrawLine(pen, 210, 329, 540, 329);
                #endregion

                #region Numero del equipo
                rect = new(36, 332, 150, 13);
                tf.DrawString("NÚMERO DE EQUIPO:", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new(210, 332, 360, 13);
                tf.DrawString("NÚMERO_DE_EQUIPO", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                graphics.DrawLine(pen, 210, 345, 540, 345);
                #endregion

                #region Capacidad del equipo
                rect = new(36, 348, 150, 13);
                tf.DrawString("CAPACIDAD DEL EQUIPO:", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new(210, 348, 360, 13);
                tf.DrawString("CAPACIDAD_DEL_EQUIPO", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                graphics.DrawLine(pen, 210, 361, 540, 361);
                #endregion

                #region Numero de certificado
                rect = new(36, 364, 150, 13);
                tf.DrawString("NUM. DE CERTIFICADO:", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new(210, 364, 360, 13);
                tf.DrawString("NUMERO_DE_CERTIFICADO", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                graphics.DrawLine(pen, 210, 377, 540, 377);
                #endregion

                #region Numero de placas
                rect = new(36, 380, 150, 13);
                tf.DrawString("NUM. DE PLACAS:", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new(210, 380, 360, 13);
                tf.DrawString("NUMUERO_DE_PLACAS", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                graphics.DrawLine(pen, 210, 393, 540, 393);
                #endregion

                #region Producto anterior
                rect = new(36, 396, 305, 13);
                tf.DrawString("PRODUCTO QUE TRANSPORTÓ EN EL VIAJE ANTERIOR:", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new(343, 396, 230, 13);
                tf.DrawString("PRODUCTO_TRANSPORTADO_ANTERIOR", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                graphics.DrawLine(pen, 343, 409, 540, 409);
                #endregion

                #region Destino
                rect = new(36, 412, 70, 13);
                tf.DrawString("DESTINO:", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                rect = new(105, 412, 470, 13);
                tf.DrawString("NOMBRE_COMPLETO_DEL_DESTINO", font, XBrushes.Black, rect, XStringFormats.TopLeft);

                graphics.DrawLine(pen, 105, 425, 540, 425);
                #endregion

                #region Firma de chofer
                rect = new(36, 435, 540, 26);
                tf.Alignment = XParagraphAlignment.Justify;
                tf.DrawString("NOMBRE Y FIRMA DEL OPERADOR AUTORIZADO PARA FIRMAR LA REMISIÓN POR LA RECEPCIÓN  DEL PRODUCTO:", font, XBrushes.Black, rect, XStringFormats.TopLeft);
                tf.Alignment = XParagraphAlignment.Left;

                rect = new(106, 503, 400, 13);
                tf.Alignment = XParagraphAlignment.Center;
                tf.DrawString("NOMBRE_COMPLETO_DEL_OPERADOR", font, XBrushes.Black, rect, XStringFormats.TopLeft);
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
                tf.DrawString("NOMBRE_COMPLETO_DEL_ENCARGADO", font, XBrushes.Black, rect, XStringFormats.TopLeft);
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
    }
}
