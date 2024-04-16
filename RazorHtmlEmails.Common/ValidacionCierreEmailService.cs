using System;
using GComFuelManager.Shared.Modelos;
using GComFuelManager.Shared.DTOs;
using RazorHtmlEmails.GComFuelManagerMigracion.Services;
using MimeKit;
using MimeKit.Text;


namespace RazorHtmlEmails.Common
{
    public class ValidacionCierreEmailService : EmailSendService, IValidacionCierreEmailService
    {
        private readonly IRazorViewToStringRenderer razorView;

        public ValidacionCierreEmailService(IRazorViewToStringRenderer razorView)
		{
            this.razorView = razorView;
        }

        public async Task ValidacionCierre(EmailContent<OrdenCierre> content)
        {
            string body = await razorView.RenderViewToStringAsync("/Views/Emails/Avisos/AvisoCierre.cshtml", content);
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress("Gcom Fuel Management", "admon@energasmx.mx"));
            message.To.AddRange(content.ToList);
            message.Cc.AddRange(content.CC);
            message.Subject = content.Subject;
            message.Body = new TextPart(TextFormat.Html) { Text = body };

            SendEmail(message);
        }

	}
    public interface IValidacionCierreEmailService
    {
        Task ValidacionCierre(EmailContent<OrdenCierre> content);
    }
}

