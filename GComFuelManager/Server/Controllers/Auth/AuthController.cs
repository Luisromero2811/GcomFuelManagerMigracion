using GComFuelManager.Server.Identity;
using GComFuelManager.Shared.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Drawing;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GComFuelManager.Shared.Modelos;
using System.Text;

namespace GComFuelManager.Server.Controllers.Auth
{
    [ApiController]
    [Route("api/cuentas")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUsuario> userManager;
        private readonly SignInManager<IdentityUsuario> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration configuration;
        private readonly ApplicationDbContext context;

        public AuthController(UserManager<IdentityUsuario> userManager,
            SignInManager<IdentityUsuario> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ApplicationDbContext context)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
            this.configuration = configuration;
            this.context = context;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserTokenDTO>> Login([FromBody] UsuarioInfo info)
        {
            try
            {
                var result = await signInManager.PasswordSignInAsync(info.UserName, info.Password, isPersistent: false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return await BuildToken(info);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("crearUser")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Create([FromBody] UsuarioInfo info)
        {
            try
            {

                var resultadoUser = await context.Usuario.FirstOrDefaultAsync(x => x.Usu == info.UserName);
                Usuario usuario = new Usuario();
                if (resultadoUser == null)
                {
                    //context.Add();
                    await context.SaveChangesAsync();
                    //return;
     
                }
                else
                {
                    return BadRequest("Ya existe un usuario registrado con esos datos.");
                }

                //Instancia hacia IdentityUsuario quien este nos maneja a los usuarios nuevos
                var user = new IdentityUsuario
                {
                    UserName = usuario.Den,
                    UserCod = usuario.Cod,
                };
                //Buscar por medio de userManager a un usuario por su nombre
                var result = await userManager.FindByNameAsync(user.UserName!);
                //Sino se encuentra un usuario por su nombre lo creará 
                if (result == null)
                {
                    //Creación del usuario 
                    var success = await userManager.CreateAsync(user, info.Password!);
                    if (success.Succeeded)
                    {
                        return Ok();
                    }
                    //En caso de error retorna un estatus erroneo valga la redundancia 
                    else
                    {
                        return BadRequest(success.Errors);
                    }
                }
                //Si se encuentra un usuario por búsqueda de su nombre, muestra una alerta de la existencia de este. 
                else
                {
                    return BadRequest("Ya existe un usuario registrado con esos datos.");
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        [HttpGet("renovarToken")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<UserTokenDTO>> Renovar()
        {
            //Construimos un userInfo para poder utilizar el method de BuildToken
            var userInfo = new UsuarioInfo()
            {
                UserName = HttpContext.User.Identity!.Name!
            };

            return await BuildToken(userInfo);
        }

        private async Task<UserTokenDTO> BuildToken(UsuarioInfo info)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, info.UserName),
                new Claim(JwtRegisteredClaimNames.UniqueName, info.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var usuario = await userManager.FindByNameAsync(info.UserName);
            if (usuario != null)
            {
                var roles = await userManager.GetRolesAsync(usuario);

                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["jwtkey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //var expiration = DateTime.Now.AddHours(1);
            var expiration = DateTime.Now.AddMinutes(30);
            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: expiration,
                signingCredentials: creds);
            return new UserTokenDTO
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration,
            };
        }
        //Crear usuarios ya definidos de la tabla Usuarios
        [HttpPost("crear")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Create()
        {
            try
            {
                var u = await context.Usuario.ToListAsync();
                foreach (var item in u)
                {
                    var user = new IdentityUsuario
                    {
                        UserName = item.Usu,
                        UserCod = item.Cod
                    };

                    var result = await userManager.FindByNameAsync(user.UserName!);

                    if (result == null)
                    {
                        await userManager.CreateAsync(user, item.Cve!);
                    }
                }
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
