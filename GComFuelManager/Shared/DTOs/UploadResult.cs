using System;
namespace GComFuelManager.Shared.DTOs
{
	public class UploadResult
	{
        public bool Upload { get; set; }
        public string? FileName { get; set; }
        public string? StoredFileName { get; set; }
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public bool HasError { get; set; } = false;
    }
}

