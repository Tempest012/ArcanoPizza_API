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
| **Repository** | Clase que encapsula el acceso a la base de datos. Los controllers no hablan con la BD directamente, sino a través de repositorios. |
| **DbContext** | Componente de Entity Framework Core que representa la conexión con la base de datos y las tablas. |
| **Migración** | Archivo que describe cambios en el esquema de la BD (crear tablas, agregar columnas, etc.). |

---

## Estructura general: las 4 capas

El proyecto está dividido en **4 proyectos** (capas):

```
Cliente (Postman, frontend, etc.)
        │
        ▼
┌─────────────────────────────┐
│  ArcanoPizza_API            │  ← Entrada HTTP (Controllers, Program.cs)
│  "La puerta de entrada"      │
└─────────────┬───────────────┘
              │
              ▼
┌─────────────────────────────┐
│  ArcanoPizza_API.Data       │  ← Acceso a la base de datos (Repositories, DbContext)
│  "Habla con PostgreSQL"    │
└─────────────┬───────────────┘
              │
              ▼
┌─────────────────────────────┐
│  ArcanoPizza_API.Model      │  ← Entidades del negocio (Producto, Usuario, Pedido...)
│  "Las tablas y relaciones"  │
└─────────────────────────────┘

        ArcanoPizza_API.DTOs   ← Contratos de entrada/salida (Request/Response)
        "Lo que entra y sale por HTTP"
```

| Proyecto | Rol | Contiene |
|----------|-----|----------|
| **ArcanoPizza_API** | Web API | `Program.cs`, Controllers, configuración, `appsettings` |
| **ArcanoPizza_API.Data** | Persistencia | DbContext, Repositories, Migraciones, configuración de PostgreSQL |
| **ArcanoPizza_API.Model** | Dominio | Entidades (clases que mapean a tablas) |
| **ArcanoPizza_API.DTOs** | Contratos | DTOs para request y response |

---

## Flujo de una petición HTTP

Ejemplo: el cliente hace `GET /api/Extras` para obtener todos los extras.

```
1. Cliente → GET /api/Extras
2. ExtrasController recibe la petición
3. Controller llama a IExtraRepository.GetAllAsync()
4. ExtraRepository (implementación) consulta ArcanoPizzaDbContext
5. DbContext ejecuta la query en PostgreSQL
6. Los datos vuelven: DbContext → Repository → Controller
7. Controller convierte las entidades Extra en ExtraResponseDto
8. Cliente recibe JSON con los extras
```

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
| `Program.cs` | Punto de entrada: configura la app, registra servicios (incluida la capa Data) y define el pipeline HTTP. |
| `Controllers/` | Controllers que exponen los endpoints. Ejemplo: `ExtrasController` maneja `/api/Extras`. |
| `appsettings.json` | Configuración base. La cadena de conexión está vacía por seguridad. |
| `appsettings.Development.json` | Configuración adicional solo en desarrollo (logging, etc.). |
| `Properties/launchSettings.json` | Perfiles de ejecución (cómo se lanza la app en debug). |
| `ArcanoPizza_API.http` | Archivo para probar endpoints desde el IDE. |

---

### Proyecto `ArcanoPizza_API.Data` (Persistencia)

Aquí vive todo lo relacionado con la base de datos.

#### Carpetas principales

| Carpeta | Propósito | Uso actual |
|---------|-----------|------------|
| `Repositories/` | Implementaciones que acceden a la BD. `Repository<T>` es genérico; `ExtraRepository` es específico para extras. | En uso: `Repository.cs`, `ExtraRepository.cs` |
| `Interface/` | Contratos (interfaces) que definen qué puede hacer cada repositorio. Los controllers dependen de estas interfaces, no de las implementaciones. | En uso: `IRepository.cs`, `IExtraRepository.cs` |
| `Migrations/` | Archivos de migraciones de EF Core. Cada uno describe un cambio en el esquema de la BD (crear/modificar tablas). | En uso: migración inicial del esquema |
| **`Helpers/`** | Utilidades compartidas de la capa Data (extensiones, métodos auxiliares, etc.). | Vacía por ahora; reservada para futuro. |
| **`Exceptions/`** | Excepciones propias de la capa Data cuando algo falle (ej: entidad no encontrada, conflicto de datos). | Vacía por ahora; reservada para futuro. |
| **`Filters/`** | Filtros o abstracciones para consultas complejas (ej: paginación, filtros reutilizables). | Vacía por ahora; reservada para futuro. |
| **`AuthorizationPolicies/`** | Políticas de autorización ligadas a la capa de datos (si en el futuro se necesitan aquí). | Vacía por ahora; reservada para futuro. |

#### Archivos clave

| Archivo | ¿Qué hace? |
|---------|-------------|
| `ArcanoPizzaDbContext.cs` | Define las tablas (`DbSet<>`), el mapeo y las relaciones. Es el "puente" entre el código y PostgreSQL. |
| `PostgresConfiguration.cs` | Registra el DbContext con la cadena de conexión y los repositorios. Se llama desde `Program.cs` con `AddData()`. |
| `PostgresConnectionString.cs` | Convierte una URL tipo `postgresql://user:pass@host/db` al formato que espera Npgsql. |

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
dotnet user-secrets init
Se añadirá un UserSecretsId en el .csproj y se creará la carpeta para los secretos.

Paso 4: Guardar la cadena de conexión
Sustituye con tu cadena real de PostgreSQL:
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "postgresql://TU_USUARIO:TU_PASSWORD@TU_HOST:5432/TU_BASE_DE_DATOS?sslmode=require"

Paso 5: Comprobar que se guardó
dotnet user-secrets list
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

| Convención | Descripción |
|------------|-------------|
| **No versionar secretos** | Usar User Secrets o `DATABASE_URL`; nunca credenciales en `appsettings.json` subido a Git. |
| **Controllers** | Se encargan de HTTP, validación básica y mapeo entre DTOs y entidades. |
| **Repositories** | Todo el acceso a datos pasa por repositorios; los controllers no usan DbContext directamente. |
| **Fechas** | Usar `DateTime.UtcNow` para consistencia. |
| **DTOs** | La API no expone entidades directamente; siempre mapear a DTOs. |

---

## Seguridad: OWASP Top 10:2025

La API aplica medidas para cubrir las vulnerabilidades del [OWASP Top 10:2025](https://owasp.org/Top10/2025/):

| Categoría OWASP | Medidas implementadas |
|-----------------|------------------------|
| **A01 - Broken Access Control** | Estructura JWT configurada. Aplicar `[Authorize]` en endpoints sensibles cuando el login esté implementado. |
| **A02 - Security Misconfiguration** | Cabeceras de seguridad (X-Frame-Options, X-Content-Type-Options, CSP, HSTS), Swagger solo en desarrollo. |
| **A03 - Software Supply Chain** | Usar paquetes NuGet oficiales, mantener dependencias actualizadas, revisar alertas de Dependabot. |
| **A04 - Cryptographic Failures** | HTTPS forzado, `sslmode=require` en PostgreSQL. Para contraseñas: usar bcrypt/Argon2 (pendiente en módulo Usuario). |
| **A05 - Injection** | EF Core con consultas parametrizadas, validación en DTOs (`[Required]`, `[MaxLength]`, `[Range]`), rate limiting. |
| **A06 - Insecure Design** | Arquitectura en capas, separación API/Data/Model/DTOs, principio de mínimo privilegio. |
| **A07 - Authentication Failures** | JWT preparado (activar con `Jwt:Key`). Rate limiting en endpoints de auth. |
| **A08 - Software/Data Integrity** | No usar scripts o paquetes sin firmar. Configurar verificación de integridad en CI/CD. |
| **A09 - Security Logging** | Excepciones logueadas con `TraceId` sin datos sensibles. Extensible a auditoría de accesos. |
| **A10 - Mishandling of Exceptions** | Manejador global de excepciones: respuestas genéricas en producción, sin stack traces al cliente. |

### Configurar JWT (cuando implementes login)

```bash
dotnet user-secrets set "Jwt:Key" "tu-clave-secreta-de-al-menos-32-caracteres"
```

---

## Glosario rápido

- **CRUD**: Create, Read, Update, Delete (crear, leer, actualizar, eliminar).
- **EF Core**: Entity Framework Core, ORM para .NET que mapea objetos a tablas de la BD.
- **Inyección de dependencias**: El framework crea y pasa las dependencias (repositorios, DbContext) automáticamente a los controllers.
- **Npgsql**: Proveedor de PostgreSQL para .NET.
- **ORM**: Object-Relational Mapping; traduce entre objetos en código y filas en la BD.
