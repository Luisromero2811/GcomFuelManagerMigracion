using AutoMapper;
using FluentValidation;
using GComFuelManager.Server.Helpers;
using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.ModelDTOs;
using GComFuelManager.Shared.Modelos;
using GComFuelManager.Shared.ReportesDTO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace GComFuelManager.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class EstacionController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUsuario> userManager;
        private readonly VerifyUserId verifyUser;
        private readonly IUsuarioHelper helper;
        private readonly IMapper mapper;
        private readonly IValidator<DestinoPostDTO> validator;
        private readonly User_Terminal _terminal;

        public EstacionController(ApplicationDbContext context,
                                  User_Terminal _Terminal,
                                  UserManager<IdentityUsuario> userManager,
                                  VerifyUserId verifyUser,
                                  IUsuarioHelper helper,
                                  IMapper mapper,
                                  IValidator<DestinoPostDTO> validator)
        {
            this.context = context;
            this._terminal = _Terminal;
            this.userManager = userManager;
            this.verifyUser = verifyUser;
            this.helper = helper;
            this.mapper = mapper;
            this.validator = validator;
        }

        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] DestinoDTO destino)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var estaciones_filtradas = context.Destino
                    .AsNoTracking()
                    .Where(x => x.Id_Tad == id_terminal)
                    .OrderBy(x => x.Den)
                    .AsQueryable();

                if (destino.Codcte != 0)
                    estaciones_filtradas = estaciones_filtradas.Where(x => x.Codcte == destino.Codcte);

                if (!string.IsNullOrEmpty(destino.Den))
                    estaciones_filtradas = estaciones_filtradas.Where(x => !string.IsNullOrEmpty(x.Den) && x.Den.ToLower().Contains(destino.Den.ToLower()));

                if (destino.Activo)
                    estaciones_filtradas = estaciones_filtradas.Where(x => x.Activo);

                var destinosdtos = await estaciones_filtradas.Select(x => mapper.Map<DestinoDTO>(x)).ToListAsync();

                return Ok(destinosdtos);
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }

        [HttpGet("{destino:int}/post")]
        public async Task<ActionResult> GetPostById([FromRoute] int destino)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var estacion = await context.Destino.AsNoTracking().FirstOrDefaultAsync(x => x.Cod == destino);
                if (estacion is null) { return NotFound(); }

                return Ok(mapper.Map<DestinoPostDTO>(estacion));
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }

        [HttpGet("{cliente:int}")]
        public async Task<ActionResult> Get([FromRoute] int cliente)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var estaciones = await context.Destino.IgnoreAutoIncludes()
                    .Where(x => x.Codcte == cliente && x.Activo == true && x.Terminales.Any(x => x.Cod == id_terminal))
                    .Include(x => x.Terminales).IgnoreAutoIncludes()
                    .Select(x => new CodDenDTO { Cod = x.Cod, Den = x.Den! })
                    .OrderBy(x => x.Den)
                    .ToListAsync();
                return Ok(estaciones);
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }

        [HttpGet("{cliente:int}/all")]
        public async Task<ActionResult> GetAll([FromRoute] int cliente)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var estaciones = await context.Destino.IgnoreAutoIncludes()
                    .Where(x => x.Codcte == cliente && x.Activo == true && x.Terminales.Any(x => x.Cod == id_terminal))
                    .Include(x => x.Terminales).IgnoreAutoIncludes()
                    .OrderBy(x => x.Den)
                    .ToListAsync();
                return Ok(estaciones);
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }

        [HttpGet("all")]
        public ActionResult GetAllDestins()
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var destinos = context.Destino.IgnoreAutoIncludes().Where(x => x.Terminales.Any(x => x.Cod == id_terminal)).Include(x => x.Terminales).IgnoreAutoIncludes().OrderBy(x => x.Den);
                return Ok(destinos);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditDestino([FromBody] DestinoPostDTO destinodto)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var result = validator.Validate(destinodto);
                if (!result.IsValid) { return BadRequest(result.Errors.Select(x => x.ErrorMessage)); }

                var destino = mapper.Map<Destino>(destinodto);

                if (!string.IsNullOrEmpty(destino.Id_DestinoGobierno) && !string.IsNullOrWhiteSpace(destino.Id_DestinoGobierno))
                {
                    var exist = context.Destino.Any(x => x.Id_DestinoGobierno == destino.Id_DestinoGobierno && x.Id_Tad == id_terminal && x.Cod != destino.Cod);
                    if (exist) { return BadRequest("El ID de Gobierno ya existe, por favor ingrese otro identificador"); }
                }

                if (destino.Cod != 0)
                {
                    var destinodb = await context.Destino.FindAsync(destino.Cod);
                    if (destinodb is null) { return NotFound(); }

                    var newdestino = mapper.Map(destino, destinodb);

                    context.Update(newdestino);
                }
                else
                {
                    destino.Id_Tad = id_terminal;
                    destino.Codsyn = destino.GetHashCode().ToString();

                    await context.AddAsync(destino);
                }

                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }

        [HttpPost("crear")]
        public async Task<ActionResult> PostDestino([FromBody] Destino destino)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                if (destino is null)
                {
                    return NotFound();
                }
                //Si el destino viene en ceros del front lo agregamos como nuevo sino actualizamos
                if (destino.Cod == 0)
                {
                    if (!string.IsNullOrEmpty(destino.Id_DestinoGobierno) && !string.IsNullOrWhiteSpace(destino.Id_DestinoGobierno))
                    {
                        //Con Any compruebo si el número aleatorio existe en la BD
                        var exist = context.Destino.Any(x => x.Id_DestinoGobierno == destino.Id_DestinoGobierno && x.Id_Tad == id_terminal);
                        //Si ya existe, genera un nuevo número Random
                        if (exist) { return BadRequest("El ID de Gobierno ya existe, por favor ingrese otro identificador"); }
                    }
                    //Se liga de forma directa a la terminal donde fue creada
                    destino.Id_Tad = id_terminal;

                    if (string.IsNullOrEmpty(destino.Codsyn) || string.IsNullOrWhiteSpace(destino.Codsyn))
                        destino.Codsyn = Codsyn_Random();

                    //Agregamos cliente
                    context.Add(destino);
                    await context.SaveChangesAsync();
                    if (!context.Destino_Tad.Any(x => x.Id_Terminal == id_terminal && x.Id_Destino == destino.Cod))
                    {
                        Destino_Tad destino_Tad = new()
                        {
                            Id_Destino = destino.Cod,
                            Id_Terminal = id_terminal
                        };
                        context.Add(destino_Tad);
                        await context.SaveChangesAsync();
                    }
                }
                else
                {
                    destino.Id_Tad = id_terminal;
                    destino.Terminales = null!;

                    if (!string.IsNullOrEmpty(destino.Id_DestinoGobierno) && !string.IsNullOrWhiteSpace(destino.Id_DestinoGobierno))
                    {
                        //Verifico si es diferente al ID que ya tenía
                        if (context.Destino.Any(x => x.Id_DestinoGobierno != destino.Id_DestinoGobierno))
                        {
                            //Con Any compruebo si el número aleatorio existe en la BD
                            var exist = context.Destino.Any(x => x.Id_DestinoGobierno == destino.Id_DestinoGobierno && x.Codciu != destino.Codciu);
                            //Si ya existe, genera un nuevo número Random
                            if (exist)
                            {
                                return BadRequest("El ID de Gobierno ya existe, por favor ingrese otro identificador");
                            }
                        }
                        else
                        {
                            return BadRequest("El ID de Gobierno ya existe, por favor ingrese otro identificador");
                        }
                    }

                    context.Update(destino);
                    await context.SaveChangesAsync();
                }

                return Ok();

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("asignar")]
        public async Task<ActionResult> PostAsignar([FromQuery] ClienteDestinoDTO dTO)
        {
            try
            {
                var destino = await context.Destino.FirstOrDefaultAsync(x => x.Cod == dTO.Coddes);

                if (destino is null)
                    return NotFound();

                destino.Codcte = dTO.Codcte;
                context.Update(destino);
                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("relacion")]
        public async Task<ActionResult> PostClienteTerminal([FromBody] ClienteTadDTO clienteTadDTO)
        {
            try
            {
                //Si el cliente es nulo, retornamos un badrequest
                if (clienteTadDTO is null)
                    return BadRequest();
                foreach (var terminal in clienteTadDTO.Tads)
                {
                    foreach (var destino in clienteTadDTO.Destinos)
                    {
                        if (!context.Destino_Tad.Any(x => x.Id_Terminal == terminal.Cod && x.Id_Destino == destino.Cod))
                        {
                            Destino_Tad destino_Tad = new()
                            {
                                Id_Destino = destino.Cod,
                                Id_Terminal = terminal.Cod
                            };
                            context.Add(destino_Tad);
                        }
                    }
                }
                await context.SaveChangesAsync();
                return Ok();

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("borrar/relacion")]
        public async Task<ActionResult> Borrar_Relacion([FromBody] Destino_Tad clienteterminal)
        {
            try
            {
                if (clienteterminal is null)
                    return NotFound();

                if (context.OrdenEmbarque.Any(x => x.Codtad == clienteterminal.Id_Terminal && x.Coddes == clienteterminal.Id_Destino) ||
                    context.OrdenCierre.Any(x => x.Id_Tad == clienteterminal.Id_Terminal && x.CodDes == clienteterminal.Id_Destino)
                    || context.Orden.Any(x => x.Id_Tad == clienteterminal.Id_Terminal && x.Coddes == clienteterminal.Id_Destino))
                {
                    return BadRequest("Error, no puede eliminar la relación debido a órdenes activas ligadas a este Destino y Unidad de Negocio");
                }

                var id = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                context.Remove(clienteterminal);
                await context.SaveChangesAsync();

                return Ok(clienteterminal);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("{cod:int}")]
        public async Task<ActionResult> ChangeStatus([FromRoute] int cod)
        {
            try
            {
                if (cod == 0)
                    return BadRequest();

                var destino = context.Destino.Where(x => x.Cod == cod).FirstOrDefault();
                if (destino == null)
                {
                    return NotFound();
                }

                destino.Activo = !destino.Activo;

                context.Update(destino);
                var acc = destino.Activo ? 26 : 27;
                var id = await verifyUser.GetId(HttpContext, userManager);
                if (string.IsNullOrEmpty(id))
                    return BadRequest();

                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("multidestino")]
        public ActionResult Obtener_Multidestinos()
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var multidestinos = context.Destino.Where(x => x.Activo && x.Es_Multidestino == true && x.Id_Tad == id_terminal).ToList();
                return Ok(multidestinos);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("catalogo")]
        public async Task<ActionResult> GetCatalogo()
        {
            try
            {
                var id_terminal = await helper.GetTerminalId();

                var estaciones = await context.Destino
                    .Where(x => x.Id_Tad == id_terminal)
                    .Include(x => x.Cliente)
                    .OrderBy(x => x.Den)
                    .ToListAsync();

                ExcelPackage.LicenseContext = LicenseContext.Commercial;
                ExcelPackage excel = new();
                ExcelWorksheet ws = excel.Workbook.Worksheets.Add("Destinos");

                ws.Cells["A1"].LoadFromCollection(estaciones.Select(mapper.Map<CatalogoDestinoDTO>), c =>
                {
                    c.PrintHeaders = true;
                    c.TableStyle = OfficeOpenXml.Table.TableStyles.Medium2;
                });

                ws.Cells[1, 1, ws.Dimension.End.Row, ws.Dimension.End.Column].AutoFitColumns();

                return Ok(excel.GetAsByteArray());
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        private string Codsyn_Random()
        {
            var codsyn = new Random().Next(1, 999999).ToString();
            if (context.Destino.Any(x => !string.IsNullOrEmpty(x.Codsyn) && x.Codsyn.Equals(codsyn)))
                Codsyn_Random();
            return codsyn;
        }
    }

}
