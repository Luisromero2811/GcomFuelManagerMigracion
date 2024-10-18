using AutoMapper;
using FluentValidation;
using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.ModelDTOs;
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
        private readonly IMapper mapper;
        private readonly IValidator<TerminalPostDTO> validator;

        public TerminalController(ApplicationDbContext context,
            UserManager<IdentityUsuario> userManager,
            VerifyUserId verifyUser,
            User_Terminal _Terminal,
            IMapper mapper,
            IValidator<TerminalPostDTO> validator)
        {
            this.context = context;
            this.userManager = userManager;
            this.verifyUser = verifyUser;
            this._terminal = _Terminal;
            this.mapper = mapper;
            this.validator = validator;
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
                var terminales = context.Tad.Include(x=>x.TipoTerminal).OrderBy(x => x.Den).Select(x => mapper.Map<TerminalDTO>(x)).ToList();
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
        public async Task<ActionResult> Post([FromBody] TerminalPostDTO tad)
        {
            try
            {
                var id = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                var result = validator.Validate(tad);
                if (!result.IsValid) { return BadRequest(result.Errors.Select(x => x.ErrorMessage)); }

                var terminal = mapper.Map<Tad>(tad);

                if (await context.Tad.AnyAsync(x => !string.IsNullOrEmpty(x.Den) && x.Den.ToLower().Equals(terminal.Den!.ToLower()) && x.Cod != terminal.Cod))
                    return BadRequest("Ya existe una terminal con el mismo nombre");

                if (await context.Tad.AnyAsync(x => !string.IsNullOrEmpty(x.Codigo) && x.Codigo.ToLower().Equals(terminal.Codigo!.ToLower()) && x.Cod != terminal.Cod))
                    return BadRequest("Ya existe una terminal con la misma abreviacion");

                if (await context.Tad.AnyAsync(x => !string.IsNullOrEmpty(x.CodigoOrdenes) && x.CodigoOrdenes.ToLower().Equals(terminal.CodigoOrdenes!.ToLower()) && x.Cod != terminal.Cod))
                    return BadRequest("Ya existe una terminal con el mismo identificador de orden");

                if (terminal.Cod != 0)
                {
                    //var taddb = await context.Tad.AsNoTracking().FirstOrDefaultAsync(x => x.Cod.Equals(terminal.Cod));
                    //if (taddb is null) { return NotFound(); }

                    //var tadupdate = mapper.Map(terminal, taddb);

                    context.Update(terminal);
                    await context.SaveChangesAsync(id, 44);
                }
                else
                {
                    var tipo_vale = context.Catalogo_Fijo.FirstOrDefault(x => x.Valor.ToLower() == "pemex");
                    if (tipo_vale != null) { terminal.Tipo_Vale = tipo_vale.Id; }

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

        [HttpDelete("{Id}")]
        public async Task<ActionResult> ChangeStatus([FromRoute] short Id)
        {
            try
            {
                var id = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                var terminal = await context.Tad.FindAsync(Id);
                if (terminal is null) return NotFound();

                if (terminal.Activo is true)
                {
                    if (context.OrdenEmbarque.Any(x => x.Codtad == terminal.Cod) ||
                            context.OrdenCierre.Any(x => x.Id_Tad == terminal.Cod) ||
                            context.Orden.Any(x => x.Id_Tad == terminal.Cod))
                    {
                        return BadRequest("No se puede desactivar esta terinal, cuanta con registros de ordenes en la base de datos");
                    }
                }

                terminal.Activo = !terminal.Activo;

                context.Update(terminal);
                await context.SaveChangesAsync(id, 44);

                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("{Id}")]
        public async Task<ActionResult> GetById([FromRoute] short Id)
        {
            try
            {
                var id = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                var terminal = await context.Tad.AsNoTracking().FirstOrDefaultAsync(x => x.Cod.Equals(Id));
                if (terminal is null) return NotFound();

                var terminaldto = mapper.Map<TerminalPostDTO>(terminal);
                return Ok(terminaldto);
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
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
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

        [HttpGet("configuracion/{id}")]
        public async Task<ActionResult> Obtener_Configuracion_Terminal([FromRoute] short id)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0) { return BadRequest(); }

                var terminal = await context.Tad.FirstOrDefaultAsync(x => x.Cod == id);
                if (terminal is null) { return NotFound(); }

                var consecutivo_vale = await context.Consecutivo.FirstOrDefaultAsync(x => x.Id_Tad == id && x.Nombre == "Vale");

                var catalogo = await context.Catalogo_Fijo.FirstOrDefaultAsync(x => x.Id == terminal.Tipo_Vale);
                if (catalogo is null) { catalogo = new(); }

                Configuracion_Terminal_DTO configuracion_ = new()
                {
                    //Consecutivo = consecutivo ?? 1,
                    Consecutivo_Vale = consecutivo_vale?.Numeracion ?? 1,
                    Id_Terminal = id,
                    Tipo_De_Vale = catalogo.Id
                };

                return Ok(configuracion_);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("configuracion")]
        public async Task<ActionResult> Guardar_Configuracion_Terminal([FromBody] Configuracion_Terminal_DTO configuracion_)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0) { return BadRequest(); }

                var terminal = await context.Tad.IgnoreAutoIncludes().FirstOrDefaultAsync(x => x.Cod == configuracion_.Id_Terminal);
                if (terminal is null) { return NotFound(); }

                terminal.Tipo_Vale = configuracion_.Tipo_De_Vale;
                context.Update(terminal);
                await context.SaveChangesAsync();

                var consecutivo_vale = await context.Consecutivo.FirstOrDefaultAsync(x => x.Id_Tad == configuracion_.Id_Terminal && x.Nombre == "Vale");
                if (consecutivo_vale is null)
                {
                    Consecutivo Nuevo_Consecutivo = new() { Numeracion = configuracion_.Consecutivo_Vale, Nombre = "Vale", Id_Tad = configuracion_.Id_Terminal };
                    context.Add(Nuevo_Consecutivo);
                    await context.SaveChangesAsync();
                }
                else
                {
                    consecutivo_vale.Numeracion = configuracion_.Consecutivo_Vale;
                    context.Update(consecutivo_vale);
                    await context.SaveChangesAsync();
                }

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/tipovale")]
        public ActionResult Obtener_Catalogo_Conjunto()
        {
            try
            {
                var catalogo = context.Accion.FirstOrDefault(x => x.Nombre.Equals("Catalogo_Formato_Vale"));
                if (catalogo is null) { return BadRequest("No existe el catalogo para el tipo de vale"); }

                var catalogo_fijo = context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo.Cod)).ToList();

                return Ok(catalogo_fijo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo/tipoterminal")]
        public async Task<ActionResult> CatalogoTipoTerminal()
        {
            try
            {
                var catalogo = await context.Accion.FirstOrDefaultAsync(x => x.Nombre.Equals("Catalogo_Terminal_Tipo_Terminal"));
                if (catalogo is null) { return BadRequest("No se encontro un catalogo para los tipos de terminales"); }

                var valores = await context.Catalogo_Fijo.Where(x => x.Catalogo.Equals(catalogo.Cod)).ToListAsync();

                return Ok(valores);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
