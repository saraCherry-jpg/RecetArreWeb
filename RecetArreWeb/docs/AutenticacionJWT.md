# Autenticación JWT con Identity en Blazor WebAssembly
## Guía completa paso a paso para implementar Login y Registro

---

## 🎯 Objetivo

Implementar un sistema completo de autenticación usando JWT (JSON Web Tokens) con ASP.NET Core Identity en el backend y Blazor WebAssembly en el frontend.

---

## 📚 Arquitectura del Sistema

```
┌──────────────────────┐         JWT Token          ┌──────────────────────┐
│  Blazor WebAssembly  │ ◄────────────────────────► │   ASP.NET Core API   │
│     (Frontend)       │   Login/Registro/Token     │   (Backend + DB)     │
└──────────────────────┘                            └──────────────────────┘
         ↓
    localStorage
    (guarda token)
```

### Flujo de autenticación:

1. **Usuario se registra/login** → Frontend envía email + password
2. **Backend valida** → Si es correcto, genera JWT token
3. **Frontend guarda token** → En localStorage del navegador
4. **Peticiones protegidas** → Frontend envía token en header `Authorization: Bearer {token}`
5. **Backend valida token** → Si es válido, permite acceso

---

## 🔧 Parte 1: Configurar el Backend (Ya hecho)

### ✅ Verificar que el backend tenga:

1. **Identity configurado** en `Program.cs`
2. **JWT configurado** con clave secreta
3. **Controlador de Cuentas** con endpoints:
   - `POST /api/Cuentas/registrar`
   - `POST /api/Cuentas/Login`
   - `GET /api/Cuentas/RenovarToken`
4. **CORS habilitado** para permitir peticiones del frontend

---

## 🔧 Parte 2: DTOs en el Frontend (Data Transfer Objects)

### Paso 2.1: Crear DTOs de autenticación

**Archivo:** `DTOs/IdentityDtos.cs`

```csharp
public class CredencialesUsuario
{
    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El email no es válido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida")]
    [MinLength(6, ErrorMessage = "Mínimo 6 caracteres")]
    public string Password { get; set; } = string.Empty;
}

public class RespuestaAutenticacion
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiracion { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
}
```

**Explicación:**
- `CredencialesUsuario`: Lo que envía el usuario (email + password)
- `RespuestaAutenticacion`: Lo que devuelve el backend (token + info)

---

## 🔧 Parte 3: Servicio de Tokens

### Paso 3.1: Crear TokenService

**Archivo:** `Services/TokenService.cs`

Este servicio maneja el almacenamiento del token en el navegador.

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

**Métodos clave:**

1. **GuardarToken**: Guarda en `localStorage`
```csharp
await jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", token);
```

2. **ObtenerToken**: Lee de `localStorage` y verifica si expiró
```csharp
var token = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", "authToken");
```

3. **EliminarToken**: Borra de `localStorage` (logout)
```csharp
await jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
```

**¿Por qué localStorage?**
- Persiste entre sesiones del navegador
- Accesible desde JavaScript
- El token sobrevive a recargas de página

---

## 🔧 Parte 4: Servicio de Autenticación

### Paso 4.1: Crear AuthService

**Archivo:** `Services/AuthService.cs`

```csharp
public interface IAuthService
{
    Task<RespuestaAutenticacion?> Login(CredencialesUsuario credenciales);
    Task<RespuestaAutenticacion?> Registrar(CredencialesUsuario credenciales);
    Task<RespuestaAutenticacion?> RenovarToken();
    Task Logout();
}
```

### Método Login (paso a paso):

```csharp
public async Task<RespuestaAutenticacion?> Login(CredencialesUsuario credenciales)
{
    // 1. Enviar POST al backend
    var response = await httpClient.PostAsJsonAsync("api/Cuentas/Login", credenciales);

    // 2. Si es exitoso (200-299)
    if (response.IsSuccessStatusCode)
    {
        // 3. Leer la respuesta (token + info)
        var respuesta = await response.Content.ReadFromJsonAsync<RespuestaAutenticacion>();
        
        // 4. Guardar el token en localStorage
        await tokenService.GuardarToken(respuesta.Token, respuesta.Expiracion);
        
        return respuesta;
    }

    return null; // Login falló
}
```

---

## 🔧 Parte 5: Estado de Autenticación

### Paso 5.1: Crear AuthStateProvider

**Archivo:** `Auth/AuthStateProvider.cs`

Este componente gestiona el **estado de autenticación** en toda la aplicación.

```csharp
public class AuthStateProvider : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await tokenService.ObtenerToken();

        if (string.IsNullOrEmpty(token))
            return anonimo; // Usuario no autenticado

        return ConstruirAuthenticationState(token); // Usuario autenticado
    }
}
```

**¿Qué hace?**
1. Lee el token de localStorage
2. Si hay token, lee los **claims** (email, roles, etc.) del JWT
3. Crea un `ClaimsPrincipal` con la info del usuario
4. Notifica a todos los componentes que usan `<AuthorizeView>`

### Paso 5.2: Leer claims del JWT

```csharp
private AuthenticationState ConstruirAuthenticationState(string token)
{
    var handler = new JwtSecurityTokenHandler();
    var jwtToken = handler.ReadJwtToken(token); // Decodifica el token

    var claims = jwtToken.Claims; // Extrae los claims
    var identity = new ClaimsIdentity(claims, "jwt");
    var user = new ClaimsPrincipal(identity);

    return new AuthenticationState(user);
}
```

**Claims típicos en el token:**
- `email`: Email del usuario
- `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier`: ID del usuario
- `http://schemas.microsoft.com/ws/2008/06/identity/claims/role`: Roles

---

## 🔧 Parte 6: Agregar Token Automáticamente a las Peticiones

### Paso 6.1: Crear AuthorizationMessageHandler

**Archivo:** `Handlers/AuthorizationMessageHandler.cs`

Este handler **intercepta** todas las peticiones HTTP y agrega el token automáticamente.

```csharp
public class AuthorizationMessageHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        // 1. Obtener el token
        var token = await tokenService.ObtenerToken();

        // 2. Si hay token, agregarlo al header
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

**Header que se agrega:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Paso 6.2: Configurar HttpClient con el handler

**En `Program.cs`:**

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

**Ahora todas las peticiones incluyen el token automáticamente.**

---

## 🔧 Parte 7: Página de Login

### Paso 7.1: Actualizar Login.razor

**Elementos clave:**

1. **EditForm con validación:**
```razor
<EditForm Model="credenciales" OnValidSubmit="HandleLogin">
    <DataAnnotationsValidator />
    <InputText @bind-Value="credenciales.Email" class="form-control" />
    <ValidationMessage For="@(() => credenciales.Email)" />
</EditForm>
```

2. **Método HandleLogin:**
```csharp
private async Task HandleLogin()
{
    var respuesta = await authService.Login(credenciales);

    if (respuesta != null)
    {
        // Notificar que el usuario se autenticó
        authProvider.NotificarLogin(respuesta.Token);
        
        // Redirigir al inicio
        navigation.NavigateTo("/");
    }
}
```

---

## 🔧 Parte 8: Proteger Rutas

### Paso 8.1: Usar atributo [Authorize]

En cualquier página que quieras proteger:

```razor
@page "/categorias"
@attribute [Authorize]

<!-- Solo usuarios autenticados pueden ver esto -->
```

### Paso 8.2: Configurar App.razor

```razor
<CascadingAuthenticationState>
    <Router AppAssembly="@typeof(App).Assembly">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)">
                <NotAuthorized>
                    <p>Debes <a href="/login">iniciar sesión</a></p>
                </NotAuthorized>
            </AuthorizeRouteView>
        </Found>
    </Router>
</CascadingAuthenticationState>
```

**¿Qué hace?**
- `CascadingAuthenticationState`: Propaga el estado de auth a toda la app
- `AuthorizeRouteView`: Verifica si el usuario puede ver la ruta
- `NotAuthorized`: Qué mostrar si no está autenticado

---

## 🔧 Parte 9: UI Condicional con AuthorizeView

### En NavMenu.razor:

```razor
<AuthorizeView>
    <NotAuthorized>
        <!-- Usuario NO autenticado -->
        <NavLink href="/login">Login</NavLink>
        <NavLink href="/registro">Registro</NavLink>
    </NotAuthorized>
    
    <Authorized>
        <!-- Usuario autenticado -->
        <p>Hola, @context.User.Identity?.Name</p>
        <NavLink href="/categorias">Categorías</NavLink>
        <button @onclick="Logout">Cerrar Sesión</button>
    </Authorized>
</AuthorizeView>
```

**Explicación:**
- `<NotAuthorized>`: Se muestra si NO hay token válido
- `<Authorized>`: Se muestra si HAY token válido
- `@context.User`: Acceso a los claims del usuario

---

## 🔧 Parte 10: Logout

```csharp
private async Task Logout()
{
    // 1. Eliminar token de localStorage
    await authService.Logout();
    
    // 2. Notificar a la app que el usuario cerró sesión
    authProvider.NotificarLogout();
    
    // 3. Redirigir al login
    navigation.NavigateTo("/login");
}
```

---

## 🔍 Debugging: Ver el Token

### En la consola del navegador (F12):

```javascript
// Ver el token guardado
localStorage.getItem('authToken')

// Decodificar el token (copiarlo en jwt.io)
// Ver claims, expiración, etc.
```

### Ver peticiones autenticadas:

1. F12 → Network
2. Hacer una petición a la API
3. Ver Headers → `Authorization: Bearer {token}`

---

## 📝 Ejercicios para Alumnos

### Ejercicio 1: Ver información del usuario
Crear una página `/perfil` que muestre:
- Email del usuario (`@context.User.FindFirst(ClaimTypes.Email)?.Value`)
- ID del usuario
- Fecha de expiración del token

### Ejercicio 2: Renovar token automáticamente
Implementar un timer que renueve el token antes de que expire.

### Ejercicio 3: Roles
Agregar un rol "Admin" y mostrar un menú especial solo para admins:
```razor
<AuthorizeView Roles="Admin">
    <NavLink href="/admin">Panel Admin</NavLink>
</AuthorizeView>
```

### Ejercicio 4: Mensaje de bienvenida
Al hacer login, mostrar un mensaje "Bienvenido {nombre}" usando un toast.

### Ejercicio 5: Remember me
Agregar un checkbox "Recordarme" que guarde el token por más tiempo (30 días en vez de 1 día).

---

## ✅ Checklist de Implementación

- [ ] DTOs creados (CredencialesUsuario, RespuestaAutenticacion)
- [ ] TokenService implementado
- [ ] AuthService implementado
- [ ] AuthStateProvider configurado
- [ ] AuthorizationMessageHandler agregado
- [ ] Program.cs configurado con todos los servicios
- [ ] App.razor con CascadingAuthenticationState
- [ ] Login.razor funcional
- [ ] Registro.razor funcional
- [ ] NavMenu con AuthorizeView
- [ ] Página protegida con [Authorize]
- [ ] Logout funcional
- [ ] Paquetes NuGet instalados:
  - Microsoft.AspNetCore.Components.Authorization
  - System.IdentityModel.Tokens.Jwt

---

## 🎓 Conceptos Clave

1. **JWT Token**: String codificado que contiene claims (info) del usuario
2. **Claims**: Piezas de información (email, roles, id)
3. **localStorage**: Almacenamiento del navegador que persiste entre sesiones
4. **Bearer Token**: Estándar para enviar tokens en el header `Authorization`
5. **AuthenticationState**: Estado global de autenticación en Blazor
6. **DelegatingHandler**: Interceptor de peticiones HTTP
7. **[Authorize]**: Atributo que protege rutas
8. **AuthorizeView**: Componente que muestra contenido según autenticación

---

## ⚠️ Seguridad

### ✅ Buenas prácticas:
- Usar HTTPS siempre (nunca HTTP)
- Tokens con fecha de expiración corta (1-7 días)
- Renovar tokens automáticamente
- Validar en el backend (nunca confiar solo en el frontend)
- No guardar passwords en ningún lado

### ❌ Nunca hacer:
- Guardar contraseñas en localStorage
- Usar tokens sin expiración
- Confiar solo en validación del frontend
- Exponer claves secretas en el frontend

---

## 🔗 Recursos Adicionales

- **JWT.io**: https://jwt.io (decodificar tokens)
- **Docs Blazor Auth**: https://learn.microsoft.com/es-es/aspnet/core/blazor/security/
- **Identity**: https://learn.microsoft.com/es-es/aspnet/core/security/authentication/identity

---

## 🏆 Resumen del Flujo Completo

1. Usuario ingresa email + password → Login.razor
2. Frontend envía POST a `/api/Cuentas/Login`
3. Backend valida con Identity
4. Backend genera JWT token con claims
5. Frontend guarda token en localStorage
6. AuthStateProvider lee el token y actualiza el estado
7. `<AuthorizeView>` muestra contenido de usuario autenticado
8. Peticiones automáticamente incluyen el token en el header
9. Backend valida el token en cada petición
10. Usuario hace logout → se borra el token
