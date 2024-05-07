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
    public class EstacionController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUsuario> userManager;
        private readonly VerifyUserId verifyUser;
        private readonly User_Terminal terminal;
        private readonly User_Terminal _terminal;

        public EstacionController(ApplicationDbContext context, User_Terminal _Terminal, UserManager<IdentityUsuario> userManager, VerifyUserId verifyUser)
        {
            this.context = context;
            this._terminal = _Terminal;
            this.userManager = userManager;
            this.verifyUser = verifyUser;
        }

        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] Folio_Activo_Vigente filtro_)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var estaciones_filtradas = context.Destino.IgnoreAutoIncludes().Where(x => x.Terminales.Any(x => x.Cod == id_terminal)).Include(x => x.Terminales).IgnoreAutoIncludes().AsQueryable();

                if (filtro_.ID_Cliente != 0)
                    estaciones_filtradas = estaciones_filtradas.Where(x => x.Codcte == filtro_.ID_Cliente);

                if (!string.IsNullOrEmpty(filtro_.Destino_Filtrado))
                    estaciones_filtradas = estaciones_filtradas.Where(x => !string.IsNullOrEmpty(x.Den) && x.Den.ToLower().Contains(filtro_.Destino_Filtrado.ToLower()));

                var estaciones = estaciones_filtradas.OrderBy(x => x.Den);

                return Ok(estaciones);
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

        [HttpGet("filtro")]
        public async Task<ActionResult> GetCliente([FromQuery] ParametrosBusquedaCatalogo cliente)
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var estaciones = context.Destino
                    .Where(x => x.Codcte == cliente.codcte && x.Activo == true && x.Terminales.Any(x => x.Cod == id_terminal))
                    .Include(x => x.Terminales)
                    .OrderBy(x => x.Den)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(cliente.nombredestino))
                    estaciones = estaciones.Where(x => x.Den != null && !string.IsNullOrEmpty(x.Den) && x.Den.ToLower().Contains(cliente.nombredestino.ToLower()));

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

        [HttpPost()]
        public async Task<ActionResult> EditDestino([FromBody] Destino destino)
        {
            try
            {
                if (destino is null)
                    return BadRequest();

                destino.Destino_Tads = null!;
                destino.Terminales = null!;

                context.Update(destino);
                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }

        [HttpGet("estaciones")]
        public async Task<ActionResult> GetAll()
        {
            try
            {
                var id_terminal = _terminal.Obtener_Terminal(context, HttpContext);
                if (id_terminal == 0)
                    return BadRequest();

                var estaciones = await context.Destino
                    .Where(x => x.Activo == true && x.Terminales.Any(x => x.Cod == id_terminal))
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
                    //Con Any compruebo si el número aleatorio existe en la BD
                    var exist = context.Destino.Any(x => x.Id_DestinoGobierno == destino.Id_DestinoGobierno);
                    //Si ya existe, genera un nuevo número Random
                    if (exist)
                    {
                        return BadRequest("El ID de Gobierno ya existe, por favor ingrese otro identificador");
                    }
                    //Se liga de forma directa a la terminal donde fue creada
                    destino.Id_Tad = id_terminal;

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

        [HttpPost("asignar/{cod:int}")]
        public async Task<ActionResult> PostAsignar([FromBody] Destino codden, [FromRoute] int cod)
        {
            try
            {
                var destino = await context.Destino.FirstOrDefaultAsync(x => x.Cod == codden.Cod);

                if (destino == null)
                {
                    return NotFound();
                }

                destino.Codcte = cod;
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
        public async Task<ActionResult> ChangeStatus([FromRoute] int cod, [FromBody] bool status)
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

                destino.Activo = status;

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

                var multidestinos = context.Destino.Where(x =>x.Activo && x.Es_Multidestino == true && x.Id_Tad == id_terminal).ToList();
                return Ok(multidestinos);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }

}
