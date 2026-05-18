# Consumo de API desde Blazor WebAssembly
## Guía paso a paso para conectar el frontend con el backend

---

## 🎯 Objetivo

Aprender a consumir una API REST desde Blazor WebAssembly utilizando servicios HttpClient, implementando las operaciones CRUD completas (Crear, Leer, Actualizar, Eliminar).

---

## 📚 Conceptos Básicos

### ¿Qué es un servicio en Blazor?

Un **servicio** es una clase que encapsula la lógica para comunicarse con el backend. En lugar de escribir código HttpClient directamente en los componentes, creamos servicios reutilizables que manejan todas las llamadas a la API.

**Ventajas:**
- **Reutilización**: El mismo servicio se usa en múltiples páginas
- **Mantenimiento**: Si cambia la URL del API, solo se modifica en un lugar
- **Testeo**: Puedes crear versiones de prueba (mock) fácilmente
- **Separación de responsabilidades**: La UI no se mezcla con la lógica de datos

### Patrón de diseño utilizado

Usamos el patrón **Repository/Service** con **Dependency Injection (Inyección de Dependencias)**:

1. Definimos una **interfaz** (`ICategoriaService`) con los métodos
2. Creamos una **implementación** (`CategoriaService`) que usa HttpClient
3. **Registramos** el servicio en `Program.cs`
4. **Inyectamos** el servicio en los componentes con `@inject`

---

## 🔧 Parte 1: Crear los DTOs (Data Transfer Objects)

### Paso 1.1: Entender qué son los DTOs

Los DTOs son clases que representan la estructura de datos que viaja entre el frontend y el backend. Son como "contratos" que definen qué información se envía y se recibe.

### Paso 1.2: Crear el archivo de DTOs

**Archivo:** `DTOs/CategoriaDto.cs`

```csharp
namespace RecetArreWeb.DTOs
{
    // DTO para LEER datos (GET)
    public class CategoriaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = default!;
        public string? Descripcion { get; set; }
        public DateTime CreadoUtc { get; set; }
    }

    // DTO para CREAR (POST)
    public class CategoriaCreacionDto
    {   
        public string Nombre { get; set; } = default!;
        public string? Descripcion { get; set; }
        // Nota: No enviamos el Id porque lo genera la base de datos
    }

    // DTO para ACTUALIZAR (PUT)
    public class CategoriaModificacionDto
    {
        public string Nombre { get; set; } = default!;
        public string? Descripcion { get; set; }
        // Nota: El Id se envía en la URL, no en el body
    }
}
```

**Explicación:**
- `CategoriaDto`: Contiene **todos** los campos que devuelve el API (incluye Id y fecha)
- `CategoriaCreacionDto`: Solo los campos que el usuario puede **crear**
- `CategoriaModificacionDto`: Solo los campos que el usuario puede **modificar**

**¿Por qué 3 DTOs diferentes?**
- Seguridad: El usuario no puede cambiar el Id o la fecha de creación
- Claridad: Es obvio qué campos se usan en cada operación
- Validación: Podemos tener reglas diferentes para crear vs actualizar

---

## 🔧 Parte 2: Crear el Servicio

### Paso 2.1: Crear la interfaz del servicio

**Archivo:** `Services/CategoriaService.cs`

```csharp
public interface ICategoriaService
{
    Task<List<CategoriaDto>> ObtenerTodas();
    Task<CategoriaDto?> ObtenerPorId(int id);
    Task<CategoriaDto?> Crear(CategoriaCreacionDto categoriaDto);
    Task<bool> Actualizar(int id, CategoriaModificacionDto categoriaDto);
    Task<bool> Eliminar(int id);
}
```

**Explicación:**
- `Task<>`: Todas las llamadas a API son **asíncronas** (no bloquean la UI)
- `?`: Puede devolver `null` si falla (ej: categoría no encontrada)
- `List<CategoriaDto>`: Devuelve una lista de categorías
- `bool`: Devuelve `true` si tuvo éxito, `false` si falló

### Paso 2.2: Implementar el servicio

```csharp
public class CategoriaService : ICategoriaService
{
    private readonly HttpClient httpClient;
    private const string endpoint = "api/Categorias";

    public CategoriaService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<List<CategoriaDto>> ObtenerTodas()
    {
        try
        {
            var categorias = await httpClient.GetFromJsonAsync<List<CategoriaDto>>(endpoint);
            return categorias ?? new List<CategoriaDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return new List<CategoriaDto>();
        }
    }

    // ...otros métodos
}
```

**Desglose línea por línea:**

1. **`private readonly HttpClient httpClient;`**
   - Variable para hacer peticiones HTTP
   - `readonly`: Solo se asigna en el constructor

2. **`private const string endpoint = "api/Categorias";`**
   - La ruta del API (se concatena con la URL base)
   - URL completa: `https://localhost:7019/api/Categorias`

3. **`public CategoriaService(HttpClient httpClient)`**
   - Constructor que recibe HttpClient por inyección de dependencias

4. **`await httpClient.GetFromJsonAsync<List<CategoriaDto>>(endpoint)`**
   - `GetFromJsonAsync`: Hace un GET y deserializa la respuesta JSON automáticamente
   - `<List<CategoriaDto>>`: Especifica el tipo de dato esperado
   - `await`: Espera la respuesta sin bloquear la UI

5. **`return categorias ?? new List<CategoriaDto>();`**
   - Si `categorias` es null, devuelve una lista vacía

6. **`try-catch`**
   - Captura errores (ej: API no disponible, timeout)
   - Imprime el error en la consola del navegador (F12)

### Paso 2.3: Implementar CREAR

```csharp
public async Task<CategoriaDto?> Crear(CategoriaCreacionDto categoriaDto)
{
    try
    {
        var response = await httpClient.PostAsJsonAsync(endpoint, categoriaDto);
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<CategoriaDto>();
        }
        
        return null;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        return null;
    }
}
```

**Explicación:**
- `PostAsJsonAsync`: Hace un POST y serializa el objeto a JSON automáticamente
- `response.IsSuccessStatusCode`: Verifica si el código HTTP es 200-299 (éxito)
- `ReadFromJsonAsync<CategoriaDto>()`: Lee la respuesta y la convierte a objeto

### Paso 2.4: Implementar ACTUALIZAR

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
        Console.WriteLine($"Error: {ex.Message}");
        return false;
    }
}
```

**Explicación:**
- `PutAsJsonAsync`: Hace un PUT (actualizar)
- `$"{endpoint}/{id}"`: Concatena el Id en la URL → `api/Categorias/5`
- Devuelve `true` si tuvo éxito, `false` si falló

### Paso 2.5: Implementar ELIMINAR

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
        Console.WriteLine($"Error: {ex.Message}");
        return false;
    }
}
```

**Explicación:**
- `DeleteAsync`: Hace un DELETE
- Similar al PUT, pero no enviamos body

---

## 🔧 Parte 3: Registrar el Servicio

### Paso 3.1: Configurar HttpClient y servicios

**Archivo:** `Program.cs`

```csharp
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RecetArreWeb;
using RecetArreWeb.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configurar HttpClient con la URL del backend
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri("https://localhost:7019/") 
});

// Registrar servicios
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IIngredienteService, IngredienteService>();

await builder.Build().RunAsync();
```

**Explicación:**

1. **`builder.Services.AddScoped<ICategoriaService, CategoriaService>();`**
   - Registra el servicio en el contenedor de inyección de dependencias
   - `AddScoped`: Una instancia por usuario (por sesión del navegador)
   - `<Interfaz, Implementación>`: Relaciona la interfaz con la clase concreta

2. **`BaseAddress = new Uri("https://localhost:7019/")`**
   - URL base del backend
   - Todas las peticiones se hacen a esta dirección
   - **IMPORTANTE:** Cambiar esta URL según tu backend

**Ciclos de vida de servicios:**
- `AddScoped`: Una instancia por usuario/sesión (recomendado para Blazor WASM)
- `AddSingleton`: Una única instancia para toda la app (compartida)
- `AddTransient`: Una nueva instancia cada vez que se solicita

---

## 🔧 Parte 4: Consumir el Servicio en el Componente

### Paso 4.1: Inyectar el servicio

**Archivo:** `Pages/Categorias.razor`

```razor
@page "/categorias"
@inject ICategoriaService categoriaService
@inject IJSRuntime JSRuntime

<PageTitle>Gestión de Categorías</PageTitle>
```

**Explicación:**
- `@inject`: Inyecta el servicio en el componente
- `categoriaService`: Nombre de la variable (puedes usar cualquier nombre)
- `IJSRuntime`: Para llamar funciones JavaScript (como `confirm`)

### Paso 4.2: Variables de estado

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
}
```

**Explicación:**
- `categorias`: Lista de categorías (null mientras carga)
- `guardando`: Para deshabilitar el botón mientras guarda
- `mensajeError/mensajeExito`: Para mostrar alertas al usuario

### Paso 4.3: Cargar datos al iniciar

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

**Explicación:**
- `OnInitializedAsync`: Método del ciclo de vida, se ejecuta al cargar el componente
- `await categoriaService.ObtenerTodas()`: Llama al servicio
- Si hay error, muestra mensaje y crea lista vacía (evita crashes)

### Paso 4.4: Crear categoría

```csharp
private async Task GuardarCategoria()
{
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
            // CREAR
            var nuevaCategoria = new CategoriaCreacionDto
            {
                Nombre = nombreCategoria,
                Descripcion = string.IsNullOrWhiteSpace(descripcionCategoria) ? null : descripcionCategoria
            };

            var resultado = await categoriaService.Crear(nuevaCategoria);
            
            if (resultado != null)
            {
                mensajeExito = "Categoría creada exitosamente";
                await CargarCategorias(); // Recargar la lista
                CerrarModal();
            }
            else
            {
                mensajeError = "No se pudo crear la categoría";
            }
        }
        else
        {
            // ACTUALIZAR (ver siguiente sección)
        }
    }
    catch (Exception ex)
    {
        mensajeError = $"Error: {ex.Message}";
    }
    finally
    {
        guardando = false; // Siempre se ejecuta (éxito o error)
    }
}
```

**Flujo:**
1. Validar que el nombre no esté vacío
2. Mostrar indicador de carga (`guardando = true`)
3. Crear el DTO con los datos del formulario
4. Llamar al servicio
5. Si tuvo éxito, recargar la lista y cerrar el modal
6. Si falló, mostrar mensaje de error
7. Quitar indicador de carga

### Paso 4.5: Eliminar categoría

```csharp
private async Task EliminarCategoria(int id)
{
    // Confirmar con el usuario
    if (!await JSRuntime.InvokeAsync<bool>("confirm", "¿Estás seguro?"))
        return;

    try
    {
        var resultado = await categoriaService.Eliminar(id);
        
        if (resultado)
        {
            mensajeExito = "Categoría eliminada";
            await CargarCategorias();
        }
        else
        {
            mensajeError = "No se pudo eliminar";
        }
    }
    catch (Exception ex)
    {
        mensajeError = $"Error: {ex.Message}";
    }
}
```

**Explicación:**
- `JSRuntime.InvokeAsync<bool>("confirm", ...)`: Llama a la función JavaScript `confirm()`
- Si el usuario cancela, sale del método
- Si elimina exitosamente, recarga la lista

---

## 🎨 Parte 5: Mostrar Estados en la UI

### Paso 5.1: Estado de carga

```razor
@if (categorias == null)
{
    <div class="text-center py-5">
        <div class="spinner-border text-brand"></div>
        <p class="text-muted mt-3">Cargando...</p>
    </div>
}
```

**Explicación:**
- Mientras `categorias` es `null`, muestra spinner
- Una vez que carga, `categorias` será una lista (vacía o con datos)

### Paso 5.2: Lista vacía

```razor
else if (!categorias.Any())
{
    <div class="alert alert-info">
        <h5>No hay categorías</h5>
        <p>Comienza creando tu primera categoría.</p>
    </div>
}
```

### Paso 5.3: Mostrar alertas

```razor
@if (!string.IsNullOrEmpty(mensajeError))
{
    <div class="alert alert-danger alert-dismissible fade show">
        <strong>Error:</strong> @mensajeError
        <button type="button" class="btn-close" 
                @onclick="() => mensajeError = string.Empty"></button>
    </div>
}
```

**Explicación:**
- `alert-dismissible`: Permite cerrar la alerta
- `@onclick="() => mensajeError = string.Empty"`: Limpia el mensaje al cerrar

---

## 🔍 Debugging: Cómo ver qué está pasando

### En el navegador (F12)

1. **Consola:** Ver mensajes de `Console.WriteLine()`
2. **Red (Network):** Ver todas las peticiones HTTP
   - Filtrar por `XHR` para ver solo llamadas a API
   - Ver request/response completos
   - Ver códigos de estado (200, 404, 500, etc.)

### Errores comunes

| Error | Causa | Solución |
|-------|-------|----------|
| CORS | Backend no permite peticiones desde el frontend | Configurar CORS en el backend |
| 404 Not Found | URL incorrecta | Verificar endpoint y BaseAddress |
| 401 Unauthorized | Falta token de autenticación | Implementar autenticación JWT |
| 500 Internal Server Error | Error en el backend | Ver logs del backend |
| Timeout | Backend no responde | Verificar que el backend esté corriendo |

---

## 📝 Ejercicios para Alumnos

### Ejercicio 1: Crear servicio de Ingredientes
Siguiendo el mismo patrón de `CategoriaService`, crea `IngredienteService` con los mismos métodos CRUD.

### Ejercicio 2: Agregar paginación
Modifica el servicio para aceptar parámetros de paginación:
```csharp
Task<List<CategoriaDto>> ObtenerTodas(int pagina = 1, int tamañoPagina = 10);
```

### Ejercicio 3: Búsqueda en el backend
En lugar de filtrar en el frontend, envía la búsqueda al backend:
```csharp
Task<List<CategoriaDto>> Buscar(string termino);
```

### Ejercicio 4: Manejo de errores mejorado
Crea un servicio de notificaciones para centralizar los mensajes de error/éxito.

### Ejercicio 5: Caché local
Guarda las categorías en `localStorage` para que no se tengan que recargar cada vez.

---

## ✅ Checklist de Comprensión

Después de esta guía, los alumnos deberían poder:
- [ ] Explicar qué es un servicio y por qué se usa
- [ ] Crear DTOs apropiados para cada operación
- [ ] Implementar los 5 métodos CRUD en un servicio
- [ ] Registrar servicios en `Program.cs`
- [ ] Inyectar servicios en componentes
- [ ] Manejar estados de carga y errores
- [ ] Usar el navegador para debuggear peticiones HTTP
- [ ] Entender la diferencia entre `AddScoped`, `AddSingleton` y `AddTransient`

---

## 🔗 Recursos Adicionales

- **HttpClient en Blazor:** https://learn.microsoft.com/es-es/aspnet/core/blazor/call-web-api
- **Dependency Injection:** https://learn.microsoft.com/es-es/aspnet/core/blazor/fundamentals/dependency-injection
- **Manejo de errores:** https://learn.microsoft.com/es-es/aspnet/core/blazor/fundamentals/handle-errors

---

## 🎓 Conceptos Clave

1. **Separación de responsabilidades:** UI (componente) vs lógica de datos (servicio)
2. **Async/Await:** No bloquear la UI mientras se esperan respuestas
3. **Try-Catch:** Siempre manejar errores de red
4. **Inyección de dependencias:** El contenedor provee las instancias
5. **DTOs:** Contratos claros entre frontend y backend
6. **Estados de UI:** Carga, éxito, error, vacío
