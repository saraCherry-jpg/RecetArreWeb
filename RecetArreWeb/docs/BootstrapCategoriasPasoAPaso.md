# Guía de Bootstrap para la Gestión de Categorías
## Tutorial paso a paso para alumnos

---

## 📘 Conceptos Básicos de Bootstrap

### ¿Qué es Bootstrap?
Bootstrap es un framework CSS que nos ayuda a crear diseños web responsivos rápidamente. Ya incluye estilos predefinidos para botones, formularios, tablas, grillas y más.

### Sistema de Grid (Grilla)
Bootstrap divide la pantalla en **12 columnas**. Usamos clases para decidir cuántas columnas ocupa cada elemento según el tamaño de pantalla.

**Breakpoints (tamaños de pantalla):**
- `col-` → Extra pequeño (móviles < 576px)
- `col-sm-` → Pequeño (≥ 576px)
- `col-md-` → Mediano (≥ 768px)
- `col-lg-` → Grande (≥ 992px)
- `col-xl-` → Extra grande (≥ 1200px)

**Ejemplo:**
```html
<div class="row">
    <div class="col-12 col-md-6 col-lg-4">
        <!-- En móvil ocupa 12 columnas (100%)
             En tablet ocupa 6 columnas (50%)
             En desktop ocupa 4 columnas (33%) -->
    </div>
</div>
```

---

## 🎯 Paso 1: Estructura del Container

### Código:
```razor
<div class="container-fluid py-4">
    <!-- Contenido aquí -->
</div>
```

### Explicación:
- `container-fluid`: Ocupa todo el ancho de la pantalla
- `py-4`: Padding vertical (arriba y abajo) de tamaño 4

**Alternativa:**
- `container`: Ancho fijo centrado (usa esto para un diseño más tradicional)

---

## 🎯 Paso 2: Encabezado con Row y Columns

### Código:
```razor
<div class="row mb-4">
    <div class="col-12">
        <div class="d-flex justify-content-between align-items-center">
            <div>
                <h1 class="h3 text-brand mb-1">Gestión de Categorías</h1>
                <p class="text-muted mb-0">Administra las categorías de recetas</p>
            </div>
            <button class="btn btn-brand">
                <span class="bi bi-plus-circle"></span> Nueva Categoría
            </button>
        </div>
    </div>
</div>
```

### Explicación:
- `row`: Crea una fila horizontal
- `col-12`: La columna ocupa las 12 columnas (ancho completo)
- `mb-4`: Margin bottom (espacio abajo) de tamaño 4
- `d-flex`: Activa flexbox
- `justify-content-between`: Espacia los elementos (título a la izquierda, botón a la derecha)
- `align-items-center`: Centra verticalmente
- `text-brand`: Clase de nuestra paleta (usa el color primario)
- `btn btn-brand`: Botón con estilo de nuestra paleta

---

## 🎯 Paso 3: Tarjeta (Card) con Filtros

### Código:
```razor
<div class="row mb-3">
    <div class="col-12">
        <div class="card border-0 shadow-sm">
            <div class="card-body">
                <div class="row g-3">
                    <div class="col-12 col-md-6 col-lg-4">
                        <input type="text" class="form-control" 
                               placeholder="Buscar categoría..." />
                    </div>
                    <div class="col-12 col-md-6 col-lg-3">
                        <button class="btn btn-outline-secondary w-100">
                            Limpiar
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
```

### Explicación:
- `card`: Contenedor con bordes y fondo
- `border-0`: Sin borde (quitamos el borde predeterminado)
- `shadow-sm`: Sombra suave
- `card-body`: Contenido interno con padding automático
- `g-3`: Gap (espacio) entre columnas dentro del row (gutter)
- `form-control`: Estilo para inputs de formulario
- `btn-outline-secondary`: Botón con borde, sin relleno
- `w-100`: Width 100% (ocupa todo el ancho disponible)

---

## 🎯 Paso 4: Grid de Cards (Tarjetas Responsivas)

### Código:
```razor
<div class="row">
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
    <!-- Más cards aquí -->
</div>
```

### Explicación:
- `col-12 col-sm-6 col-md-4 col-lg-3`:
  - Móvil: 1 tarjeta por fila (12 columnas)
  - Tablet: 2 tarjetas por fila (6 columnas cada una)
  - Desktop mediano: 3 tarjetas por fila (4 columnas)
  - Desktop grande: 4 tarjetas por fila (3 columnas)
- `h-100`: Height 100% (todas las tarjetas con la misma altura)
- `d-flex flex-column`: Flexbox vertical
- `flex-grow-1`: El texto crece para ocupar espacio disponible
- `mt-auto`: Margin top automático (empuja el elemento al final)
- `border-top`: Borde superior
- `pt-2`: Padding top (arriba) de tamaño 2
- `small`: Texto más pequeño

---

## 🎯 Paso 5: Tabla Responsiva

### Código:
```razor
<div class="table-responsive">
    <table class="table table-hover mb-0">
        <thead class="table-light">
            <tr>
                <th scope="col">ID</th>
                <th scope="col">Nombre</th>
                <th scope="col" class="text-end">Acciones</th>
            </tr>
        </thead>
        <tbody>
            <tr>
                <td>1</td>
                <td class="fw-semibold">Postres</td>
                <td class="text-end">
                    <button class="btn btn-sm btn-outline-primary">Editar</button>
                </td>
            </tr>
        </tbody>
    </table>
</div>
```

### Explicación:
- `table-responsive`: Activa scroll horizontal en pantallas pequeñas
- `table`: Estilo base de tabla
- `table-hover`: Efecto hover (resalta fila al pasar el mouse)
- `table-light`: Fondo claro en el encabezado
- `mb-0`: Sin margen inferior
- `fw-semibold`: Font weight semi-bold (texto semi-negrita)
- `text-end`: Texto alineado a la derecha
- `btn-sm`: Botón pequeño

---

## 🎯 Paso 6: Modal (Ventana Emergente)

### Código:
```razor
<div class="modal fade show d-block" tabindex="-1" 
     style="background-color: rgba(0,0,0,0.5);">
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">Nueva Categoría</h5>
                <button type="button" class="btn-close"></button>
            </div>
            <div class="modal-body">
                <div class="mb-3">
                    <label class="form-label">Nombre</label>
                    <input type="text" class="form-control" />
                </div>
            </div>
            <div class="modal-footer">
                <button class="btn btn-secondary">Cancelar</button>
                <button class="btn btn-brand">Guardar</button>
            </div>
        </div>
    </div>
</div>
```

### Explicación:
- `modal`: Contenedor del modal
- `fade show`: Animación de aparición
- `d-block`: Display block (hace visible el modal)
- `modal-dialog`: Contenedor interno
- `modal-dialog-centered`: Centra verticalmente
- `modal-content`: Contenido del modal
- `modal-header`: Encabezado
- `modal-body`: Cuerpo (contenido principal)
- `modal-footer`: Pie (botones de acción)
- `btn-close`: Botón X para cerrar

---

## 🎨 Clases de Utilidad Importantes

### Espaciado
- `m-*`: Margin (todos los lados)
- `mt-*`, `mb-*`, `ms-*`, `me-*`: Margin top, bottom, start (izq), end (der)
- `p-*`, `pt-*`, `pb-*`, `ps-*`, `pe-*`: Padding (igual que margin)
- Tamaños: 0, 1, 2, 3, 4, 5, auto

### Texto
- `text-center`, `text-start`, `text-end`: Alineación
- `text-muted`: Texto gris (secundario)
- `fw-bold`, `fw-semibold`, `fw-normal`: Grosor de fuente
- `small`: Texto pequeño

### Display
- `d-none`: Ocultar elemento
- `d-block`, `d-flex`, `d-grid`: Tipo de display
- `d-sm-block`, `d-md-flex`: Display según breakpoint

### Colores
- `bg-*`: Color de fondo (primary, secondary, success, danger, etc.)
- `text-*`: Color de texto

---

## 📝 Ejercicios para Alumnos

### Ejercicio 1: Cambiar el Grid
Modifica las clases del grid de cards para que:
- En móvil: 1 tarjeta por fila
- En tablet: 3 tarjetas por fila
- En desktop: 6 tarjetas por fila

**Pista:** Usa `col-12 col-sm-4 col-lg-2`

### Ejercicio 2: Agregar Alertas
Crea una alerta de Bootstrap arriba del grid que diga "Tienes 5 categorías creadas"

**Pista:**
```html
<div class="alert alert-success">Mensaje aquí</div>
```

### Ejercicio 3: Botón de Acciones
Agrega un menú dropdown con opciones "Ver", "Editar", "Eliminar" en cada tarjeta.

**Pista:** Usa `dropdown`, `dropdown-toggle`, `dropdown-menu`

### Ejercicio 4: Formulario Completo
Agrega un campo de selección (select) al modal para elegir un "color" de categoría.

**Pista:**
```html
<select class="form-select">
    <option>Rojo</option>
    <option>Azul</option>
</select>
```

### Ejercicio 5: Paginación
Agrega un componente de paginación debajo de la tabla.

**Pista:**
```html
<nav>
    <ul class="pagination">
        <li class="page-item"><a class="page-link" href="#">1</a></li>
        <li class="page-item"><a class="page-link" href="#">2</a></li>
    </ul>
</nav>
```

---

## 🎓 Conceptos Clave para Recordar

1. **Mobile First**: Bootstrap diseña primero para móvil, luego añade clases para pantallas más grandes
2. **Grid de 12 columnas**: Siempre suma 12 (ej: 3+3+3+3, 4+4+4, 6+6, etc.)
3. **Clases de utilidad**: Puedes combinar múltiples clases para lograr el diseño deseado
4. **Responsividad**: Usa breakpoints (`sm`, `md`, `lg`) para adaptar el diseño
5. **Paleta personalizada**: Usa `text-brand`, `btn-brand`, `bg-brand` de tu `palette.css`

---

## 🔗 Recursos Adicionales

- Documentación oficial: https://getbootstrap.com/docs/5.3
- Grid system: https://getbootstrap.com/docs/5.3/layout/grid/
- Componentes: https://getbootstrap.com/docs/5.3/components/
- Utilidades: https://getbootstrap.com/docs/5.3/utilities/

---

## ✅ Checklist de Comprensión

Después de completar esta guía, los alumnos deberían poder:
- [ ] Explicar cómo funciona el sistema de grid de 12 columnas
- [ ] Crear layouts responsivos usando breakpoints
- [ ] Usar cards para mostrar contenido
- [ ] Implementar formularios con clases de Bootstrap
- [ ] Crear tablas responsivas
- [ ] Mostrar/ocultar modales
- [ ] Aplicar clases de utilidad para espaciado y alineación
- [ ] Integrar la paleta de colores personalizada con Bootstrap
