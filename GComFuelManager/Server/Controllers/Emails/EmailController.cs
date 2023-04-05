using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MimeKit;

namespace GComFuelManager.Server.Controllers.Emails
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public EmailController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult> SendEmail()
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Angel", "innovacion2@gasamigas.com"));
                message.To.Add(new MailboxAddress("Angel", "angelzapata582@gmail.com"));
                message.Subject = "Prueba";
                var s = 2;
                message.Body = new TextPart("plain")
                {
                    Text = @"Cuerpo de correo de prueba.{s}"
                };
                
                SmtpClient smtpClient = new SmtpClient();

                await smtpClient.ConnectAsync("smtp.exchangeadministrado.com", 587, SecureSocketOptions.Auto);

                await smtpClient.SendAsync(message);

                await smtpClient.DisconnectAsync(true);
                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
