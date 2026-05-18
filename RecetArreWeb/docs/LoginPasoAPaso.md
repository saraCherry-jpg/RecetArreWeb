# Login con Bootstrap + paleta de colores

## Objetivo
Crear una pantalla de login con Bootstrap y una paleta de colores centralizada en un archivo CSS para reutilizar estilos.

## Paso 1: Crear la paleta de colores
1. Se agregó el archivo `wwwroot/css/palette.css`.
2. Dentro se definieron variables en `:root` para colores primarios, secundarios, fondo, texto y estados.
3. Se agregaron clases utilitarias: `bg-brand`, `text-brand`, `btn-brand` y `link-brand` para usar la paleta en los componentes.

## Paso 2: Cargar la paleta en la app
1. En `wwwroot/index.html` se agregó la referencia:
   - `css/palette.css` después de `css/app.css`.
2. De esta forma, todo el proyecto puede usar las variables y clases de la paleta.

## Paso 3: Crear la página de login
1. Se creó `Pages/Login.razor` con la ruta `@page "/login"`.
2. Se usó la estructura de Bootstrap:
   - `container` y `row` para centrar el contenido.
   - `col-*` para controlar el ancho en distintos tamaños.
   - `card` para el contenedor del formulario.
3. El formulario incluye:
   - Campo de correo.
   - Campo de contraseña con enlace de recuperación.
   - Checkbox “Recuérdame”.
   - Botón principal con `btn-brand`.

## Paso 4: Agregar navegación
1. En `Layout/NavMenu.razor` se añadió el enlace a la ruta `login`.

## Explicación del diseño (para clase)
- La paleta define colores únicos del proyecto y evita repetir valores hex en cada página.
- Las clases `btn-brand` y `text-brand` permiten aplicar el color principal con una sola clase.
- Bootstrap se encarga del layout responsivo y la tarjeta (`card`).
- `login-page` centra el formulario y aplica el fondo definido en la paleta.

## Actividad sugerida para alumnos
1. Cambiar el valor de `--color-primary` y observar cómo se actualiza el botón y el título.
2. Agregar un nuevo color de “éxito” y crear una clase `text-success-brand`.
3. Agregar un nuevo campo al formulario (por ejemplo, “Usuario”).
4. Cambiar el texto principal y volver a revisar la alineación con Bootstrap.
