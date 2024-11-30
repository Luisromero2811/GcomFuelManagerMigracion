
using GComFuelManager.Server.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace GComFuelManager.Server.Helpers
{
    public class UsuarioHelper : IUsuarioHelper
    {
        private readonly ApplicationDbContext context;
        private readonly IHttpContextAccessor httpContext;

        public UsuarioHelper(ApplicationDbContext context, IHttpContextAccessor httpContext)
        {
            this.context = context;
            this.httpContext = httpContext;
        }

        public async Task<short> GetTerminalId()
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
    }
}
