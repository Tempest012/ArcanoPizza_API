# Cobertura detallada del OWASP Top 10:2025

Este documento explica cómo el proyecto ArcanoPizza API aborda cada vulnerabilidad del [OWASP Top 10:2025](https://owasp.org/Top10/2025/), con referencias a archivos, código y recomendaciones.

---

## A01:2025 — Broken Access Control (Control de acceso roto)

**¿Qué es?** Fallos en la aplicación del control de acceso: un usuario puede acceder a recursos o funciones que no le corresponden. Incluye IDOR (Insecure Direct Object Reference), escalada de privilegios, bypass de restricciones, etc.

### Cómo lo cubrimos

| Medida | Implementación | Ubicación |
|--------|-----------------|-----------|
| **Autenticación JWT** | La infraestructura para verificar identidad está preparada. Cuando se configure `Jwt:Key`, la API validará tokens Bearer. | `Extensions/ServiceCollectionExtensions.cs` → `AddJwtAuthentication()` |
| **`[Authorize]`** | ASP.NET Core Authorization está registrado. Podrás proteger controllers o acciones con `[Authorize]` o `[Authorize(Roles = "Admin")]`. | Disponible en `Program.cs` via `app.UseAuthorization()` |
| **Validación de existencia** | En endpoints con ID (ej: `GetById`, `Update`, `Delete`), se comprueba que el recurso exista antes de operar. Evita revelar si un ID existe o no mediante timing. | `Controllers/ExtrasController.cs` → `if (extra is null) return NotFound()` |

---

## A02:2025 — Security Misconfiguration (Configuración insegura)

**¿Qué es?** Ajustes incorrectos o por defecto en la aplicación, frameworks, servidor o infraestructura. Swagger expuesto en producción, cabeceras faltantes, mensajes de error detallados, etc.

### Cómo lo cubrimos

| Medida | Implementación | Ubicación |
|--------|-----------------|-----------|
| **Cabeceras de seguridad** | Middleware que añade cabeceras en cada respuesta. | `Middleware/SecurityHeadersMiddleware.cs` |
| **X-Frame-Options: DENY** | Impide que la API se cargue en iframes (evita clickjacking). | Línea 18 |
| **X-Content-Type-Options: nosniff** | Evita que el navegador interprete respuestas como otro MIME type. | Línea 21 |
| **Referrer-Policy: strict-origin-when-cross-origin** | Limita qué información del referrer se envía. | Línea 24 |
| **Permissions-Policy** | Desactiva geolocation, microphone, camera que no usa la API. | Línea 27 |
| **Content-Security-Policy** | Restringe orígenes de contenido ejecutable; `frame 
| **Swagger solo en desarrollo** | OpenAPI y Swagger UI solo se mapean cuando `Environment.IsDevelopment()`. | `Program.cs` líneas 31–37 |
| **HTTPS forzado** | Redirección HTTP → HTTPS para todo el tráfico. | `Program.cs` → `app.UseHttpsRedirection()` |
| **Antiforgery** | Servicio de protección contra CSRF registrado (útil si se añaden cookies/formularios). | `ServiceCollectionExtensions.cs` → `AddAntiforgery()` |

---

## A03:2025 — Software Supply Chain Failures (Fallos en la cadena de suministro)

**¿Qué es?** Riesgos por dependencias comprometidas, paquetes maliciosos o falta de verificación de integridad de componentes externos.

### Cómo lo cubrimos

| Medida | Implementación |
|--------|-----------------|
| **Paquetes oficiales** | Uso de paquetes NuGet de Microsoft y ecosistema conocido (Npgsql, EF Core). |
| **Versiones explícitas** | En el `.csproj` se usan versiones fijas (ej: `10.0.0`, `10.0.1`), no wildcards. |
| **Restauración determinista** | `dotnet restore` genera un lock file implícito según el SDK. |

### En proceso

- Activar Dependabot o Renovate en GitHub para actualizaciones de dependencias.
- Revisar alertas de seguridad de NuGet.
- En CI/CD: `dotnet list package --vulnerable` para detectar paquetes con vulnerabilidades conocidas.
- verificación de firmas de paquetes (NuGet signed packages).

---

## A04:2025 — Cryptographic Failures (Fallos criptográficos)

**¿Qué es?** Datos sensibles transmitidos o almacenados sin cifrado adecuado, algoritmos débiles, o claves mal gestionadas.

### Cómo lo cubrimos

| Medida | Implementación | Ubicación |
|--------|-----------------|-----------|
| **HTTPS** | Todo el tráfico HTTP se redirige a HTTPS. La API está pensada para ejecutarse detrás de TLS. | `Program.cs` |
| **TLS en PostgreSQL** | La cadena de conexión admite `sslmode=require` (o `verify-full`). La documentación recomienda usarla. | `PostgresConnectionString.cs`, `PostgresConfiguration.cs` |
| **Secrets fuera del código** | User Secrets y variables de entorno (`DATABASE_URL`, `JWT_KEY`). No se suben credenciales a Git. | `appsettings.json` sin valores sensibles |
| **Clave JWT mínima** | Se exige al menos 32 caracteres para HS256. | `ServiceCollectionExtensions.cs` línea 68–69 |

### Pendiente

- Cuando exista entidad Usuario con contraseña: usar `BCrypt.Net` o `Argon2` para hashear; **nunca** almacenar en texto plano.

---

## A05:2025 — Injection (Inyección)

**¿Qué es?** Datos no confiables que se interpretan como código o comandos: SQL, NoSQL, OS, LDAP, etc.

### Cómo lo cubrimos

| Medida | Implementación | Ubicación |
|--------|-----------------|-----------|
| **Consultas parametrizadas** | Entity Framework Core genera SQL parametrizado. No se concatenan strings con datos de usuario. | `Repositories/Repository.cs` → `FindAsync`, `GetByIdAsync`, etc. |
| **LINQ/Expressions** | `Where(predicate)` se traduce a parámetros, no a SQL dinámico. | `Repository.cs` línea 30 |
| **Validación en DTOs** | `[Required]`, `[MaxLength(100)]`, `[Range(0, 99999.99)]` validan entrada antes de llegar a la BD. | `ArcanoPizza_API.DTOs/ExtraDto.cs` |
| **Validación automática** | `[ApiController]` activa la validación por defecto; peticiones inválidas retornan 400. | `Controllers/ExtrasController.cs` |
| **Rate limiting** | Limita peticiones por minuto, reduce abuso masivo y fuerza bruta. | `ServiceCollectionExtensions.cs` → `AddRateLimiter()` |

### Ejemplo de protección

```csharp
// SEGURO: EF Core parametriza automáticamente
await _dbSet.Where(e => e.Nombre == userInput).ToListAsync(ct);

// INSEGURO (no se usa en el proyecto):
// var sql = "SELECT * FROM Extras WHERE Nombre = '" + userInput + "'";
```

---

## A06:2025 — Insecure Design (Diseño inseguro)

**¿Qué es?** Fallos en el diseño o arquitectura, no en la implementación: ausencia de threat modeling, flujos que asumen que el usuario siempre actúa correctamente, etc.

### Cómo lo cubrimos

| Medida | Implementación |
|--------|-----------------|
| **Arquitectura en capas** | Separación API → Data → Model → DTOs. Los controllers no acceden a la BD directamente. |
| **Repositorios** | La lógica de datos está encapsulada. Facilita aplicar políticas de acceso y validación. |
| **DTOs** | La API no expone entidades de dominio. Los contratos de entrada/salida están definidos de forma explícita. |
| **Principio de mínimo privilegio** | La configuración de seguridad (cabeceras, rate limit, etc.) se aplica de forma global sin excepciones innecesarias. |

### Pendiente

- Realizar threat modeling periódico (STRIDE, etc.).
- Documentar supuestos de seguridad en el diseño.
- Revisar flujos críticos (checkout, pago, roles) con ojos de atacante.

---

## A07:2025 — Authentication Failures (Fallos de autenticación)

**¿Qué es?** Problemas en la identificación y autenticación: credenciales por defecto, bypass de login, sesiones inseguras, fuerza bruta, etc.

### Cómo lo cubrimos

| Medida | Implementación | Ubicación |
|--------|-----------------|-----------|
| **JWT Bearer** | Autenticación basada en tokens. Se activa cuando `Jwt:Key` está configurado. | `ServiceCollectionExtensions.cs` → `AddJwtAuthentication()` |
| **Validación de token** | Issuer, Audience, firma y expiración se validan. `ClockSkew = TimeSpan.Zero` evita ventanas de gracia innecesarias. | Líneas 74–84 |
| **Rate limiting en auth** | Política `auth` con 10 peticiones/minuto para futuros endpoints de login. | `AddFixedWindowLimiter("auth", ...)` |
| **Clave mínima** | JWT exige al menos 32 caracteres para HS256. | Líneas 67–69 |

### Pendiente

- Endpoint `/login` que valide credenciales y emita JWT.
- Proteger operaciones sensibles con `[Authorize]`.
- Para contraseñas: hashear con bcrypt/Argon2, nunca en texto plano.

---

## A08:2025 — Software or Data Integrity Failures (Fallos de integridad)

**¿Qué es?** Código, datos o pipelines comprometidos: paquetes modificados, CI/CD sin verificación, deserialización insegura, etc.

### Cómo lo cubrimos

| Medida | Implementación |
|--------|-----------------|
| **Deserialización** | ASP.NET Core deserializa JSON de forma segura. No se usan serializadores inseguros (BinaryFormatter, etc.). |
| **Fuentes de paquetes** | NuGet.org como fuente principal; no se usan feeds no verificados. |

### Pendiente

- Usar paquetes firmados cuando estén disponibles.
- En CI/CD: verificar checksums o firmas de artefactos.
- Evitar `[AllowAnonymous]` en endpoints que modifican datos sin otra capa de verificación.
- Revisar workflows de GitHub Actions y secrets.

---

## A09:2025 — Security Logging and Alerting Failures (Fallos en registro y alertas)

**¿Qué es?** Falta de logging de eventos de seguridad, logs insuficientes o que exponen datos sensibles, y ausencia de alertas ante incidentes.

### Cómo lo cubrimos

| Medida | Implementación | Ubicación |
|--------|-----------------|-----------|
| **Logging de excepciones** | Cada excepción no controlada se registra con método, ruta y `TraceId`. No se incluyen contraseñas ni datos de tarjetas. | `Middleware/GlobalExceptionHandlerMiddleware.cs` líneas 30–33 |
| **Log estructurado** | Se usan parámetros nombrados (`{Method}`, `{Path}`, `{RequestId}`) para facilitar búsqueda y análisis. | `_logger.LogError(exception, "Error no controlado en {Method} {Path}...", ...)` |
| **TraceId en respuesta** | El cliente recibe `traceId` en la respuesta de error para correlacionar con logs sin filtrar información interna. | `ProblemDetails.Extensions["traceId"]` |

### Pendiente

- Añadir logging explícito para: login fallido, acceso denegado, cambios críticos (create/update/delete en recursos sensibles).
- Conectar el logger a un sistema centralizado (Application Insights, Seq, ELK, etc.).
- Definir alertas cuando se supere un umbral de errores o accesos denegados.

---

## A10:2025 — Mishandling of Exceptional Conditions (Manejo inadecuado de excepciones)

**¿Qué es?** Respuestas de error que filtran información sensible (stack trace, rutas, versiones, detalles de BD) o que permiten a un atacante provocar comportamientos inesperados.

### Cómo lo cubrimos

| Medida | Implementación | Ubicación |
|--------|-----------------|-----------|
| **Manejador global** | `IExceptionHandler` captura todas las excepciones no controladas. | `Middleware/GlobalExceptionHandlerMiddleware.cs` |
| **Producción vs desarrollo** | En producción, `Detail` es un mensaje genérico. No se envía `exception.Message`, stack trace ni tipos internos. | Líneas 40–56 |
| **Formato estándar** | Se usa `ProblemDetails` (RFC 7807) con `application/problem+json`. | Líneas 36–48 |
| **Sin información sensible** | En producción no se exponen rutas de archivos, cadenas de conexión ni detalles de implementación. | Líneas 51–56 |

### Contraste

```text
Desarrollo:  Detail = exception.Message, Extensions incluye stackTrace
Producción:  Detail = "Ha ocurrido un error interno. Por favor, inténtelo más tarde."
             Extensions = solo traceId, requestId
```

---

## Resumen por archivo

| Archivo | OWASP que cubre |
|---------|------------------|
| `Middleware/GlobalExceptionHandlerMiddleware.cs` | A09, A10 |
| `Middleware/SecurityHeadersMiddleware.cs` | A02 |
| `Extensions/ServiceCollectionExtensions.cs` | A01, A02, A05, A07, A09 |
| `Program.cs` | A02, A05, A07, A10 |
| `ArcanoPizza_API.DTOs/ExtraDto.cs` | A05 |
| `ArcanoPizza_API.Data/Repositories/Repository.cs` | A05 |
| `ArcanoPizza_API.Data/PostgresConnectionString.cs` | A04 |
| `Controllers/ExtrasController.cs` | A01 (validación), A05 (`[ApiController]`) |

---

## Referencias

- [OWASP Top 10:2025](https://owasp.org/Top10/2025/)
- [OWASP Cheat Sheet Series](https://cheatsheetseries.owasp.org/)
- [ASP.NET Core Security](https://learn.microsoft.com/en-us/aspnet/core/security/)
