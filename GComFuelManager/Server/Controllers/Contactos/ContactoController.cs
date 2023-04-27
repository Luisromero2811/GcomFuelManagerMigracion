using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GComFuelManager.Server.Controllers.Contactos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ContactoController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public ContactoController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet("cliente/{cod:int}")]
        public async Task<ActionResult> GetByCliente([FromRoute] int cod)
        {
            try
            {
                var contactos = context.Contacto.Where(x => x.CodCte == cod).OrderBy(x => x.Nombre).AsEnumerable();

                return Ok(contactos);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("{cod:int}")]
        public ActionResult GetContacto([FromRoute] int cod)
        {
            try
            {
                if (cod == 0)
                    return NotFound();

                var contactos = context.Contacto.Where(x => x.CodCte == cod && x.Estado == true).OrderBy(x => x.Nombre).AsEnumerable();

                return Ok(contactos);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Contacto contacto)
        {
            try
            {
                if (contacto == null)
                    return BadRequest();

                context.Add(contacto);
                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut]
        public async Task<ActionResult> Put([FromBody] Contacto contacto)
        {
            try
            {
                if (contacto == null)
                    return BadRequest();

                context.Update(contacto);
                await context.SaveChangesAsync();

                return Ok();
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

                var contacto = context.Contacto.Where(x => x.Cod == cod).FirstOrDefault();

                if (contacto == null)
                    return NotFound();

                contacto.Estado = status;

                context.Update(contacto);
                await context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("comprador")]
        public ActionResult GetContactoComprador()
        {
            try
            {
                var user = context.Usuario.FirstOrDefault(x => x.Usu == HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.UniqueName));
                if (user == null)
                    return BadRequest();

                var clientes = context.Contacto.Where(x => x.CodCte == user!.CodCte).AsEnumerable().OrderBy(x => x.Nombre);

                return Ok(clientes);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
