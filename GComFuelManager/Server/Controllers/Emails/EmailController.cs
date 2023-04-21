using GComFuelManager.Shared.DTOs;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MimeKit.Text;
using RazorHtmlEmails.Common;
using RazorHtmlEmails.GComFuelManagerMigracion.Services;
using System.Security.Cryptography.Xml;

namespace GComFuelManager.Server.Controllers.Emails
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IRazorViewToStringRenderer razorView;
        private readonly IRegisterAccountService registerAccount;

        public EmailController(ApplicationDbContext context, IRazorViewToStringRenderer razorView, IRegisterAccountService registerAccount)
        {
            this.context = context;
            this.razorView = razorView;
            this.registerAccount = registerAccount;
        }

        [HttpPost]
        public async Task<ActionResult> SendEmail([FromBody] EmailContent content)
        {
            try
            {
                //var message = new MimeMessage();

                //MemoryStream stream = new MemoryStream();
                //StreamWriter writter = new StreamWriter(stream);
                //writter.Write(result);
                //writter.Flush();
                //stream.Position = 0;

                //message.From.Add(new MailboxAddress("Gcom Fuel Manager", "endpoint@gasamigas.com"));
                //message.To.Add(new MailboxAddress(content.Nombre, content.Email));
                //message.Subject = content.Subject;
                //var bodyBuilder = new BodyBuilder();

                //bodyBuilder.HtmlBody = @"<div>HTML email body</div>";

                //bodyBuilder.Attachments.Add("msg.html", stream);

                //message.Body = new TextPart(TextFormat.Html) { Text = result};

                //SmtpClient smtpClient = new SmtpClient();

                //await smtpClient.ConnectAsync("smtp.exchangeadministrado.com", 587, SecureSocketOptions.Auto);
                //await smtpClient.AuthenticateAsync("endpoint@gasamigas.com", "ZZR5tp_");
                //await smtpClient.SendAsync(message);

                //await smtpClient.DisconnectAsync(true);

                await registerAccount.Register(content);

                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
