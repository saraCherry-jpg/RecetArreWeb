# 3Ingredientes - CRUD de Ingredientes en Blazor WebAssembly
## Guía didáctica paso a paso (mismo patrón que Categorías)

---

## 📋 Índice

1. [Entender el módulo de Ingredientes](#parte-1-entender-el-módulo)
2. [Crear los DTOs](#parte-2-crear-los-dtos)
3. [Crear el Servicio](#parte-3-crear-el-servicio)
4. [Registrar el Servicio](#parte-4-registrar-el-servicio)
5. [Crear la Página](#parte-5-crear-la-página)
6. [Agregar al menú de navegación](#parte-6-agregar-al-menú)

---

# PARTE 1: Entender el Módulo

## 🎯 Objetivo
Crear un CRUD completo para gestionar ingredientes. Es casi idéntico a Categorías pero con campos diferentes.

---

## Paso 1.1: ¿Qué es un Ingrediente?

Un ingrediente es un componente que se usa en las recetas. Ejemplos: Tomate, Cebolla, Aceite.

```
Ingrediente
├── Id          → Identificador único
├── Nombre      → "Tomate" (obligatorio, máx 80 caracteres)
├── Notas       → "Preferiblemente orgánico" (opcional, máx 250 caracteres)
└── CreadoUtc   → Fecha de creación
```

**Diferencia con Categoría:**
- Categoría tiene `Descripcion` → Ingrediente tiene `Notas`
- El resto de la estructura es idéntica
- ¡Mismo patrón CRUD!

---

## Paso 1.2: Endpoints del Backend

| Método | URL | Descripción | Auth |
|--------|-----|-------------|------|
| `GET` | `/api/Ingredientes` | Obtener todos | No |
| `GET` | `/api/Ingredientes/{id}` | Obtener uno | No |
| `POST` | `/api/Ingredientes` | Crear nuevo | No* |
| `PUT` | `/api/Ingredientes/{id}` | Actualizar | Sí |
| `DELETE` | `/api/Ingredientes/{id}` | Eliminar | Sí |

*El POST de ingredientes no requiere auth actualmente (puede cambiar).

---

## Paso 1.3: Comparación con Categorías

| Aspecto | Categorías | Ingredientes |
|---------|-----------|-------------|
| Campo principal | `Nombre` | `Nombre` |
| Campo secundario | `Descripcion` | `Notas` |
| Endpoint | `/api/Categorias` | `/api/Ingredientes` |
| Servicio | `ICategoriaService` | `IIngredienteService` |
| Página | `Categorias.razor` | `Ingredientes.razor` |

**El patrón es idéntico.** Solo cambian los nombres de campos y endpoints.

---

# PARTE 2: Crear los DTOs

## 🎯 Objetivo
Crear las clases de transferencia de datos para Ingredientes.

---

## Paso 2.1: Agregar al archivo de DTOs

**Ubicación:** `RecetArreWeb/DTOs/CategoriaDto.cs` (se agregan en el mismo archivo)

```csharp
    // DTO para LEER ingredientes
    public class IngredienteDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = default!;
        public string? Notas { get; set; }          // ← En vez de "Descripcion"
        public DateTime CreadoUtc { get; set; }
    }

    // DTO para CREAR ingredientes
    public class IngredienteCreacionDto
    {
        public string Nombre { get; set; } = default!;
        public string? Notas { get; set; }
    }

    // DTO para EDITAR ingredientes
    public class IngredienteModificacionDto
    {
        public string Nombre { get; set; } = default!;
        public string? Notas { get; set; }
    }
```

**Comparación directa con Categoría:**
```
CategoriaDto                    IngredienteDto
├── Id                          ├── Id
├── Nombre                      ├── Nombre
├── Descripcion (string?)       ├── Notas (string?)      ← Diferente
└── CreadoUtc                   └── CreadoUtc
```

---

# PARTE 3: Crear el Servicio

## 🎯 Objetivo
Crear `IngredienteService` siguiendo exactamente el mismo patrón que `CategoriaService`.

---

## Paso 3.1: Crear el archivo

**Crear archivo:** `RecetArreWeb/Services/IngredienteService.cs`

```csharp
using System.Net.Http.Json;
using RecetArreWeb.DTOs;

namespace RecetArreWeb.Services
{
    public interface IIngredienteService
    {
        Task<List<IngredienteDto>> ObtenerTodos();
        Task<IngredienteDto?> ObtenerPorId(int id);
        Task<IngredienteDto?> Crear(IngredienteCreacionDto ingredienteDto);
        Task<bool> Actualizar(int id, IngredienteModificacionDto ingredienteDto);
        Task<bool> Eliminar(int id);
    }

    public class IngredienteService : IIngredienteService
    {
        private readonly HttpClient httpClient;
        private const string endpoint = "api/Ingredientes";  // ← Diferente endpoint

        public IngredienteService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<List<IngredienteDto>> ObtenerTodos()
        {
            try
            {
                var ingredientes = await httpClient.GetFromJsonAsync<List<IngredienteDto>>(endpoint);
                return ingredientes ?? new List<IngredienteDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener ingredientes: {ex.Message}");
                return new List<IngredienteDto>();
            }
        }

        public async Task<IngredienteDto?> ObtenerPorId(int id)
        {
            try
            {
                return await httpClient.GetFromJsonAsync<IngredienteDto>($"{endpoint}/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener ingrediente {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<IngredienteDto?> Crear(IngredienteCreacionDto ingredienteDto)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync(endpoint, ingredienteDto);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<IngredienteDto>();
                }
                
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error al crear ingrediente: {error}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear ingrediente: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> Actualizar(int id, IngredienteModificacionDto ingredienteDto)
        {
            try
            {
                var response = await httpClient.PutAsJsonAsync($"{endpoint}/{id}", ingredienteDto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar ingrediente {id}: {ex.Message}");
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
                Console.WriteLine($"Error al eliminar ingrediente {id}: {ex.Message}");
                return false;
            }
        }
    }
}
```

**Lo que cambia respecto a CategoriaService:**

| Elemento | CategoriaService | IngredienteService |
|----------|-----------------|-------------------|
| Endpoint | `"api/Categorias"` | `"api/Ingredientes"` |
| DTO Lectura | `CategoriaDto` | `IngredienteDto` |
| DTO Creación | `CategoriaCreacionDto` | `IngredienteCreacionDto` |
| DTO Edición | `CategoriaModificacionDto` | `IngredienteModificacionDto` |
| Mensajes error | `"categoría"` | `"ingrediente"` |

**Todo lo demás es idéntico.** Este es el poder de seguir un patrón consistente.

---

# PARTE 4: Registrar el Servicio

## 🎯 Objetivo
Registrar `IngredienteService` en el contenedor de DI.

---

## Paso 4.1: Agregar al Program.cs

**Ubicación:** `RecetArreWeb/Program.cs`

```csharp
// Registrar servicios
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IIngredienteService, IngredienteService>();  // ← Agregar
```

---

# PARTE 5: Crear la Página

## 🎯 Objetivo
Crear `Ingredientes.razor` siguiendo el mismo diseño que `Categorias.razor`.

---

## Paso 5.1: Crear el archivo

**Crear archivo:** `RecetArreWeb/Pages/Ingredientes.razor`

## Paso 5.2: Encabezado

```razor
@page "/ingredientes"
@attribute [Authorize]
@inject IIngredienteService ingredienteService
@inject IJSRuntime JSRuntime

<PageTitle>Gestión de Ingredientes</PageTitle>
```

**Diferencias con Categorías:**
- `@page "/ingredientes"` → ruta diferente
- `@inject IIngredienteService` → servicio diferente

---

## Paso 5.3: Encabezado visual

```razor
<div class="container-fluid py-4">
    <div class="row mb-4">
        <div class="col-12">
            <div class="d-flex justify-content-between align-items-center">
                <div>
                    <h1 class="h3 text-brand mb-1">Gestión de Ingredientes</h1>
                    <p class="text-muted mb-0">Administra los ingredientes disponibles</p>
                </div>
                <button class="btn btn-brand" @onclick="AbrirModalCrear">
                    <span class="bi bi-plus-circle"></span> Nuevo Ingrediente
                </button>
            </div>
        </div>
    </div>
```

---

## Paso 5.4: Alertas (idénticas al patrón)

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

---

## Paso 5.5: Grid de tarjetas

La estructura es la misma, solo cambian los campos:

```razor
            @foreach (var ingrediente in IngredientesFiltrados)
            {
                <div class="col-12 col-sm-6 col-md-4 col-lg-3 mb-3">
                    <div class="card h-100 border-0 shadow-sm">
                        <div class="card-body d-flex flex-column">
                            <div class="d-flex justify-content-between align-items-start mb-2">
                                <h5 class="card-title text-brand mb-0">@ingrediente.Nombre</h5>
                                <!-- ... dropdown igual que categorías ... -->
                            </div>

                            <p class="card-text text-muted small flex-grow-1">
                                @(string.IsNullOrEmpty(ingrediente.Notas) ? "Sin notas" : ingrediente.Notas)
                            </p>

                            <div class="border-top pt-2 mt-auto">
                                <small class="text-muted">
                                    <span class="bi bi-clock"></span>
                                    @ingrediente.CreadoUtc.ToString("dd/MM/yyyy")
                                </small>
                            </div>
                        </div>
                    </div>
                </div>
            }
```

**Cambios respecto a Categorías:**
- `categoria` → `ingrediente`
- `categoria.Descripcion` → `ingrediente.Notas`
- `"Sin descripción"` → `"Sin notas"`
- `CategoriasFiltradas` → `IngredientesFiltrados`

---

## Paso 5.6: Modal crear/editar

```razor
@if (mostrarModal)
{
    <div class="modal fade show d-block" tabindex="-1" style="background-color: rgba(0,0,0,0.5);">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">
                        @(ingredienteEditando == null ? "Nuevo Ingrediente" : "Editar Ingrediente")
                    </h5>
                    <button type="button" class="btn-close" @onclick="CerrarModal"></button>
                </div>
                <div class="modal-body">
                    <div class="mb-3">
                        <label class="form-label">Nombre <span class="text-danger">*</span></label>
                        <input type="text" class="form-control" @bind="nombreIngrediente"
                               placeholder="Ej: Tomate, Cebolla..." />
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Notas</label>
                        <textarea class="form-control" rows="3" @bind="notasIngrediente"
                                  placeholder="Notas adicionales..."></textarea>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" @onclick="CerrarModal">Cancelar</button>
                    <button type="button" class="btn btn-brand" @onclick="GuardarIngrediente" disabled="@guardando">
                        @(ingredienteEditando == null ? "Crear" : "Guardar cambios")
                    </button>
                </div>
            </div>
        </div>
    </div>
}
```

**Cambios respecto a Categorías:**
- `categoriaEditando` → `ingredienteEditando`
- `nombreCategoria` → `nombreIngrediente`
- `descripcionCategoria` → `notasIngrediente`
- `"Descripción"` → `"Notas"`
- `GuardarCategoria` → `GuardarIngrediente`

---

## Paso 5.7: Bloque @code

```csharp
@code {
    private List<IngredienteDto>? ingredientes;
    private string busqueda = "";
    private bool mostrarModal = false;
    private bool guardando = false;
    private IngredienteDto? ingredienteEditando = null;
    private string nombreIngrediente = "";
    private string notasIngrediente = "";
    private string mensajeError = string.Empty;
    private string mensajeExito = string.Empty;

    private IEnumerable<IngredienteDto> IngredientesFiltrados
    {
        get
        {
            if (ingredientes == null) return Enumerable.Empty<IngredienteDto>();
            if (string.IsNullOrWhiteSpace(busqueda)) return ingredientes;
            return ingredientes.Where(i =>
                i.Nombre.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                (i.Notas?.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ?? false)
            );
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await CargarIngredientes();
    }

    private async Task CargarIngredientes()
    {
        try
        {
            ingredientes = await ingredienteService.ObtenerTodos();
        }
        catch (Exception ex)
        {
            mensajeError = $"Error al cargar ingredientes: {ex.Message}";
            ingredientes = new List<IngredienteDto>();
        }
    }

    private void AbrirModalCrear()
    {
        ingredienteEditando = null;
        nombreIngrediente = "";
        notasIngrediente = "";
        mensajeError = string.Empty;
        mostrarModal = true;
    }

    private void AbrirModalEditar(IngredienteDto ingrediente)
    {
        ingredienteEditando = ingrediente;
        nombreIngrediente = ingrediente.Nombre;
        notasIngrediente = ingrediente.Notas ?? "";
        mensajeError = string.Empty;
        mostrarModal = true;
    }

    private void CerrarModal()
    {
        mostrarModal = false;
        ingredienteEditando = null;
        nombreIngrediente = "";
        notasIngrediente = "";
        mensajeError = string.Empty;
    }

    private async Task GuardarIngrediente()
    {
        if (string.IsNullOrWhiteSpace(nombreIngrediente))
        {
            mensajeError = "El nombre es requerido";
            return;
        }

        guardando = true;
        mensajeError = string.Empty;
        mensajeExito = string.Empty;

        try
        {
            if (ingredienteEditando == null)
            {
                var nuevo = new IngredienteCreacionDto
                {
                    Nombre = nombreIngrediente,
                    Notas = string.IsNullOrWhiteSpace(notasIngrediente) ? null : notasIngrediente
                };
                var resultado = await ingredienteService.Crear(nuevo);
                if (resultado != null)
                {
                    mensajeExito = "Ingrediente creado exitosamente";
                    await CargarIngredientes();
                    CerrarModal();
                }
                else
                {
                    mensajeError = "No se pudo crear. Verifica que el nombre no esté duplicado.";
                }
            }
            else
            {
                var modificacion = new IngredienteModificacionDto
                {
                    Nombre = nombreIngrediente,
                    Notas = string.IsNullOrWhiteSpace(notasIngrediente) ? null : notasIngrediente
                };
                var resultado = await ingredienteService.Actualizar(ingredienteEditando.Id, modificacion);
                if (resultado)
                {
                    mensajeExito = "Ingrediente actualizado exitosamente";
                    await CargarIngredientes();
                    CerrarModal();
                }
                else
                {
                    mensajeError = "No se pudo actualizar el ingrediente";
                }
            }
        }
        catch (Exception ex) { mensajeError = $"Error: {ex.Message}"; }
        finally { guardando = false; }
    }

    private async Task EliminarIngrediente(int id)
    {
        if (!await JSRuntime.InvokeAsync<bool>("confirm", "¿Estás seguro de eliminar este ingrediente?"))
            return;

        mensajeError = string.Empty;
        mensajeExito = string.Empty;

        try
        {
            var resultado = await ingredienteService.Eliminar(id);
            if (resultado)
            {
                mensajeExito = "Ingrediente eliminado exitosamente";
                await CargarIngredientes();
            }
            else { mensajeError = "No se pudo eliminar el ingrediente"; }
        }
        catch (Exception ex) { mensajeError = $"Error al eliminar: {ex.Message}"; }
    }

    private void LimpiarBusqueda() { busqueda = ""; }
}
```

---

# PARTE 6: Agregar al Menú

## Paso 6.1: Editar NavMenu.razor

Dentro de `<AuthorizeView><Authorized>`:

```razor
<div class="nav-item px-3">
    <NavLink class="nav-link" href="ingredientes">
        <span class="bi bi-basket" aria-hidden="true"></span> Ingredientes
    </NavLink>
</div>
```

---

# RESUMEN: Patrón de Copia

## 🔄 Para crear un módulo CRUD nuevo, sigue este checklist:

1. **DTO**: Copiar CategoriaDto → cambiar nombres de campos
2. **Service**: Copiar CategoriaService → cambiar endpoint y DTOs
3. **Program.cs**: Agregar `AddScoped<IService, Service>()`
4. **Página**: Copiar Categorias.razor → cambiar variables, campos, textos
5. **NavMenu**: Agregar enlace con ícono

**Tiempo estimado**: 15-20 minutos si copias el patrón.

## 📁 Archivos para Ingredientes

| Archivo | Acción |
|---------|--------|
| `DTOs/CategoriaDto.cs` | Agregar IngredienteDto, CreacionDto, ModificacionDto |
| `Services/IngredienteService.cs` | Crear (copiar patrón de CategoriaService) |
| `Pages/Ingredientes.razor` | Crear (copiar patrón de Categorias.razor) |
| `Program.cs` | Agregar `AddScoped<IIngredienteService, IngredienteService>()` |
| `Layout/NavMenu.razor` | Agregar enlace "Ingredientes" |
