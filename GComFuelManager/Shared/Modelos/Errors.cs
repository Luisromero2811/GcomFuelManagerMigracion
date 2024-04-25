using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Shared.Modelos
{
	public class Errors
	{
        [Key]
        public string Cod { get; set; } = Guid.NewGuid().ToString();
        public string Error { get; set; } = string.Empty;
        public DateTime Fch { get; set; } = DateTime.Now;
        public string Accion { get; set; } = string.Empty;
    }
    [Keyless]
    public class Error
    {
        public string Inner { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string StackTrace { get; set; } = string.Empty;
    }
}

