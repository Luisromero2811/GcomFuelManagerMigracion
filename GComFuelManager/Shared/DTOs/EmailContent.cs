using System;
using MimeKit;

namespace GComFuelManager.Shared.DTOs
{
	public class EmailContent<T>
	{
        public string Nombre { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;
        public IEnumerable<MailboxAddress>? CC { get; set; } = null!;
        public IEnumerable<MailboxAddress>? ToList { get; set; } = null!;
        public IEnumerable<T>? Lista { get; set; } = null!;
        public T Item { get; set; } = default!;
    }
}

