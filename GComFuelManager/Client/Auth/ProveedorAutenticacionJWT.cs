using GComFuelManager.Client.Helpers;
using GComFuelManager.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace GComFuelManager.Client.Auth
{
	public class ProveedorAutenticacionJWT:AuthenticationStateProvider, ILoginService
	{
        private readonly IJSRuntime js;
        private readonly HttpClient client;
        private readonly NavigationManager navigation;

        public ProveedorAutenticacionJWT(IJSRuntime js, HttpClient client, NavigationManager navigation)
		{
            this.js = js;
            this.client = client;
            this.navigation = navigation;
        }

        public static readonly string TOKENKEY = "TOKENKEY";

        private AuthenticationState anonimo => 
            new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await js.GetItemLocalStorage(TOKENKEY);

            if (string.IsNullOrWhiteSpace(token))
            {
                navigation.NavigateTo("/login");
                return this.anonimo;
            }

            return ConstruirAuthenticationState(token.ToString()!);
        }

        private AuthenticationState ConstruirAuthenticationState(string token)
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("bearer", token);
            var claims = ParsearClaimsJwt(token);
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt")));
        }

        private IEnumerable<Claim> ParsearClaimsJwt(string token)
        {
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var tokenDesearizado = jwtSecurityTokenHandler.ReadJwtToken(token);
            return tokenDesearizado.Claims;
        }

        public async Task Login(UserTokenDTO token)
        {
            await js.SetItemLocalStorage(TOKENKEY, token.Token);
            var authentication = ConstruirAuthenticationState(token.Token);
            NotifyAuthenticationStateChanged(Task.FromResult(authentication));
        }

        public async Task Logoute()
        {
            await js.RemoveItemLocalStorage(TOKENKEY);
            client.DefaultRequestHeaders.Authorization = null;
            NotifyAuthenticationStateChanged(Task.FromResult(anonimo));
        }
    }
}

