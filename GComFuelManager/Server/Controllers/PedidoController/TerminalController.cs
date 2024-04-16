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
        [HttpGet]
        public ActionResult Get()
        {
            try
            {
                var terminales = context.Tad.Where(x => x.Activo == true).OrderBy((x => x.Den)).ToList();
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

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Tad terminal)
        {
            try
            {
                var id = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                if (terminal is null) { return NotFound(); }

                if (string.IsNullOrEmpty(terminal.Den) || string.IsNullOrWhiteSpace(terminal.Den)) { return BadRequest("La terminal no puede tener un nombre vacio."); }
                if (string.IsNullOrEmpty(terminal.Codigo) || string.IsNullOrWhiteSpace(terminal.Codigo)) { return BadRequest("La terminal no puede tener una abreviacion vacia."); }
                if (string.IsNullOrEmpty(terminal.CodigoOrdenes) || string.IsNullOrWhiteSpace(terminal.CodigoOrdenes)) { return BadRequest("La terminal no puede tener un identificador de ordenes vacio."); }

                if (terminal.Cod != 0)
                {
                    if (context.Tad.Any(x => !string.IsNullOrEmpty(x.Den) && x.Den.ToLower().Equals(terminal.Den.ToLower()) && x.Cod != terminal.Cod))
                    { return BadRequest("Ya existe una terminal con el mismo nombre"); }

                    if (context.Tad.Any(x => !string.IsNullOrEmpty(x.Codigo) && x.Codigo.ToLower().Equals(terminal.Codigo.ToLower()) && x.Cod != terminal.Cod))
                    { return BadRequest("Ya existe una terminal con la misma abreviacion"); }

                    if (context.Tad.Any(x => !string.IsNullOrEmpty(x.CodigoOrdenes) && x.CodigoOrdenes.ToLower().Equals(terminal.CodigoOrdenes.ToLower()) && x.Cod != terminal.Cod))
                    { return BadRequest("Ya existe una terminal con el mismo identificador de orden"); }

                    if (terminal.Activo == false)
                    {
                        if (context.OrdenEmbarque.Any(x => x.Codtad == terminal.Cod) ||
                            context.OrdenCierre.Any(x => x.Id_Tad == terminal.Cod) ||
                            context.Orden.Any(x => x.Id_Tad == terminal.Cod))
                        {
                            return BadRequest("No se puede desactivar esta terinal, cuanta con registros de ordenes en la base de datos");
                        }
                    }

                    context.Update(terminal);
                    await context.SaveChangesAsync(id, 44);
                }
                else
                {
                    if (context.Tad.Any(x => !string.IsNullOrEmpty(x.Den) && x.Den.ToLower().Equals(terminal.Den.ToLower())))
                    { return BadRequest("Ya existe una terminal con el mismo nombre"); }

                    if (context.Tad.Any(x => !string.IsNullOrEmpty(x.Codigo) && x.Codigo.ToLower().Equals(terminal.Codigo.ToLower())))
                    { return BadRequest("Ya existe una terminal con la misma abreviacion"); }

                    if (context.Tad.Any(x => !string.IsNullOrEmpty(x.CodigoOrdenes) && x.CodigoOrdenes.ToLower().Equals(terminal.CodigoOrdenes.ToLower())))
                    { return BadRequest("Ya existe una terminal con el mismo identificador de orden"); }

                    context.Add(terminal);
                    await context.SaveChangesAsync(id, 43);
                }

                return Ok(terminal);
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
