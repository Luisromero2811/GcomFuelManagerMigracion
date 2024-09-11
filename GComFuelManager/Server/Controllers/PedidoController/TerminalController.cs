using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TerminalController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUsuario> userManager;
        private readonly VerifyUserId verifyUser;
        private readonly User_Terminal _terminal;

        public TerminalController(ApplicationDbContext context, UserManager<IdentityUsuario> userManager, VerifyUserId verifyUser, User_Terminal _Terminal)
        {
            this.context = context;
            this.userManager = userManager;
            this.verifyUser = verifyUser;
            this._terminal = _Terminal;
        }

        [HttpGet]
        public async Task<ActionResult<List<Tad>>> Obtener_Terminales_De_Login()
        {
            try
            {
                return await context.Tad.Where(x => x.Activo == true).ToListAsync();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
