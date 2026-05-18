using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using RecetArreWeb.Services;

namespace RecetArreWeb.Auth
{
    public class AuthStateProvider : AuthenticationStateProvider
    {
        private readonly ITokenService tokenService;
        private readonly AuthenticationState anonimo = new(new ClaimsPrincipal(new ClaimsIdentity()));

        public AuthStateProvider(ITokenService tokenService)
        {
            this.tokenService = tokenService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await tokenService.ObtenerToken();

            if (string.IsNullOrEmpty(token))
                return anonimo;

            return ConstruirAuthenticationState(token);
        }

        public AuthenticationState ConstruirAuthenticationState(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var claims = jwtToken.Claims;
                var identity = new ClaimsIdentity(claims, "jwt");
                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }
            catch
            {
                return anonimo;
            }
        }

        public void NotificarLogin(string token)
        {
            var authState = ConstruirAuthenticationState(token);
            NotifyAuthenticationStateChanged(Task.FromResult(authState));
        }

        public void NotificarLogout()
        {
            NotifyAuthenticationStateChanged(Task.FromResult(anonimo));
        }
    }
}
