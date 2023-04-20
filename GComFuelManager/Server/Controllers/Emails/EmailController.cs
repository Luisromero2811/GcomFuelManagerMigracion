using GComFuelManager.Shared.DTOs;
using MailKit.Net.Smtp;
using MailKit.Security;
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

        [HttpPost]
        public async Task<ActionResult> SendEmail([FromBody] EmailContent content)
        {
            try
            {
                var message = new MimeMessage();
                var body = new BodyBuilder();

                message.From.Add(new MailboxAddress("Gcom Fuel Manager", "endpoint@gasamigas.com"));
                message.To.Add(new MailboxAddress(content.Nombre, content.Email));
                message.Subject = content.Subject;
                body.HtmlBody = content.Body;
                message.Body = body.ToMessageBody();
                SmtpClient smtpClient = new SmtpClient();

                await smtpClient.ConnectAsync("smtp.exchangeadministrado.com", 587, SecureSocketOptions.Auto);
                await smtpClient.AuthenticateAsync("endpoint@gasamigas.com", "ZZR5tp_");
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
