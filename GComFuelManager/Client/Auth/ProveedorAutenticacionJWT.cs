using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Security.Claims;

namespace GComFuelManager.Client.Auth
{
	public class ProveedorAutenticacionJWT:AuthenticationStateProvider
	{
		public ProveedorAutenticacionJWT()
		{
		}

        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            //await Task.Delay(3000);
            var anonimo = new ClaimsIdentity();
            return await Task.FromResult(new AuthenticationState(new ClaimsPrincipal(anonimo)));
        }
    }
}

