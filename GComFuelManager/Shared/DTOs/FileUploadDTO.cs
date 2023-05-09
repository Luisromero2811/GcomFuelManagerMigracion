using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.DTOs
{
    public class FileUploadDTO
    {
        public IFormFile? File { get; set; } = null!;
        public long? Size { get; set; } = 0;
        public string Name { get;set; } = string.Empty;
        public Stream? Stream { get; set; } = null!;

        public bool Uploaded { get; set; }
        public string? FileName { get; set; }
        public string? StoredFileName { get; set; }
        public int ErrorCode { get; set; }

    }
}
