# 4Recetas - CRUD de Recetas en Blazor WebAssembly
## Guía didáctica paso a paso — Módulo complejo con relaciones, formularios y múltiples páginas

---

## 📋 Índice

1. [Entender el módulo de Recetas](#parte-1-entender-el-módulo)
2. [Crear los DTOs](#parte-2-crear-los-dtos)
3. [Crear el Servicio](#parte-3-crear-el-servicio)
4. [Registrar el Servicio](#parte-4-registrar-el-servicio)
5. [Crear la Página de Listado](#parte-5-página-de-listado-recetasrazor)
6. [Crear el Formulario Crear/Editar](#parte-6-formulario-recetaformrazor)
7. [Crear la Página de Detalle](#parte-7-página-de-detalle-recetadetallerazor)
8. [Agregar al menú de navegación](#parte-8-agregar-al-menú)

---

# PARTE 1: Entender el Módulo

## 🎯 Objetivo
Crear un CRUD completo para Recetas. Es más complejo que Categorías porque:
- Tiene **más campos** (título, descripción, instrucciones, tiempos, porciones)
- Tiene **relaciones** con Categorías e Ingredientes (muchos a muchos)
- Necesita **3 páginas** en vez de 1 (listado, formulario, detalle)
- Incluye **comentarios** en el detalle

---

## Paso 1.1: Estructura de una Receta

```
Receta
├── Id                           → Identificador único
├── Titulo                       → "Pasta a la boloñesa" (obligatorio)
├── Descripcion                  → Resumen corto (opcional)
├── Instrucciones                → Pasos detallados (obligatorio)
├── TiempoPreparacionMinutos     → 15 (numérico)
├── TiempoCoccionMinutos         → 30 (numérico)
├── Porciones                    → 4 (numérico)
├── EstaPublicado                → true/false
├── CreadoUtc                    → Fecha creación
├── ModificadoUtc                → Fecha última modificación
├── AutorId                      → ID del usuario que la creó
├── CategoriaIds                 → [1, 3, 5] (relación muchos a muchos)
└── IngredienteIds               → [2, 7, 12] (relación muchos a muchos)
```

---

## Paso 1.2: Endpoints del Backend

| Método | URL | Descripción | Auth |
|--------|-----|-------------|------|
| `GET` | `/api/Recetas` | Obtener todas | No |
| `GET` | `/api/Recetas/{id}` | Obtener una | No |
| `POST` | `/api/Recetas` | Crear nueva | Sí |
| `PUT` | `/api/Recetas/{id}` | Actualizar | Sí |
| `DELETE` | `/api/Recetas/{id}` | Eliminar | Sí |

---

## Paso 1.3: Arquitectura de 3 páginas

```
/recetas              → Listado (grid de tarjetas + búsqueda + filtros)
/recetas/crear        → Formulario vacío para crear
/recetas/editar/{id}  → Formulario con datos para editar
/recetas/{id}         → Detalle con comentarios
```

**¿Por qué 3 páginas y no un modal como Categorías?**
- Las recetas tienen **muchos campos** → un modal sería muy pequeño
- El formulario necesita **checkboxes** para categorías e ingredientes
- El detalle incluye **comentarios** y es una experiencia de lectura
- Es mejor UX tener páginas dedicadas

---

# PARTE 2: Crear los DTOs

## 🎯 Objetivo
Crear DTOs que incluyan las relaciones con Categorías e Ingredientes.

---

## Paso 2.1: Crear archivo de DTOs

**Crear archivo:** `RecetArreWeb/DTOs/RecetaDtos.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace RecetArreWeb.DTOs
{
    // DTO para LEER recetas (lo que devuelve el backend)
    public class RecetaDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = default!;
        public string? Descripcion { get; set; }
        public string Instrucciones { get; set; } = default!;
        public int TiempoPreparacionMinutos { get; set; }
        public int TiempoCoccionMinutos { get; set; }
        public int Porciones { get; set; }
        public bool EstaPublicado { get; set; }
        public DateTime CreadoUtc { get; set; }
        public DateTime ModificadoUtc { get; set; }
        public string AutorId { get; set; } = default!;
        public List<int> CategoriaIds { get; set; } = new();      // ← Relación
        public List<int> IngredienteIds { get; set; } = new();    // ← Relación
    }
}
```

**Novedad: Relaciones muchos a muchos**

```csharp
public List<int> CategoriaIds { get; set; } = new();
```

- No enviamos objetos `CategoriaDto` completos
- Solo enviamos/recibimos los **IDs** de las categorías
- El backend resuelve los objetos completos
- `new()` = inicializa como lista vacía

---

## Paso 2.2: DTOs de Creación y Modificación

```csharp
    // DTO para CREAR recetas
    public class RecetaCreacionDto
    {
        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(120, MinimumLength = 3, ErrorMessage = "El título debe tener entre 3 y 120 caracteres")]
        public string Titulo { get; set; } = default!;

        [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "Las instrucciones son requeridas")]
        [StringLength(15000)]
        public string Instrucciones { get; set; } = default!;

        [Range(0, 1440, ErrorMessage = "El tiempo debe estar entre 0 y 1440 minutos")]
        public int TiempoPreparacionMinutos { get; set; }

        [Range(0, 1440, ErrorMessage = "El tiempo debe estar entre 0 y 1440 minutos")]
        public int TiempoCoccionMinutos { get; set; }

        [Range(1, 100, ErrorMessage = "Las porciones deben estar entre 1 y 100")]
        public int Porciones { get; set; } = 1;

        public bool EstaPublicado { get; set; } = true;

        public List<int> CategoriaIds { get; set; } = new();
        public List<int> IngredienteIds { get; set; } = new();
    }

    // DTO para EDITAR recetas (misma estructura que creación)
    public class RecetaModificacionDto
    {
        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(120, MinimumLength = 3, ErrorMessage = "...")]
        public string Titulo { get; set; } = default!;

        // ... mismos campos que RecetaCreacionDto ...

        public List<int> CategoriaIds { get; set; } = new();
        public List<int> IngredienteIds { get; set; } = new();
    }
```

**Novedad: Data Annotations para validación**

| Atributo | Propósito | Ejemplo |
|----------|-----------|---------|
| `[Required]` | Campo obligatorio | `"El título es requerido"` |
| `[StringLength(120, MinimumLength = 3)]` | Longitud min/max | 3-120 caracteres |
| `[Range(0, 1440)]` | Rango numérico | 0-1440 minutos |

**¿Para qué sirven?**
- Se usan con `<DataAnnotationsValidator />` en los formularios Blazor
- Muestran mensajes de error automáticos con `<ValidationMessage />`
- Evitan enviar datos inválidos al backend

---

# PARTE 3: Crear el Servicio

## 🎯 Objetivo
Crear `RecetaService` — mismo patrón CRUD.

---

## Paso 3.1: Crear el archivo

**Crear archivo:** `RecetArreWeb/Services/RecetaService.cs`

```csharp
using System.Net.Http.Json;
using RecetArreWeb.DTOs;

namespace RecetArreWeb.Services
{
    public interface IRecetaService
    {
        Task<List<RecetaDto>> ObtenerTodas();
        Task<RecetaDto?> ObtenerPorId(int id);
        Task<RecetaDto?> Crear(RecetaCreacionDto recetaDto);
        Task<bool> Actualizar(int id, RecetaModificacionDto recetaDto);
        Task<bool> Eliminar(int id);
    }

    public class RecetaService : IRecetaService
    {
        private readonly HttpClient httpClient;
        private const string endpoint = "api/Recetas";

        public RecetaService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        // ... mismos 5 métodos que CategoriaService ...
        // ObtenerTodas(), ObtenerPorId(), Crear(), Actualizar(), Eliminar()
        // Solo cambian: endpoint, tipos de DTOs, y mensajes de error
    }
}
```

**El servicio es idéntico al de Categorías.** Solo cambia:
- `endpoint = "api/Recetas"`
- Tipos: `RecetaDto`, `RecetaCreacionDto`, `RecetaModificacionDto`
- Mensajes: `"receta"` en vez de `"categoría"`

---

# PARTE 4: Registrar el Servicio

## Paso 4.1: Agregar al Program.cs

```csharp
builder.Services.AddScoped<IRecetaService, RecetaService>();
```

---

# PARTE 5: Página de Listado (Recetas.razor)

## 🎯 Objetivo
Crear la página principal que muestra todas las recetas en tarjetas.

---

## Paso 5.1: Encabezado del archivo

```razor
@page "/recetas"
@attribute [Authorize]
@inject IRecetaService recetaService
@inject ICategoriaService categoriaService
@inject NavigationManager navigation
@inject IJSRuntime JSRuntime
```

**Novedades respecto a Categorías:**
- `@inject ICategoriaService` → Para el filtro por categoría
- `@inject NavigationManager navigation` → Para navegar a otras páginas
- Necesita **2 servicios** porque muestra filtros de categorías

---

## Paso 5.2: Encabezado con botón "Nueva Receta"

```razor
<div class="container-fluid py-4">
    <div class="row mb-4">
        <div class="col-12">
            <div class="d-flex justify-content-between align-items-center">
                <div>
                    <h1 class="h3 text-brand mb-1">Mis Recetas</h1>
                    <p class="text-muted mb-0">Descubre y gestiona tus recetas favoritas</p>
                </div>
                <button class="btn btn-brand" @onclick="IrACrear">
                    <span class="bi bi-plus-circle me-1"></span> Nueva Receta
                </button>
            </div>
        </div>
    </div>
```

**Diferencia clave: Navegación**

En Categorías, el botón abría un modal:
```razor
<button @onclick="AbrirModalCrear">  <!-- Modal -->
```

En Recetas, el botón navega a otra página:
```razor
<button @onclick="IrACrear">  <!-- Navega a /recetas/crear -->
```

---

## Paso 5.3: Filtros (búsqueda + categoría)

```razor
    <div class="row mb-3">
        <div class="col-12">
            <div class="card border-0 shadow-sm">
                <div class="card-body">
                    <div class="row g-3">
                        <div class="col-12 col-md-5 col-lg-4">
                            <input type="text" class="form-control" placeholder="Buscar receta..."
                                   @bind="busqueda" @bind:event="oninput" />
                        </div>
                        <div class="col-12 col-md-4 col-lg-3">
                            <select class="form-select" @bind="categoriaFiltro">
                                <option value="0">Todas las categorías</option>
                                @if (categorias != null)
                                {
                                    @foreach (var cat in categorias)
                                    {
                                        <option value="@cat.Id">@cat.Nombre</option>
                                    }
                                }
                            </select>
                        </div>
                        <div class="col-12 col-md-3 col-lg-2">
                            <button class="btn btn-outline-secondary w-100" @onclick="LimpiarFiltros">
                                Limpiar
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
```

**Novedad: Filtro por categoría con `<select>`**

```razor
<select class="form-select" @bind="categoriaFiltro">
    <option value="0">Todas las categorías</option>
    @foreach (var cat in categorias)
    {
        <option value="@cat.Id">@cat.Nombre</option>
    }
</select>
```

- `@bind="categoriaFiltro"`: Enlaza al entero que guarda el ID seleccionado
- `value="0"`: Opción "Todas" usa ID 0
- El `@foreach` genera opciones dinámicamente desde el backend

---

## Paso 5.4: Tarjetas de recetas

```razor
            @foreach (var receta in RecetasFiltradas)
            {
                <div class="col-12 col-sm-6 col-lg-4 col-xl-3 mb-4">
                    <div class="card h-100 border-0 shadow-sm">
                        <div class="card-body d-flex flex-column">
                            <div class="d-flex justify-content-between align-items-start mb-2">
                                <h5 class="card-title text-brand mb-0 me-2">@receta.Titulo</h5>
                                @if (receta.EstaPublicado)
                                {
                                    <span class="badge bg-success">Publicado</span>
                                }
                                else
                                {
                                    <span class="badge bg-secondary">Borrador</span>
                                }
                            </div>

                            <p class="card-text text-muted small flex-grow-1">
                                @(string.IsNullOrEmpty(receta.Descripcion) 
                                    ? "Sin descripción" 
                                    : (receta.Descripcion.Length > 100 
                                        ? receta.Descripcion[..100] + "..." 
                                        : receta.Descripcion))
                            </p>

                            <div class="d-flex gap-3 text-muted small mb-3">
                                <span>
                                    <span class="bi bi-clock me-1"></span>
                                    @(receta.TiempoPreparacionMinutos + receta.TiempoCoccionMinutos) min
                                </span>
                                <span>
                                    <span class="bi bi-people me-1"></span>
                                    @receta.Porciones porc.
                                </span>
                            </div>

                            <div class="d-flex gap-2 mt-auto">
                                <button class="btn btn-sm btn-brand flex-grow-1"
                                        @onclick="() => IrADetalle(receta.Id)">
                                    Ver
                                </button>
                                <button class="btn btn-sm btn-outline-primary"
                                        @onclick="() => IrAEditar(receta.Id)">
                                    <span class="bi bi-pencil"></span>
                                </button>
                                <button class="btn btn-sm btn-outline-danger"
                                        @onclick="() => EliminarReceta(receta.Id)">
                                    <span class="bi bi-trash"></span>
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            }
```

**Novedades respecto a Categorías:**

1. **Badge de estado:**
```razor
@if (receta.EstaPublicado)
    <span class="badge bg-success">Publicado</span>
else
    <span class="badge bg-secondary">Borrador</span>
```

2. **Truncar descripción:**
```csharp
receta.Descripcion.Length > 100 
    ? receta.Descripcion[..100] + "..." 
    : receta.Descripcion
```
- `[..100]` = tomar primeros 100 caracteres (Range operator de C#)

3. **Íconos de tiempo y porciones:**
```razor
<span class="bi bi-clock"></span> @(prep + coccion) min
<span class="bi bi-people"></span> @receta.Porciones porc.
```

4. **3 botones de acción:**
- Ver → navega a `/recetas/{id}`
- Editar → navega a `/recetas/editar/{id}`
- Eliminar → confirmación + DELETE

---

## Paso 5.5: Bloque @code - Filtrado

```csharp
@code {
    private List<RecetaDto>? recetas;
    private List<CategoriaDto>? categorias;
    private string busqueda = "";
    private int categoriaFiltro = 0;

    private IEnumerable<RecetaDto> RecetasFiltradas
    {
        get
        {
            if (recetas == null) return Enumerable.Empty<RecetaDto>();
            var resultado = recetas.AsEnumerable();

            // Filtro por texto
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                resultado = resultado.Where(r =>
                    r.Titulo.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                    (r.Descripcion?.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            // Filtro por categoría
            if (categoriaFiltro > 0)
            {
                resultado = resultado.Where(r => r.CategoriaIds.Contains(categoriaFiltro));
            }

            return resultado;
        }
    }
```

**Novedad: Filtro combinado**
- Primero filtra por texto (título o descripción)
- Luego filtra por categoría seleccionada
- `r.CategoriaIds.Contains(categoriaFiltro)`: ¿La receta tiene esa categoría?

---

## Paso 5.6: Carga inicial y navegación

```csharp
    protected override async Task OnInitializedAsync()
    {
        // Cargar ambos en paralelo
        var tareaRecetas = recetaService.ObtenerTodas();
        var tareaCategorias = categoriaService.ObtenerTodas();
        await Task.WhenAll(tareaRecetas, tareaCategorias);
        recetas = tareaRecetas.Result;
        categorias = tareaCategorias.Result;
    }

    // Métodos de navegación (evitan problemas con comillas en Razor)
    private void IrACrear() => navigation.NavigateTo("/recetas/crear");
    private void IrADetalle(int id) => navigation.NavigateTo($"/recetas/{id}");
    private void IrAEditar(int id) => navigation.NavigateTo($"/recetas/editar/{id}");
```

**Novedad: `Task.WhenAll` para cargas paralelas**

```csharp
var tareaRecetas = recetaService.ObtenerTodas();      // Inicia petición 1
var tareaCategorias = categoriaService.ObtenerTodas(); // Inicia petición 2
await Task.WhenAll(tareaRecetas, tareaCategorias);     // Espera ambas
```

**Sin WhenAll (secuencial):** ~400ms + ~200ms = **~600ms**
**Con WhenAll (paralelo):** max(~400ms, ~200ms) = **~400ms**

**Novedad: Métodos de navegación**

En Razor, usar `NavigateTo` con strings interpolados en `@onclick` causa problemas de comillas. La solución es crear métodos helper:

```csharp
// En vez de: @onclick="() => navigation.NavigateTo($"/recetas/{id}")"
// Usamos:   @onclick="() => IrADetalle(id)"
private void IrADetalle(int id) => navigation.NavigateTo($"/recetas/{id}");
```

---

# PARTE 6: Formulario (RecetaForm.razor)

## 🎯 Objetivo
Crear una página de formulario reutilizable para Crear y Editar recetas.

---

## Paso 6.1: ¿Por qué una página con 2 rutas?

```razor
@page "/recetas/crear"
@page "/recetas/editar/{Id:int}"
```

**Una sola página maneja ambas rutas:**
- `/recetas/crear` → `Id` es `null` → Modo Crear
- `/recetas/editar/5` → `Id` es `5` → Modo Editar

```csharp
[Parameter] public int? Id { get; set; }  // null = crear, valor = editar
```

---

## Paso 6.2: Encabezado del archivo

```razor
@page "/recetas/crear"
@page "/recetas/editar/{Id:int}"
@attribute [Authorize]
@inject IRecetaService recetaService
@inject ICategoriaService categoriaService
@inject IIngredienteService ingredienteService
@inject NavigationManager navigation
```

**3 servicios inyectados:**
- `recetaService` → CRUD de recetas
- `categoriaService` → Lista de categorías para checkboxes
- `ingredienteService` → Lista de ingredientes para checkboxes

---

## Paso 6.3: Layout del formulario (2 columnas)

```
┌─────────────────────────────────┬──────────────────────┐
│ Información General (col-8)     │ Detalles (col-4)     │
│ ├── Título                      │ ├── Prep. (min)      │
│ ├── Descripción                 │ ├── Cocción (min)    │
│ └── Instrucciones               │ ├── Porciones        │
│                                 │ └── Publicar (switch) │
│ Categorías e Ingredientes       │                      │
│ ├── ☐ Postres ☐ Sopas          │ [Crear Receta]       │
│ └── ☐ Tomate ☐ Cebolla         │ [Cancelar]           │
└─────────────────────────────────┴──────────────────────┘
```

---

## Paso 6.4: EditForm con DataAnnotationsValidator

```razor
<EditForm Model="modelo" OnValidSubmit="HandleSubmit">
    <DataAnnotationsValidator />

    <!-- Campos del formulario -->

</EditForm>
```

**¿Qué hace cada parte?**

### `<EditForm>`
- Componente Blazor para formularios
- `Model="modelo"`: Objeto que contiene los datos
- `OnValidSubmit="HandleSubmit"`: Solo se ejecuta si la validación pasa

### `<DataAnnotationsValidator />`
- Lee los atributos `[Required]`, `[Range]`, `[StringLength]` del DTO
- Valida automáticamente al hacer submit
- Marca campos con error

### Componentes de entrada Blazor vs HTML

| Blazor | HTML | Diferencia |
|--------|------|-----------|
| `<InputText>` | `<input type="text">` | Validación integrada |
| `<InputTextArea>` | `<textarea>` | Validación integrada |
| `<InputNumber>` | `<input type="number">` | Binding tipado (int) |
| `<InputCheckbox>` | `<input type="checkbox">` | Binding a bool |
| `<ValidationMessage>` | (no existe) | Muestra error |

---

## Paso 6.5: Campos de texto

```razor
<div class="mb-3">
    <label class="form-label">Título <span class="text-danger">*</span></label>
    <InputText @bind-Value="modelo.Titulo" class="form-control"
               placeholder="Ej: Pasta a la boloñesa..." />
    <ValidationMessage For="@(() => modelo.Titulo)" />
</div>

<div class="mb-3">
    <label class="form-label">Instrucciones <span class="text-danger">*</span></label>
    <InputTextArea @bind-Value="modelo.Instrucciones" class="form-control" rows="8"
                   placeholder="Paso 1: ..." />
    <ValidationMessage For="@(() => modelo.Instrucciones)" />
</div>
```

**`<ValidationMessage For="@(() => modelo.Titulo)" />`**
- Muestra el mensaje de error del atributo `[Required]`
- Solo aparece si hay error de validación
- Ejemplo: "El título es requerido"

---

## Paso 6.6: Checkboxes de categorías e ingredientes

```razor
<div class="mb-3">
    <label class="form-label">Categorías</label>
    @if (categorias != null)
    {
        <div class="row g-2">
            @foreach (var cat in categorias)
            {
                <div class="col-auto">
                    <div class="form-check">
                        <input class="form-check-input" type="checkbox" id="cat-@cat.Id"
                               checked="@modelo.CategoriaIds.Contains(cat.Id)"
                               @onchange="e => ToggleCategoria(cat.Id, (bool)(e.Value ?? false))" />
                        <label class="form-check-label" for="cat-@cat.Id">@cat.Nombre</label>
                    </div>
                </div>
            }
        </div>
    }
</div>
```

**Desglose de cada parte:**

### Generar checkboxes dinámicamente
```razor
@foreach (var cat in categorias)
```
- Itera sobre las categorías cargadas del backend
- Genera un checkbox por cada categoría

### Checked condicional
```razor
checked="@modelo.CategoriaIds.Contains(cat.Id)"
```
- `Contains(cat.Id)`: ¿Esta categoría está seleccionada?
- Si `CategoriaIds = [1, 3]` y `cat.Id = 3` → checked = true

### Manejar cambio
```razor
@onchange="e => ToggleCategoria(cat.Id, (bool)(e.Value ?? false))"
```
- Se ejecuta al marcar/desmarcar el checkbox
- `e.Value`: Nuevo valor (true/false)
- `(bool)(e.Value ?? false)`: Cast seguro

### Método Toggle
```csharp
private void ToggleCategoria(int id, bool seleccionado)
{
    if (seleccionado && !modelo.CategoriaIds.Contains(id))
        modelo.CategoriaIds.Add(id);    // Agregar
    else if (!seleccionado)
        modelo.CategoriaIds.Remove(id); // Quitar
}
```

---

## Paso 6.7: Campos numéricos y switch

```razor
<!-- Tiempo en minutos -->
<div class="mb-3">
    <label class="form-label">Tiempo de preparación (min)</label>
    <InputNumber @bind-Value="modelo.TiempoPreparacionMinutos" class="form-control" />
    <ValidationMessage For="@(() => modelo.TiempoPreparacionMinutos)" />
</div>

<!-- Switch publicar -->
<div class="mb-3 form-check form-switch">
    <InputCheckbox @bind-Value="modelo.EstaPublicado" class="form-check-input" id="publicado" />
    <label class="form-check-label" for="publicado">Publicar receta</label>
</div>
```

**`<InputNumber>`**: Componente para valores numéricos
- `@bind-Value`: Binding tipado (int, no string)
- Validación automática con `[Range(0, 1440)]`

**`<InputCheckbox>` con `form-switch`**: Toggle visual
- `@bind-Value`: Binding a bool
- `form-switch`: Bootstrap lo muestra como toggle

---

## Paso 6.8: Lógica de Crear/Editar

```csharp
protected override async Task OnInitializedAsync()
{
    // 1. Cargar categorías e ingredientes en paralelo
    var tareaCategorias = categoriaService.ObtenerTodas();
    var tareaIngredientes = ingredienteService.ObtenerTodos();
    await Task.WhenAll(tareaCategorias, tareaIngredientes);
    categorias = tareaCategorias.Result;
    ingredientes = tareaIngredientes.Result;

    // 2. Si es edición, cargar datos existentes
    if (Id.HasValue)
    {
        var receta = await recetaService.ObtenerPorId(Id.Value);
        if (receta == null)
        {
            navigation.NavigateTo("/recetas");
            return;
        }

        // Mapear RecetaDto → RecetaCreacionDto (para el formulario)
        modelo = new RecetaCreacionDto
        {
            Titulo = receta.Titulo,
            Descripcion = receta.Descripcion,
            Instrucciones = receta.Instrucciones,
            TiempoPreparacionMinutos = receta.TiempoPreparacionMinutos,
            TiempoCoccionMinutos = receta.TiempoCoccionMinutos,
            Porciones = receta.Porciones,
            EstaPublicado = receta.EstaPublicado,
            CategoriaIds = receta.CategoriaIds,
            IngredienteIds = receta.IngredienteIds
        };
    }

    cargando = false;
}
```

**Flujo:**
```
¿Id tiene valor?
  NO → Formulario vacío (modo crear)
  SÍ → Cargar receta del backend
       ¿Existe?
         NO → Redirigir a /recetas
         SÍ → Llenar formulario con datos
```

---

## Paso 6.9: HandleSubmit (enviar formulario)

```csharp
private async Task HandleSubmit()
{
    guardando = true;
    mensajeError = string.Empty;

    try
    {
        if (Id.HasValue)
        {
            // EDITAR: Convertir a RecetaModificacionDto
            var modificacion = new RecetaModificacionDto
            {
                Titulo = modelo.Titulo,
                Descripcion = modelo.Descripcion,
                Instrucciones = modelo.Instrucciones,
                TiempoPreparacionMinutos = modelo.TiempoPreparacionMinutos,
                TiempoCoccionMinutos = modelo.TiempoCoccionMinutos,
                Porciones = modelo.Porciones,
                EstaPublicado = modelo.EstaPublicado,
                CategoriaIds = modelo.CategoriaIds,
                IngredienteIds = modelo.IngredienteIds
            };
            var resultado = await recetaService.Actualizar(Id.Value, modificacion);
            if (resultado)
                navigation.NavigateTo($"/recetas/{Id.Value}");  // Ir al detalle
            else
                mensajeError = "No se pudo actualizar la receta";
        }
        else
        {
            // CREAR
            var resultado = await recetaService.Crear(modelo);
            if (resultado != null)
                navigation.NavigateTo($"/recetas/{resultado.Id}");  // Ir al detalle
            else
                mensajeError = "No se pudo crear la receta";
        }
    }
    catch (Exception ex) { mensajeError = $"Error: {ex.Message}"; }
    finally { guardando = false; }
}
```

**Diferencia clave vs Categorías:**
- Después de guardar → **navega al detalle** (no cierra modal)
- Crear devuelve la receta con su ID → navega a `/recetas/{resultado.Id}`

---

# PARTE 7: Página de Detalle (RecetaDetalle.razor)

## 🎯 Objetivo
Crear una página de solo lectura que muestra toda la información de una receta.

---

## Paso 7.1: Encabezado

```razor
@page "/recetas/{Id:int}"
@inject IRecetaService recetaService
@inject IComentarioService comentarioService
@inject ICategoriaService categoriaService
@inject IIngredienteService ingredienteService
@inject NavigationManager navigation
@inject IJSRuntime JSRuntime
```

**No tiene `@attribute [Authorize]`:**
- El detalle es público (cualquiera puede ver una receta)
- Los botones de editar/eliminar están dentro de `<AuthorizeView>`
- Los comentarios requieren auth para crear

---

## Paso 7.2: Layout (2 columnas)

```
┌─────────────────────────────────┬──────────────────────┐
│ Título + Estado      [⋮ menú]  │ Categorías           │
│ Descripción                     │ ├ 🏷 Postres         │
│ ⏱ Prep: 15m  🔥 Cocción: 30m   │ └ 🏷 Italiana        │
│ 👥 4 porciones  ⏳ Total: 45m   │                      │
│                                 │ Ingredientes         │
│ Instrucciones                   │ ├ ✓ Tomate           │
│ Paso 1: ...                     │ ├ ✓ Cebolla          │
│ Paso 2: ...                     │ └ ✓ Aceite           │
│                                 │                      │
│ Comentarios (3)                 │ Información          │
│ ┌──────────────────────┐        │ Creado: 15/12/2024   │
│ │ Escribe un comentario│        │ Modificado: 16/12    │
│ └──────────────────────┘        │                      │
│ 👤 user... | hace 2h            │                      │
│    Excelente receta!            │                      │
└─────────────────────────────────┴──────────────────────┘
```

---

## Paso 7.3: Mostrar categorías e ingredientes por nombre

Las recetas solo tienen IDs (`CategoriaIds = [1, 3]`), pero necesitamos mostrar los **nombres**. Por eso cargamos las listas completas:

```csharp
protected override async Task OnInitializedAsync()
{
    var tareaReceta = recetaService.ObtenerPorId(Id);
    var tareaComentarios = comentarioService.ObtenerPorReceta(Id);
    var tareaCategorias = categoriaService.ObtenerTodas();
    var tareaIngredientes = ingredienteService.ObtenerTodos();

    await Task.WhenAll(tareaReceta, tareaComentarios, tareaCategorias, tareaIngredientes);

    receta = tareaReceta.Result;
    comentarios = tareaComentarios.Result;
    categorias = tareaCategorias.Result;
    ingredientes = tareaIngredientes.Result;
}
```

Y en el HTML se hace el cruce:

```razor
@foreach (var catId in receta.CategoriaIds)
{
    var cat = categorias.FirstOrDefault(c => c.Id == catId);
    if (cat != null)
    {
        <span class="badge bg-brand">@cat.Nombre</span>
    }
}
```

---

## Paso 7.4: Instrucciones con saltos de línea

```razor
<div style="white-space: pre-line;">@receta.Instrucciones</div>
```

- `white-space: pre-line`: Respeta los saltos de línea (`\n`) del texto
- Sin esto, todo se muestra en una sola línea

---

# PARTE 8: Agregar al Menú

## Paso 8.1: Editar NavMenu.razor

```razor
<div class="nav-item px-3">
    <NavLink class="nav-link" href="recetas">
        <span class="bi bi-book" aria-hidden="true"></span> Recetas
    </NavLink>
</div>
```

---

# RESUMEN

## 📁 Archivos

| Archivo | Descripción |
|---------|-------------|
| `DTOs/RecetaDtos.cs` | RecetaDto, RecetaCreacionDto, RecetaModificacionDto |
| `Services/RecetaService.cs` | CRUD + HttpClient |
| `Pages/Recetas.razor` | Listado con grid, búsqueda y filtros |
| `Pages/RecetaForm.razor` | Formulario crear/editar con checkboxes |
| `Pages/RecetaDetalle.razor` | Detalle con comentarios |
| `Program.cs` | Registrar IRecetaService |
| `Layout/NavMenu.razor` | Enlace "Recetas" |

## 🎓 Conceptos nuevos vs Categorías

| Concepto | Categorías | Recetas |
|----------|-----------|---------|
| Páginas | 1 (con modal) | 3 (listado, form, detalle) |
| Relaciones | No | Sí (CategoriaIds, IngredienteIds) |
| Validación | Manual (if empty) | DataAnnotations + EditForm |
| Navegación | Modal | NavigationManager |
| Servicios | 1 | 3+ (receta, categoría, ingrediente) |
| Filtros | Solo texto | Texto + categoría |
| Carga | Secuencial | Paralela (Task.WhenAll) |
| Ruta | 1 (@page) | 2+ rutas por página |
