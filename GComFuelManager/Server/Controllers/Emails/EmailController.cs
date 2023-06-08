using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MimeKit.Text;
using RazorHtmlEmails.Common;
using RazorHtmlEmails.GComFuelManagerMigracion.Services;
using System.Net.Mail;
using System.Security.Cryptography.Xml;

namespace GComFuelManager.Server.Controllers.Emails
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class EmailController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IRazorViewToStringRenderer razorView;
        private readonly IRegisterAccountService registerAccount;
        private readonly IVencimientoService vencimientoService;
        private readonly IPreciosService preciosService;

        public EmailController(ApplicationDbContext context, 
            IRazorViewToStringRenderer razorView, 
            IRegisterAccountService registerAccount,
            IVencimientoService vencimientoService,
            IPreciosService preciosService)
        {
            this.context = context;
            this.razorView = razorView;
            this.registerAccount = registerAccount;
            this.vencimientoService = vencimientoService;
            this.preciosService = preciosService;
        }

        [HttpPost("confirmacion")]
        public async Task<ActionResult> SendEmailConfirmacion([FromBody] List<OrdenCierre> ordenCierres)
        {
            try
            {
                EmailContent<OrdenCierre> emailContent = new EmailContent<OrdenCierre>();
                int? VolumenTotal = 0;

                var cc = context.Contacto.Where(x => x.CodCte == 0 && x.Estado == true).Select(x => new MailboxAddress(x.Nombre,x.Correo)).AsEnumerable();               
                var Cliwc = context.Contacto.Where(x => x.CodCte == ordenCierres.FirstOrDefault()!.CodCte && x.Estado == true).Select(x => new MailboxAddress(x.Nombre, x.Correo)).AsEnumerable();
                                                                        
                emailContent.CC = cc;
                emailContent.CC = Cliwc;

                //Funcion para el volumen
                IEnumerable<OrdenCierre> cierresDistinc = ordenCierres.DistinctBy(x => x.Producto!.Den);

                foreach (var item in cierresDistinc)
                {
                    foreach (var cierre in ordenCierres)
                        if (cierre.Producto!.Den == item.Producto!.Den)
                            VolumenTotal = VolumenTotal + cierre.Volumen;
                    cierresDistinc.FirstOrDefault(x => x.Producto!.Den == item.Producto!.Den)!.Volumen = VolumenTotal;
                    VolumenTotal = 0;
                }
                //Formación y envió del correo
                emailContent.Nombre = ordenCierres.FirstOrDefault()!.ContactoN!.Nombre;
                emailContent.Email = ordenCierres.FirstOrDefault()!.ContactoN!.Correo;
                emailContent.Subject = "Confirmacion de compra";
                emailContent.Lista = cierresDistinc;

                await registerAccount.Register(emailContent);

                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("precios")]
        public async Task<ActionResult> SendEmailPrecios([FromBody] IEnumerable<Precio> precios)
        {
            try
            {
                var clientes = precios.DistinctBy(x => x.NombreCliente).Select(x=>x.codCte);
                foreach (var item in clientes)
                {
                    var list = precios.Where(x => x.codCte == item);

                    EmailContent<Precio> emailContent = new EmailContent<Precio>();

                    var cc = context.Contacto.Where(x => x.CodCte == 0 && x.Estado == true).Select(x => new MailboxAddress(x.Nombre, x.Correo)).AsEnumerable();
                    emailContent.CC = cc;

                    var contacto = context.Contacto.FirstOrDefault(x => x.CodCte == precios.FirstOrDefault()!.codCte && x.Estado == true);
                    if (contacto is null)
                        return BadRequest("No tiene un contacto asignado");

                    emailContent.Nombre = contacto.Nombre;
                    emailContent.Email = contacto.Correo;
                    emailContent.Subject = "Listado de precios del dia";
                    emailContent.Lista = list;

                    await preciosService.NotifyPrecio(emailContent);

                }

                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
