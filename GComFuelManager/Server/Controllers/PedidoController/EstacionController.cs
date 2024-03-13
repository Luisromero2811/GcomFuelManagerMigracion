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
        private readonly VerifyUserId verifyUserId;
        private readonly User_Terminal terminal;

        public EstacionController(ApplicationDbContext context, UserManager<IdentityUsuario> userManager, VerifyUserId verifyUserId, User_Terminal _Terminal)
        {
            this.context = context;
            this.userManager = userManager;
            this.verifyUserId = verifyUserId;
            terminal = _Terminal;
        }

        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] Folio_Activo_Vigente filtro_)
        {
            try
            {
                var estaciones_filtradas = context.Destino.AsQueryable();

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
                var estaciones = await context.Destino
                    .Where(x => x.Codcte == cliente && x.Activo == true)
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
                var estaciones = await context.Destino
                    .Where(x => x.Codcte == cliente && x.Activo == true)
                    .OrderBy(x => x.Den)
                    .ToListAsync();
                return Ok(estaciones);
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

                context.Update(destino);
                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }

        //[HttpGet]
        public async Task<ActionResult> GetAll()
        {
            try
            {
                var estaciones = await context.Destino
                    .Where(x => x.Activo == true)
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
                if (destino is null)
                {
                    return NotFound();
                }
                //Si el destino viene en ceros del front lo agregamos como nuevo sino actualizamos
                if (destino.Cod == 0)
                {
                    //Agregamos cliente
                    context.Add(destino);
                }
                else
                {
                    context.Update(destino);
                }
                await context.SaveChangesAsync();
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

    }
}
