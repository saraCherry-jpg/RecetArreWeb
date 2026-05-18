using Microsoft.JSInterop;

namespace RecetArreWeb.Services
{
    public interface ITokenService
    {
        Task GuardarToken(string token, DateTime expiracion);
        Task<string?> ObtenerToken();
        Task<DateTime?> ObtenerExpiracion();
        Task<bool> EstaAutenticado();
        Task EliminarToken();
    }

    public class TokenService : ITokenService
    {
        private readonly IJSRuntime jsRuntime;
        private const string TOKEN_KEY = "authToken";
        private const string EXPIRACION_KEY = "tokenExpiracion";

        public TokenService(IJSRuntime jsRuntime)
        {
            this.jsRuntime = jsRuntime;
        }

        public async Task GuardarToken(string token, DateTime expiracion)
        {
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", TOKEN_KEY, token);
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", EXPIRACION_KEY, expiracion.ToString("o"));
        }

        public async Task<string?> ObtenerToken()
        {
            try
            {
                var token = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", TOKEN_KEY);
                
                if (string.IsNullOrEmpty(token))
                    return null;

                // Verificar si el token expiró
                var expiracion = await ObtenerExpiracion();
                if (expiracion.HasValue && expiracion.Value < DateTime.UtcNow)
                {
                    await EliminarToken();
                    return null;
                }

                return token;
            }
            catch
            {
                return null;
            }
        }

        public async Task<DateTime?> ObtenerExpiracion()
        {
            try
            {
                var expiracionStr = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", EXPIRACION_KEY);
                
                if (string.IsNullOrEmpty(expiracionStr))
                    return null;

                if (DateTime.TryParse(expiracionStr, out var expiracion))
                    return expiracion;

                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> EstaAutenticado()
        {
            var token = await ObtenerToken();
            return !string.IsNullOrEmpty(token);
        }

        public async Task EliminarToken()
        {
            await jsRuntime.InvokeVoidAsync("localStorage.removeItem", TOKEN_KEY);
            await jsRuntime.InvokeVoidAsync("localStorage.removeItem", EXPIRACION_KEY);
        }
    }
}
