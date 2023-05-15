using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GComFuelManager.Server.Controllers.Precios
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PrecioController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUsuario> userManager;
        private readonly VerifyUserToken verifyUser;

        public PrecioController(ApplicationDbContext context, UserManager<IdentityUsuario> userManager, VerifyUserToken verifyUser)
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
                                Debug.WriteLine($"Row:{r}, Column:{c}, data:{worksheet.Cells[r, c].Value}");
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

        [HttpGet]
        public async Task<ActionResult> GetPrecios()
        {
            try
            {
                var precios = await context.Precio
                    .Include(x => x.Zona)
                    .Include(x => x.Cliente)
                    .Include(x => x.Producto)
                    .Include(x => x.Destino)
                    .ToListAsync();

                return Ok(precios);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Precio precio)
        {
            try
            {
                precio.Producto = null!;
                precio.Zona = null!;
                precio.Destino = null!;
                precio.Cliente = null!;
                precio.FchActualizacion = DateTime.Now;

                if (precio.Cod != 0)
                    context.Update(precio);
                else
                    context.Add(precio);

                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("productos")]
        public async Task<ActionResult> GetPrecios([FromBody] ZonaCliente? zonaCliente)
        {
            try
            {
                List<Precio> precios = new List<Precio>();

                var userId = verifyUser.GetName(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return BadRequest();

                var user = await userManager.FindByNameAsync(userId);

                if (user == null)
                    return BadRequest();

                var role = await userManager.IsInRoleAsync(user, "Comprador");

                if (role == true)
                {
                    var usuario = context.Usuario.FirstOrDefault(x => x.Usu == user.UserName);
                    if (usuario == null)
                        return BadRequest();

                    var zona = context.ZonaCliente.FirstOrDefault(x => x.CteCod == usuario.CodCte && x.DesCod == zonaCliente.DesCod);

                    if (zona == null)
                        return BadRequest();

                    precios = await context.Precio.Where(x => x.codCte == usuario.CodCte
                    && x.codDes == zonaCliente.DesCod
                    && x.codZona == zona.ZonaCod)
                        .Include(x => x.Producto)
                        .ToListAsync();
                }
                else
                {
                    var zona = context.ZonaCliente.FirstOrDefault(x => x.CteCod == zonaCliente.CteCod && x.DesCod == zonaCliente.DesCod);

                    if (zona == null)
                        return BadRequest();

                    precios = await context.Precio.Where(x => x.codCte == zonaCliente.CteCod
                    && x.codDes == zonaCliente.DesCod
                    && x.codZona == zona.ZonaCod)
                        .Include(x => x.Producto)
                        .ToListAsync();
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
