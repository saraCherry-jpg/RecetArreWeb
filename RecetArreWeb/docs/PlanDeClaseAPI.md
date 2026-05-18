# Plan de Clase: Consumo de API desde Blazor WebAssembly
## Conectar Frontend con Backend

---

## 🎯 Objetivos de Aprendizaje

Al finalizar esta clase, los alumnos serán capaces de:
1. Entender el patrón de servicios y la inyección de dependencias
2. Crear servicios para consumir APIs REST
3. Implementar operaciones CRUD completas desde el frontend
4. Manejar estados de carga y errores
5. Debuggear peticiones HTTP con DevTools

---

## 📚 Duración Estimada
**2.5-3 horas** (puede dividirse en 2 sesiones)

---

## 🛠️ Requisitos Previos

- Backend corriendo en `https://localhost:7019`
- Página de Categorías con diseño Bootstrap (clase anterior)
- Entender conceptos básicos de HTTP (GET, POST, PUT, DELETE)
- Conocer async/await en C#

---

## 📖 Contenido de la Clase

### **Parte 1: Teoría de Arquitectura (30 min)**

#### Actividad 1.1: Explicar la arquitectura Frontend-Backend
**Tiempo:** 10 min

**Dibujar en pizarra:**
```
┌──────────────┐    HTTP    ┌──────────────┐    ┌──────────────┐
│   Blazor     │ ────────>  │   API REST   │───>│  Base de     │
│  (Frontend)  │ <────────  │  (Backend)   │<───│  Datos       │
└──────────────┘   JSON     └──────────────┘    └──────────────┘
```

**Explicar:**
- Frontend: Lo que ve el usuario (UI)
- Backend: Lógica de negocio y acceso a datos
- HTTP: Protocolo de comunicación
- JSON: Formato de datos

**Métodos HTTP:**
| Método | Operación | Ejemplo |
|--------|-----------|---------|
| GET | Leer | Obtener todas las categorías |
| POST | Crear | Crear nueva categoría |
| PUT | Actualizar | Modificar categoría existente |
| DELETE | Eliminar | Borrar categoría |

#### Actividad 1.2: Patrón de Servicios
**Tiempo:** 10 min

**Código sin servicio (❌ MAL):**
```csharp
// Directamente en el componente
var response = await httpClient.GetAsync("https://localhost:7019/api/Categorias");
var categorias = await response.Content.ReadFromJsonAsync<List<CategoriaDto>>();
```

**Problema:** Si cambia la URL, hay que modificar cada componente.

**Código con servicio (✅ BIEN):**
```csharp
// En el componente
var categorias = await categoriaService.ObtenerTodas();
```

**Ventajas:**
- Un solo lugar para cambiar URLs
- Reutilizable en múltiples componentes
- Más fácil de testear
- Código más limpio

#### Actividad 1.3: Inyección de Dependencias
**Tiempo:** 10 min

**Explicar el concepto:**
```
1. Registro (Program.cs)
   builder.Services.AddScoped<ICategoriaService, CategoriaService>();

2. Inyección (Componente)
   @inject ICategoriaService categoriaService

3. Uso
   await categoriaService.ObtenerTodas();
```

**Analogía:** Es como un "mayordomo" que te trae lo que necesitas sin que tengas que ir a buscarlo.

**Preguntas a los alumnos:**
- ¿Por qué usar una interfaz? (Para poder cambiar la implementación sin cambiar el componente)
- ¿Qué pasa si no registramos el servicio? (Error en tiempo de ejecución)

---

### **Parte 2: Crear DTOs (20 min)**

#### Actividad 2.1: Explicar qué son los DTOs
**Tiempo:** 5 min

**DTO = Data Transfer Object**

Es un objeto que define la estructura de datos que viaja entre frontend y backend.

**Analogía:** Es como un formulario de envío. Tiene campos específicos que debes llenar.

#### Actividad 2.2: Crear los DTOs
**Tiempo:** 15 min

**Guiar a los alumnos paso a paso:**

1. Crear carpeta `DTOs`
2. Crear archivo `CategoriaDto.cs`
3. Agregar las 3 clases:

```csharp
// Para LEER (incluye todo)
public class CategoriaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = default!;
    public string? Descripcion { get; set; }
    public DateTime CreadoUtc { get; set; }
}

// Para CREAR (sin Id ni fecha)
public class CategoriaCreacionDto
{   
    public string Nombre { get; set; } = default!;
    public string? Descripcion { get; set; }
}

// Para ACTUALIZAR (sin Id ni fecha)
public class CategoriaModificacionDto
{
    public string Nombre { get; set; } = default!;
    public string? Descripcion { get; set; }
}
```

**Preguntas:**
- ¿Por qué `CategoriaCreacionDto` no tiene `Id`? (Lo genera la BD)
- ¿Qué significa `?` en `string?`? (Puede ser null)
- ¿Qué significa `= default!`? (Inicialización por defecto, no null)

---

### **Parte 3: Crear el Servicio (50 min)**

#### Actividad 3.1: Estructura básica
**Tiempo:** 10 min

1. Crear carpeta `Services`
2. Crear archivo `CategoriaService.cs`
3. Definir la interfaz:

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

**Explicar:**
- `Task<>`: Operación asíncrona
- `?`: Puede devolver null
- `List<CategoriaDto>`: Lista de categorías
- `bool`: True si tuvo éxito

#### Actividad 3.2: Implementar GET (Leer)
**Tiempo:** 10 min

**Construir juntos:**
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
            var categorias = await httpClient
                .GetFromJsonAsync<List<CategoriaDto>>(endpoint);
            return categorias ?? new List<CategoriaDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return new List<CategoriaDto>();
        }
    }
}
```

**Explicar línea por línea:**
- `readonly HttpClient`: Para hacer peticiones
- `const string endpoint`: Ruta del API
- `GetFromJsonAsync`: GET + deserialización automática
- `?? new List`: Si es null, devuelve lista vacía
- `try-catch`: Capturar errores

**Ejercicio:** Los alumnos escriben este método en su proyecto

#### Actividad 3.3: Implementar POST (Crear)
**Tiempo:** 10 min

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

**Explicar:**
- `PostAsJsonAsync`: POST + serialización automática
- `IsSuccessStatusCode`: Verifica código 200-299
- `ReadFromJsonAsync`: Lee la respuesta

#### Actividad 3.4: Implementar PUT y DELETE
**Tiempo:** 10 min

**Los alumnos implementan estos métodos siguiendo el patrón:**

```csharp
// PUT
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

// DELETE
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

#### Actividad 3.5: Verificar el servicio completo
**Tiempo:** 10 min

**Checklist:**
- [ ] Interfaz con 5 métodos
- [ ] Implementación de todos los métodos
- [ ] Try-catch en todos
- [ ] Constructor recibiendo HttpClient

---

### **Parte 4: Registrar el Servicio (15 min)**

#### Actividad 4.1: Configurar Program.cs
**Tiempo:** 15 min

**Modificar `Program.cs`:**

```csharp
using RecetArreWeb.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
// ...existing code...

// 1. Configurar HttpClient con URL del backend
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri("https://localhost:7019/") 
});

// 2. Registrar el servicio
builder.Services.AddScoped<ICategoriaService, CategoriaService>();

await builder.Build().RunAsync();
```

**Explicar:**
1. **BaseAddress**: URL base del backend
   - Todas las peticiones se harán a esta dirección
   - El endpoint se concatena: `https://localhost:7019/` + `api/Categorias`

2. **AddScoped**: Ciclo de vida
   - Una instancia por usuario/sesión
   - Se crea al solicitarla, se destruye al cerrar el navegador

**Ciclos de vida:**
| Tipo | Duración | Uso |
|------|----------|-----|
| Scoped | Por sesión | Servicios de datos (recomendado) |
| Singleton | Toda la app | Configuración, caché |
| Transient | Por solicitud | Servicios ligeros, stateless |

**Probar:** Ejecutar la app y verificar que no hay errores

---

### **Parte 5: Consumir el Servicio (40 min)**

#### Actividad 5.1: Inyectar el servicio
**Tiempo:** 5 min

**En `Pages/Categorias.razor`:**

```razor
@page "/categorias"
@inject ICategoriaService categoriaService
@inject IJSRuntime JSRuntime
```

**Explicar:**
- `@inject`: Inyecta el servicio
- `categoriaService`: Nombre de la variable
- `IJSRuntime`: Para llamar funciones JavaScript

#### Actividad 5.2: Modificar variables
**Tiempo:** 5 min

```csharp
@code {
    private List<CategoriaDto>? categorias;  // null = cargando
    private string mensajeError = string.Empty;
    private string mensajeExito = string.Empty;
    private bool guardando = false;  // Para deshabilitar botón
    // ...existing variables...
}
```

#### Actividad 5.3: Cargar datos
**Tiempo:** 10 min

**Reemplazar el método `CargarCategorias`:**

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

**Probar:**
1. Ejecutar la app
2. Navegar a `/categorias`
3. Abrir F12 → Network → Ver petición a `api/Categorias`
4. Verificar que se muestran los datos del backend

#### Actividad 5.4: Implementar Crear
**Tiempo:** 10 min

**Modificar `GuardarCategoria`:**

```csharp
private async Task GuardarCategoria()
{
    if (string.IsNullOrWhiteSpace(nombreCategoria))
    {
        mensajeError = "El nombre es requerido";
        return;
    }

    guardando = true;

    try
    {
        var nuevaCategoria = new CategoriaCreacionDto
        {
            Nombre = nombreCategoria,
            Descripcion = string.IsNullOrWhiteSpace(descripcionCategoria) 
                ? null 
                : descripcionCategoria
        };

        var resultado = await categoriaService.Crear(nuevaCategoria);
        
        if (resultado != null)
        {
            mensajeExito = "Categoría creada exitosamente";
            await CargarCategorias();
            CerrarModal();
        }
        else
        {
            mensajeError = "No se pudo crear la categoría";
        }
    }
    catch (Exception ex)
    {
        mensajeError = $"Error: {ex.Message}";
    }
    finally
    {
        guardando = false;
    }
}
```

**Probar:**
1. Crear una nueva categoría
2. Ver en Network la petición POST
3. Ver la respuesta del servidor
4. Verificar que se agregó a la lista

#### Actividad 5.5: Implementar Eliminar
**Tiempo:** 10 min

**Los alumnos implementan:**

```csharp
private async Task EliminarCategoria(int id)
{
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

---

### **Parte 6: Manejo de Estados (20 min)**

#### Actividad 6.1: Agregar alertas
**Tiempo:** 10 min

**Agregar después del encabezado:**

```razor
@if (!string.IsNullOrEmpty(mensajeError))
{
    <div class="alert alert-danger alert-dismissible fade show">
        <strong>Error:</strong> @mensajeError
        <button type="button" class="btn-close" 
                @onclick="() => mensajeError = string.Empty"></button>
    </div>
}

@if (!string.IsNullOrEmpty(mensajeExito))
{
    <div class="alert alert-success alert-dismissible fade show">
        <strong>Éxito:</strong> @mensajeExito
        <button type="button" class="btn-close" 
                @onclick="() => mensajeExito = string.Empty"></button>
    </div>
}
```

#### Actividad 6.2: Deshabilitar botón mientras guarda
**Tiempo:** 10 min

**Modificar el botón del modal:**

```razor
<button type="button" class="btn btn-brand" 
        @onclick="GuardarCategoria"
        disabled="@guardando">
    @if (guardando)
    {
        <span class="spinner-border spinner-border-sm me-2"></span>
    }
    @(categoriaEditando == null ? "Crear" : "Guardar cambios")
</button>
```

**Explicar:**
- `disabled="@guardando"`: Deshabilitado mientras guarda
- `spinner-border-sm`: Spinner pequeño
- Mejora la UX: Usuario sabe que se está procesando

---

### **Parte 7: Debugging (20 min)**

#### Actividad 7.1: DevTools - Network
**Tiempo:** 10 min

**Mostrar en vivo:**

1. Abrir F12 → Network
2. Crear una categoría
3. Ver la petición POST
   - URL completa
   - Request Headers
   - Request Payload (JSON enviado)
   - Response (JSON recibido)
   - Status Code (200, 400, 500, etc.)

**Errores comunes a mostrar:**
- 404: URL incorrecta
- 500: Error en el backend
- CORS: Backend no permite peticiones

#### Actividad 7.2: Consola
**Tiempo:** 10 min

**Mostrar:**
1. Los mensajes de `Console.WriteLine()` aparecen aquí
2. Ver errores de JavaScript
3. Usar `console.log()` para debuggear

**Agregar logs al servicio:**
```csharp
Console.WriteLine($"Enviando petición a: {endpoint}");
Console.WriteLine($"Respuesta: {response.StatusCode}");
```

---

## 🎨 Ejercicios Prácticos

### Nivel Básico
1. Implementar el método `ObtenerPorId`
2. Agregar actualización de categoría (PUT)
3. Mostrar el tiempo de respuesta del API

### Nivel Intermedio
4. Crear el servicio de Ingredientes completo
5. Agregar un contador de categorías activas
6. Implementar caché local con `localStorage`

### Nivel Avanzado
7. Agregar reintentos automáticos en caso de error
8. Implementar paginación en el backend y frontend
9. Crear un servicio genérico reutilizable para todos los endpoints

---

## ✅ Evaluación

### Preguntas Teóricas
1. ¿Qué es un servicio y para qué sirve?
2. ¿Cuál es la diferencia entre `AddScoped` y `AddSingleton`?
3. ¿Qué métodos HTTP se usan para cada operación CRUD?
4. ¿Por qué usamos DTOs diferentes para crear y actualizar?
5. ¿Qué hace `await` en una llamada async?

### Ejercicio Práctico
Crear el CRUD completo de Ingredientes:
1. DTOs (crear, modificar, leer)
2. Servicio con interfaz
3. Registrar en Program.cs
4. Crear página similar a Categorías
5. Probar todas las operaciones

---

## 📌 Notas para el Profesor

- **CORS:** Si hay problemas, configurar en el backend:
  ```csharp
  builder.Services.AddCors(options => {
      options.AddPolicy("AllowAll", builder => {
          builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
      });
  });
  app.UseCors("AllowAll");
  ```

- **URL del backend:** Asegurar que todos usen la misma URL

- **Errores comunes:**
  - Olvidar registrar el servicio → NullReferenceException
  - URL incorrecta → 404
  - Backend no corriendo → Timeout

- **Timing flexible:** Esta clase tiene mucho contenido, considerar dividirla en 2 sesiones

---

## 🏆 Objetivos Alcanzados

Al finalizar, los alumnos habrán:
- ✅ Creado un servicio completo con inyección de dependencias
- ✅ Implementado las 5 operaciones CRUD
- ✅ Conectado el frontend con el backend real
- ✅ Manejado estados de carga y errores
- ✅ Usado DevTools para debuggear HTTP
- ✅ Aplicado buenas prácticas de arquitectura
