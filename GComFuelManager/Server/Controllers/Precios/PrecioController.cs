using GComFuelManager.Shared.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Diagnostics;

namespace GComFuelManager.Server.Controllers.Precios
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrecioController : ControllerBase
    {
        [HttpPost]
        [Route("upload")]
        public async Task<ActionResult> Convert(IFormFile file)
        {
            try
            {
                if(file == null)
                    return BadRequest("No se pudo leer el archivo enviado.");

                using var stream = new MemoryStream();
                file.CopyTo(stream);

                List<PreciosDTO> precios = new List<PreciosDTO>();

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                ExcelPackage package = new ExcelPackage();

                package.Load(stream);

                if (package.Workbook.Worksheets.Count > 0)
                {
                    using (ExcelWorksheet worksheet = package.Workbook.Worksheets.First())
                    {
                        for (int r = 1; r < worksheet.Dimension.End.Row; r++)
                        {
                            for (int c = 1; c < worksheet.Dimension.End.Column; c++)
                            {
                                Debug.WriteLine($"Row:{r}, Column:{c}, data:{worksheet.Cells[r,c].Value}");
                            }
                        }
                    }
                }

                return Ok(precios);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
