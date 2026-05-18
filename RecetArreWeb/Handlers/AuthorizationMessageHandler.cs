using System.Net.Http.Headers;
using RecetArreWeb.Services;

namespace RecetArreWeb.Handlers
{
    public class AuthorizationMessageHandler : DelegatingHandler
    {
        private readonly ITokenService tokenService;

        public AuthorizationMessageHandler(ITokenService tokenService)
        {
            this.tokenService = tokenService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, 
            CancellationToken cancellationToken)
        {
            var token = await tokenService.ObtenerToken();

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
