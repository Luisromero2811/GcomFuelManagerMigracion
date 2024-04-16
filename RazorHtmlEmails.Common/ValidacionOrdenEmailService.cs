using System;
using GComFuelManager.Shared.Modelos;
using GComFuelManager.Shared.DTOs;
using RazorHtmlEmails.GComFuelManagerMigracion.Services;
using MimeKit;
using MimeKit.Text;


namespace RazorHtmlEmails.Common
{
    public class ValidacionOrdenEmailService : EmailSendService, IValidacionOrdenEmailService
    {
        private readonly IRazorViewToStringRenderer razorView;

        public ValidacionOrdenEmailService(IRazorViewToStringRenderer razorView)
        {
            this.razorView = razorView;
        }

        public async Task ValidacionOrden(EmailContent<OrdenCierre> content)
        {
            string body = await razorView.RenderViewToStringAsync("/Views/Emails/Avisos/AvisoOrden.cshtml", content);
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress("Gcom Fuel Management", "admon@energasmx.mx"));
            message.To.AddRange(content.ToList);
            message.Cc.AddRange(content.CC);
            message.Subject = content.Subject;
            message.Body = new TextPart(TextFormat.Html) { Text = body };

            SendEmail(message);
        }

    }
    public interface IValidacionOrdenEmailService
    {
        Task ValidacionOrden(EmailContent<OrdenCierre> content);
    }
}


