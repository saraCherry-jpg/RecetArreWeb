# 1Identity - Autenticación JWT en Blazor WebAssembly
## Guía didáctica paso a paso para implementar Login y conexión con Backend

---

## 📋 Índice

1. [Configurar conexión Frontend → Backend](#parte-1-conectar-frontend-con-backend)
2. [Instalar paquetes necesarios](#parte-2-instalar-paquetes-nuget)
3. [Crear DTOs (contratos de datos)](#parte-3-crear-dtos)
4. [Crear servicio de Tokens (localStorage)](#parte-4-servicio-de-tokens)
5. [Crear servicio de Autenticación](#parte-5-servicio-de-autenticación)
6. [Crear proveedor de estado](#parte-6-proveedor-de-estado-de-autenticación)
7. [Configurar envío automático de tokens](#parte-7-handler-http-para-tokens-automáticos)
8. [Registrar servicios](#parte-8-registrar-servicios-en-programcs)
9. [Configurar App.razor](#parte-9-configurar-approazar)
10. [Crear páginas de Login y Registro](#parte-10-páginas-de-login-y-registro)
11. [Actualizar navegación](#parte-11-actualizar-navegación)
12. [Proteger páginas](#parte-12-proteger-páginas-con-authorize)

---

# PARTE 1: Conectar Frontend con Backend

## 🎯 Objetivo
Configurar el frontend Blazor WebAssembly para que pueda hacer peticiones HTTP al backend en `https://localhost:7019`

---

## Paso 1.1: Entender la arquitectura

```
┌────────────────────────┐         HTTP         ┌────────────────────────┐
│  Blazor WebAssembly    │ ─────────────────►   │   ASP.NET Core API     │
│  (Frontend)            │                       │   (Backend)            │
│  localhost:7097        │ ◄─────────────────   │   localhost:7019       │
└────────────────────────┘      JSON            └────────────────────────┘
```

**Conceptos clave:**
- **Frontend (Blazor)**: Corre en el navegador del usuario
- **Backend (API)**: Corre en un servidor (o localhost en desarrollo)
- **HTTP**: Protocolo para comunicación (GET, POST, PUT, DELETE)
- **JSON**: Formato de datos que viaja entre frontend y backend

---

## Paso 1.2: Configurar URL del backend en Program.cs

**Ubicación:** `RecetArreWeb/Program.cs`

**ANTES (solo frontend):**
```csharp
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) 
});
```

**DESPUÉS (conectado al backend):**
```csharp
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri("https://localhost:7019/") 
});
```

**Explicación:**
- `HttpClient`: Clase que hace peticiones HTTP
- `BaseAddress`: URL base de tu backend
- `AddScoped`: Registra el servicio (una instancia por usuario)

**⚠️ IMPORTANTE:** 
- Cambiar `7019` por el puerto de tu backend
- La URL debe terminar en `/`
- Debe ser `https://` (no `http://`)

---

## Paso 1.3: Verificar que el backend permita peticiones (CORS)

**Problema común:** Error de CORS

```
Access to fetch at 'https://localhost:7019/api/Categorias' 
from origin 'https://localhost:7097' has been blocked by CORS policy
```

**Solución en el backend** (ya lo hiciste antes):

En `Backend/Program.cs`:
```csharp
// Antes de var app = builder.Build();
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Después de var app = builder.Build();
app.UseCors("PermitirTodo");
```

**Verificar que funciona:**
1. Ejecutar backend
2. Abrir navegador en `https://localhost:7019/api/Categorias`
3. Debes ver un JSON con categorías

---

# PARTE 2: Instalar Paquetes NuGet

## 🎯 Objetivo
Instalar las librerías necesarias para autenticación y manejo de JWT

---

## Paso 2.1: Agregar paquetes al .csproj

**Ubicación:** `RecetArreWeb/RecetArreWeb.csproj`

**Agregar dentro de `<ItemGroup>`:**

```xml
<PackageReference Include="Microsoft.AspNetCore.Components.Authorization" Version="8.0.24" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.2.1" />
```

**¿Qué hace cada paquete?**

1. **`Microsoft.AspNetCore.Components.Authorization`**
   - Proporciona: `<AuthorizeView>`, `<CascadingAuthenticationState>`, `[Authorize]`
   - Para: Proteger páginas y mostrar contenido según autenticación

2. **`System.IdentityModel.Tokens.Jwt`**
   - Proporciona: `JwtSecurityTokenHandler`
   - Para: Leer y decodificar tokens JWT

---

## Paso 2.2: Restaurar paquetes

**En la terminal:**
```bash
dotnet restore
```

O en Visual Studio: Click derecho en el proyecto → Restaurar paquetes NuGet

---

# PARTE 3: Crear DTOs

## 🎯 Objetivo
Crear clases que representan los datos que viajan entre frontend y backend

---

## Paso 3.1: ¿Qué es un DTO?

**DTO** = Data Transfer Object (Objeto de Transferencia de Datos)

Es una clase que define qué información se envía/recibe en las peticiones HTTP.

**Analogía:** Es como un formulario. Tiene campos específicos que debes llenar.

---

## Paso 3.2: Crear archivo de DTOs de Identity

**Ubicación:** `RecetArreWeb/DTOs/IdentityDtos.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace RecetArreWeb.DTOs
{
    // DTO para enviar al backend (Login/Registro)
    public class CredencialesUsuario
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;
    }

    // DTO para recibir del backend (Respuesta)
    public class RespuestaAutenticacion
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiracion { get; set; }
        public string UsuarioId { get; set; } = string.Empty;
    }
}
```

**Desglose:**

### CredencialesUsuario (Lo que ENVIAMOS al backend)
- `Email`: Correo del usuario
- `Password`: Contraseña del usuario
- `[Required]`: Validación - este campo es obligatorio
- `[EmailAddress]`: Validación - debe ser un email válido
- `[MinLength(6)]`: Validación - mínimo 6 caracteres

### RespuestaAutenticacion (Lo que RECIBIMOS del backend)
- `Token`: String JWT que identifica al usuario
- `Expiracion`: Fecha cuando el token deja de ser válido
- `UsuarioId`: ID único del usuario en la base de datos

---

## Paso 3.3: Flujo de datos

```
Usuario escribe email + password
         ↓
CredencialesUsuario (DTO)
         ↓
Se envía al backend (POST /api/Cuentas/Login)
         ↓
Backend valida con Identity
         ↓
RespuestaAutenticacion (DTO)
         ↓
Frontend recibe: Token + Expiracion + UsuarioId
```

---

# PARTE 4: Servicio de Tokens

## 🎯 Objetivo
Crear un servicio para guardar, leer y eliminar el token JWT en el navegador

---

## Paso 4.1: ¿Qué es localStorage?

`localStorage` es un almacenamiento del navegador que:
- ✅ Persiste entre sesiones (sobrevive al cerrar el navegador)
- ✅ Es accesible desde JavaScript
- ✅ Puede guardar strings
- ⚠️ Es específico por dominio (cada sitio tiene su propio localStorage)

**Ver en el navegador:**
1. F12 → Application → Local Storage
2. Ver `authToken` y `tokenExpiracion`

---

## Paso 4.2: Crear TokenService

**Ubicación:** `RecetArreWeb/Services/TokenService.cs`

### 4.2.1: Definir la interfaz

```csharp
public interface ITokenService
{
    Task GuardarToken(string token, DateTime expiracion);
    Task<string?> ObtenerToken();
    Task<DateTime?> ObtenerExpiracion();
    Task<bool> EstaAutenticado();
    Task EliminarToken();
}
```

**¿Por qué una interfaz?**
- Para usar inyección de dependencias
- Para poder hacer testing (crear mocks)
- Para seguir el principio SOLID

---

### 4.2.2: Implementar GuardarToken

```csharp
private readonly IJSRuntime jsRuntime;
private const string TOKEN_KEY = "authToken";
private const string EXPIRACION_KEY = "tokenExpiracion";

public async Task GuardarToken(string token, DateTime expiracion)
{
    // Guardar el token en localStorage
    await jsRuntime.InvokeVoidAsync("localStorage.setItem", TOKEN_KEY, token);
    
    // Guardar la fecha de expiración en formato ISO 8601
    await jsRuntime.InvokeVoidAsync("localStorage.setItem", EXPIRACION_KEY, expiracion.ToString("o"));
}
```

**Desglose:**
1. `IJSRuntime`: Permite ejecutar código JavaScript desde C#
2. `InvokeVoidAsync`: Llama a una función JS que no devuelve nada
3. `"localStorage.setItem"`: Función JS para guardar en localStorage
4. `TOKEN_KEY`: Clave bajo la que se guarda el token
5. `expiracion.ToString("o")`: Formato ISO 8601 (2024-12-15T10:30:00Z)

---

### 4.2.3: Implementar ObtenerToken

```csharp
public async Task<string?> ObtenerToken()
{
    try
    {
        // 1. Leer el token de localStorage
        var token = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", TOKEN_KEY);
        
        // 2. Si no hay token, devolver null
        if (string.IsNullOrEmpty(token))
            return null;

        // 3. Verificar si el token expiró
        var expiracion = await ObtenerExpiracion();
        if (expiracion.HasValue && expiracion.Value < DateTime.UtcNow)
        {
            // Token expirado, eliminarlo
            await EliminarToken();
            return null;
        }

        // 4. Token válido, devolverlo
        return token;
    }
    catch
    {
        return null;
    }
}
```

**Desglose:**
1. Lee de localStorage
2. Valida que no esté vacío
3. **Verifica expiración** (importante para seguridad)
4. Si expiró, lo elimina automáticamente
5. Si es válido, lo devuelve

**¿Por qué verificar expiración?**
- Evita usar tokens caducados
- Previene errores en el backend
- Mejor experiencia de usuario (detecta antes que falle)

---

### 4.2.4: Implementar EliminarToken

```csharp
public async Task EliminarToken()
{
    await jsRuntime.InvokeVoidAsync("localStorage.removeItem", TOKEN_KEY);
    await jsRuntime.InvokeVoidAsync("localStorage.removeItem", EXPIRACION_KEY);
}
```

**Cuándo se usa:**
- Al hacer logout
- Cuando el token expira
- Cuando hay un error de autenticación

---

## Paso 4.3: Resumen del TokenService

```
┌─────────────────────────┐
│   TokenService          │
├─────────────────────────┤
│ GuardarToken()          │ → Guarda en localStorage
│ ObtenerToken()          │ → Lee de localStorage (verifica expiración)
│ ObtenerExpiracion()     │ → Lee fecha de expiración
│ EstaAutenticado()       │ → true si hay token válido
│ EliminarToken()         │ → Borra de localStorage
└─────────────────────────┘
         ↕
   localStorage
   ├─ authToken: "eyJhbGc..."
   └─ tokenExpiracion: "2024-12-15T..."
```

---

# PARTE 5: Servicio de Autenticación

## 🎯 Objetivo
Crear un servicio que se comunica con el backend para login, registro y renovación de tokens

---

## Paso 5.1: Crear AuthService

**Ubicación:** `RecetArreWeb/Services/AuthService.cs`

### 5.1.1: Definir la interfaz

```csharp
public interface IAuthService
{
    Task<RespuestaAutenticacion?> Login(CredencialesUsuario credenciales);
    Task<RespuestaAutenticacion?> Registrar(CredencialesUsuario credenciales);
    Task<RespuestaAutenticacion?> RenovarToken();
    Task Logout();
}
```

---

### 5.1.2: Constructor

```csharp
private readonly HttpClient httpClient;
private readonly ITokenService tokenService;
private const string endpoint = "api/Cuentas";

public AuthService(HttpClient httpClient, ITokenService tokenService)
{
    this.httpClient = httpClient;
    this.tokenService = tokenService;
}
```

**Inyección de dependencias:**
- `HttpClient`: Para hacer peticiones al backend
- `ITokenService`: Para guardar/leer tokens

---

### 5.1.3: Implementar Login (paso a paso)

```csharp
public async Task<RespuestaAutenticacion?> Login(CredencialesUsuario credenciales)
{
    try
    {
        // PASO 1: Enviar POST al backend
        var response = await httpClient.PostAsJsonAsync($"{endpoint}/Login", credenciales);
        
        // PASO 2: Verificar si fue exitoso (código 200-299)
        if (response.IsSuccessStatusCode)
        {
            // PASO 3: Leer la respuesta JSON
            var respuesta = await response.Content.ReadFromJsonAsync<RespuestaAutenticacion>();
            
            // PASO 4: Si hay respuesta, guardar el token
            if (respuesta != null)
            {
                await tokenService.GuardarToken(respuesta.Token, respuesta.Expiracion);
                return respuesta;
            }
        }
        else
        {
            // Login falló (credenciales incorrectas)
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error en login: {error}");
        }

        return null;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al hacer login: {ex.Message}");
        return null;
    }
}
```

**Desglose detallado:**

#### PASO 1: Enviar POST
```csharp
var response = await httpClient.PostAsJsonAsync($"{endpoint}/Login", credenciales);
```
- `PostAsJsonAsync`: Hace POST y serializa el objeto a JSON automáticamente
- URL completa: `https://localhost:7019/api/Cuentas/Login`
- Body: `{ "email": "usuario@correo.com", "password": "123456" }`

#### PASO 2: Verificar éxito
```csharp
if (response.IsSuccessStatusCode)
```
- `IsSuccessStatusCode`: true si el código HTTP es 200-299
- Códigos comunes:
  - 200 OK: Login exitoso
  - 400 Bad Request: Datos inválidos
  - 401 Unauthorized: Credenciales incorrectas

#### PASO 3: Leer respuesta
```csharp
var respuesta = await response.Content.ReadFromJsonAsync<RespuestaAutenticacion>();
```
- Convierte el JSON a objeto C#
- Ejemplo de respuesta:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiracion": "2024-12-22T10:30:00Z",
  "usuarioId": "abc123"
}
```

#### PASO 4: Guardar token
```csharp
await tokenService.GuardarToken(respuesta.Token, respuesta.Expiracion);
```
- Guarda en localStorage
- Ahora el usuario está "logueado"

---

### 5.1.4: Implementar Registrar

```csharp
public async Task<RespuestaAutenticacion?> Registrar(CredencialesUsuario credenciales)
{
    try
    {
        var response = await httpClient.PostAsJsonAsync($"{endpoint}/registrar", credenciales);

        if (response.IsSuccessStatusCode)
        {
            var respuesta = await response.Content.ReadFromJsonAsync<RespuestaAutenticacion>();
            
            if (respuesta != null)
            {
                await tokenService.GuardarToken(respuesta.Token, respuesta.Expiracion);
                return respuesta;
            }
        }
        
        return null;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al registrar: {ex.Message}");
        return null;
    }
}
```

**Similar a Login, pero:**
- Endpoint diferente: `/registrar`
- Crea un nuevo usuario en la base de datos
- También devuelve un token (auto-login después de registrarse)

---

### 5.1.5: Implementar Logout

```csharp
public async Task Logout()
{
    await tokenService.EliminarToken();
}
```

**Simple pero crucial:**
- Elimina el token de localStorage
- El usuario ya no está autenticado

---

## Paso 5.2: Flujo completo de Login

```
1. Usuario escribe email + password
         ↓
2. Login.razor llama a authService.Login(credenciales)
         ↓
3. AuthService hace POST a /api/Cuentas/Login
         ↓
4. Backend valida con Identity
         ↓
5. Backend genera JWT token
         ↓
6. AuthService recibe: { token, expiracion, usuarioId }
         ↓
7. TokenService guarda en localStorage
         ↓
8. Usuario autenticado ✅
```

---

# PARTE 6: Proveedor de Estado de Autenticación

## 🎯 Objetivo
Crear un componente que gestiona el estado global de autenticación en toda la aplicación

---

## Paso 6.1: ¿Qué es AuthenticationStateProvider?

Es una clase de Blazor que:
- ✅ Mantiene el **estado de autenticación** global
- ✅ Notifica a **todos los componentes** cuando cambia
- ✅ Proporciona información del usuario actual
- ✅ Permite usar `<AuthorizeView>` y `[Authorize]`

---

## Paso 6.2: ¿Qué es un JWT Token?

**JWT** = JSON Web Token

Es un string codificado que contiene información (claims) del usuario.

**Estructura de un JWT:**
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6InVzZXJAZW1haWwuY29tIiwibmFtZWlkIjoiYWJjMTIzIn0.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c

         HEADER                    PAYLOAD (claims)               SIGNATURE
```

**Decodificar en jwt.io:**
```json
{
  "email": "user@email.com",
  "sub": "abc123",
  "exp": 1702639200
}
```

**Claims comunes:**
- `email`: Email del usuario
- `sub` / `nameidentifier`: ID del usuario
- `role`: Roles del usuario (Admin, User, etc.)
- `exp`: Fecha de expiración (Unix timestamp)

---

## Paso 6.3: Crear AuthStateProvider

**Ubicación:** `RecetArreWeb/Auth/AuthStateProvider.cs`

```csharp
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using RecetArreWeb.Services;

public class AuthStateProvider : AuthenticationStateProvider
{
    private readonly ITokenService tokenService;
    
    // Usuario anónimo (sin autenticar)
    private readonly AuthenticationState anonimo = 
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public AuthStateProvider(ITokenService tokenService)
    {
        this.tokenService = tokenService;
    }
}
```

---

### 6.3.1: Método principal - GetAuthenticationStateAsync

```csharp
public override async Task<AuthenticationState> GetAuthenticationStateAsync()
{
    // 1. Obtener el token de localStorage
    var token = await tokenService.ObtenerToken();

    // 2. Si no hay token, usuario no autenticado
    if (string.IsNullOrEmpty(token))
        return anonimo;

    // 3. Si hay token, construir el estado de autenticación
    return ConstruirAuthenticationState(token);
}
```

**¿Cuándo se llama este método?**
- Al cargar la aplicación
- Al cambiar de página
- Cuando se usa `<AuthorizeView>`

---

### 6.3.2: Construir AuthenticationState desde el token

```csharp
public AuthenticationState ConstruirAuthenticationState(string token)
{
    try
    {
        // 1. Crear un handler para leer JWT
        var handler = new JwtSecurityTokenHandler();
        
        // 2. Decodificar el token (leer los claims)
        var jwtToken = handler.ReadJwtToken(token);

        // 3. Extraer los claims del token
        var claims = jwtToken.Claims;
        
        // 4. Crear una identidad con los claims
        var identity = new ClaimsIdentity(claims, "jwt");
        
        // 5. Crear un usuario (principal) con la identidad
        var user = new ClaimsPrincipal(identity);

        // 6. Devolver el estado de autenticación
        return new AuthenticationState(user);
    }
    catch
    {
        // Si falla decodificar, usuario anónimo
        return anonimo;
    }
}
```

**Desglose visual:**

```
Token JWT (string)
    ↓
JwtSecurityTokenHandler.ReadJwtToken()
    ↓
Claims: [ { type: "email", value: "user@mail.com" }, { type: "sub", value: "123" } ]
    ↓
ClaimsIdentity (contiene los claims)
    ↓
ClaimsPrincipal (representa al usuario)
    ↓
AuthenticationState (estado de autenticación)
```

---

### 6.3.3: Métodos para notificar cambios

```csharp
public void NotificarLogin(string token)
{
    var authState = ConstruirAuthenticationState(token);
    NotifyAuthenticationStateChanged(Task.FromResult(authState));
}

public void NotificarLogout()
{
    NotifyAuthenticationStateChanged(Task.FromResult(anonimo));
}
```

**¿Por qué notificar?**
- Para que `<AuthorizeView>` se actualice inmediatamente
- Para que el menú muestre/oculte opciones
- Para que las páginas protegidas se refresque

**Cuándo llamar:**
- `NotificarLogin()`: Después de hacer login exitoso
- `NotificarLogout()`: Después de eliminar el token

---

## Paso 6.4: Flujo completo del AuthStateProvider

```
1. App.razor carga → GetAuthenticationStateAsync()
         ↓
2. Lee token de localStorage
         ↓
3. Si hay token → Decodifica claims
         ↓
4. Crea AuthenticationState con usuario
         ↓
5. Todos los <AuthorizeView> se actualizan
         ↓
6. Usuario hace login → NotificarLogin(token)
         ↓
7. Todos los componentes se re-renderizan
```

---

# PARTE 7: Handler HTTP para Tokens Automáticos

## 🎯 Objetivo
Agregar el token JWT automáticamente a **todas** las peticiones HTTP

---

## Paso 7.1: ¿Por qué un Handler?

**Sin handler:**
```csharp
// En CADA petición debes hacer esto:
var token = await tokenService.ObtenerToken();
httpClient.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", token);
var response = await httpClient.PostAsJsonAsync(...);
```

**Con handler:**
```csharp
// El handler lo hace automáticamente
var response = await httpClient.PostAsJsonAsync(...);
// ✅ Token agregado automáticamente
```

---

## Paso 7.2: Crear AuthorizationMessageHandler

**Ubicación:** `RecetArreWeb/Handlers/AuthorizationMessageHandler.cs`

```csharp
using System.Net.Http.Headers;
using RecetArreWeb.Services;

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
        // 1. Obtener el token
        var token = await tokenService.ObtenerToken();

        // 2. Si hay token, agregarlo al header Authorization
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
        }

        // 3. Continuar con la petición
        return await base.SendAsync(request, cancellationToken);
    }
}
```

**Desglose:**

### 1. Hereda de DelegatingHandler
```csharp
public class AuthorizationMessageHandler : DelegatingHandler
```
- `DelegatingHandler`: Clase base que intercepta peticiones HTTP
- Permite modificar peticiones antes de enviarlas

### 2. Override SendAsync
```csharp
protected override async Task<HttpResponseMessage> SendAsync(...)
```
- Se ejecuta **antes** de cada petición HTTP
- Permite agregar headers, modificar URL, etc.

### 3. Agregar header Authorization
```csharp
request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
```
- `Bearer`: Estándar para tokens JWT
- Resultado: `Authorization: Bearer eyJhbGc...`

---

## Paso 7.3: Flujo del Handler

```
httpClient.PostAsJsonAsync("/api/Categorias", ...)
         ↓
AuthorizationMessageHandler.SendAsync()
         ↓
Obtiene token de localStorage
         ↓
Agrega header: Authorization: Bearer {token}
         ↓
Petición continúa al backend con token
         ↓
Backend valida el token
         ↓
Acceso permitido ✅
```

---

# PARTE 8: Registrar Servicios en Program.cs

## 🎯 Objetivo
Configurar el contenedor de inyección de dependencias

---

## Paso 8.1: ¿Qué es la Inyección de Dependencias?

**Problema sin DI:**
```csharp
// Cada componente crea sus propias instancias
var httpClient = new HttpClient();
var tokenService = new TokenService(jsRuntime);
var authService = new AuthService(httpClient, tokenService);
```

**Con DI:**
```csharp
// El contenedor gestiona las instancias
@inject IAuthService authService  // ✅ Automático
```

**Ventajas:**
- ✅ No duplicas código
- ✅ Fácil cambiar implementaciones
- ✅ Mejor para testing
- ✅ Gestión automática del ciclo de vida

---

## Paso 8.2: Configurar Program.cs completo

**Ubicación:** `RecetArreWeb/Program.cs`

```csharp
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

// ========== PASO 1: Configurar HttpClient con Handler ==========
builder.Services.AddScoped<AuthorizationMessageHandler>();

builder.Services.AddScoped(sp => 
{
    // Obtener el handler del contenedor
    var handler = sp.GetRequiredService<AuthorizationMessageHandler>();
    handler.InnerHandler = new HttpClientHandler();
    
    // Crear HttpClient con el handler
    var httpClient = new HttpClient(handler)
    {
        BaseAddress = new Uri("https://localhost:7019/")
    };
    
    return httpClient;
});

// ========== PASO 2: Registrar servicios ==========
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IIngredienteService, IngredienteService>();

// ========== PASO 3: Configurar autenticación ==========
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, AuthStateProvider>();

await builder.Build().RunAsync();
```

---

## Paso 8.3: Desglose de cada sección

### PASO 1: HttpClient con Handler

```csharp
builder.Services.AddScoped<AuthorizationMessageHandler>();
```
- Registra el handler como servicio

```csharp
builder.Services.AddScoped(sp => 
{
    var handler = sp.GetRequiredService<AuthorizationMessageHandler>();
    handler.InnerHandler = new HttpClientHandler();
    
    return new HttpClient(handler)
    {
        BaseAddress = new Uri("https://localhost:7019/")
    };
});
```
- `sp`: Service Provider (contenedor de DI)
- `GetRequiredService`: Obtiene una instancia del handler
- `InnerHandler`: Handler interno que hace la petición real
- Crea HttpClient con el handler
- **Resultado:** Todas las peticiones pasan por el handler

---

### PASO 2: Registrar servicios

```csharp
builder.Services.AddScoped<ITokenService, TokenService>();
```

**Sintaxis:**
```csharp
AddScoped<Interfaz, Implementación>();
```

**¿Qué hace?**
- Cuando pidas `ITokenService`, te da una instancia de `TokenService`
- Ciclo de vida `Scoped`: Una instancia por usuario/sesión

**Ejemplo de uso:**
```csharp
@inject ITokenService tokenService  // ✅ Funciona
// tokenService es una instancia de TokenService
```

---

### PASO 3: Configurar autenticación

```csharp
builder.Services.AddAuthorizationCore();
```
- Habilita: `[Authorize]`, `<AuthorizeView>`, `<CascadingAuthenticationState>`

```csharp
builder.Services.AddScoped<AuthenticationStateProvider, AuthStateProvider>();
```
- Registra nuestro `AuthStateProvider` personalizado
- Blazor lo usará para verificar autenticación

---

## Paso 8.4: Ciclos de vida de servicios

| Tipo | Duración | Cuándo usar |
|------|----------|-------------|
| **Scoped** | Por usuario/sesión | Servicios con estado del usuario (TokenService, AuthService) |
| **Singleton** | Toda la aplicación | Configuración, caché global |
| **Transient** | Nueva instancia cada vez | Servicios sin estado, cálculos |

**En Blazor WASM:**
- Usa `Scoped` para casi todo
- `Singleton` solo para datos globales
- `Transient` rara vez necesario

---

# PARTE 9: Configurar App.razor

## 🎯 Objetivo
Habilitar autenticación en el router de la aplicación

---

## Paso 9.1: Entender el routing en Blazor

```
Usuario navega a /categorias
         ↓
Router busca @page "/categorias"
         ↓
Verifica si requiere autenticación
         ↓
Si tiene [Authorize] y NO está autenticado → Bloquea
Si tiene [Authorize] y SÍ está autenticado → Permite
```

---

## Paso 9.2: Actualizar App.razor

**Ubicación:** `RecetArreWeb/App.razor`

**ANTES:**
```razor
<Router AppAssembly="@typeof(App).Assembly">
    <Found Context="routeData">
        <RouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)" />
    </Found>
</Router>
```

**DESPUÉS:**
```razor
<CascadingAuthenticationState>
    <Router AppAssembly="@typeof(App).Assembly">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)">
                <NotAuthorized>
                    <div class="container mt-5">
                        <div class="alert alert-warning">
                            <h4>Acceso Denegado</h4>
                            <p>Debes <a href="/login">iniciar sesión</a> para acceder a esta página.</p>
                        </div>
                    </div>
                </NotAuthorized>
            </AuthorizeRouteView>
            <FocusOnNavigate RouteData="@routeData" Selector="h1" />
        </Found>
        <NotFound>
            <PageTitle>Not found</PageTitle>
            <LayoutView Layout="@typeof(MainLayout)">
                <p role="alert">Sorry, there's nothing at this address.</p>
            </LayoutView>
        </NotFound>
    </Router>
</CascadingAuthenticationState>
```

---

## Paso 9.3: Desglose de cada componente

### CascadingAuthenticationState
```razor
<CascadingAuthenticationState>
    <!-- Contenido -->
</CascadingAuthenticationState>
```

**¿Qué hace?**
- Propaga el estado de autenticación a **todos** los componentes hijos
- Permite usar `<AuthorizeView>` en cualquier parte
- Obtiene el estado de `AuthenticationStateProvider`

**Sin esto:**
- `<AuthorizeView>` no funciona
- `@context.User` no existe
- `[Authorize]` no funciona

---

### AuthorizeRouteView (reemplaza RouteView)
```razor
<AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)">
```

**Diferencia:**
- `RouteView`: No verifica autenticación
- `AuthorizeRouteView`: Verifica si la página tiene `[Authorize]`

**Flujo:**
```
Usuario va a /categorias
         ↓
AuthorizeRouteView verifica [Authorize]
         ↓
Verifica AuthenticationState
         ↓
¿Autenticado? → Muestra la página
¿No autenticado? → Muestra <NotAuthorized>
```

---

### NotAuthorized
```razor
<NotAuthorized>
    <div class="alert alert-warning">
        <h4>Acceso Denegado</h4>
        <p>Debes <a href="/login">iniciar sesión</a></p>
    </div>
</NotAuthorized>
```

**Cuándo se muestra:**
- Página tiene `[Authorize]`
- Usuario NO está autenticado

**Personalización:**
- Puedes cambiar el mensaje
- Agregar un botón para login
- Redirigir automáticamente

---

# PARTE 10: Páginas de Login y Registro

## 🎯 Objetivo
Crear páginas funcionales para que el usuario inicie sesión o se registre

---

## Paso 10.1: Actualizar Login.razor

**Ubicación:** `RecetArreWeb/Pages/Login.razor`

### Estructura completa

```razor
@page "/login"
@inject IAuthService authService
@inject NavigationManager navigation
@inject AuthenticationStateProvider authStateProvider

<PageTitle>Login</PageTitle>

<div class="login-page">
    <div class="container">
        <div class="row justify-content-center">
            <div class="col-12 col-sm-10 col-md-7 col-lg-5">
                <div class="card shadow border-0 login-card">
                    <div class="card-body p-4">
                        
                        <!-- Logo y título -->
                        <div class="text-center mb-4">
                            <div class="login-logo bg-brand text-white rounded-circle d-inline-flex align-items-center justify-content-center mb-3">
                                <span class="fw-bold">RA</span>
                            </div>
                            <h2 class="h4 text-brand mb-1">Bienvenido</h2>
                            <p class="text-muted mb-0">Inicia sesión para continuar</p>
                        </div>

                        <!-- Alerta de error -->
                        @if (!string.IsNullOrEmpty(mensajeError))
                        {
                            <div class="alert alert-danger alert-dismissible fade show">
                                <strong>Error:</strong> @mensajeError
                                <button type="button" class="btn-close" 
                                        @onclick="() => mensajeError = string.Empty"></button>
                            </div>
                        }

                        <!-- Formulario -->
                        <EditForm Model="credenciales" OnValidSubmit="HandleLogin">
                            <DataAnnotationsValidator />

                            <!-- Campo Email -->
                            <div class="mb-3">
                                <label class="form-label">Correo electrónico</label>
                                <InputText @bind-Value="credenciales.Email" 
                                           class="form-control" 
                                           placeholder="nombre@correo.com" />
                                <ValidationMessage For="@(() => credenciales.Email)" />
                            </div>

                            <!-- Campo Password -->
                            <div class="mb-3">
                                <label class="form-label">Contraseña</label>
                                <InputText type="password" 
                                           @bind-Value="credenciales.Password" 
                                           class="form-control" 
                                           placeholder="••••••••" />
                                <ValidationMessage For="@(() => credenciales.Password)" />
                            </div>

                            <!-- Botón submit -->
                            <button type="submit" class="btn btn-brand w-100" disabled="@cargando">
                                @if (cargando)
                                {
                                    <span class="spinner-border spinner-border-sm me-2"></span>
                                }
                                Entrar
                            </button>
                        </EditForm>

                        <!-- Link a registro -->
                        <div class="text-center mt-3">
                            <span class="text-muted">¿No tienes cuenta?</span>
                            <a class="link-brand fw-semibold ms-1" href="/registro">Regístrate</a>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

@code {
    private CredencialesUsuario credenciales = new();
    private bool cargando = false;
    private string mensajeError = string.Empty;

    private async Task HandleLogin()
    {
        cargando = true;
        mensajeError = string.Empty;

        try
        {
            // 1. Llamar al servicio de autenticación
            var respuesta = await authService.Login(credenciales);

            if (respuesta != null)
            {
                // 2. Notificar al AuthStateProvider
                var authProvider = (AuthStateProvider)authStateProvider;
                authProvider.NotificarLogin(respuesta.Token);

                // 3. Redirigir a la página principal
                navigation.NavigateTo("/");
            }
            else
            {
                // Login falló
                mensajeError = "Credenciales incorrectas. Verifica tu email y contraseña.";
            }
        }
        catch (Exception ex)
        {
            mensajeError = $"Error al iniciar sesión: {ex.Message}";
        }
        finally
        {
            cargando = false;
        }
    }
}
```

---

## Paso 10.2: Desglose de componentes Blazor

### EditForm
```razor
<EditForm Model="credenciales" OnValidSubmit="HandleLogin">
```

**Propiedades:**
- `Model`: Objeto que contiene los datos del formulario
- `OnValidSubmit`: Método que se ejecuta si la validación pasa

**Validación automática:**
- Lee los `[Required]`, `[EmailAddress]` del DTO
- Muestra errores automáticamente
- Solo permite submit si todo es válido

---

### DataAnnotationsValidator
```razor
<DataAnnotationsValidator />
```

**¿Qué hace?**
- Habilita validación basada en atributos del DTO
- Verifica `[Required]`, `[EmailAddress]`, `[MinLength]`

---

### InputText
```razor
<InputText @bind-Value="credenciales.Email" class="form-control" />
```

**Diferencia con `<input>`:**
- `<input>`: HTML básico
- `<InputText>`: Componente Blazor con validación integrada

**`@bind-Value`:**
- Two-way binding
- Cambios en el input → actualiza `credenciales.Email`
- Cambios en el código → actualiza el input

---

### ValidationMessage
```razor
<ValidationMessage For="@(() => credenciales.Email)" />
```

**¿Qué muestra?**
- Mensajes de error de validación
- Solo se muestra si hay error
- Ejemplo: "El email es requerido"

---

### Botón con spinner
```razor
<button type="submit" class="btn btn-brand w-100" disabled="@cargando">
    @if (cargando)
    {
        <span class="spinner-border spinner-border-sm me-2"></span>
    }
    Entrar
</button>
```

**UX mejorada:**
- Deshabilitado mientras carga
- Muestra spinner visual
- Evita múltiples clicks

---

## Paso 10.3: Lógica del HandleLogin

```csharp
private async Task HandleLogin()
{
    // PASO 1: Preparar
    cargando = true;
    mensajeError = string.Empty;

    try
    {
        // PASO 2: Hacer login
        var respuesta = await authService.Login(credenciales);

        if (respuesta != null)
        {
            // PASO 3: Notificar cambio de estado
            var authProvider = (AuthStateProvider)authStateProvider;
            authProvider.NotificarLogin(respuesta.Token);

            // PASO 4: Redirigir
            navigation.NavigateTo("/");
        }
        else
        {
            mensajeError = "Credenciales incorrectas.";
        }
    }
    catch (Exception ex)
    {
        mensajeError = $"Error: {ex.Message}";
    }
    finally
    {
        // PASO 5: Siempre quitar el spinner
        cargando = false;
    }
}
```

**Flujo visual:**

```
Usuario click "Entrar"
         ↓
cargando = true (spinner aparece, botón deshabilitado)
         ↓
authService.Login() → Backend
         ↓
¿Éxito?
   ↓ SÍ                    ↓ NO
authProvider.NotificarLogin    mensajeError = "..."
   ↓
navigation.NavigateTo("/")
   ↓
cargando = false (spinner desaparece)
```

---

## Paso 10.4: Página de Registro

**Ubicación:** `RecetArreWeb/Pages/Registro.razor`

**Estructura similar a Login, con estas diferencias:**

1. **Campo adicional:**
```razor
<div class="mb-3">
    <label class="form-label">Confirmar Contraseña</label>
    <InputText type="password" 
               @bind-Value="confirmarPassword" 
               class="form-control" />
</div>
```

2. **Validación de contraseñas:**
```csharp
private async Task HandleRegistro()
{
    // Verificar que las contraseñas coincidan
    if (credenciales.Password != confirmarPassword)
    {
        mensajeError = "Las contraseñas no coinciden";
        return;
    }
    
    // ... resto del código
}
```

3. **Llamar a Registrar en lugar de Login:**
```csharp
var respuesta = await authService.Registrar(credenciales);
```

---

# PARTE 11: Actualizar Navegación

## 🎯 Objetivo
Mostrar menú diferente según si el usuario está autenticado

---

## Paso 11.1: Actualizar NavMenu.razor

**Ubicación:** `RecetArreWeb/Layout/NavMenu.razor`

```razor
<div class="@NavMenuCssClass nav-scrollable" @onclick="ToggleNavMenu">
    <nav class="flex-column">
        
        <!-- Home - Siempre visible -->
        <div class="nav-item px-3">
            <NavLink class="nav-link" href="" Match="NavLinkMatch.All">
                <span class="bi bi-house-door-fill"></span> Home
            </NavLink>
        </div>

        <!-- Sección para usuarios AUTENTICADOS -->
        <AuthorizeView>
            <Authorized>
                <div class="nav-item px-3">
                    <NavLink class="nav-link" href="categorias">
                        <span class="bi bi-grid-fill"></span> Categorías
                    </NavLink>
                </div>
                <div class="nav-item px-3">
                    <NavLink class="nav-link" href="counter">
                        <span class="bi bi-plus-square-fill"></span> Counter
                    </NavLink>
                </div>
                <div class="nav-item px-3">
                    <NavLink class="nav-link" href="weather">
                        <span class="bi bi-list-nested"></span> Weather
                    </NavLink>
                </div>
            </Authorized>
        </AuthorizeView>

        <!-- Sección para usuarios NO autenticados -->
        <AuthorizeView>
            <NotAuthorized>
                <div class="nav-item px-3">
                    <NavLink class="nav-link" href="login">
                        <span class="bi bi-box-arrow-in-right"></span> Login
                    </NavLink>
                </div>
                <div class="nav-item px-3">
                    <NavLink class="nav-link" href="registro">
                        <span class="bi bi-person-plus"></span> Registro
                    </NavLink>
                </div>
            </NotAuthorized>
            
            <!-- Sección para usuarios AUTENTICADOS (info y logout) -->
            <Authorized>
                <div class="nav-item px-3">
                    <div class="nav-link text-muted small">
                        <span class="bi bi-person-circle"></span> 
                        @context.User.Identity?.Name
                    </div>
                </div>
                <div class="nav-item px-3">
                    <button class="nav-link btn btn-link" @onclick="Logout">
                        <span class="bi bi-box-arrow-left"></span> Cerrar Sesión
                    </button>
                </div>
            </Authorized>
        </AuthorizeView>
    </nav>
</div>

@code {
    // ...existing code...

    [Inject]
    private IAuthService authService { get; set; } = default!;

    [Inject]
    private AuthenticationStateProvider authStateProvider { get; set; } = default!;

    [Inject]
    private NavigationManager navigation { get; set; } = default!;

    private async Task Logout()
    {
        // 1. Eliminar token
        await authService.Logout();
        
        // 2. Notificar cambio de estado
        var authProvider = (AuthStateProvider)authStateProvider;
        authProvider.NotificarLogout();
        
        // 3. Redirigir al login
        navigation.NavigateTo("/login");
    }
}
```

---

## Paso 11.2: Desglose de AuthorizeView

```razor
<AuthorizeView>
    <NotAuthorized>
        <!-- Usuario NO autenticado -->
        <NavLink href="/login">Login</NavLink>
    </NotAuthorized>
    
    <Authorized>
        <!-- Usuario SÍ autenticado -->
        <NavLink href="/categorias">Categorías</NavLink>
        <button @onclick="Logout">Cerrar Sesión</button>
    </Authorized>
</AuthorizeView>
```

**¿Cómo funciona?**
1. Lee el `AuthenticationState` de `AuthStateProvider`
2. Verifica si hay usuario autenticado
3. Muestra `<NotAuthorized>` o `<Authorized>` según corresponda

**Acceso al usuario:**
```razor
<Authorized>
    @context.User.Identity?.Name
    @context.User.FindFirst(ClaimTypes.Email)?.Value
</Authorized>
```

---

## Paso 11.3: Inyección en @code

```csharp
[Inject]
private IAuthService authService { get; set; } = default!;
```

**Alternativa con @inject:**
```razor
@inject IAuthService authService
```

**Ambas formas son equivalentes.**

---

# PARTE 12: Proteger Páginas con [Authorize]

## 🎯 Objetivo
Hacer que ciertas páginas solo sean accesibles para usuarios autenticados

---

## Paso 12.1: Agregar [Authorize] a Categorias

**Ubicación:** `RecetArreWeb/Pages/Categorias.razor`

```razor
@page "/categorias"
@attribute [Authorize]
@inject ICategoriaService categoriaService
@inject IJSRuntime JSRuntime

<PageTitle>Gestión de Categorías</PageTitle>

<!-- ...resto del código... -->
```

---

## Paso 12.2: ¿Qué hace [Authorize]?

```
Usuario NO autenticado → /categorias
         ↓
AuthorizeRouteView verifica [Authorize]
         ↓
AuthenticationState es anónimo
         ↓
Muestra <NotAuthorized> de App.razor
         ↓
"Debes iniciar sesión para acceder"
```

```
Usuario SÍ autenticado → /categorias
         ↓
AuthorizeRouteView verifica [Authorize]
         ↓
AuthenticationState tiene usuario
         ↓
Muestra la página normalmente
```

---

## Paso 12.3: Proteger otras páginas

```razor
@page "/counter"
@attribute [Authorize]

@page "/weather"
@attribute [Authorize]

@page "/perfil"
@attribute [Authorize]
```

---

## Paso 12.4: Authorize con Roles (Avanzado)

```razor
@page "/admin"
@attribute [Authorize(Roles = "Admin")]
```

**Solo usuarios con rol "Admin" pueden acceder.**

**Claims necesarios en el token:**
```json
{
  "role": "Admin"
}
```

---

# RESUMEN COMPLETO

## 🎯 Flujo de Autenticación Completo

### 1. Registro/Login
```
Usuario → Login.razor → AuthService.Login() 
    → POST /api/Cuentas/Login 
    → Backend valida con Identity 
    → Genera JWT token 
    → Frontend recibe token 
    → TokenService.GuardarToken() 
    → localStorage 
    → AuthStateProvider.NotificarLogin() 
    → Todos los <AuthorizeView> se actualizan 
    → Redirige a "/"
```

### 2. Peticiones Autenticadas
```
Component → httpClient.PostAsync() 
    → AuthorizationMessageHandler.SendAsync() 
    → Obtiene token de localStorage 
    → Agrega header: Authorization: Bearer {token} 
    → Backend recibe y valida token 
    → Permite acceso ✅
```

### 3. Protección de Páginas
```
Usuario → /categorias 
    → AuthorizeRouteView verifica [Authorize] 
    → AuthStateProvider.GetAuthenticationStateAsync() 
    → TokenService.ObtenerToken() 
    → localStorage 
    → ¿Token válido? 
        SÍ → Muestra página 
        NO → Muestra <NotAuthorized>
```

### 4. UI Condicional
```
<AuthorizeView> 
    → Lee AuthenticationState 
    → ¿Usuario autenticado? 
        SÍ → Muestra <Authorized> 
        NO → Muestra <NotAuthorized>
```

---

## 📁 Archivos Creados/Modificados

### Archivos nuevos:
1. ✅ `DTOs/IdentityDtos.cs` - Contratos de datos
2. ✅ `Services/TokenService.cs` - Manejo de localStorage
3. ✅ `Services/AuthService.cs` - Login, Registro, Logout
4. ✅ `Auth/AuthStateProvider.cs` - Estado global de autenticación
5. ✅ `Handlers/AuthorizationMessageHandler.cs` - Token automático en peticiones
6. ✅ `Pages/Registro.razor` - Página de registro

### Archivos modificados:
1. ✅ `RecetArreWeb.csproj` - Paquetes NuGet
2. ✅ `Program.cs` - Registro de servicios
3. ✅ `App.razor` - CascadingAuthenticationState
4. ✅ `Pages/Login.razor` - Funcionalidad de login
5. ✅ `Layout/NavMenu.razor` - Menú condicional
6. ✅ `Pages/Categorias.razor` - [Authorize]
7. ✅ `_Imports.razor` - Namespaces globales

---

## 🎓 Conceptos Clave Explicados

### 1. JWT Token
- String codificado con información del usuario
- Contiene claims (email, id, roles)
- Tiene fecha de expiración
- Se envía en header Authorization

### 2. localStorage
- Almacenamiento del navegador
- Persiste entre sesiones
- Accesible desde JavaScript
- Específico por dominio

### 3. Claims
- Piezas de información del usuario
- Ejemplos: email, id, roles
- Vienen dentro del token JWT
- Se leen con JwtSecurityTokenHandler

### 4. AuthenticationState
- Representa el estado de autenticación
- Contiene ClaimsPrincipal (usuario)
- Se propaga con CascadingAuthenticationState
- Lo gestiona AuthenticationStateProvider

### 5. Inyección de Dependencias
- Contenedor gestiona las instancias
- Registras con AddScoped/AddSingleton/AddTransient
- Inyectas con @inject o [Inject]
- Evita crear instancias manualmente

### 6. DelegatingHandler
- Intercepta peticiones HTTP
- Permite modificar headers, URL, etc.
- Se ejecuta antes de enviar la petición
- Usado para agregar token automáticamente

---

## ✅ Checklist Final

- [ ] Paquetes NuGet instalados
- [ ] DTOs creados (CredencialesUsuario, RespuestaAutenticacion)
- [ ] TokenService implementado
- [ ] AuthService implementado
- [ ] AuthStateProvider configurado
- [ ] AuthorizationMessageHandler creado
- [ ] Program.cs configurado con todos los servicios
- [ ] App.razor con CascadingAuthenticationState
- [ ] Login.razor funcional
- [ ] Registro.razor funcional
- [ ] NavMenu con AuthorizeView
- [ ] Páginas protegidas con [Authorize]
- [ ] Backend con CORS configurado
- [ ] Backend con Identity y JWT configurado

---

## 🔍 Debugging

### Ver el token en el navegador:
```javascript
// F12 → Console
localStorage.getItem('authToken')
```

### Decodificar token:
1. Copiar token de localStorage
2. Ir a https://jwt.io
3. Pegar token
4. Ver claims decodificados

### Ver peticiones HTTP:
1. F12 → Network
2. Filtrar por XHR
3. Ver Headers de peticiones
4. Buscar `Authorization: Bearer ...`

---

## 🎓 Para Explicar a los Alumnos

### Orden sugerido:
1. **Conceptos básicos:** HTTP, JSON, Token JWT
2. **Frontend → Backend:** Program.cs, HttpClient
3. **DTOs:** Contratos de datos
4. **TokenService:** localStorage
5. **AuthService:** Login, Registro
6. **AuthStateProvider:** Estado global
7. **Handler:** Token automático
8. **Login.razor:** Formulario
9. **AuthorizeView:** UI condicional
10. **[Authorize]:** Proteger páginas

### Ejercicios prácticos:
1. ✅ Implementar "Recordarme" (token más duradero)
2. ✅ Crear página de perfil con datos del usuario
3. ✅ Agregar roles y proteger rutas por rol
4. ✅ Renovar token automáticamente antes de expirar
5. ✅ Agregar foto de perfil del usuario

---

## 📚 Recursos Adicionales

- **JWT.io:** https://jwt.io
- **Blazor Auth Docs:** https://learn.microsoft.com/aspnet/core/blazor/security/
- **Identity Docs:** https://learn.microsoft.com/aspnet/core/security/authentication/identity
- **DI Docs:** https://learn.microsoft.com/aspnet/core/blazor/fundamentals/dependency-injection

---

**¡Sistema de autenticación completo implementado!** 🎉
