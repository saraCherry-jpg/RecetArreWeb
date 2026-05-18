using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using RecetArreWeb;
using RecetArreWeb.Services;
using RecetArreWeb.Auth;
using RecetArreWeb.Handlers;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configurar HttpClient con handler de autorización
builder.Services.AddScoped<AuthorizationMessageHandler>();

builder.Services.AddScoped(sp => 
{
    var handler = sp.GetRequiredService<AuthorizationMessageHandler>();
    handler.InnerHandler = new HttpClientHandler();
    
    var httpClient = new HttpClient(handler)
    {
        BaseAddress = new Uri("https://RecetArre1.somee.com/") //Se cambio para entrar a Somee el server, anteriormente es un localhost || https:

    };
    
    return httpClient;
});

// Registrar servicios
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IIngredienteService, IngredienteService>();
builder.Services.AddScoped<IRecetaService, RecetaService>();
builder.Services.AddScoped<IComentarioService, ComentarioService>();

// Configurar autenticación
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, AuthStateProvider>();

//SE AGREGO ESTO
//Configura rating
builder.Services.AddScoped<IRatingService, RatingService>();

await builder.Build().RunAsync();
