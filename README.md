# ArcanoPizza API

> API REST para un sistema de pedidos de pizza. Construida con .NET y PostgreSQL.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dot.net)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-4169E1?logo=postgresql&logoColor=white)](https://www.postgresql.org/)

---

## Inicio rápido

```bash
# 1. Restaurar paquetes
dotnet restore

# 2. Configurar la cadena de conexión (User Secrets)
cd ArcanoPizza_API
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "postgresql://USER:PASS@HOST:5432/DB?sslmode=require"

# 3. Aplicar migraciones (si es la primera vez)
dotnet ef database update --project ArcanoPizza_API.Data --startup-project ArcanoPizza_API

# 4. Ejecutar la API
dotnet run --project ArcanoPizza_API
```

---

## Workflow de desarrollo

### Flujo de ramas (Git Flow simplificado)

| Rama | Propósito |
|------|-----------|
| `main` | Producción estable. Solo recibe merges desde `develop` tras release. |
| `develop` | Integración. Rama base para features y fixes. |
| `feature/*` | Nuevas funcionalidades. Ej: `feature/crud-productos`, `feature/login-jwt`. |
| `fix/*` | Correcciones de bugs. Ej: `fix/validacion-precio`. |
| `release/*` | Preparación de releases. Solo fixes menores antes de producción. |

### Ciclo típico de un cambio

1. **Crear rama** desde `develop`:
   ```bash
   git checkout develop
   git pull origin develop  # Siempre trabajar a partir de develop nunca de main
   git checkout -b feature/nombre-descriptivo 
   ```

2. **Desarrollar** y hacer commits pequeños y descriptivos.

3. **Asegurar calidad** antes de push:
   ```bash
   dotnet build
   dotnet ef database update --project ArcanoPizza_API.Data --startup-project ArcanoPizza_API  # si hay migraciones
   ```

4. **Push y Pull Request** a `develop`:
   ```bash
   git push origin feature/nombre-descriptivo
   ```
   Crear PR con descripción, referencias a issues si aplica, y checklist: compila, sin credenciales, migraciones revisadas.

5. **Merge** 

6. **Eliminar rama** tras el merge (ver más abajo).

### Cómo eliminar una rama

**Eliminar rama local** (debes estar en otra rama):

```bash
git checkout develop
git branch -d feature/nombre-descriptivo    # Elimina si ya fue mergeada
git branch -D feature/nombre-descriptivo   # Fuerza eliminación (aunque no esté mergeada)
```

**Eliminar rama remota**:

```bash
git push origin --delete feature/nombre-descriptivo
```

**Limpiar referencias a ramas remotas eliminadas**:

```bash
git fetch --prune
```

---

## Convenciones de commits

Usamos [Conventional Commits](https://www.conventionalcommits.org/) para un historial claro:

| Prefijo | Uso | Ejemplo |
|---------|-----|---------|
| `feat:` | Nueva funcionalidad | `feat: add CRUD Extras endpoint` |
| `fix:` | Corrección de bug | `fix: validación de precio en ExtraCreateDto` |
| `docs:` | Solo documentación | `docs: update README con workflow` |
| `refactor:` | Código sin cambiar comportamiento | `refactor: extract validation to separate class` |
| `test:` | Tests | `test: add ExtrasController unit tests` |
| `chore:` | Mantenimiento, dependencias | `chore: upgrade EF Core to 8.0.12` |
| `security:` | Cambios de seguridad | `security: add rate limiting to auth endpoints` |

**Formato:** `tipo(ámbito): descripción` — Ej: `feat(extras): add PUT endpoint for updating extras`

---

## Workflow de migraciones

1. **Crear migración** tras cambios en el modelo:
   ```bash
   dotnet ef migrations add NombreDescriptivo --project ArcanoPizza_API.Data --startup-project ArcanoPizza_API
   ```

2. **Revisar** el código generado en `Migrations/` antes de commitear.

3. **Aplicar** en desarrollo:
   ```bash
   dotnet ef database update --project ArcanoPizza_API.Data --startup-project ArcanoPizza_API
   ```

---

# ArcanoPizza API — Guía de arquitectura y uso

Este documento explica cómo está organizado el proyecto, qué hace cada parte y cómo ponerlo en marcha. Está pensado para personas que llegan nuevas al proyecto.

---

## ¿Qué es este proyecto?

Es una **API REST** para un sistema de pedidos de pizza. Permite gestionar productos, extras, pedidos, etc. Está construida con .NET y usa **PostgreSQL** como base de datos.

---

## Conceptos que conviene conocer

| Término | ¿Qué significa? |
|---------|-----------------|
| **API / Web API** | Una aplicación que expone endpoints HTTP (URLs) que otros programas pueden llamar para obtener o enviar datos. |
| **Capas / Layers** | División del código en bloques con responsabilidades claras (ej: una capa para HTTP, otra para base de datos). |
| **Entidad (Model)** | Clase que representa una tabla en la base de datos (ej: `Usuario`, `Pedido`, `Extra`). |
| **DTO** | Objeto de transferencia de datos: la forma en que la API recibe y devuelve información (diferente a las entidades internas). |
| **Controller** | Clase que recibe las peticiones HTTP y decide qué hacer con ellas (GET, POST, PUT, DELETE). |
| **Repository** | Contrato + clase que encapsula el acceso a la BD por agregado/tabla. Los controllers **no** usan repositorios directamente; hablan con **servicios de aplicación** (`I*Service`). |
| **Servicio de aplicación (`I*Service`)** | Contrato en `IServices/`, implementación en `Services/`. Orquesta repositorios, reglas de negocio, integraciones (Stripe, Cloudinary) y devuelve DTOs. |
| **DbContext** | Componente de Entity Framework Core que representa la conexión con la base de datos y las tablas. |
| **Migración** | Archivo que describe cambios en el esquema de la BD (crear tablas, agregar columnas, etc.). |

---

## Arquitectura actual (proyectos y responsabilidades)

El código está dividido en **cuatro proyectos** de biblioteca más el host web:

```
                    Cliente (Angular, Postman, integraciones)
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────┐
│  ArcanoPizza_API — Host ASP.NET Core (.NET 10)                       │
│  • Punto de entrada: Program.cs (pipeline HTTP mínimo)               │
│  • Extensions: AddArcanoPizzaCore (app), AddSecurity (OWASP)         │
│  • Controllers: solo HTTP, autorización y mapeo a códigos/DTOs       │
│  • Servicios propios del host: AuthService, JwtTokenService,        │
│    AuditLogService, middleware de auditoría y cabeceras               │
└─────────────────────────────────────────┬───────────────────────────┘
                                            │
                                            ▼
┌─────────────────────────────────────────────────────────────────────┐
│  ArcanoPizza_API.Data                                               │
│  • PostgresConfiguration.AddData() → DbContext, repos, servicios     │
│  • Interface/        → I*Repository                                 │
│  • IServices/        → contratos I*Service + AuthOutcome              │
│  • Services/         → implementaciones (*Service) + Models/        │
│  • Repositories/     → acceso EF Core                                │
│  • Migrations/       → esquema PostgreSQL                             │
└───────────────────────────┬───────────────────────────────────────────┘
                            │
            ┌───────────────┴───────────────┐
            ▼                               ▼
┌───────────────────────┐       ┌───────────────────────┐
│ ArcanoPizza_API.Model │       │ ArcanoPizza_API.DTOs  │
│ Entidades EF / dominio│       │ Request/Response HTTP │
└───────────────────────┘       └───────────────────────┘
```

| Proyecto | Rol |
|----------|-----|
| **ArcanoPizza_API** | Expone la API REST, JWT, CORS, logging, rate limiting (no dev), OpenAPI/Swagger en desarrollo. |
| **ArcanoPizza_API.Data** | Persistencia, repositorios y **casos de uso** compartidos (servicios que implementan `IServices`). |
| **ArcanoPizza_API.Model** | Entidades mapeadas a tablas. |
| **ArcanoPizza_API.DTOs** | Contratos de entrada/salida; la API no expone entidades. |

**Por qué `IAuthService` vive en Data pero `AuthService` en la API:** el contrato (`IAuthService`, `AuthOutcome`) lo consumen los controllers y está en `ArcanoPizza_API.Data.IServices`; la implementación usa JWT y `PasswordHasher` del stack web y reside en `ArcanoPizza_API/Services`, registrada en `AddArcanoPizzaCore`.

---

## Flujo de arranque (`Program.cs`)

1. **`builder.Services.AddArcanoPizzaCore(configuration)`**  
   Registra la capa de datos (`AddData`), opciones JWT, `PasswordHasher`, `IJwtTokenService`, `IAuthService` → `AuthService`, auditoría (servicio + retención en background), autenticación JWT Bearer, autorización, controllers, OpenAPI, HTTP logging, CORS (`Frontend`), forwarded headers.

2. **`builder.Services.AddSecurity(configuration, environment)`**  
   Manejador global de excepciones, Problem Details, antiforgery, HSTS y **rate limiting** (solo fuera de Development).

3. **Pipeline HTTP** (orden relevante):  
   `UseForwardedHeaders` → HTTPS/HSTS según entorno → `UseExceptionHandler` → `UseHttpLogging` → `UseRequestTraceLogging` (incluye trazas de login sin contraseña) → `UseCors("Frontend")` → `SecurityHeadersMiddleware` → `UseRateLimiter` (no dev) → OpenAPI + Swagger UI (solo dev) → `UseAuthentication` → `AuditLogMiddleware` → `UseAuthorization` → `MapControllers`.

---

## Flujo de una petición HTTP (ejemplo)

Ejemplo: `GET /api/Extras` (listado público o según endpoint).

```
1. Cliente envía la petición (headers, JWT si aplica).
2. Pasa por middleware: logging, CORS, cabeceras de seguridad, autenticación/autorización, auditoría.
3. ExtrasController recibe la petición y delega en IExtraService.
4. ExtraService usa IExtraRepository (y si aplica DbContext) para leer datos.
5. Los datos se proyectan a DTOs (p. ej. ExtraResponseDto).
6. El controller devuelve el código HTTP y el JSON.
```

Otros flujos típicos:

| Área | Ruta / rol | Piezas principales |
|------|------------|---------------------|
| Auth | `/api/Auth/*` | `AuthController` → `IAuthService` / `AuthService` → repos usuario/refresh token |
| Pedidos | `/api/Pedidos` | `IPedidosService`, `IPedidoCreacionService`, `IPedidoRepository` |
| Pagos Stripe | `/api/Pagos` | `IStripeCheckoutService` (Stripe.net + creación/confirmación de pedido) |
| Catálogo | `/api/Productos` | `IProductoCatalogoService` |
| Admin | `/api/Admin` | `IAdminService` + `IAdminRepository` |
| Subidas | `/api/uploads` | `ICloudinarySignatureService` (firma server-side) |
| Auditoría (técnico) | `/api/audit-logs` | `IAuditLogsQueryService` |

---

## Estructura del repositorio (carpetas y archivos)

### Raíz del repositorio

| Archivo/Carpeta | Descripción |
|-----------------|-------------|
| `ArcanoPizza_API.slnx` | Archivo de solución: lista de proyectos que forman la aplicación. |
| `README.md` | Este documento. |
| `ArcanoPizza_API/` | Proyecto principal (API web). |
| `ArcanoPizza_API.Data/` | Proyecto de acceso a datos. |
| `ArcanoPizza_API.Model/` | Proyecto de entidades. |
| `ArcanoPizza_API.DTOs/` | Proyecto de DTOs. |
| `.github/` | Configuración de GitHub (por ejemplo, workflows de CI/CD). |

---

### Proyecto `ArcanoPizza_API` (Web API)

| Archivo/Carpeta | ¿Para qué sirve? |
|-----------------|------------------|
| `Program.cs` | Punto de entrada: `AddArcanoPizzaCore` + `AddSecurity` y el pipeline HTTP (sección **Flujo de arranque** más arriba). |
| `Extensions/` | `ArcanoPizzaCoreExtensions` (registro de servicios de aplicación), `ServiceCollectionExtensions` (`AddSecurity`). |
| `Middleware/` | Cabeceras de seguridad, auditoría de solicitudes, manejo global de excepciones. |
| `Controllers/` | REST: `Auth`, `Admin`, `AuditLogs`, `Direcciones`, `Extras`, `Pagos`, `Pedidos`, `Productos`, `Promociones`, `Uploads`. |
| `Services/` | Implementaciones del host: `AuthService`, `JwtTokenService`, servicios de auditoría (los contratos `IAuthService` están en `ArcanoPizza_API.Data.IServices`). |
| `Options/` | Opciones fuertemente tipadas (`JwtOptions`, retención de audit logs, validadores). |
| `Helpers/` | Extensiones sobre claims y utilidades usadas por controllers. |
| `appsettings.json` | Configuración base; secretos vía User Secrets o variables de entorno. |
| `appsettings.Development.json` | Ajustes solo en desarrollo. |
| `Properties/launchSettings.json` | Perfiles de ejecución. |
| `ArcanoPizza_API.http` | Ejemplos de llamadas HTTP desde el IDE. |

---

### Proyecto `ArcanoPizza_API.Data` (persistencia y casos de uso)

Incluye **DbContext**, **repositorios**, **contratos de servicios** (`IServices`) e **implementaciones** (`Services`).

#### Carpetas principales

| Carpeta | Propósito |
|---------|-----------|
| `Interface/` | Interfaces `I*Repository`. |
| `IServices/` | Interfaces `I*Service`, `AuthOutcome` y contratos de servicios. Namespace: `ArcanoPizza_API.Data.IServices`. |
| `Services/` | Implementaciones de servicios e integraciones (Stripe, Cloudinary); `Models/` para tipos auxiliares (p. ej. firma Cloudinary). Namespace: `ArcanoPizza_API.Data.Services`. |
| `Repositories/` | Acceso a datos vía EF Core. |
| `Migrations/` | Migraciones aplicadas al esquema PostgreSQL. |

#### Archivos clave

| Archivo | ¿Qué hace? |
|---------|------------|
| `ArcanoPizzaDbContext.cs` | `DbSet<>`, mapeos y relaciones. |
| `PostgresConfiguration.cs` | `AddData`: registra `DbContext`, repositorios y parejas `I*Service` → implementación. Usa `DATABASE_URL` o `ConnectionStrings:DefaultConnection`. Lo invoca `AddArcanoPizzaCore` en el host. |
| `PostgresConnectionString.cs` | Normaliza URLs `postgresql://...` al formato Npgsql. |

---

### Proyecto `ArcanoPizza_API.Model` (Dominio)

Contiene las **entidades**: clases que representan las tablas de la base de datos.

Ejemplos: `Extra`, `Producto`, `Usuario`, `Pedido`, `PedidoItem`, `CategoriaProducto`, etc.

Entity Framework usa estas clases para mapear filas de la BD a objetos en memoria.

---

### Proyecto `ArcanoPizza_API.DTOs` (Contratos)

Define **qué entra y sale** por la API:

- **Request DTOs** (ej: `ExtraCreateDto`, `ExtraUpdateDto`): lo que el cliente envía en el body.
- **Response DTOs** (ej: `ExtraResponseDto`): lo que la API devuelve.

Usar DTOs en lugar de entidades evita exponer la estructura interna de la BD y permite cambiar el contrato de la API sin tocar las entidades.

---

## Configuración de la base de datos

La API necesita una **cadena de conexión** para conectar con PostgreSQL. Por seguridad, no debe subirse a Git.

**User Secrets** (clave `ConnectionStrings:DefaultConnection`).

Paso 1: Abrir la terminal en la raíz del proyecto

Paso 2: Ir al proyecto de la API
cd C:\ProyectosJP\arcanoPizza\ArcanoPizza_API

Paso 3: Inicializar User Secrets

**dotnet user-secrets init**

Se añadirá un UserSecretsId en el .csproj y se creará la carpeta para los secretos.

Paso 4: Guardar la cadena de conexión
Sustituye con tu cadena real de PostgreSQL:

**dotnet user-secrets set "ConnectionStrings:DefaultConnection" "postgresql://TU_USUARIO:TU_PASSWORD@TU_HOST:5432/TU_BASE_DE_DATOS?sslmode=require"**


Paso 5: Comprobar que se guardó

**dotnet user-secrets list**

Deberías ver algo como:
ConnectionStrings:DefaultConnection = postgresql://...

Paso 6: Ejecutar la API



---

## Cómo ejecutar el proyecto

### Requisitos

- **.NET SDK 10** (los proyectos usan `net10.0`).
- PostgreSQL accesible (local, Neon, etc.) con la cadena configurada (User Secrets o `DATABASE_URL`).

### Comandos

```bash
# Restaurar paquetes
dotnet restore

# Ejecutar la API
dotnet run --project ArcanoPizza_API
```

La API quedará disponible (por defecto en `https://localhost:5xxx` o similar; revisa la salida en consola).

### Aplicar migraciones

Si es la primera vez o hubo cambios en el esquema:

```bash
dotnet ef database update --project ArcanoPizza_API.Data --startup-project ArcanoPizza_API
```

---

## Convenciones del proyecto

### Código y arquitectura

| Área | Convención |
|------|------------|
| **No versionar secretos** | Usar User Secrets o `DATABASE_URL`; nunca credenciales en `appsettings.json` subido a Git. |
| **Controllers** | Solo HTTP: autorización, delegación en `I*Service` y mapeo a códigos/DTOs. |
| **Servicios (`IServices` / `Services`)** | Reglas de negocio, orquestación y proyección a DTOs; pueden usar repositorios y `DbContext`. |
| **Repositories** | Persistencia; los controllers no inyectan repositorios ni `DbContext` directamente. |
| **DTOs** | Nunca exponer entidades. Siempre mapear a DTOs de request/response. |
| **Fechas** | Usar `DateTime.UtcNow` para consistencia. |
| **Async** | Métodos que hacen I/O usan `async/await` y reciben `CancellationToken` donde aplique. |

### Convenciones de código (C#)

| Elemento | Formato | Ejemplo |
|----------|---------|---------|
| **Variables locales** | camelCase | `var extras = ...`, `var created = ...` |
| **Parámetros** | camelCase | `(int id, CancellationToken ct)`, `(ExtraCreateDto dto)` |
| **Campos privados** | _camelCase | `private readonly IExtraRepository _extraRepository` |
| **Constantes** | PascalCase o UPPER_SNAKE_CASE | `MaxRetries = 3`, `DEFAULT_TIMEOUT` |
| **Clases** | PascalCase | `ExtrasController`, `ExtraRepository` |
| **Interfaces** | I + PascalCase | `IExtraRepository`, `IRepository<T>` |
| **Métodos** | PascalCase | `GetAllAsync`, `GetByIdAsync`, `AddAsync` |
| **Propiedades** | PascalCase | `IdExtra`, `Nombre`, `PrecioBase` |
| **Namespaces** | PascalCase (proyecto.Carpeta) | `ArcanoPizza_API.Controllers` |
| **Archivos** | Mismo nombre que la clase principal | `ExtrasController.cs`, `ExtraRepository.cs` |

**Otros:**

- **CancellationToken**: usar el parámetro `ct` para abreviar.
- **Booleans**: prefijo `Is`, `Has` o `Can` cuando aporte claridad — `IsActive`, `HasItems`.
- **Colecciones**: plural cuando sea una lista — `extras`, `productos`, `PedidosItem`.
- **Evitar abreviaciones** salvo las muy comunes (id, ct, dto, etc.).
- **Idioma**: propiedades y entidades en español cuando el dominio lo sea; código interno (variables, métodos) puede ir en inglés o español de forma consistente.

### Nomenclatura (Git y API)

| Elemento | Formato | Ejemplo |
|----------|---------|---------|
| Ramas | `tipo/descripcion-kebab-case` | `feature/crud-productos`, `fix/validacion-precio` |
| Commits | `tipo(ámbito): descripción` | `feat(extras): add PATCH support` |
| Controllers | `{Entidad}Controller` | `ExtrasController` |
| DTOs | `{Entidad}{Create,Update,Response}Dto` | `ExtraCreateDto`, `ExtraResponseDto` |
| Servicios (contrato / impl.) | `I{Nombre}Service` en `IServices/` · `{Nombre}Service` en `Services/` | `IExtraService`, `ExtraService` |
| Repositories | `I{Entidad}Repository` en `Interface/` · `{Entidad}Repository` en `Repositories/` | `IExtraRepository`, `ExtraRepository` |
| Rutas API | `/api/{Entidad}` o rutas dedicadas | `/api/Extras`, `/api/audit-logs` |

### Estilo de código

- **Llaves**: llave de apertura en la misma línea que la declaración.
- **Indentación**: 4 espacios.
- **Línea en blanco** antes de `return` cuando hay varias líneas de lógica.
- **Orden de miembros** en clases: campos → constructor → métodos públicos → métodos privados.

### Antes de hacer commit

- [ ] `dotnet build` sin errores
- [ ] Sin credenciales ni datos sensibles en el código
- [ ] Mensaje de commit descriptivo (preferiblemente Conventional Commits)

---

## Seguridad: OWASP Top 10:2025

La API aplica medidas para cubrir las vulnerabilidades del [OWASP Top 10:2025](https://owasp.org/Top10/2025/):

| Categoría OWASP | Medidas implementadas |
|-----------------|------------------------|
| **A01 - Broken Access Control** | JWT + `[Authorize]` y roles en endpoints administración/técnico/pedidos según corresponda. |
| **A02 - Security Misconfiguration** | Cabeceras (`SecurityHeadersMiddleware`), HSTS en producción, OpenAPI/Swagger solo en desarrollo. |
| **A03 - Software Supply Chain** | Paquetes NuGet oficiales; mantener dependencias y revisar Dependabot. |
| **A04 - Cryptographic Failures** | HTTPS; `sslmode=require` en PostgreSQL; contraseñas con `PasswordHasher` (ASP.NET Identity). JWT firmado con clave configurada (`Jwt:SigningKey`). |
| **A05 - Injection** | EF Core parametrizado; validación en DTOs; rate limiting fuera de Development. |
| **A06 - Insecure Design** | Capas API / Data / Model / DTOs; servicios de aplicación entre controller y persistencia. |
| **A07 - Authentication Failures** | JWT Bearer; límites de rate limiting (`auth` / global) en entornos no desarrollo. |
| **A08 - Software/Data Integrity** | Revisar cadena de suministro en CI/CD. |
| **A09 - Security Logging** | `GlobalExceptionHandler`, HTTP logging, `AuditLogMiddleware`, trazas de request sin secretos (login solo registra correo). |
| **A10 - Mishandling of Exceptions** | Problem Details; sin detalles internos al cliente en producción. |

### Configurar JWT

```bash
dotnet user-secrets set "Jwt:SigningKey" "tu-clave-secreta-de-al-menos-32-caracteres"
```

---

## Glosario rápido

- **CRUD**: Create, Read, Update, Delete (crear, leer, actualizar, eliminar).
- **EF Core**: Entity Framework Core, ORM para .NET que mapea objetos a tablas de la BD.
- **`IServices` / `Services` (Data)**: Carpeta y namespace de **contratos** de casos de uso (`I*Service`) frente a **implementaciones** (`*Service`) en el proyecto `ArcanoPizza_API.Data`.
- **Inyección de dependencias**: El contenedor DI registra interfaces y resuelve implementaciones en controllers y servicios (p. ej. `IExtraService` → `ExtraService`).
- **Npgsql**: Proveedor de PostgreSQL para .NET.
- **ORM**: Object-Relational Mapping; traduce entre objetos en código y filas en la BD.
