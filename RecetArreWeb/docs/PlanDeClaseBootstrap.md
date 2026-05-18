# Plan de Clase: Bootstrap en Blazor WebAssembly
## Gestión de Categorías

---

## 🎯 Objetivos de Aprendizaje

Al finalizar esta clase, los alumnos serán capaces de:
1. Comprender el sistema de grillas de Bootstrap (12 columnas)
2. Crear layouts responsivos usando breakpoints
3. Implementar componentes de Bootstrap (cards, tablas, modales, formularios)
4. Aplicar clases de utilidad para espaciado, alineación y estilo
5. Integrar una paleta de colores personalizada con Bootstrap

---

## 📚 Duración Estimada
**2-3 horas** (puede dividirse en 2 sesiones)

---

## 🛠️ Requisitos Previos

- Proyecto Blazor WebAssembly configurado con Bootstrap
- Archivo `palette.css` creado (ya visto en clase anterior)
- Conocimientos básicos de HTML y CSS
- DTOs de Categoría creados

---

## 📖 Contenido de la Clase

### **Parte 1: Teoría del Grid System (30 min)**

#### Actividad 1.1: Explicar el concepto de Grid
**Tiempo:** 10 min

**Explicación en pizarra/pantalla:**
```
Pantalla dividida en 12 columnas

[1][2][3][4][5][6][7][8][9][10][11][12]

Ejemplos:
- 1 elemento de 12 columnas = 100% del ancho
- 2 elementos de 6 columnas = 50% cada uno
- 3 elementos de 4 columnas = 33.3% cada uno
- 4 elementos de 3 columnas = 25% cada uno
```

**Preguntas a los alumnos:**
- ¿Cómo dividirías la pantalla en 5 elementos iguales? (No se puede exactamente, pero puedes usar combinaciones)
- ¿Qué pasa si sumas más de 12 columnas? (Se pasa a la siguiente línea)

#### Actividad 1.2: Breakpoints
**Tiempo:** 10 min

**Mostrar en tabla:**
| Clase | Pantalla | Dispositivo | Ejemplo |
|-------|----------|-------------|---------|
| `col-` | < 576px | Móvil pequeño | iPhone SE |
| `col-sm-` | ≥ 576px | Móvil grande | iPhone 12 |
| `col-md-` | ≥ 768px | Tablet | iPad |
| `col-lg-` | ≥ 992px | Desktop | Laptop |
| `col-xl-` | ≥ 1200px | Desktop grande | Monitor |

**Ejercicio práctico:**
Abrir DevTools → Modo responsive → Cambiar tamaños y ver cómo se adapta el layout

#### Actividad 1.3: Código de ejemplo básico
**Tiempo:** 10 min

Crear un archivo de prueba `GridDemo.razor`:
```razor
@page "/grid-demo"

<div class="container">
    <div class="row mb-3">
        <div class="col-12 bg-primary text-white p-3">12 columnas</div>
    </div>
    
    <div class="row mb-3">
        <div class="col-6 bg-success text-white p-3">6 columnas</div>
        <div class="col-6 bg-info text-white p-3">6 columnas</div>
    </div>
    
    <div class="row mb-3">
        <div class="col-4 bg-warning text-white p-3">4</div>
        <div class="col-4 bg-danger text-white p-3">4</div>
        <div class="col-4 bg-secondary text-white p-3">4</div>
    </div>
</div>
```

**Actividad:** Los alumnos recrean esto en su proyecto

---

### **Parte 2: Estructura de la Página de Categorías (40 min)**

#### Actividad 2.1: Crear el archivo y estructura base
**Tiempo:** 10 min

**Paso a paso con los alumnos:**
1. Crear `Pages/Categorias.razor`
2. Agregar `@page "/categorias"`
3. Crear estructura básica:
```razor
<PageTitle>Gestión de Categorías</PageTitle>

<div class="container-fluid py-4">
    <h1>Categorías</h1>
</div>
```

**Explicar:**
- `container-fluid` vs `container`
- Clases de espaciado: `py-4` = padding vertical de tamaño 4
- Agregar link en `NavMenu.razor`

#### Actividad 2.2: Encabezado con Flexbox
**Tiempo:** 15 min

**Construir juntos:**
```razor
<div class="row mb-4">
    <div class="col-12">
        <div class="d-flex justify-content-between align-items-center">
            <div>
                <h1 class="h3 text-brand mb-1">Gestión de Categorías</h1>
                <p class="text-muted mb-0">Administra las categorías de recetas</p>
            </div>
            <button class="btn btn-brand">Nueva Categoría</button>
        </div>
    </div>
</div>
```

**Explicar cada clase:**
- `row` y `col-12`: Sistema de grillas
- `mb-4`: Margin bottom
- `d-flex`: Activa flexbox
- `justify-content-between`: Espacio entre elementos
- `align-items-center`: Centrado vertical
- `text-brand`: De nuestra paleta personalizada
- `btn btn-brand`: Botón con estilo de la paleta

**Ejercicio:** Los alumnos cambian el color primario en `palette.css` y ven el cambio en el botón

#### Actividad 2.3: Card de Filtros
**Tiempo:** 15 min

**Construir paso a paso:**
```razor
<div class="row mb-3">
    <div class="col-12">
        <div class="card border-0 shadow-sm">
            <div class="card-body">
                <div class="row g-3">
                    <div class="col-12 col-md-6 col-lg-4">
                        <input type="text" class="form-control" 
                               placeholder="Buscar categoría..." 
                               @bind="busqueda" />
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
```

**Explicar:**
- Anatomía de un card: `card` → `card-body`
- `border-0`: Sin borde
- `shadow-sm`: Sombra suave
- `g-3`: Gap (espacio) entre columnas
- `form-control`: Estilo de input
- `@bind`: Data binding de Blazor

**Agregar en @code:**
```csharp
@code {
    private string busqueda = "";
}
```

---

### **Parte 3: Grid de Cards (45 min)**

#### Actividad 3.1: Crear una Card
**Tiempo:** 15 min

**Explicar la estructura:**
```razor
<div class="col-12 col-sm-6 col-md-4 col-lg-3 mb-3">
    <div class="card h-100 border-0 shadow-sm">
        <div class="card-body d-flex flex-column">
            <h5 class="card-title text-brand">Postres</h5>
            <p class="card-text text-muted small flex-grow-1">
                Recetas dulces y deliciosas
            </p>
            <div class="border-top pt-2 mt-auto">
                <small class="text-muted">10/12/2024</small>
            </div>
        </div>
    </div>
</div>
```

**Dibujar en pizarra:**
```
┌─────────────────────┐
│ Postres  (título)   │
│                     │
│ Recetas dulces...   │ ← flex-grow-1 (crece)
│                     │
├─────────────────────┤
│ 10/12/2024          │ ← mt-auto (al final)
└─────────────────────┘
```

**Explicar:**
- `col-12 col-sm-6 col-md-4 col-lg-3`: Responsividad
  - Móvil: 1 por fila
  - Tablet: 2 por fila
  - Desktop: 3-4 por fila
- `h-100`: Todas las cards con la misma altura
- `d-flex flex-column`: Flexbox vertical
- `flex-grow-1`: El texto crece
- `mt-auto`: Empuja al final

#### Actividad 3.2: Crear lista de categorías
**Tiempo:** 15 min

**Agregar datos de ejemplo:**
```csharp
@code {
    private List<CategoriaDto> categorias = new()
    {
        new() { Id = 1, Nombre = "Postres", Descripcion = "Recetas dulces", CreadoUtc = DateTime.UtcNow },
        new() { Id = 2, Nombre = "Ensaladas", Descripcion = "Platos frescos", CreadoUtc = DateTime.UtcNow }
    };
}
```

**Crear el foreach:**
```razor
<div class="row">
    @foreach (var categoria in categorias)
    {
        <div class="col-12 col-sm-6 col-md-4 col-lg-3 mb-3">
            <!-- Card aquí -->
        </div>
    }
</div>
```

#### Actividad 3.3: Estados de carga y vacío
**Tiempo:** 15 min

**Explicar los 3 estados:**
1. Cargando (spinner)
2. Sin datos (alert)
3. Con datos (grid)

**Implementar:**
```razor
@if (categorias == null)
{
    <div class="text-center py-5">
        <div class="spinner-border text-brand"></div>
        <p class="text-muted mt-3">Cargando...</p>
    </div>
}
else if (!categorias.Any())
{
    <div class="alert alert-info">
        No hay categorías
    </div>
}
else
{
    <!-- Grid de cards -->
}
```

---

### **Parte 4: Tabla de Datos (30 min)**

#### Actividad 4.1: Crear tabla básica
**Tiempo:** 20 min

**Construir juntos:**
```razor
<div class="table-responsive">
    <table class="table table-hover mb-0">
        <thead class="table-light">
            <tr>
                <th scope="col">ID</th>
                <th scope="col">Nombre</th>
                <th scope="col">Descripción</th>
                <th scope="col">Fecha</th>
                <th scope="col" class="text-end">Acciones</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var categoria in categorias)
            {
                <tr>
                    <td>@categoria.Id</td>
                    <td class="fw-semibold">@categoria.Nombre</td>
                    <td class="text-muted">@categoria.Descripcion</td>
                    <td>@categoria.CreadoUtc.ToString("dd/MM/yyyy")</td>
                    <td class="text-end">
                        <button class="btn btn-sm btn-outline-primary">
                            Editar
                        </button>
                    </td>
                </tr>
            }
        </tbody>
    </table>
</div>
```

**Explicar:**
- `table-responsive`: Scroll horizontal en móvil
- `table-hover`: Efecto al pasar el mouse
- `table-light`: Fondo claro en thead
- `fw-semibold`: Fuente semi-negrita
- `text-end`: Alineación derecha
- `btn-sm`: Botón pequeño

#### Actividad 4.2: Comparar grid vs tabla
**Tiempo:** 10 min

**Discusión con alumnos:**
- ¿Cuándo usar cards? → Contenido visual, pocas columnas
- ¿Cuándo usar tablas? → Muchos datos, comparación de columnas

---

### **Parte 5: Modal (30 min)**

#### Actividad 5.1: Crear el modal
**Tiempo:** 20 min

**Implementar:**
```razor
@if (mostrarModal)
{
    <div class="modal fade show d-block" tabindex="-1" 
         style="background-color: rgba(0,0,0,0.5);">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Nueva Categoría</h5>
                    <button type="button" class="btn-close" 
                            @onclick="CerrarModal"></button>
                </div>
                <div class="modal-body">
                    <div class="mb-3">
                        <label class="form-label">Nombre</label>
                        <input type="text" class="form-control" 
                               @bind="nombreCategoria" />
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Descripción</label>
                        <textarea class="form-control" rows="3" 
                                  @bind="descripcionCategoria"></textarea>
                    </div>
                </div>
                <div class="modal-footer">
                    <button class="btn btn-secondary" 
                            @onclick="CerrarModal">Cancelar</button>
                    <button class="btn btn-brand" 
                            @onclick="GuardarCategoria">Guardar</button>
                </div>
            </div>
        </div>
    </div>
}
```

**Agregar código:**
```csharp
private bool mostrarModal = false;
private string nombreCategoria = "";
private string descripcionCategoria = "";

private void AbrirModal()
{
    mostrarModal = true;
}

private void CerrarModal()
{
    mostrarModal = false;
    nombreCategoria = "";
    descripcionCategoria = "";
}

private void GuardarCategoria()
{
    // Agregar a la lista
    CerrarModal();
}
```

**Explicar:**
- Anatomía del modal: header, body, footer
- `modal-dialog-centered`: Centrado vertical
- `d-block`: Mostrar el modal
- `btn-close`: Botón X

#### Actividad 5.2: Conectar botón con modal
**Tiempo:** 10 min

**Modificar el botón del encabezado:**
```razor
<button class="btn btn-brand" @onclick="AbrirModal">
    Nueva Categoría
</button>
```

**Probar:** Hacer clic y ver el modal aparecer

---

### **Parte 6: Funcionalidad Completa (30 min)**

#### Actividad 6.1: Implementar crear categoría
**Tiempo:** 10 min

```csharp
private void GuardarCategoria()
{
    if (string.IsNullOrWhiteSpace(nombreCategoria)) return;
    
    var nueva = new CategoriaDto
    {
        Id = (categorias?.Max(c => c.Id) ?? 0) + 1,
        Nombre = nombreCategoria,
        Descripcion = descripcionCategoria,
        CreadoUtc = DateTime.UtcNow
    };
    
    categorias?.Add(nueva);
    CerrarModal();
    StateHasChanged();
}
```

#### Actividad 6.2: Implementar eliminar
**Tiempo:** 10 min

```csharp
private void EliminarCategoria(int id)
{
    var categoria = categorias?.FirstOrDefault(c => c.Id == id);
    if (categoria != null)
    {
        categorias?.Remove(categoria);
        StateHasChanged();
    }
}
```

#### Actividad 6.3: Implementar búsqueda
**Tiempo:** 10 min

```csharp
private IEnumerable<CategoriaDto> CategoriasFiltradas
{
    get
    {
        if (string.IsNullOrWhiteSpace(busqueda))
            return categorias ?? Enumerable.Empty<CategoriaDto>();
        
        return categorias?.Where(c => 
            c.Nombre.Contains(busqueda, StringComparison.OrdinalIgnoreCase)
        ) ?? Enumerable.Empty<CategoriaDto>();
    }
}
```

Cambiar `@foreach (var categoria in categorias)` por `@foreach (var categoria in CategoriasFiltradas)`

---

## 🎨 Ejercicios Prácticos

### Nivel Básico
1. Cambiar los breakpoints del grid de cards
2. Agregar una alerta de éxito al crear una categoría
3. Cambiar los colores de la paleta y observar cambios

### Nivel Intermedio
4. Agregar un dropdown de acciones en cada card
5. Implementar editar categoría
6. Agregar paginación a la tabla

### Nivel Avanzado
7. Crear un filtro por fecha
8. Agregar iconos personalizados a cada categoría
9. Implementar ordenamiento en la tabla (por nombre, fecha, etc.)

---

## ✅ Evaluación

### Preguntas de Comprensión
1. ¿Cuántas columnas tiene el grid de Bootstrap?
2. ¿Qué significa `col-md-6`?
3. ¿Cuál es la diferencia entre `container` y `container-fluid`?
4. ¿Para qué sirve `d-flex`?
5. ¿Qué hace la clase `mb-3`?

### Tarea
Crear una página de "Recetas" siguiendo el mismo patrón de la página de Categorías, pero con los siguientes campos:
- Nombre
- Ingredientes
- Tiempo de preparación
- Dificultad (fácil, media, difícil)

---

## 📌 Notas para el Profesor

- **Timing flexible:** Ajusta según el ritmo de los alumnos
- **Live coding:** Escribe el código en vivo, no copies y pegues
- **Errores intencionales:** Comete errores a propósito para enseñar debugging
- **DevTools:** Usa las herramientas de desarrollador para mostrar cómo funciona el grid
- **Responsive testing:** Cambia el tamaño de la ventana constantemente
- **Preguntas frecuentes:** 
  - "¿Por qué no se ve el cambio?" → Verificar que guardó el archivo
  - "¿Por qué no funciona el botón?" → Revisar el `@onclick`
  - "¿Cómo centro un div?" → `mx-auto`, `text-center`, `d-flex justify-content-center`

---

## 🔗 Recursos Adicionales para Alumnos

- Bootstrap Docs: https://getbootstrap.com
- Bootstrap Cheatsheet: https://bootstrap-cheatsheet.themeselection.com/
- Blazor Docs: https://learn.microsoft.com/es-es/aspnet/core/blazor/
- CSS Flexbox: https://css-tricks.com/snippets/css/a-guide-to-flexbox/

---

## 🏆 Objetivos Alcanzados

Al finalizar, los alumnos habrán:
- ✅ Creado una página completa de gestión (CRUD visual)
- ✅ Implementado diseño responsivo con grillas
- ✅ Usado componentes de Bootstrap (cards, tables, modals, forms)
- ✅ Aplicado una paleta de colores personalizada
- ✅ Conectado eventos de Blazor con la UI
