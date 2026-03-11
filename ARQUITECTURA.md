# ArcanoPizza API — Arquitectura y uso

Este documento describe la arquitectura del backend, la estructura de carpetas/proyectos y cómo configurarlo para ejecutarlo de forma segura.

## Visión general

La solución está separada por **capas** (proyectos .NET):

- **`ArcanoPizza_API`**: capa **Web API** (entrada HTTP, controllers, wiring en `Program.cs`).
- **`ArcanoPizza_API.Data`**: capa de **persistencia** (EF Core `DbContext`, repositorios, migraciones, configuración de base de datos).
- **`ArcanoPizza_API.Model`**: capa de **dominio** (entidades/modelos del negocio).
- **`ArcanoPizza_API.DTOs`**: capa de **contratos** (DTOs para request/response).

La API sigue el patrón típico:

`HTTP request` → `Controller` → `Repository` → `DbContext (EF Core)` → `PostgreSQL`

## Cómo se configura el acceso a base de datos (seguro)

La aplicación toma la cadena de conexión en este orden:

1. Variable de entorno **`DATABASE_URL`** (recomendado para no versionar secretos).
2. `ConnectionStrings:DefaultConnection` en `appsettings.json` (debe evitarse con credenciales reales).

La integración está en `ArcanoPizza_API.Data/SqlServerConfiguration.cs` (el nombre histórico se mantiene, pero ya usa PostgreSQL/Npgsql).

### Ejemplo (PowerShell)

Configurar para la sesión actual:

```powershell
$env:DATABASE_URL = "postgresql://usuario:password@host/db?sslmode=require"
```

Persistir para tu usuario (Windows):

```powershell
setx DATABASE_URL "postgresql://usuario:password@host/db?sslmode=require"
```

## Cómo ejecutar (cuando tengas .NET 10)

Requisitos:

- **.NET SDK 10** instalado (los proyectos apuntan a `net10.0`).
- Acceso a PostgreSQL (por ejemplo Neon) y `DATABASE_URL` configurada.

Comandos típicos:

```bash
dotnet restore
dotnet run --project ArcanoPizza_API/ArcanoPizza_API.csproj
```

## Estructura del repositorio

En la raíz:

- **`ArcanoPizza_API/`**: proyecto Web API.
- **`ArcanoPizza_API.Data/`**: proyecto de persistencia (EF Core).
- **`ArcanoPizza_API.Model/`**: entidades del dominio.
- **`ArcanoPizza_API.DTOs/`**: DTOs (contratos de entrada/salida).
- **`Data/`**: carpeta adicional (si no se usa, conviene eliminarla o documentar su propósito cuando se defina).
- **`WebApplication1/`**: proyecto adicional (probable plantilla/experimento; si no se usa, conviene removerlo del repo o dejar claro su objetivo).
- **`ArcanoPizza_API.slnx`**: lista de proyectos de la solución.

## Proyecto `ArcanoPizza_API` (Web API)

Contenido típico:

- **`Program.cs`**
  - Construye la app (`WebApplication.CreateBuilder`).
  - Registra dependencias con `builder.Services.AddData(builder.Configuration)`.
  - Habilita Controllers y OpenAPI en desarrollo.
- **`Controllers/`**
  - Endpoints HTTP. Ejemplo: `ProductosController` expone CRUD de productos.
- **`appsettings.json`**
  - Configuración base. `DefaultConnection` está vacío por seguridad; se recomienda `DATABASE_URL`.
- **`appsettings.Development.json`**
  - Configuración solo para desarrollo (logging, etc.).

### Ejemplo de flujo real (Productos)

- `ProductosController` depende de `IProductoRepository`.
- La implementación `ProductoRepository` hereda de `Repository<Producto>`.
- `Repository<T>` opera con `DbSet<T>` y persiste con `SaveChangesAsync`.

Esto mantiene el controller enfocado en HTTP/DTOs y la capa Data enfocada en acceso a datos.

## Proyecto `ArcanoPizza_API.Data` (Persistencia)

Carpetas principales:

- **`Migrations/`**
  - Migraciones de EF Core (actualmente adaptadas a **PostgreSQL/Npgsql**).
- **`Repositories/`**
  - Implementaciones de repositorios.
  - `Repository<T>` es un repositorio genérico (GetById/GetAll/Find/Add/Update/Delete).
  - `ProductoRepository` es un repositorio concreto para `Producto`.
- **`Interface/`**
  - Contratos de repositorios (`IRepository<T>`, `IProductoRepository`).
- **`Helpers/`**
  - Utilidades compartidas de la capa Data.
  - Actualmente contiene `.gitkeep` (carpeta reservada para helpers futuros).
- **`Exceptions/`**
  - Excepciones específicas de la capa Data (cuando se usen).
- **`Filters/`**
  - Filtros/abstracciones para queries (cuando se definan).
- **`AuthorizationPolicies/`**
  - Políticas relacionadas a autorización (si se implementan en esta capa; si crece, puede moverse a la Web API).

Archivos clave:

- **`ArcanoPizzaDbContext.cs`**
  - Define `DbSet<>` y el mapeo (tablas, longitudes, precision, relaciones).
- **`SqlServerConfiguration.cs`**
  - Registra `ArcanoPizzaDbContext` con `UseNpgsql(...)`.
  - Toma la cadena desde `DATABASE_URL` o `DefaultConnection`.
- **`PostgresConnectionString.cs`**
  - Normaliza `DATABASE_URL` tipo `postgresql://...` a un connection string compatible.

## Proyecto `ArcanoPizza_API.Model` (Dominio)

Contiene las **entidades** del negocio (por ejemplo `Producto`, `Usuario`, `Pedido`, etc.).  
Estas clases representan el estado y relaciones del dominio; EF Core las usa desde `ArcanoPizzaDbContext`.

## Proyecto `ArcanoPizza_API.DTOs` (Contratos)

Contiene DTOs para:

- **Requests** (crear/actualizar).
- **Responses** (lo que devuelve la API).

Esto evita exponer directamente las entidades del dominio y te permite versionar el contrato con control.

## Convenciones recomendadas

- **No versionar secretos**: usar `DATABASE_URL` (ya se ignoran `.env` y `appsettings.*.json`).
- **Separación por capas**:
  - Controllers: HTTP, validación básica y mapeo DTO ↔ modelo.
  - Repositorios: acceso a datos.
  - Model: entidades.
  - DTOs: contratos.
- **Fechas**: se usa `DateTime.UtcNow` en controllers (consistente para un backend).

