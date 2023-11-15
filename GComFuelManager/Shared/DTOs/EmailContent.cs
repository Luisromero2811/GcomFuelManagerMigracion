using GComFuelManager.Shared.Modelos;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
