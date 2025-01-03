
using GComFuelManager.Server.Exceptions;
using GComFuelManager.Server.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace GComFuelManager.Server.Helpers
{
    public class UsuarioHelper : IUsuarioHelper
    {
        private readonly ApplicationDbContext context;
        private readonly IHttpContextAccessor httpContext;
        private readonly UserManager<IdentityUsuario> userManager;

        public UsuarioHelper(ApplicationDbContext context,
                             IHttpContextAccessor httpContext,
                             UserManager<IdentityUsuario> userManager)
        {
            this.context = context;
            this.httpContext = httpContext;
            this.userManager = userManager;
        }

        public short GetTerminalId()
        {
            string Id = string.Empty;

            var authheader = httpContext.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();

            if (authheader is not null && authheader.StartsWith("Bearer"))
                Id = authheader["Bearer ".Length..];

            var handler = new JwtSecurityTokenHandler();

            if (handler.CanReadToken(Id))
            {
                var token = handler.ReadJwtToken(Id);
                var nombre_terminal = token.Claims.FirstOrDefault(x => x.Type == "Terminal")?.Value;
                if (nombre_terminal is not null)
                {
                    var terminal = context.Tad.FirstOrDefault(x => !string.IsNullOrEmpty(x.Den) && x.Den.Equals(nombre_terminal) && x.Activo == true);
                    if (terminal is not null)
                        return terminal.Cod;
                }
            }

            throw new UsuarioIdsException("Terminal no valida");
        }

        public async Task<short> GetTerminalIdAsync()
        {
            string Id = string.Empty;

            var authheader = httpContext.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();

            if (authheader is not null && authheader.StartsWith("Bearer"))
                Id = authheader["Bearer ".Length..];

            var handler = new JwtSecurityTokenHandler();

            if (handler.CanReadToken(Id))
            {
                var token = handler.ReadJwtToken(Id);
                var nombre_terminal = token.Claims.FirstOrDefault(x => x.Type == "Terminal")?.Value;
                if (nombre_terminal is not null)
                {
                    var terminal = await context.Tad.FirstOrDefaultAsync(x => !string.IsNullOrEmpty(x.Den) && x.Den.Equals(nombre_terminal) && x.Activo == true);
                    if (terminal is not null)
                        return terminal.Cod;
                }
            }

            throw new UsuarioIdsException("Terminal no valida");
        }

        public async Task<string> GetUserId()
        {
            string Id = string.Empty;

            var authHeader = httpContext.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();

            if (authHeader != null && authHeader.StartsWith("Bearer "))
            {
                Id = authHeader.Substring("Bearer ".Length);
            }

            var handler = new JwtSecurityTokenHandler();

            if (handler.CanReadToken(Id))
            {
                var token = handler.ReadJwtToken(Id);

                var userId = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.UniqueName)?.Value;
                if (!string.IsNullOrEmpty(userId) && !string.IsNullOrWhiteSpace(userId))
                {
                    var user = await userManager.FindByNameAsync(userId);
                    if (user is not null)
                    {
                        Id = user.Id;
                        return Id;
                    }
                }
            }

            throw new UsuarioIdsException("Usuario no valido");
        }

        public async Task<IdentityUsuario> GetUsuario()
        {
            string Id = string.Empty;

            var authHeader = httpContext.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();

            if (authHeader != null && authHeader.StartsWith("Bearer "))
            {
                Id = authHeader.Substring("Bearer ".Length);
            }

            var handler = new JwtSecurityTokenHandler();

            if (handler.CanReadToken(Id))
            {
                var token = handler.ReadJwtToken(Id);

                var userId = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.UniqueName)?.Value;
                if (!string.IsNullOrEmpty(userId) && !string.IsNullOrWhiteSpace(userId))
                {
                    var user = await userManager.FindByNameAsync(userId);
                    if (user is not null)
                        return user;
                }
            }

            throw new UsuarioIdsException("Usuario no valido");
        }
    }
}
