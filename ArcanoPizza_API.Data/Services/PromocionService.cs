using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.Data.IServices;
using ArcanoPizza_API.DTOs;
using ArcanoPizza_API.Model;
using Microsoft.EntityFrameworkCore;

namespace ArcanoPizza_API.Data.Services;

public class PromocionService : IPromocionService
{
    private readonly IPromocionRepository _promociones;
    private readonly ArcanoPizzaDbContext _db;

    public PromocionService(IPromocionRepository promociones, ArcanoPizzaDbContext db)
    {
        _promociones = promociones;
        _db = db;
    }

    public async Task<IReadOnlyList<PromocionResponseDto>> GetActivasAsync(CancellationToken ct)
    {
        var lista = await _promociones.FindAsync(p => p.Activo, ct);
        return lista
            .OrderBy(p => p.IdPromocion)
            .Select(ToDto)
            .ToList();
    }

    public async Task<IReadOnlyList<PromocionResponseDto>> GetAllAdminAsync(CancellationToken ct)
    {
        var lista = await _promociones.FindAsync(_ => true, ct);
        return lista
            .OrderBy(p => p.IdPromocion)
            .Select(ToDto)
            .ToList();
    }

    public async Task<PromocionResponseDto?> GetByIdActivaAsync(int id, CancellationToken ct)
    {
        var p = await _promociones.GetByIdAsync(id, ct);
        if (p is null || !p.Activo) return null;
        return ToDto(p);
    }

    public async Task<(PromocionResponseDto? Created, string? Error)> CreateAsync(PromocionCreateDto dto, CancellationToken ct)
    {
        if (!Enum.IsDefined(typeof(TipoVigenciaPromocion), dto.TipoVigencia))
            return (null, "TipoVigencia no válido.");

        var tipo = (TipoVigenciaPromocion)dto.TipoVigencia;
        if (tipo == TipoVigenciaPromocion.FechaHasta && !dto.FechaValidaHasta.HasValue)
            return (null, "FechaValidaHasta es requerida para vigencia por fecha.");
        if (tipo == TipoVigenciaPromocion.DiaSemanaRecurrente && dto.DiaSemanaRecurrente is null)
            return (null, "DiaSemanaRecurrente es requerido para promoción recurrente.");

        var ahora = DateTime.UtcNow;

        // Crear (o reutilizar) la categoría "Promociones" para el producto combo vendible.
        var categoria = await _db.CategoriasProducto.FirstOrDefaultAsync(c => c.Nombre == "Promociones", ct);
        if (categoria is null)
        {
            categoria = new CategoriaProducto
            {
                Nombre = "Promociones",
                CreatedAt = ahora,
                UpdatedAt = ahora,
            };
            _db.CategoriasProducto.Add(categoria);
            await _db.SaveChangesAsync(ct);
        }

        var productoCombo = new Producto
        {
            Nombre = dto.Titulo,
            Descripcion = dto.Descripcion,
            Ingredientes = dto.Contenido,
            ImagenURL = dto.ImagenURL,
            PrecioBase = dto.PrecioPromocional,
            Activo = dto.Activo,
            FkIdCategoria = categoria.IdCategoriasProductos,
            CreatedAt = ahora,
            UpdatedAt = ahora,
        };
        _db.Productos.Add(productoCombo);
        await _db.SaveChangesAsync(ct);

        var entity = new Promocion
        {
            Titulo = dto.Titulo,
            Descripcion = dto.Descripcion,
            Contenido = dto.Contenido,
            ImagenURL = dto.ImagenURL,
            PrecioOriginal = dto.PrecioOriginal,
            PrecioPromocional = dto.PrecioPromocional,
            TipoVigencia = tipo,
            FechaValidaHasta = tipo == TipoVigenciaPromocion.FechaHasta ? dto.FechaValidaHasta : null,
            DiaSemanaRecurrente = tipo == TipoVigenciaPromocion.DiaSemanaRecurrente ? dto.DiaSemanaRecurrente : null,
            Activo = dto.Activo,
            CreatedAt = ahora,
            UpdatedAt = ahora,
            FkIdProductoCombo = productoCombo.IdProducto,
        };

        var created = await _promociones.AddAsync(entity, ct);
        return (ToDto(created), null);
    }

    public async Task<(bool Found, string? Error)> UpdateAsync(int id, PromocionUpdateDto dto, CancellationToken ct)
    {
        var p = await _promociones.GetByIdAsync(id, ct);
        if (p is null) return (false, null);

        var error = AplicarActualizacionParcial(p, dto);
        if (error is not null) return (true, error);

        p.UpdatedAt = DateTime.UtcNow;
        await _promociones.UpdateAsync(p, ct);

        // Si existe producto combo asociado, mantenerlo sincronizado.
        if (p.FkIdProductoCombo is { } prodId && prodId > 0)
        {
            var prod = await _db.Productos.FirstOrDefaultAsync(x => x.IdProducto == prodId, ct);
            if (prod is not null)
            {
                if (dto.Titulo is not null) prod.Nombre = dto.Titulo;
                if (dto.Descripcion is not null) prod.Descripcion = dto.Descripcion;
                if (dto.Contenido is not null) prod.Ingredientes = dto.Contenido;
                if (dto.ImagenURL is not null) prod.ImagenURL = dto.ImagenURL;
                if (dto.PrecioPromocional.HasValue) prod.PrecioBase = dto.PrecioPromocional.Value;
                if (dto.Activo.HasValue) prod.Activo = dto.Activo.Value;
                prod.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync(ct);
            }
        }

        return (true, null);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var p = await _promociones.GetByIdAsync(id, ct);
        if (p is null) return false;

        // No eliminar producto combo (puede estar referenciado por pedidos). Solo desactivarlo.
        if (p.FkIdProductoCombo is { } prodId && prodId > 0)
        {
            var prod = await _db.Productos.FirstOrDefaultAsync(x => x.IdProducto == prodId, ct);
            if (prod is not null)
            {
                prod.Activo = false;
                prod.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync(ct);
            }
        }

        await _promociones.DeleteAsync(p, ct);
        return true;
    }

    private static string? AplicarActualizacionParcial(Promocion p, PromocionUpdateDto dto)
    {
        if (dto.Titulo is not null) p.Titulo = dto.Titulo;
        if (dto.Descripcion is not null) p.Descripcion = dto.Descripcion;
        if (dto.Contenido is not null) p.Contenido = dto.Contenido;
        if (dto.ImagenURL is not null) p.ImagenURL = dto.ImagenURL;
        if (dto.PrecioOriginal.HasValue) p.PrecioOriginal = dto.PrecioOriginal.Value;
        if (dto.PrecioPromocional.HasValue) p.PrecioPromocional = dto.PrecioPromocional.Value;

        if (dto.TipoVigencia.HasValue)
        {
            if (!Enum.IsDefined(typeof(TipoVigenciaPromocion), dto.TipoVigencia.Value))
                return "TipoVigencia no válido.";
            p.TipoVigencia = (TipoVigenciaPromocion)dto.TipoVigencia.Value;
        }

        if (dto.FechaValidaHasta.HasValue) p.FechaValidaHasta = dto.FechaValidaHasta;
        if (dto.DiaSemanaRecurrente.HasValue) p.DiaSemanaRecurrente = dto.DiaSemanaRecurrente;
        if (dto.Activo.HasValue) p.Activo = dto.Activo.Value;

        return null;
    }

    private static PromocionResponseDto ToDto(Promocion p)
    {
        var ahorro = p.PrecioOriginal >= p.PrecioPromocional
            ? p.PrecioOriginal - p.PrecioPromocional
            : 0m;

        return new PromocionResponseDto(
            p.IdPromocion,
            p.FkIdProductoCombo,
            p.Titulo,
            p.Descripcion,
            p.Contenido,
            p.ImagenURL,
            p.PrecioOriginal,
            p.PrecioPromocional,
            ahorro,
            (int)p.TipoVigencia,
            p.FechaValidaHasta,
            p.DiaSemanaRecurrente,
            p.Activo);
    }
}

