using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly IValidacionCierreEmailService validacionCierre;
        private readonly IValidacionOrdenEmailService validacionOrden;

        public EmailController(ApplicationDbContext context,
            IRazorViewToStringRenderer razorView,
            IRegisterAccountService registerAccount,
            IVencimientoService vencimientoService,
            IPreciosService preciosService,
            IValidacionCierreEmailService validacionCierre,
            IValidacionOrdenEmailService validacionOrden)
        {
            this.context = context;
            this.razorView = razorView;
            this.registerAccount = registerAccount;
            this.vencimientoService = vencimientoService;
            this.preciosService = preciosService;
            this.validacionCierre = validacionCierre;
            this.validacionOrden = validacionOrden;
        }


        [HttpPost("validacioncierre")]
        public async Task<ActionResult> SendEmailValidacionCierre([FromBody] List<OrdenCierre> ordenCierres)
        {
            try
            {
                int? VolumenTotal = 0;
                EmailContent<OrdenCierre> emailContent = new EmailContent<OrdenCierre>();
                var cc = context.Contacto.Where(x => x.CodCte == 0 && x.Estado == true).Select(x => new MailboxAddress(x.Nombre, x.Correo)).AsEnumerable();
                var ToList = context.AccionCorreo.Where(x => x.Contacto.CodCte == ordenCierres.FirstOrDefault().CodCte && x.Contacto.Estado == true
                && x.Accion.Nombre.Equals("ValidacionCierre"))
                    .Include(x => x.Accion)
                    .Include(x => x.Contacto)
                    .Select(x => new MailboxAddress(x.Contacto.Nombre, x.Contacto.Correo))
                    .AsEnumerable();

                if (ToList is null || ToList.Count() == 0)
                    return BadRequest($"{ordenCierres.FirstOrDefault().Cliente.Den}, No cuenta con un correo activo o registrado");

                emailContent.CC = cc;
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

                emailContent.ToList = ToList;
                emailContent.Subject = "Aviso de creación de cierre de pedido";
                emailContent.Lista = cierresDistinc;

                await validacionCierre.ValidacionCierre(emailContent);

                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("validacionorden")]
        public async Task<ActionResult> SendEmailValidacionOrden([FromBody] List<OrdenCierre> ordenCierres)
        {
            try
            {
                int? VolumenTotal = 0;
                EmailContent<OrdenCierre> emailContent = new EmailContent<OrdenCierre>();
                var cc = context.Contacto.Where(x => x.CodCte == 0 && x.Estado == true).Select(x => new MailboxAddress(x.Nombre, x.Correo)).AsEnumerable();
                var ToList = context.AccionCorreo.Where(x => x.Contacto.CodCte == ordenCierres.FirstOrDefault().CodCte && x.Contacto.Estado == true
                && x.Accion.Nombre.Equals("ValidacionOrden"))
                    .Include(x => x.Accion)
                    .Include(x => x.Contacto)
                    .Select(x => new MailboxAddress(x.Contacto.Nombre, x.Contacto.Correo))
                    .AsEnumerable();

                emailContent.CC = cc;
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

                emailContent.ToList = ToList;
                emailContent.Subject = "Aviso de creación de orden individual";
                emailContent.Lista = cierresDistinc;

                await validacionOrden.ValidacionOrden(emailContent);

                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("confirmacion")]
        public async Task<ActionResult> SendEmailConfirmacion([FromBody] List<OrdenCierre> ordenCierres)
        {
            try
            {
                EmailContent<OrdenCierre> emailContent = new EmailContent<OrdenCierre>();
                int? VolumenTotal = 0;
                var cc = context.Contacto.Where(x => x.CodCte == 0 && x.Estado == true).Select(x => new MailboxAddress(x.Nombre, x.Correo)).AsEnumerable();
                //var ToList = context.Contacto.Where(x => x.CodCte == ordenCierres.FirstOrDefault().CodCte && x.Estado == true)
                //    .Include(x=>x.AccionCorreos)
                //    .ThenInclude(x=>x.Accion)
                //    .Select(x => new MailboxAddress(x.Nombre,x.Correo)).AsEnumerable();
                var ToList = context.AccionCorreo.Where(x => x.Contacto.CodCte == ordenCierres.FirstOrDefault().CodCte && x.Contacto.Estado == true
                && x.Accion.Nombre.Equals("Compra"))
                    .Include(x => x.Accion)
                    .Include(x => x.Contacto)
                    .Select(x => new MailboxAddress(x.Contacto.Nombre, x.Contacto.Correo))
                    .AsEnumerable();

                emailContent.CC = cc;

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

                emailContent.ToList = ToList;
                //emailContent.Nombre = ordenCierres.FirstOrDefault()!.ContactoN!.Nombre;
                //emailContent.Email = ordenCierres.FirstOrDefault()!.ContactoN!.Correo;
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
                var clientes = precios.DistinctBy(x => x.NombreCliente).Select(x => x.CodCte);
                foreach (var item in clientes)
                {
                    var list = precios.Where(x => x.CodCte == item);

                    EmailContent<Precio> emailContent = new EmailContent<Precio>();

                    var cc = context.Contacto.Where(x => x.CodCte == 0 && x.Estado == true).Select(x => new MailboxAddress(x.Nombre, x.Correo)).AsEnumerable();
                    emailContent.CC = cc;

                    var ToList = context.AccionCorreo.Where(x => x.Contacto.CodCte == precios.FirstOrDefault().CodCte && x.Contacto.Estado == true
                        && x.Accion.Nombre.Equals("Precios"))
                            .Include(x => x.Accion)
                            .Include(x => x.Contacto)
                            .Select(x => new MailboxAddress(x.Contacto.Nombre, x.Contacto.Correo))
                            .AsEnumerable();

                    var contacto = context.Contacto.FirstOrDefault(x => x.CodCte == precios.FirstOrDefault()!.CodCte && x.Estado == true);
                    if (contacto is null)
                        return BadRequest("No tiene un contacto asignado");

                    //emailContent.Nombre = contacto.Nombre;
                    //emailContent.Email = contacto.Correo;
                    emailContent.Subject = "Listado de precios del dia";
                    emailContent.Lista = list;
                    emailContent.ToList = ToList;

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
