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
        private readonly User_Terminal terminal;

        public TerminalController(ApplicationDbContext context, UserManager<IdentityUsuario> userManager, VerifyUserId verifyUser, User_Terminal _Terminal)
        {
            this.context = context;
            this.userManager = userManager;
            this.verifyUser = verifyUser;
            terminal = _Terminal;
        }

        public async Task<ActionResult> Get()
        {
            try
            {
                var terminales = await context.Tad.Where(x => x.Activo == true).OrderBy(x => x.Den)
                //.Select(x => new CodDenDTO { Cod = x.Cod, Den = x.Den! })
                .ToListAsync();
                return Ok(terminales);
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }

        [HttpGet("all")]
        public ActionResult GetAll()
        {
            try
            {
                var terminales = context.Tad.OrderBy((x => x.Den)).ToList();
                return Ok(terminales);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("login"), AllowAnonymous]
        public ActionResult Obteer_Terminales_De_Login()
        {
            try
            {
                List<string?> tads = new();
                tads = context.Tad.Where(x => x.Activo == true && !string.IsNullOrEmpty(x.Den)).Select(x => x.Den).OrderBy(x => x).ToList();
                return Ok(tads);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("check")]
        public ActionResult GetTerminal()
        {
            try
            {
                var id_terminal = terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var terminales = context.Tad.IgnoreAutoIncludes().FirstOrDefault(x => x.Activo == true && x.Cod == id_terminal);
                if (terminales is null) { return NotFound(); }

                return Ok(terminales);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        [HttpGet("activas")]
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

        [HttpGet("activasNOTXP")]
        public async Task<ActionResult<List<Tad>>> Obtener_Terminales_De_LoginNOTXP()
        {
            try
            {
                return await context.Tad.Where(x => x.Den != "TAS TUXPAN" && x.Activo == true).ToListAsync();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
