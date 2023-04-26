using GComFuelManager.Shared.DTOs;
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

        public EmailController(ApplicationDbContext context, 
            IRazorViewToStringRenderer razorView, 
            IRegisterAccountService registerAccount,
            IVencimientoService vencimientoService)
        {
            this.context = context;
            this.razorView = razorView;
            this.registerAccount = registerAccount;
            this.vencimientoService = vencimientoService;
        }

        [HttpPost("confirmacion")]
        public async Task<ActionResult> SendEmailConfirmacion([FromBody] EmailContent content)
        {
            try
            {
                var cc = context.Contacto.Where(x => x.CodCte == 0 && x.Estado == true).Select(x => new MailboxAddress(x.Nombre,x.Correo)).AsEnumerable();

                content.CC = cc;

                await registerAccount.Register(content);

                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("vencimiento")]
        public async Task<ActionResult> SendEmailVencimiento([FromBody] EmailContent content)
        {
            try
            {
                var cc = context.Contacto.Where(x => x.CodCte == 0 && x.Estado == true).Select(x => new MailboxAddress(x.Nombre, x.Correo)).AsEnumerable();

                content.CC = cc;

                await vencimientoService.Vencimiento(content);

                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
