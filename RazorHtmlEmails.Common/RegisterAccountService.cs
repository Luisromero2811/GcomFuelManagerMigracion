using MimeKit.Text;
using MimeKit;
using RazorHtmlEmails.GComFuelManagerMigracion.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RazorHtmlEmails.GComFuelManagerMigracion.Views.Emails.ConfirmationAccount;
using MailKit.Net.Smtp;
using MailKit.Security;
using GComFuelManager.Shared.DTOs;

namespace RazorHtmlEmails.Common
{
    public class RegisterAccountService : IRegisterAccountService
    {
        private readonly IRazorViewToStringRenderer razorView;

        public RegisterAccountService(IRazorViewToStringRenderer razorView)
        {
            this.razorView = razorView;
        }

        public async Task Register(EmailContent content)
        {

            string body = await razorView.RenderViewToStringAsync("/Views/Emails/ConfirmationAccount/ConfirmaAccount.cshtml",content);
            var message = new MimeMessage();

            //MemoryStream stream = new MemoryStream();
            //StreamWriter writter = new StreamWriter(stream);
            //writter.Write(result);
            //writter.Flush();
            //stream.Position = 0;

            message.From.Add(new MailboxAddress("Gcom Fuel Manager", "endpoint@gasamigas.com"));
            message.To.Add(new MailboxAddress(content.Nombre, content.Email));
            message.Subject = content.Subject;
            //var bodyBuilder = new BodyBuilder();

            //bodyBuilder.HtmlBody = @"<div>HTML email body</div>";

            //bodyBuilder.Attachments.Add("msg.html", stream);

            message.Body = new TextPart(TextFormat.Html) { Text = body };

            SendEmail(message);
        }

        private static void SendEmail(MimeMessage message)
        {
            SmtpClient smtpClient = new SmtpClient()
            {
                //solo para pruebas -- remover despues de pruebas
                ServerCertificateValidationCallback = (_,_,_,_) => true
            };

            smtpClient.Connect("smtp.exchangeadministrado.com", 587, SecureSocketOptions.Auto);
            smtpClient.Authenticate("endpoint@gasamigas.com", "ZZR5tp_");
            smtpClient.Send(message);

            smtpClient.Disconnect(true);
        }
    }

    public interface IRegisterAccountService
    {
        Task Register(EmailContent content);
    }
}
