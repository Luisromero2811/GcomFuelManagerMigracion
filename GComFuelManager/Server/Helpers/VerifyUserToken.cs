using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;

namespace GComFuelManager.Server.Helpers
{
    public class VerifyUserToken
    {
        public string GetName(HttpContext httpContext)
        {
            string bearerToken = null;

            var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();

            if (authHeader != null && authHeader.StartsWith("Bearer "))
            {
                bearerToken = authHeader.Substring("Bearer ".Length);
            }

            var handler = new JwtSecurityTokenHandler();

            if (handler.CanReadToken(bearerToken))
            {
                var token = handler.ReadJwtToken(bearerToken);

                var userId = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.UniqueName)?.Value;
                return userId;
            }

            return "";
        }
    }
}
