using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GComFuelManager.Shared.DTOs;
using OfficeOpenXml;

namespace GComFuelManager.Server.Controllers.Tarifas
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TarifasController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUsuario> userManager;
        private readonly VerifyUserToken verifyUser;

        public TarifasController(ApplicationDbContext context, UserManager<IdentityUsuario> userManager, VerifyUserToken verifyUser)
        {
            this.context = context;
            this.userManager = userManager;
            this.verifyUser = verifyUser;
        }

        [HttpPost]
        [Route("upload")]
        public async Task<ActionResult> Convert(IFormFile file)
        {
            try
            {
                if (file == null)
                    return BadRequest("No se pudo leer el archivo enviado");

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                file.CopyTo(stream);

                //Hacemos una lista nueva del tarifario

                ExcelPackage.LicenseContext = LicenseContext.Commercial;

                ExcelPackage package = new();

                package.Load(stream);

                if (package.Workbook.Worksheets.Count > 0)
                {
                    using (ExcelWorksheet worksheet = package.Workbook.Worksheets.First())
                    {
                        for (int r = 2; r < (worksheet.Dimension.End.Row + 1); r++)
                        {
                            //Volvemos a hacer una nueva instancia de lo que sería el modelo o DTO de las tarifas
                            var row = worksheet.Cells[r, 1, r, 10].ToList();

                            if (row.Count == 10)
                            {
                                //Definimos el orden de cada fila y al final retornamos el objeto
                            }
                        }
                    }
                }
                //Retornamos el objeto de tarifas
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("confirm/upload")]
        public async Task<ActionResult> PostTarifa()
        {
            try
            {
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("historial")]
        public async Task<ActionResult> GetHistorialTarifas([FromBody] TarifasDTO tarifas)
        {
            try
            {
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

    }
}

