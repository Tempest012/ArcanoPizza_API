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
| `ARQUITECTURA.md` | Este documento. |
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

### Origen de la cadena (en este orden)

1. **Variable de entorno `DATABASE_URL`** (recomendado).
2. **User Secrets** (clave `ConnectionStrings:DefaultConnection`).
3. **`appsettings.json`** (solo para desarrollo local, sin credenciales reales en Git).

### Opción recomendada: User Secrets

 Cada desarrollador configura sus propios secretos en su máquina:

```powershell
# Desde la raíz del repositorio
dotnet user-secrets init --project ArcanoPizza_API
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "postgresql://usuario:password@host:5432/nombre_db?sslmode=require" --project ArcanoPizza_API
```

Reemplaza `usuario`, `password`, `host` y `nombre_db` con tus datos.

### Alternativa: variable de entorno (PowerShell)

```powershell
$env:DATABASE_URL = "postgresql://usuario:password@host:5432/nombre_db?sslmode=require"
```

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

## Glosario rápido

- **CRUD**: Create, Read, Update, Delete (crear, leer, actualizar, eliminar).
- **EF Core**: Entity Framework Core, ORM para .NET que mapea objetos a tablas de la BD.
- **Inyección de dependencias**: El framework crea y pasa las dependencias (repositorios, DbContext) automáticamente a los controllers.
- **Npgsql**: Proveedor de PostgreSQL para .NET.
- **ORM**: Object-Relational Mapping; traduce entre objetos en código y filas en la BD.
