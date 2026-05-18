# Solución a Errores CORS
## Guía para conectar Frontend y Backend en desarrollo

---

## ❌ El Problema: Error CORS

### ¿Qué es CORS?

**CORS** = Cross-Origin Resource Sharing (Compartir Recursos de Origen Cruzado)

Es una **medida de seguridad** de los navegadores que impide que un sitio web (en un puerto/dominio) haga peticiones a otro sitio web (en otro puerto/dominio) sin permiso explícito.

### ¿Por qué ocurre?

```
Frontend: https://localhost:7097  (Blazor)
Backend:  https://localhost:7019  (API)
           ↑
       Diferentes puertos = Diferentes "orígenes"
```

El navegador **bloquea** la petición porque considera que puede ser peligroso que un sitio web se comunique con otro sin autorización.

### Error típico en la consola:

```
Access to fetch at 'https://localhost:7019/api/Categorias' 
from origin 'https://localhost:7097' has been blocked by CORS policy: 
No 'Access-Control-Allow-Origin' header is present on the requested resource.
```

**Traducción:** "El backend no me dio permiso para hacer esta petición"

---

## ✅ La Solución: Configurar CORS en el Backend

### Paso 1: Agregar el servicio CORS

**Archivo:** Backend `Program.cs`

**Ubicación:** ANTES de `var app = builder.Build();`

```csharp
var builder = WebApplication.CreateBuilder(args);

// ...otros servicios (AddControllers, AddSwaggerGen, etc.)

// 🔥 AGREGAR ESTA CONFIGURACIÓN
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin()      // Permite cualquier origen
              .AllowAnyMethod()      // Permite GET, POST, PUT, DELETE, etc.
              .AllowAnyHeader();     // Permite cualquier header
    });
});

var app = builder.Build();
```

**Explicación línea por línea:**

1. **`AddCors(options => ...)`**
   - Registra el servicio de CORS en el contenedor de DI

2. **`options.AddPolicy("PermitirTodo", ...)`**
   - Crea una política con nombre "PermitirTodo"
   - Puedes crear múltiples políticas con diferentes reglas

3. **`policy.AllowAnyOrigin()`**
   - Permite peticiones desde **cualquier** dominio/puerto
   - ⚠️ Solo para desarrollo

4. **`AllowAnyMethod()`**
   - Permite todos los métodos HTTP (GET, POST, PUT, DELETE, etc.)

5. **`AllowAnyHeader()`**
   - Permite cualquier header HTTP (Authorization, Content-Type, etc.)

### Paso 2: Usar la política CORS

**Ubicación:** DESPUÉS de `var app = builder.Build();` pero ANTES de `app.UseAuthorization();`

```csharp
var app = builder.Build();

// ...middleware (UseSwagger, UseHttpsRedirection, etc.)

// 🔥 USAR CORS AQUÍ
app.UseCors("PermitirTodo");

app.UseAuthorization();  // CORS debe ir antes
app.MapControllers();

app.Run();
```

**⚠️ IMPORTANTE: El orden importa**

```csharp
✅ CORRECTO:
app.UseCors("PermitirTodo");
app.UseAuthorization();

❌ INCORRECTO:
app.UseAuthorization();
app.UseCors("PermitirTodo");  // Muy tarde, no funcionará
```

---

## 🔒 CORS para Producción (Avanzado)

La configuración `AllowAnyOrigin()` es **peligrosa en producción**. Solo úsala en desarrollo.

### Configuración segura para producción:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("ProduccionSegura", policy =>
    {
        policy.WithOrigins(
                "https://miapp.com",           // Tu dominio en producción
                "https://www.miapp.com"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();  // Permite cookies/auth
    });
});
```

**Diferencias:**
- `WithOrigins(...)`: Solo permite dominios específicos
- `AllowCredentials()`: Permite enviar cookies/tokens de autenticación
- **NO** se puede usar `AllowAnyOrigin()` con `AllowCredentials()`

### Configuración por entorno:

```csharp
if (app.Environment.IsDevelopment())
{
    // En desarrollo: permisivo
    app.UseCors(policy => policy
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
}
else
{
    // En producción: restrictivo
    app.UseCors("ProduccionSegura");
}
```

---

## 🔍 Cómo Debuggear Errores CORS

### 1. Verificar en DevTools (F12)

**Consola → Ver el error:**
```
Access to fetch at '...' from origin '...' has been blocked by CORS policy
```

**Network → Ver la petición:**
1. Buscar la petición que falló
2. Ver la pestaña "Headers"
3. Buscar `Access-Control-Allow-Origin`
   - ✅ Si está presente: CORS configurado
   - ❌ Si NO está: CORS no configurado

### 2. Preflight Request (Petición de verificación)

Algunos métodos (POST, PUT, DELETE) envían una petición OPTIONS **antes** de la petición real:

```
1. Browser:  OPTIONS /api/Categorias  (pregunta: "¿puedo hacer POST?")
2. Server:   200 OK con headers CORS   (responde: "sí, puedes")
3. Browser:  POST /api/Categorias      (hace la petición real)
```

**Si falla el OPTIONS, falla todo.**

### 3. Errores comunes

| Error | Causa | Solución |
|-------|-------|----------|
| "No 'Access-Control-Allow-Origin' header" | CORS no configurado | Agregar `AddCors` y `UseCors` |
| "CORS policy: ... not allowed" | Origen no permitido | Verificar `WithOrigins` |
| "... doesn't pass access control check" | Preflight falló | Verificar que `UseCors` esté antes de `UseAuthorization` |
| "... AllowCredentials not allowed" | `AllowAnyOrigin` + `AllowCredentials` | Usar `WithOrigins` específico |

---

## 🧪 Probar la Configuración

### Test 1: Verificar que el backend permite CORS

**Herramienta:** Postman o cURL

```bash
curl -H "Origin: https://localhost:7097" \
     -H "Access-Control-Request-Method: POST" \
     -H "Access-Control-Request-Headers: Content-Type" \
     -X OPTIONS \
     https://localhost:7019/api/Categorias
```

**Respuesta esperada:** Headers con `Access-Control-Allow-*`

### Test 2: Verificar desde el navegador

1. Abrir F12 → Consola
2. Ejecutar:
```javascript
fetch('https://localhost:7019/api/Categorias')
  .then(r => r.json())
  .then(console.log)
  .catch(console.error);
```

3. ✅ Si devuelve datos: CORS funciona
4. ❌ Si devuelve error: CORS no funciona

---

## 📝 Ejercicios para Alumnos

### Ejercicio 1: Configuración básica
1. Agregar CORS al backend
2. Hacer una petición desde el frontend
3. Verificar en Network que el header `Access-Control-Allow-Origin` esté presente

### Ejercicio 2: Probar sin CORS
1. Comentar la línea `app.UseCors("PermitirTodo");`
2. Intentar crear una categoría
3. Ver el error en la consola
4. Descomentar y verificar que funciona

### Ejercicio 3: CORS restrictivo
1. Cambiar `AllowAnyOrigin()` por `WithOrigins("https://localhost:7097")`
2. Probar que funciona
3. Cambiar a `WithOrigins("https://otro-dominio.com")`
4. Ver que falla
5. Entender por qué

### Ejercicio 4: Múltiples orígenes
Configurar CORS para permitir:
- Frontend local: `https://localhost:7097`
- Frontend alternativo: `http://localhost:5000`

---

## 🎓 Conceptos Clave

1. **CORS es del navegador:** El servidor NO bloquea nada, es el navegador quien lo hace
2. **OPTIONS preflight:** Algunas peticiones verifican primero con OPTIONS
3. **El orden importa:** `UseCors` debe ir antes de `UseAuthorization`
4. **Desarrollo vs Producción:** Ser permisivo en dev, restrictivo en prod
5. **AllowAnyOrigin + AllowCredentials:** No se pueden usar juntos

---

## ⚠️ Advertencia de Seguridad

```csharp
// ❌ NUNCA hacer esto en producción
policy.AllowAnyOrigin()
      .AllowAnyMethod()
      .AllowAnyHeader();
```

**Por qué es peligroso:**
- Cualquier sitio web puede hacer peticiones a tu API
- Posibles ataques XSS y CSRF
- Datos sensibles expuestos

**Usa siempre:**
```csharp
// ✅ Seguro para producción
policy.WithOrigins("https://tu-dominio.com")
      .AllowAnyMethod()
      .AllowAnyHeader()
      .AllowCredentials();
```

---

## 🔗 Recursos Adicionales

- **Documentación oficial CORS:** https://learn.microsoft.com/es-es/aspnet/core/security/cors
- **MDN CORS:** https://developer.mozilla.org/es/docs/Web/HTTP/CORS
- **Video explicativo:** https://www.youtube.com/watch?v=4KHiSt0oLJ0

---

## ✅ Checklist de Solución CORS

Antes de pedir ayuda, verifica:
- [ ] `AddCors` agregado en `builder.Services`
- [ ] `UseCors` agregado en `app`
- [ ] `UseCors` está ANTES de `UseAuthorization`
- [ ] Backend está corriendo (URL correcta)
- [ ] Frontend apunta a la URL correcta del backend
- [ ] No hay errores de compilación en el backend
- [ ] Header `Access-Control-Allow-Origin` presente en la respuesta (ver Network)
