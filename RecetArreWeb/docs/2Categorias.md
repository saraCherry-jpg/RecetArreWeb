# 2Categorias - CRUD de Categorías en Blazor WebAssembly
## Guía didáctica paso a paso para implementar la gestión de Categorías

---

## 📋 Índice

1. [Entender el módulo de Categorías](#parte-1-entender-el-módulo-de-categorías)
2. [Crear los DTOs](#parte-2-crear-los-dtos)
3. [Crear el Servicio](#parte-3-crear-el-servicio-categoriaservice)
4. [Registrar el Servicio en Program.cs](#parte-4-registrar-el-servicio)
5. [Crear la Página principal](#parte-5-crear-la-página-categoriasrazor)
6. [Agregar búsqueda y filtros](#parte-6-búsqueda-y-filtrado)
7. [Crear el Modal para Crear/Editar](#parte-7-modal-de-creareditar)
8. [Implementar Eliminar](#parte-8-implementar-eliminar)
9. [Agregar al menú de navegación](#parte-9-agregar-al-menú-de-navegación)
10. [Proteger la página](#parte-10-proteger-la-página)

---

# PARTE 1: Entender el Módulo de Categorías

## 🎯 Objetivo
Crear un CRUD completo (Crear, Leer, Actualizar, Eliminar) para gestionar categorías de recetas.

---

## Paso 1.1: ¿Qué es una Categoría?

Una categoría clasifica las recetas. Ejemplos: Postres, Ensaladas, Sopas, Desayunos.

```
Categoría
├── Id              → Identificador único (lo genera el backend)
├── Nombre          → "Postres" (obligatorio, máx 100 caracteres)
├── Descripcion     → "Recetas dulces..." (opcional, máx 500 caracteres)
└── CreadoUtc       → Fecha de creación (lo genera el backend)
```

---

## Paso 1.2: Endpoints del Backend

El backend expone estos endpoints para categorías:

| Método | URL | Descripción | Auth |
|--------|-----|-------------|------|
| `GET` | `/api/Categorias` | Obtener todas | No |
| `GET` | `/api/Categorias/{id}` | Obtener una | No |
| `POST` | `/api/Categorias` | Crear nueva | Sí |
| `PUT` | `/api/Categorias/{id}` | Actualizar | Sí |
| `DELETE` | `/api/Categorias/{id}` | Eliminar | Sí |

---

## Paso 1.3: Flujo general

```
Usuario → Página Categorías → CategoriaService → HttpClient → Backend API
                                                                    ↓
Usuario ← Página actualiza   ← CategoriaService ← JSON      ← Base de datos
```

---

# PARTE 2: Crear los DTOs

## 🎯 Objetivo
Crear las clases que representan los datos de categorías en el frontend.

---

## Paso 2.1: ¿Por qué 3 DTOs diferentes?

Cada operación envía/recibe datos distintos:

| DTO | Propósito | Campos |
|-----|-----------|--------|
| `CategoriaDto` | **Leer** datos del backend | Id, Nombre, Descripcion, CreadoUtc |
| `CategoriaCreacionDto` | **Crear** una categoría | Nombre, Descripcion |
| `CategoriaModificacionDto` | **Editar** una categoría | Nombre, Descripcion |

**¿Por qué no usar uno solo?**
- Al crear, NO enviamos `Id` ni `CreadoUtc` (los genera el backend)
- Al leer, RECIBIMOS todos los campos
- Separar DTOs es una buena práctica

---

## Paso 2.2: Crear el archivo de DTOs

**Crear archivo:** `RecetArreWeb/DTOs/CategoriaDto.cs`

```csharp
namespace RecetArreWeb.DTOs
{
    // DTO para LEER categorías (lo que devuelve el backend)
    public class CategoriaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = default!;
        public string? Descripcion { get; set; }
        public DateTime CreadoUtc { get; set; }
    }

    // DTO para CREAR categorías (lo que enviamos al backend)
    public class CategoriaCreacionDto
    {   
        public string Nombre { get; set; } = default!;
        public string? Descripcion { get; set; }
    }

    // DTO para EDITAR categorías (lo que enviamos al backend)
    public class CategoriaModificacionDto
    {
        public string Nombre { get; set; } = default!;
        public string? Descripcion { get; set; }
    }
}
```

**Desglose:**

### CategoriaDto (Lectura)
```csharp
public int Id { get; set; }                    // Identificador único
public string Nombre { get; set; } = default!; // Nombre de la categoría
public string? Descripcion { get; set; }       // Descripción (puede ser null)
public DateTime CreadoUtc { get; set; }        // Fecha de creación
```
- `default!`: Valor por defecto para strings no-nullable
- `string?`: El `?` indica que puede ser `null`

### CategoriaCreacionDto (Creación)
```csharp
public string Nombre { get; set; } = default!;
public string? Descripcion { get; set; }
```
- **No incluye** `Id` ni `CreadoUtc` → los genera el backend

### CategoriaModificacionDto (Edición)
```csharp
public string Nombre { get; set; } = default!;
public string? Descripcion { get; set; }
```
- Mismos campos que creación en este caso
- En otros módulos pueden diferir

---

# PARTE 3: Crear el Servicio (CategoriaService)

## 🎯 Objetivo
Crear un servicio que se comunica con el backend para realizar operaciones CRUD.

---

## Paso 3.1: ¿Qué es un Servicio?

Un servicio es una clase que encapsula la lógica de comunicación con el backend.

```
Página (UI) → Servicio → HttpClient → Backend
```

**Ventajas:**
- Separa la lógica de UI de la lógica de datos
- Reutilizable desde cualquier página
- Más fácil de mantener y debuggear

---

## Paso 3.2: Definir la Interfaz

**Crear archivo:** `RecetArreWeb/Services/CategoriaService.cs`

Primero, definimos QUÉ puede hacer el servicio:

```csharp
using System.Net.Http.Json;
using RecetArreWeb.DTOs;

namespace RecetArreWeb.Services
{
    public interface ICategoriaService
    {
        Task<List<CategoriaDto>> ObtenerTodas();
        Task<CategoriaDto?> ObtenerPorId(int id);
        Task<CategoriaDto?> Crear(CategoriaCreacionDto categoriaDto);
        Task<bool> Actualizar(int id, CategoriaModificacionDto categoriaDto);
        Task<bool> Eliminar(int id);
    }
}
```

**¿Por qué una interfaz?**
- `ICategoriaService` define el contrato (qué métodos existen)
- `CategoriaService` es la implementación (cómo funcionan)
- Permite usar inyección de dependencias

**Desglose de cada método:**

| Método | Parámetros | Retorno | HTTP |
|--------|-----------|---------|------|
| `ObtenerTodas()` | Ninguno | Lista de categorías | GET |
| `ObtenerPorId(id)` | ID | Una categoría o null | GET |
| `Crear(dto)` | DTO de creación | Categoría creada o null | POST |
| `Actualizar(id, dto)` | ID + DTO | true/false | PUT |
| `Eliminar(id)` | ID | true/false | DELETE |

---

## Paso 3.3: Implementar la clase

```csharp
    public class CategoriaService : ICategoriaService
    {
        private readonly HttpClient httpClient;
        private const string endpoint = "api/Categorias";

        public CategoriaService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
    }
```

**Desglose:**
- `HttpClient httpClient`: Se inyecta automáticamente (Inyección de Dependencias)
- `endpoint = "api/Categorias"`: URL relativa del backend
- URL completa: `https://localhost:7019/api/Categorias`

---

## Paso 3.4: Implementar ObtenerTodas (GET)

```csharp
public async Task<List<CategoriaDto>> ObtenerTodas()
{
    try
    {
        // Hace GET a /api/Categorias y convierte JSON a List<CategoriaDto>
        var categorias = await httpClient.GetFromJsonAsync<List<CategoriaDto>>(endpoint);
        return categorias ?? new List<CategoriaDto>();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al obtener categorías: {ex.Message}");
        return new List<CategoriaDto>();
    }
}
```

**Desglose línea por línea:**

1. `GetFromJsonAsync<List<CategoriaDto>>(endpoint)`:
   - Hace una petición `GET` al endpoint
   - Deserializa el JSON de respuesta a `List<CategoriaDto>`
   - Es `async` porque es una operación de red

2. `categorias ?? new List<CategoriaDto>()`:
   - Si el resultado es `null`, devuelve lista vacía
   - `??` = operador de coalescencia nula

3. `catch (Exception ex)`:
   - Si hay error de red, timeout, etc.
   - Logea el error en consola
   - Devuelve lista vacía (no rompe la app)

---

## Paso 3.5: Implementar ObtenerPorId (GET por ID)

```csharp
public async Task<CategoriaDto?> ObtenerPorId(int id)
{
    try
    {
        return await httpClient.GetFromJsonAsync<CategoriaDto>($"{endpoint}/{id}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al obtener categoría {id}: {ex.Message}");
        return null;
    }
}
```

**Diferencia con ObtenerTodas:**
- URL: `/api/Categorias/5` (con ID)
- Retorna **una** categoría o `null`
- `$"{endpoint}/{id}"` → interpolación de strings

---

## Paso 3.6: Implementar Crear (POST)

```csharp
public async Task<CategoriaDto?> Crear(CategoriaCreacionDto categoriaDto)
{
    try
    {
        // Envía POST con el DTO serializado a JSON
        var response = await httpClient.PostAsJsonAsync(endpoint, categoriaDto);
        
        if (response.IsSuccessStatusCode)
        {
            // Lee la categoría creada del response
            return await response.Content.ReadFromJsonAsync<CategoriaDto>();
        }
        
        var error = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Error al crear categoría: {error}");
        return null;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al crear categoría: {ex.Message}");
        return null;
    }
}
```

**Desglose:**

1. `PostAsJsonAsync(endpoint, categoriaDto)`:
   - Serializa `categoriaDto` a JSON
   - Hace POST a `/api/Categorias`
   - Body: `{ "nombre": "Postres", "descripcion": "Recetas dulces" }`

2. `response.IsSuccessStatusCode`:
   - `true` si código HTTP es 200-299 (éxito)
   - `false` si es 400 (bad request), 401 (unauthorized), etc.

3. `response.Content.ReadFromJsonAsync<CategoriaDto>()`:
   - Lee el body de la respuesta
   - Lo convierte a `CategoriaDto`
   - Incluye el `Id` y `CreadoUtc` generados por el backend

---

## Paso 3.7: Implementar Actualizar (PUT)

```csharp
public async Task<bool> Actualizar(int id, CategoriaModificacionDto categoriaDto)
{
    try
    {
        var response = await httpClient.PutAsJsonAsync($"{endpoint}/{id}", categoriaDto);
        return response.IsSuccessStatusCode;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al actualizar categoría {id}: {ex.Message}");
        return false;
    }
}
```

**Diferencias con Crear:**
- Usa `PutAsJsonAsync` en vez de `PostAsJsonAsync`
- URL incluye el ID: `/api/Categorias/5`
- Retorna `bool` (éxito o fallo), no el objeto

---

## Paso 3.8: Implementar Eliminar (DELETE)

```csharp
public async Task<bool> Eliminar(int id)
{
    try
    {
        var response = await httpClient.DeleteAsync($"{endpoint}/{id}");
        return response.IsSuccessStatusCode;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al eliminar categoría {id}: {ex.Message}");
        return false;
    }
}
```

**Diferencias:**
- Usa `DeleteAsync` (no envía body)
- Solo necesita el ID en la URL

---

## Paso 3.9: Resumen del servicio completo

```
CategoriaService
├── ObtenerTodas()    → GET    /api/Categorias          → List<CategoriaDto>
├── ObtenerPorId(id)  → GET    /api/Categorias/{id}     → CategoriaDto?
├── Crear(dto)        → POST   /api/Categorias          → CategoriaDto?
├── Actualizar(id,dto)→ PUT    /api/Categorias/{id}     → bool
└── Eliminar(id)      → DELETE /api/Categorias/{id}     → bool
```

---

# PARTE 4: Registrar el Servicio

## 🎯 Objetivo
Registrar el servicio en el contenedor de inyección de dependencias.

---

## Paso 4.1: Agregar al Program.cs

**Ubicación:** `RecetArreWeb/Program.cs`

Agregar esta línea en la sección de servicios:

```csharp
// Registrar servicios
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
```

**Explicación:**
- `AddScoped<ICategoriaService, CategoriaService>()`:
  - Cuando alguien pida `ICategoriaService`, se le da una instancia de `CategoriaService`
  - `Scoped`: Una instancia por sesión de usuario

---

## Paso 4.2: Verificar _Imports.razor

**Ubicación:** `RecetArreWeb/_Imports.razor`

Asegurarse de que tenga estos `using`:

```razor
@using RecetArreWeb.Services
@using RecetArreWeb.DTOs
```

Estos `using` permiten usar `ICategoriaService`, `CategoriaDto`, etc. en cualquier página sin repetir el `using` en cada archivo.

---

# PARTE 5: Crear la Página (Categorias.razor)

## 🎯 Objetivo
Crear la página principal que muestra todas las categorías.

---

## Paso 5.1: Crear el archivo

**Crear archivo:** `RecetArreWeb/Pages/Categorias.razor`

---

## Paso 5.2: Encabezado de la página

```razor
@page "/categorias"
@attribute [Authorize]
@inject ICategoriaService categoriaService
@inject IJSRuntime JSRuntime

<PageTitle>Gestión de Categorías</PageTitle>
```

**Desglose:**

### `@page "/categorias"`
- Define la ruta URL de esta página
- Cuando el usuario va a `/categorias`, ve esta página

### `@attribute [Authorize]`
- Protege la página: solo usuarios autenticados
- Si no hay token, muestra `<NotAuthorized>` de App.razor

### `@inject ICategoriaService categoriaService`
- Inyecta una instancia del servicio
- Permite llamar a `categoriaService.ObtenerTodas()`, etc.
- Equivalente a recibir el servicio en el constructor

### `@inject IJSRuntime JSRuntime`
- Permite ejecutar JavaScript desde C#
- Lo usamos para el `confirm()` al eliminar

---

## Paso 5.3: Encabezado visual con botón de crear

```razor
<div class="container-fluid py-4">
    <div class="row mb-4">
        <div class="col-12">
            <div class="d-flex justify-content-between align-items-center">
                <div>
                    <h1 class="h3 text-brand mb-1">Gestión de Categorías</h1>
                    <p class="text-muted mb-0">Administra las categorías de recetas</p>
                </div>
                <button class="btn btn-brand" @onclick="AbrirModalCrear">
                    <span class="bi bi-plus-circle"></span> Nueva Categoría
                </button>
            </div>
        </div>
    </div>
```

**Elementos clave:**
- `text-brand`, `btn-brand`: Clases de nuestra paleta de colores
- `@onclick="AbrirModalCrear"`: Al hacer click, llama al método `AbrirModalCrear()`
- `bi bi-plus-circle`: Ícono de Bootstrap Icons

---

## Paso 5.4: Alertas de éxito y error

```razor
    @if (!string.IsNullOrEmpty(mensajeError))
    {
        <div class="alert alert-danger alert-dismissible fade show" role="alert">
            <strong>Error:</strong> @mensajeError
            <button type="button" class="btn-close" @onclick="() => mensajeError = string.Empty"></button>
        </div>
    }
    @if (!string.IsNullOrEmpty(mensajeExito))
    {
        <div class="alert alert-success alert-dismissible fade show" role="alert">
            <strong>Éxito:</strong> @mensajeExito
            <button type="button" class="btn-close" @onclick="() => mensajeExito = string.Empty"></button>
        </div>
    }
```

**¿Cómo funciona?**
- Solo se muestra si `mensajeError` o `mensajeExito` tienen texto
- El botón `X` limpia el mensaje (lo pone vacío)
- `@onclick="() => mensajeError = string.Empty"`: Lambda que limpia la variable

---

## Paso 5.5: Grid de tarjetas (Cards)

```razor
    <div class="row">
        @if (categorias == null)
        {
            <!-- Estado: Cargando -->
            <div class="col-12">
                <div class="text-center py-5">
                    <div class="spinner-border text-brand" role="status">
                        <span class="visually-hidden">Cargando...</span>
                    </div>
                    <p class="text-muted mt-3">Cargando categorías...</p>
                </div>
            </div>
        }
        else if (!categorias.Any())
        {
            <!-- Estado: Sin datos -->
            <div class="col-12">
                <div class="alert alert-info">
                    <h5 class="alert-heading">No hay categorías</h5>
                    <p class="mb-0">Comienza creando tu primera categoría.</p>
                </div>
            </div>
        }
        else
        {
            <!-- Estado: Con datos -->
            @foreach (var categoria in CategoriasFiltradas)
            {
                <div class="col-12 col-sm-6 col-md-4 col-lg-3 mb-3">
                    <div class="card h-100 border-0 shadow-sm">
                        <div class="card-body d-flex flex-column">
                            <div class="d-flex justify-content-between align-items-start mb-2">
                                <h5 class="card-title text-brand mb-0">@categoria.Nombre</h5>
                                <div class="dropdown">
                                    <button class="btn btn-sm btn-link text-muted" type="button" 
                                            data-bs-toggle="dropdown" aria-expanded="false">
                                        <span class="bi bi-three-dots-vertical"></span>
                                    </button>
                                    <ul class="dropdown-menu">
                                        <li>
                                            <button class="dropdown-item" @onclick="() => AbrirModalEditar(categoria)">
                                                <span class="bi bi-pencil"></span> Editar
                                            </button>
                                        </li>
                                        <li><hr class="dropdown-divider"></li>
                                        <li>
                                            <button class="dropdown-item text-danger" @onclick="() => EliminarCategoria(categoria.Id)">
                                                <span class="bi bi-trash"></span> Eliminar
                                            </button>
                                        </li>
                                    </ul>
                                </div>
                            </div>
                            
                            <p class="card-text text-muted small flex-grow-1">
                                @(string.IsNullOrEmpty(categoria.Descripcion) ? "Sin descripción" : categoria.Descripcion)
                            </p>
                            
                            <div class="border-top pt-2 mt-auto">
                                <small class="text-muted">
                                    <span class="bi bi-clock"></span> 
                                    @categoria.CreadoUtc.ToString("dd/MM/yyyy")
                                </small>
                            </div>
                        </div>
                    </div>
                </div>
            }
        }
    </div>
```

**3 estados de la UI:**

1. **`categorias == null`** → Spinner de carga
   - La lista empieza como `null`
   - Mientras se hace la petición HTTP, se muestra el spinner

2. **`!categorias.Any()`** → Mensaje "no hay datos"
   - La petición terminó pero la lista está vacía

3. **`else`** → Grid de tarjetas
   - Itera sobre `CategoriasFiltradas` (propiedad con búsqueda)

**Diseño responsive:**
```
col-12     → En móvil: 1 tarjeta por fila
col-sm-6   → En tablet: 2 tarjetas por fila
col-md-4   → En desktop: 3 tarjetas por fila
col-lg-3   → En pantallas grandes: 4 tarjetas por fila
```

**Menú dropdown (tres puntos):**
- `data-bs-toggle="dropdown"`: Bootstrap abre el menú
- Opciones: Editar y Eliminar
- `@onclick="() => AbrirModalEditar(categoria)"`: Pasa la categoría completa al método

---

## Paso 5.6: Vista de lista (tabla)

```razor
    <div class="row mt-4">
        <div class="col-12">
            <div class="card border-0 shadow-sm">
                <div class="card-header bg-white">
                    <h5 class="mb-0">Vista de Lista</h5>
                </div>
                <div class="card-body p-0">
                    <div class="table-responsive">
                        <table class="table table-hover mb-0">
                            <thead class="table-light">
                                <tr>
                                    <th scope="col">ID</th>
                                    <th scope="col">Nombre</th>
                                    <th scope="col">Descripción</th>
                                    <th scope="col">Fecha Creación</th>
                                    <th scope="col" class="text-end">Acciones</th>
                                </tr>
                            </thead>
                            <tbody>
                                @if (categorias != null && categorias.Any())
                                {
                                    @foreach (var categoria in CategoriasFiltradas)
                                    {
                                        <tr>
                                            <td>@categoria.Id</td>
                                            <td class="fw-semibold">@categoria.Nombre</td>
                                            <td class="text-muted">
                                                @(string.IsNullOrEmpty(categoria.Descripcion) ? "-" : categoria.Descripcion)
                                            </td>
                                            <td>@categoria.CreadoUtc.ToString("dd/MM/yyyy HH:mm")</td>
                                            <td class="text-end">
                                                <button class="btn btn-sm btn-outline-primary me-1" 
                                                        @onclick="() => AbrirModalEditar(categoria)">
                                                    <span class="bi bi-pencil"></span>
                                                </button>
                                                <button class="btn btn-sm btn-outline-danger" 
                                                        @onclick="() => EliminarCategoria(categoria.Id)">
                                                    <span class="bi bi-trash"></span>
                                                </button>
                                            </td>
                                        </tr>
                                    }
                                }
                                else
                                {
                                    <tr>
                                        <td colspan="5" class="text-center text-muted py-4">
                                            No hay categorías para mostrar
                                        </td>
                                    </tr>
                                }
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
```

---

## Paso 5.7: Bloque @code - Variables de estado

```csharp
@code {
    private List<CategoriaDto>? categorias;
    private string busqueda = "";
    private bool mostrarModal = false;
    private bool guardando = false;
    private CategoriaDto? categoriaEditando = null;
    private string nombreCategoria = "";
    private string descripcionCategoria = "";
    private string mensajeError = string.Empty;
    private string mensajeExito = string.Empty;
```

**Cada variable tiene un propósito:**

| Variable | Tipo | Propósito |
|----------|------|-----------|
| `categorias` | `List<CategoriaDto>?` | Lista de datos (null = cargando) |
| `busqueda` | `string` | Texto del campo de búsqueda |
| `mostrarModal` | `bool` | Controla si el modal es visible |
| `guardando` | `bool` | Deshabilita botón mientras guarda |
| `categoriaEditando` | `CategoriaDto?` | null = crear, objeto = editar |
| `nombreCategoria` | `string` | Valor del input nombre en el modal |
| `descripcionCategoria` | `string` | Valor del textarea descripción |
| `mensajeError` | `string` | Texto de la alerta roja |
| `mensajeExito` | `string` | Texto de la alerta verde |

---

## Paso 5.8: Cargar datos al iniciar

```csharp
    protected override async Task OnInitializedAsync()
    {
        await CargarCategorias();
    }

    private async Task CargarCategorias()
    {
        try
        {
            categorias = await categoriaService.ObtenerTodas();
        }
        catch (Exception ex)
        {
            mensajeError = $"Error al cargar categorías: {ex.Message}";
            categorias = new List<CategoriaDto>();
        }
    }
```

**`OnInitializedAsync()`:**
- Se ejecuta automáticamente al cargar la página
- Es un método de ciclo de vida de Blazor
- Equivalente a `ngOnInit()` en Angular

**Flujo:**
```
Página carga → categorias = null → Spinner visible
       ↓
OnInitializedAsync()
       ↓
categoriaService.ObtenerTodas()
       ↓
categorias = [lista de datos] → Grid visible
```

---

# PARTE 6: Búsqueda y Filtrado

## 🎯 Objetivo
Permitir al usuario buscar categorías por nombre o descripción.

---

## Paso 6.1: Campo de búsqueda (HTML)

```razor
    <div class="row mb-3">
        <div class="col-12">
            <div class="card border-0 shadow-sm">
                <div class="card-body">
                    <div class="row g-3">
                        <div class="col-12 col-md-6 col-lg-4">
                            <input type="text" class="form-control" placeholder="Buscar categoría..." 
                                   @bind="busqueda" @bind:event="oninput" />
                        </div>
                        <div class="col-12 col-md-6 col-lg-3">
                            <button class="btn btn-outline-secondary w-100" @onclick="LimpiarBusqueda">
                                <span class="bi bi-x-circle"></span> Limpiar
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
```

**Elementos clave:**
- `@bind="busqueda"`: Two-way binding con la variable `busqueda`
- `@bind:event="oninput"`: Filtra en **tiempo real** mientras escribe (no al perder foco)

---

## Paso 6.2: Propiedad de filtrado (C#)

```csharp
    private IEnumerable<CategoriaDto> CategoriasFiltradas
    {
        get
        {
            if (categorias == null) return Enumerable.Empty<CategoriaDto>();
            
            if (string.IsNullOrWhiteSpace(busqueda))
                return categorias;
            
            return categorias.Where(c => 
                c.Nombre.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                (c.Descripcion?.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ?? false)
            );
        }
    }
```

**Desglose:**
1. Si `categorias` es null, devuelve lista vacía
2. Si `busqueda` está vacía, devuelve todas
3. Si hay búsqueda, filtra por nombre O descripción
4. `StringComparison.OrdinalIgnoreCase`: Ignora mayúsculas/minúsculas
5. `c.Descripcion?.Contains(...)`: El `?` evita error si Descripcion es null
6. `?? false`: Si `Contains` devuelve null, usa `false`

**¿Por qué es una propiedad y no un método?**
- Se recalcula automáticamente cada vez que cambia `busqueda`
- En Razor se usa como: `@foreach (var cat in CategoriasFiltradas)`

---

## Paso 6.3: Limpiar búsqueda

```csharp
    private void LimpiarBusqueda()
    {
        busqueda = "";
    }
```

---

# PARTE 7: Modal de Crear/Editar

## 🎯 Objetivo
Crear un modal reutilizable para crear y editar categorías.

---

## Paso 7.1: ¿Por qué un solo modal para ambas acciones?

- **Crear**: El modal se abre vacío
- **Editar**: El modal se abre con los datos de la categoría
- La variable `categoriaEditando` determina el modo:
  - `null` = Crear
  - `objeto` = Editar

---

## Paso 7.2: HTML del Modal

```razor
@if (mostrarModal)
{
    <div class="modal fade show d-block" tabindex="-1" style="background-color: rgba(0,0,0,0.5);">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">
                        @(categoriaEditando == null ? "Nueva Categoría" : "Editar Categoría")
                    </h5>
                    <button type="button" class="btn-close" @onclick="CerrarModal"></button>
                </div>
                <div class="modal-body">
                    <div class="mb-3">
                        <label class="form-label">Nombre <span class="text-danger">*</span></label>
                        <input type="text" class="form-control" @bind="nombreCategoria" 
                               placeholder="Ej: Postres, Ensaladas..." />
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Descripción</label>
                        <textarea class="form-control" rows="3" @bind="descripcionCategoria"
                                  placeholder="Describe brevemente esta categoría..."></textarea>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" @onclick="CerrarModal">Cancelar</button>
                    <button type="button" class="btn btn-brand" @onclick="GuardarCategoria" disabled="@guardando">
                        @(categoriaEditando == null ? "Crear" : "Guardar cambios")
                    </button>
                </div>
            </div>
        </div>
    </div>
}
```

**Desglose:**
- `@if (mostrarModal)`: Solo renderiza si es true
- `show d-block`: Clases Bootstrap para mostrar el modal
- `style="background-color: rgba(0,0,0,0.5)"`: Fondo oscuro semi-transparente
- `@(categoriaEditando == null ? "Nueva" : "Editar")`: Título dinámico
- `@bind="nombreCategoria"`: Two-way binding
- `disabled="@guardando"`: Deshabilita el botón mientras guarda

---

## Paso 7.3: Métodos para abrir/cerrar el modal

```csharp
    private void AbrirModalCrear()
    {
        categoriaEditando = null;       // Modo: Crear
        nombreCategoria = "";           // Limpiar campos
        descripcionCategoria = "";
        mensajeError = string.Empty;
        mostrarModal = true;            // Mostrar modal
    }

    private void AbrirModalEditar(CategoriaDto categoria)
    {
        categoriaEditando = categoria;                  // Modo: Editar
        nombreCategoria = categoria.Nombre;             // Cargar datos
        descripcionCategoria = categoria.Descripcion ?? "";
        mensajeError = string.Empty;
        mostrarModal = true;                            // Mostrar modal
    }

    private void CerrarModal()
    {
        mostrarModal = false;           // Ocultar modal
        categoriaEditando = null;       // Resetear modo
        nombreCategoria = "";           // Limpiar campos
        descripcionCategoria = "";
        mensajeError = string.Empty;
    }
```

---

## Paso 7.4: Método GuardarCategoria (Crear o Actualizar)

```csharp
    private async Task GuardarCategoria()
    {
        // 1. Validar
        if (string.IsNullOrWhiteSpace(nombreCategoria))
        {
            mensajeError = "El nombre es requerido";
            return;
        }

        guardando = true;
        mensajeError = string.Empty;
        mensajeExito = string.Empty;

        try
        {
            if (categoriaEditando == null)
            {
                // 2A. CREAR nueva categoría
                var nuevaCategoria = new CategoriaCreacionDto
                {
                    Nombre = nombreCategoria,
                    Descripcion = string.IsNullOrWhiteSpace(descripcionCategoria) ? null : descripcionCategoria
                };

                var resultado = await categoriaService.Crear(nuevaCategoria);
                
                if (resultado != null)
                {
                    mensajeExito = "Categoría creada exitosamente";
                    await CargarCategorias();  // Recargar lista
                    CerrarModal();
                }
                else
                {
                    mensajeError = "No se pudo crear la categoría. Verifica que el nombre no esté duplicado.";
                }
            }
            else
            {
                // 2B. ACTUALIZAR categoría existente
                var categoriaModificacion = new CategoriaModificacionDto
                {
                    Nombre = nombreCategoria,
                    Descripcion = string.IsNullOrWhiteSpace(descripcionCategoria) ? null : descripcionCategoria
                };

                var resultado = await categoriaService.Actualizar(categoriaEditando.Id, categoriaModificacion);
                
                if (resultado)
                {
                    mensajeExito = "Categoría actualizada exitosamente";
                    await CargarCategorias();
                    CerrarModal();
                }
                else
                {
                    mensajeError = "No se pudo actualizar la categoría";
                }
            }
        }
        catch (Exception ex)
        {
            mensajeError = $"Error: {ex.Message}";
        }
        finally
        {
            guardando = false;  // Siempre quitar el spinner
        }
    }
```

**Flujo visual:**

```
GuardarCategoria()
       ↓
¿nombreCategoria vacío?
  SÍ → mensajeError, return
  NO ↓
¿categoriaEditando == null?
  SÍ → Crear (POST)
  NO → Actualizar (PUT)
       ↓
¿Éxito?
  SÍ → mensajeExito, recargar, cerrar modal
  NO → mensajeError
       ↓
finally: guardando = false
```

---

# PARTE 8: Implementar Eliminar

## 🎯 Objetivo
Eliminar una categoría con confirmación del usuario.

---

## Paso 8.1: Método EliminarCategoria

```csharp
    private async Task EliminarCategoria(int id)
    {
        // 1. Confirmación con JavaScript
        if (!await JSRuntime.InvokeAsync<bool>("confirm", "¿Estás seguro de eliminar esta categoría?"))
            return;

        mensajeError = string.Empty;
        mensajeExito = string.Empty;

        try
        {
            // 2. Llamar al servicio
            var resultado = await categoriaService.Eliminar(id);
            
            if (resultado)
            {
                mensajeExito = "Categoría eliminada exitosamente";
                await CargarCategorias();  // Recargar lista
            }
            else
            {
                mensajeError = "No se pudo eliminar la categoría";
            }
        }
        catch (Exception ex)
        {
            mensajeError = $"Error al eliminar: {ex.Message}";
        }
    }
```

**`JSRuntime.InvokeAsync<bool>("confirm", "...")`:**
- Ejecuta `confirm()` de JavaScript
- Muestra diálogo nativo del navegador
- Devuelve `true` si acepta, `false` si cancela
- Si cancela → `return` (no hace nada)

---

# PARTE 9: Agregar al Menú de Navegación

## 🎯 Objetivo
Agregar el enlace de Categorías al menú lateral.

---

## Paso 9.1: Editar NavMenu.razor

**Ubicación:** `RecetArreWeb/Layout/NavMenu.razor`

Dentro de `<AuthorizeView><Authorized>`, agregar:

```razor
<div class="nav-item px-3">
    <NavLink class="nav-link" href="categorias">
        <span class="bi bi-grid-fill" aria-hidden="true"></span> Categorías
    </NavLink>
</div>
```

**Explicación:**
- `<AuthorizeView><Authorized>`: Solo visible si está autenticado
- `href="categorias"`: Navega a `/categorias`
- `bi bi-grid-fill`: Ícono de Bootstrap Icons

---

# PARTE 10: Proteger la Página

## 🎯 Objetivo
Asegurar que solo usuarios autenticados puedan acceder.

---

## Paso 10.1: Atributo [Authorize]

Ya lo agregamos en el paso 5.2:

```razor
@attribute [Authorize]
```

**Flujo de protección:**
```
Usuario NO autenticado → /categorias
         ↓
AuthorizeRouteView verifica [Authorize]
         ↓
Muestra <NotAuthorized> de App.razor
         ↓
"Debes iniciar sesión para acceder"
```

---

# RESUMEN

## 📁 Archivos creados/modificados

| Archivo | Acción | Descripción |
|---------|--------|-------------|
| `DTOs/CategoriaDto.cs` | Crear | DTOs: CategoriaDto, CreacionDto, ModificacionDto |
| `Services/CategoriaService.cs` | Crear | Servicio CRUD con HttpClient |
| `Pages/Categorias.razor` | Crear | Página con grid, tabla, modal, búsqueda |
| `Program.cs` | Modificar | Registrar ICategoriaService |
| `Layout/NavMenu.razor` | Modificar | Agregar enlace "Categorías" |

## 🎓 Conceptos aprendidos

1. **DTOs**: Clases para transferir datos entre frontend y backend
2. **Servicios**: Clases que encapsulan lógica de comunicación HTTP
3. **Inyección de dependencias**: Registrar y consumir servicios
4. **Binding**: Two-way binding con `@bind`
5. **Renderizado condicional**: `@if`, `@foreach`
6. **Modales**: Crear/editar con un solo modal
7. **Confirmación**: Usar JSRuntime para `confirm()`
8. **Búsqueda**: Filtrado en tiempo real con propiedades computadas
9. **Estados de UI**: Cargando, sin datos, con datos
10. **Protección**: `[Authorize]` para rutas protegidas
