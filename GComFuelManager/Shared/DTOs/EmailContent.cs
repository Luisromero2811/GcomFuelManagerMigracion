using GComFuelManager.Shared.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.DTOs
{
    public class EmailContent
    {
        public string Nombre { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public IEnumerable<OrdenCierre> ordenCierres { get; set; } = null!;

    }
}
