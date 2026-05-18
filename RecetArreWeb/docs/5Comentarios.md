# 5Comentarios - Sistema de Comentarios en Blazor WebAssembly
## Guía didáctica paso a paso — Módulo integrado dentro de Recetas

---

## 📋 Índice

1. [Entender el módulo de Comentarios](#parte-1-entender-el-módulo)
2. [Crear los DTOs](#parte-2-crear-los-dtos)
3. [Crear el Servicio](#parte-3-crear-el-servicio)
4. [Registrar el Servicio](#parte-4-registrar-el-servicio)
5. [Integrar comentarios en RecetaDetalle](#parte-5-integrar-en-recetadetallerazor)
6. [Crear comentarios](#parte-6-crear-comentarios)
7. [Eliminar comentarios](#parte-7-eliminar-comentarios)
8. [AuthorizeView para acciones](#parte-8-authorizeview-en-comentarios)

---

# PARTE 1: Entender el Módulo

## 🎯 Objetivo
Crear un sistema de comentarios que se integra **dentro** de la página de detalle de una receta.

---

## Paso 1.1: ¿Qué es un Comentario?

Un comentario es un texto que un usuario deja en una receta.

```
Comentario
├── Id          → Identificador único
├── Contenido   → "¡Excelente receta!" (obligatorio, 1-1000 caracteres)
├── CreadoUtc   → Fecha y hora del comentario
├── RecetaId    → ID de la receta donde se hizo
└── UsuarioId   → ID del usuario que lo hizo
```

---

## Paso 1.2: Diferencia fundamental con otros módulos

| Aspecto | Categorías/Ingredientes/Recetas | Comentarios |
|---------|--------------------------------|-------------|
| Página propia | ✅ Sí | ❌ No |
| CRUD completo | ✅ Sí | ⚠️ Parcial (no editar) |
| Modal o formulario | ✅ Sí | ❌ Input inline |
| Listado independiente | ✅ Sí | ❌ Dentro de receta |
| Relación | Independiente | Depende de recetaId |

**Los comentarios NO tienen página propia.** Se muestran y gestionan dentro de `RecetaDetalle.razor`.

---

## Paso 1.3: Endpoints del Backend

| Método | URL | Descripción | Auth |
|--------|-----|-------------|------|
| `GET` | `/api/Comentarios/receta/{recetaId}` | Obtener comentarios de una receta | No |
| `POST` | `/api/Comentarios` | Crear comentario | Sí |
| `PUT` | `/api/Comentarios/{id}` | Actualizar comentario | Sí |
| `DELETE` | `/api/Comentarios/{id}` | Eliminar comentario | Sí |

**Nota importante sobre el GET:**
- **No es** `/api/Comentarios` (obtener todos)
- **Es** `/api/Comentarios/receta/{recetaId}` (filtrado por receta)
- Solo obtenemos los comentarios de UNA receta específica

---

## Paso 1.4: Flujo visual

```
RecetaDetalle.razor
├── Información de la receta (recetaService)
├── Categorías e ingredientes (categoriaService, ingredienteService)
└── Sección Comentarios (comentarioService)     ← AQUÍ
    ├── Input para nuevo comentario
    ├── Lista de comentarios existentes
    └── Botón eliminar en cada comentario
```

---

# PARTE 2: Crear los DTOs

## 🎯 Objetivo
Crear DTOs para comentarios, incluyendo el campo `RecetaId` que los relaciona con una receta.

---

## Paso 2.1: Crear archivo de DTOs

**Crear archivo:** `RecetArreWeb/DTOs/ComentarioDtos.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace RecetArreWeb.DTOs
{
    // DTO para LEER comentarios
    public class ComentarioDto
    {
        public int Id { get; set; }
        public string Contenido { get; set; } = default!;
        public DateTime CreadoUtc { get; set; }
        public int RecetaId { get; set; }
        public string UsuarioId { get; set; } = default!;
    }

    // DTO para CREAR comentarios
    public class ComentarioCreacionDto
    {
        [Required(ErrorMessage = "El comentario es requerido")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "El comentario debe tener entre 1 y 1000 caracteres")]
        public string Contenido { get; set; } = default!;

        [Required]
        public int RecetaId { get; set; }
    }

    // DTO para EDITAR comentarios
    public class ComentarioModificacionDto
    {
        [Required(ErrorMessage = "El comentario es requerido")]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "El comentario debe tener entre 1 y 1000 caracteres")]
        public string Contenido { get; set; } = default!;
    }
}
```

---

## Paso 2.2: Desglose de cada DTO

### ComentarioDto (Lectura)
```csharp
public int Id { get; set; }                        // ID del comentario
public string Contenido { get; set; } = default!;  // Texto del comentario
public DateTime CreadoUtc { get; set; }            // Cuándo se creó
public int RecetaId { get; set; }                  // A qué receta pertenece
public string UsuarioId { get; set; } = default!;  // Quién lo escribió
```

### ComentarioCreacionDto (Creación)
```csharp
public string Contenido { get; set; } = default!;  // Texto del comentario
public int RecetaId { get; set; }                   // A qué receta pertenece
```

**¿Por qué incluye `RecetaId` pero NO `UsuarioId`?**
- `RecetaId`: Lo enviamos nosotros (sabemos en qué receta estamos)
- `UsuarioId`: **Lo obtiene el backend** del token JWT
- El backend lee el token → extrae el UsuarioId → lo asigna al comentario

### ComentarioModificacionDto (Edición)
```csharp
public string Contenido { get; set; } = default!;  // Solo se puede cambiar el texto
```
- **No incluye `RecetaId`**: No puedes mover un comentario a otra receta
- **No incluye `UsuarioId`**: No puedes cambiar el autor

---

# PARTE 3: Crear el Servicio

## 🎯 Objetivo
Crear `ComentarioService` con endpoints ligeramente diferentes a los otros servicios.

---

## Paso 3.1: Crear el archivo

**Crear archivo:** `RecetArreWeb/Services/ComentarioService.cs`

```csharp
using System.Net.Http.Json;
using RecetArreWeb.DTOs;

namespace RecetArreWeb.Services
{
    public interface IComentarioService
    {
        Task<List<ComentarioDto>> ObtenerPorReceta(int recetaId);
        Task<ComentarioDto?> Crear(ComentarioCreacionDto comentarioDto);
        Task<bool> Actualizar(int id, ComentarioModificacionDto comentarioDto);
        Task<bool> Eliminar(int id);
    }

    public class ComentarioService : IComentarioService
    {
        private readonly HttpClient httpClient;
        private const string endpoint = "api/Comentarios";

        public ComentarioService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        // ... implementación ...
    }
}
```

---

## Paso 3.2: Diferencia clave — ObtenerPorReceta

```csharp
public async Task<List<ComentarioDto>> ObtenerPorReceta(int recetaId)
{
    try
    {
        var comentarios = await httpClient.GetFromJsonAsync<List<ComentarioDto>>(
            $"{endpoint}/receta/{recetaId}");
        return comentarios ?? new List<ComentarioDto>();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al obtener comentarios: {ex.Message}");
        return new List<ComentarioDto>();
    }
}
```

**Diferencia con otros servicios:**

| Servicio | Método | URL |
|----------|--------|-----|
| Categorías | `ObtenerTodas()` | `/api/Categorias` |
| Ingredientes | `ObtenerTodos()` | `/api/Ingredientes` |
| Recetas | `ObtenerTodas()` | `/api/Recetas` |
| **Comentarios** | `ObtenerPorReceta(recetaId)` | `/api/Comentarios/receta/{recetaId}` |

**No hay `ObtenerTodos()`** porque:
- No tiene sentido ver TODOS los comentarios de TODAS las recetas
- Siempre se obtienen los de UNA receta específica

---

## Paso 3.3: Crear, Actualizar y Eliminar (mismo patrón)

```csharp
public async Task<ComentarioDto?> Crear(ComentarioCreacionDto comentarioDto)
{
    try
    {
        var response = await httpClient.PostAsJsonAsync(endpoint, comentarioDto);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<ComentarioDto>();
        }
        var error = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Error al crear comentario: {error}");
        return null;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al crear comentario: {ex.Message}");
        return null;
    }
}

public async Task<bool> Actualizar(int id, ComentarioModificacionDto comentarioDto)
{
    try
    {
        var response = await httpClient.PutAsJsonAsync($"{endpoint}/{id}", comentarioDto);
        return response.IsSuccessStatusCode;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al actualizar comentario {id}: {ex.Message}");
        return false;
    }
}

public async Task<bool> Eliminar(int id)
{
    try
    {
        var response = await httpClient.DeleteAsync($"{endpoint}/{id}");
        return response.IsSuccessStatusCode;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al eliminar comentario {id}: {ex.Message}");
        return false;
    }
}
```

---

# PARTE 4: Registrar el Servicio

## Paso 4.1: Agregar al Program.cs

```csharp
builder.Services.AddScoped<IComentarioService, ComentarioService>();
```

**Program.cs final con todos los servicios:**

```csharp
// Registrar servicios
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IIngredienteService, IngredienteService>();
builder.Services.AddScoped<IRecetaService, RecetaService>();
builder.Services.AddScoped<IComentarioService, ComentarioService>();  // ← Agregar
```

---

# PARTE 5: Integrar en RecetaDetalle.razor

## 🎯 Objetivo
Agregar la sección de comentarios dentro de la página de detalle de receta.

---

## Paso 5.1: Inyectar el servicio

En `RecetaDetalle.razor`, agregar:

```razor
@inject IComentarioService comentarioService
```

---

## Paso 5.2: Cargar comentarios al iniciar

```csharp
private RecetaDto? receta;
private List<ComentarioDto>? comentarios;      // ← Agregar
private string nuevoComentario = "";            // ← Agregar
private bool enviandoComentario = false;        // ← Agregar

protected override async Task OnInitializedAsync()
{
    var tareaReceta = recetaService.ObtenerPorId(Id);
    var tareaComentarios = comentarioService.ObtenerPorReceta(Id);  // ← Agregar
    var tareaCategorias = categoriaService.ObtenerTodas();
    var tareaIngredientes = ingredienteService.ObtenerTodos();

    await Task.WhenAll(tareaReceta, tareaComentarios, tareaCategorias, tareaIngredientes);

    receta = tareaReceta.Result;
    comentarios = tareaComentarios.Result;       // ← Agregar
    categorias = tareaCategorias.Result;
    ingredientes = tareaIngredientes.Result;
}
```

**4 peticiones en paralelo:**
1. Datos de la receta
2. Comentarios de la receta
3. Todas las categorías (para mostrar nombres)
4. Todos los ingredientes (para mostrar nombres)

---

## Paso 5.3: Sección de comentarios (HTML)

```razor
<!-- Sección de comentarios -->
<div class="card border-0 shadow-sm mb-4">
    <div class="card-header bg-white d-flex justify-content-between align-items-center">
        <h5 class="mb-0">
            <span class="bi bi-chat-dots me-2"></span>
            Comentarios (@(comentarios?.Count ?? 0))
        </h5>
    </div>
    <div class="card-body">

        <!-- Input para nuevo comentario (solo autenticados) -->
        <AuthorizeView>
            <Authorized>
                <div class="mb-4">
                    <div class="input-group">
                        <input type="text" class="form-control" placeholder="Escribe un comentario..."
                               @bind="nuevoComentario" @bind:event="oninput"
                               @onkeydown="HandleKeyDown" />
                        <button class="btn btn-brand" @onclick="CrearComentario"
                                disabled="@(string.IsNullOrWhiteSpace(nuevoComentario) || enviandoComentario)">
                            @if (enviandoComentario)
                            {
                                <span class="spinner-border spinner-border-sm"></span>
                            }
                            else
                            {
                                <span class="bi bi-send"></span>
                            }
                        </button>
                    </div>
                </div>
            </Authorized>
            <NotAuthorized>
                <div class="alert alert-light mb-4">
                    <a href="/login" class="link-brand">Inicia sesión</a> para comentar.
                </div>
            </NotAuthorized>
        </AuthorizeView>

        <!-- Lista de comentarios -->
        @if (comentarios == null)
        {
            <p class="text-muted text-center">Cargando comentarios...</p>
        }
        else if (!comentarios.Any())
        {
            <p class="text-muted text-center">No hay comentarios todavía. ¡Sé el primero!</p>
        }
        else
        {
            @foreach (var comentario in comentarios)
            {
                <div class="d-flex mb-3 pb-3 border-bottom">
                    <div class="flex-shrink-0">
                        <div class="bg-brand text-white rounded-circle d-flex align-items-center justify-content-center"
                             style="width: 36px; height: 36px; font-size: 0.8rem;">
                            <span class="bi bi-person"></span>
                        </div>
                    </div>
                    <div class="flex-grow-1 ms-3">
                        <div class="d-flex justify-content-between align-items-start">
                            <div>
                                <span class="fw-semibold small">@comentario.UsuarioId[..8]...</span>
                                <span class="text-muted small ms-2">
                                    @comentario.CreadoUtc.ToString("dd/MM/yyyy HH:mm")
                                </span>
                            </div>
                            <AuthorizeView>
                                <Authorized>
                                    <button class="btn btn-sm btn-link text-danger p-0"
                                            @onclick="() => EliminarComentario(comentario.Id)">
                                        <span class="bi bi-x-lg small"></span>
                                    </button>
                                </Authorized>
                            </AuthorizeView>
                        </div>
                        <p class="mb-0 mt-1">@comentario.Contenido</p>
                    </div>
                </div>
            }
        }
    </div>
</div>
```

---

## Paso 5.4: Desglose de cada parte

### Contador de comentarios
```razor
Comentarios (@(comentarios?.Count ?? 0))
```
- `comentarios?.Count`: Si no es null, muestra el conteo
- `?? 0`: Si es null, muestra 0

### Input con envío por Enter
```razor
<input type="text" class="form-control"
       @bind="nuevoComentario" @bind:event="oninput"
       @onkeydown="HandleKeyDown" />
```
- `@bind:event="oninput"`: Actualiza mientras escribe
- `@onkeydown="HandleKeyDown"`: Detecta tecla Enter

### Botón deshabilitado condicionalmente
```razor
<button disabled="@(string.IsNullOrWhiteSpace(nuevoComentario) || enviandoComentario)">
```
- Deshabilitado si: input vacío O está enviando
- Previene: envíos vacíos y doble click

### Avatar circular
```razor
<div class="bg-brand text-white rounded-circle d-flex align-items-center justify-content-center"
     style="width: 36px; height: 36px;">
    <span class="bi bi-person"></span>
</div>
```
- Círculo azul con ícono de persona
- Usando la paleta de colores (`bg-brand`)

### Truncar UsuarioId
```razor
@comentario.UsuarioId[..8]...
```
- El UsuarioId es un GUID largo: `a1b2c3d4-e5f6-7890-...`
- `[..8]` toma los primeros 8 caracteres: `a1b2c3d4...`
- Es una versión simplificada (en producción usarías el nombre)

---

# PARTE 6: Crear Comentarios

## 🎯 Objetivo
Implementar la lógica de creación de comentarios.

---

## Paso 6.1: Enviar con Enter

```csharp
private async Task HandleKeyDown(KeyboardEventArgs e)
{
    if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(nuevoComentario))
    {
        await CrearComentario();
    }
}
```

**`KeyboardEventArgs`:**
- `e.Key`: Nombre de la tecla ("Enter", "Escape", "a", etc.)
- Se ejecuta en cada `keydown` del input

---

## Paso 6.2: Método CrearComentario

```csharp
private async Task CrearComentario()
{
    if (string.IsNullOrWhiteSpace(nuevoComentario)) return;

    enviandoComentario = true;

    // 1. Crear el DTO
    var dto = new ComentarioCreacionDto
    {
        Contenido = nuevoComentario.Trim(),
        RecetaId = Id       // ID de la receta actual
    };

    // 2. Enviar al backend
    var resultado = await comentarioService.Crear(dto);

    if (resultado != null)
    {
        // 3. Éxito: limpiar input y recargar comentarios
        nuevoComentario = "";
        comentarios = await comentarioService.ObtenerPorReceta(Id);
    }
    else
    {
        mensajeError = "No se pudo agregar el comentario";
    }

    enviandoComentario = false;
}
```

**Flujo:**
```
Usuario escribe "¡Gran receta!" + Enter
         ↓
CrearComentario()
         ↓
Crea DTO: { Contenido: "¡Gran receta!", RecetaId: 5 }
         ↓
POST /api/Comentarios (con token JWT en header)
         ↓
Backend: lee UsuarioId del token, crea comentario
         ↓
¿Éxito?
  SÍ → Limpiar input, recargar lista
  NO → Mostrar error
```

**Detalle importante: ¿Cómo se envía el token?**
- El `AuthorizationMessageHandler` (de 1Identity) lo hace automáticamente
- Cada petición POST tiene el header: `Authorization: Bearer eyJ...`
- El backend lee el token y obtiene el UsuarioId

---

# PARTE 7: Eliminar Comentarios

## 🎯 Objetivo
Permitir eliminar un comentario con confirmación.

---

## Paso 7.1: Método EliminarComentario

```csharp
private async Task EliminarComentario(int comentarioId)
{
    if (!await JSRuntime.InvokeAsync<bool>("confirm", "¿Eliminar este comentario?"))
        return;

    var resultado = await comentarioService.Eliminar(comentarioId);

    if (resultado)
    {
        // Recargar la lista de comentarios
        comentarios = await comentarioService.ObtenerPorReceta(Id);
    }
    else
    {
        mensajeError = "No se pudo eliminar el comentario";
    }
}
```

**Mismo patrón que EliminarCategoria:**
1. Confirmar con `JSRuntime`
2. Llamar al servicio
3. Recargar lista

**Nota sobre permisos:**
- El backend verifica que solo el autor del comentario puede eliminarlo
- Si otro usuario intenta eliminar → el backend devuelve `403 Forbidden`
- El servicio retorna `false` y se muestra el error

---

# PARTE 8: AuthorizeView en Comentarios

## 🎯 Objetivo
Mostrar/ocultar elementos según si el usuario está autenticado.

---

## Paso 8.1: Input solo para autenticados

```razor
<AuthorizeView>
    <Authorized>
        <!-- Input para escribir comentario -->
        <div class="input-group">
            <input ... />
            <button ...>Enviar</button>
        </div>
    </Authorized>
    <NotAuthorized>
        <!-- Mensaje para no autenticados -->
        <div class="alert alert-light">
            <a href="/login" class="link-brand">Inicia sesión</a> para comentar.
        </div>
    </NotAuthorized>
</AuthorizeView>
```

**Resultado:**
- **Usuario autenticado**: Ve el input para escribir comentarios
- **Usuario no autenticado**: Ve un link para iniciar sesión

---

## Paso 8.2: Botón eliminar solo para autenticados

```razor
<AuthorizeView>
    <Authorized>
        <button class="btn btn-sm btn-link text-danger"
                @onclick="() => EliminarComentario(comentario.Id)">
            <span class="bi bi-x-lg"></span>
        </button>
    </Authorized>
</AuthorizeView>
```

**Resultado:**
- **Autenticado**: Ve la `X` para eliminar
- **No autenticado**: No ve nada (sin `<NotAuthorized>` no se muestra nada)

**⚠️ Nota de seguridad:**
- Ocultar el botón es UX, no seguridad
- El backend **siempre** valida permisos
- Si alguien hace DELETE directo, el backend rechaza si no es el autor

---

# RESUMEN

## 📁 Archivos

| Archivo | Descripción |
|---------|-------------|
| `DTOs/ComentarioDtos.cs` | ComentarioDto, ComentarioCreacionDto, ComentarioModificacionDto |
| `Services/ComentarioService.cs` | CRUD con endpoint especial (por receta) |
| `Pages/RecetaDetalle.razor` | Integración de comentarios (no tiene página propia) |
| `Program.cs` | Registrar IComentarioService |

## 🎓 Conceptos nuevos

| Concepto | Descripción |
|----------|-------------|
| Módulo integrado | No tiene página propia, vive dentro de otra |
| Endpoint filtrado | `GET /receta/{recetaId}` en vez de `GET /` |
| UsuarioId del token | El backend lo extrae del JWT, no lo enviamos |
| Envío con Enter | `@onkeydown` + `KeyboardEventArgs` |
| Input deshabilitado | `disabled="@(condición1 || condición2)"` |
| AuthorizeView parcial | Solo oculta el input, la lista es pública |
| Recarga de lista | Después de crear/eliminar, re-obtiene todos |

## 🔄 Flujo completo de un comentario

```
1. Usuario ve RecetaDetalle (/recetas/5)
2. OnInitializedAsync() carga receta + comentarios
3. Usuario escribe "¡Excelente!" en el input
4. Presiona Enter → HandleKeyDown() → CrearComentario()
5. Crea ComentarioCreacionDto { Contenido: "¡Excelente!", RecetaId: 5 }
6. POST /api/Comentarios con Authorization: Bearer {token}
7. Backend lee token → UsuarioId = "abc123"
8. Backend crea Comentario en BD
9. Frontend recibe ComentarioDto
10. Limpia input, recarga lista de comentarios
11. Nuevo comentario aparece en la lista
```
