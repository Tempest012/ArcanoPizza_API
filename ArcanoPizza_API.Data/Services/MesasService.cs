using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.Data.IServices;
using ArcanoPizza_API.DTOs;
using ArcanoPizza_API.Model;

namespace ArcanoPizza_API.Data.Services;

public class MesasService : IMesasService
{
    private readonly IMesaRepository _mesas;

    public MesasService(IMesaRepository mesas)
    {
        _mesas = mesas;
    }

    public async Task<IReadOnlyList<MesaDto>> ListarAsync(CancellationToken ct = default)
    {
        var lista = await _mesas.GetAllAsync(ct);
        return lista.Select(Map).ToList();
    }

    public async Task<MesaDto?> ObtenerAsync(int id, CancellationToken ct = default)
    {
        var mesa = await _mesas.GetByIdAsync(id, ct);
        return mesa is null ? null : Map(mesa);
    }

    public async Task<(MesaDto? Mesa, string? Error, int Status)> CrearAsync(MesaCrearDto dto, CancellationToken ct = default)
    {
        if (dto.Numero <= 0)
            return (null, "El número de mesa debe ser mayor a cero.", 400);

        var existente = await _mesas.GetByNumeroAsync(dto.Numero, ct);
        if (existente is not null)
            return (null, $"Ya existe la mesa número {dto.Numero}.", 400);

        string estado;
        try
        {
            estado = SalonEstados.NormalizarEstadoMesa(dto.Estado);
        }
        catch (ArgumentException ex)
        {
            return (null, ex.Message, 400);
        }

        var now = DateTime.UtcNow;
        var creada = await _mesas.CrearAsync(new Mesa
        {
            Numero = dto.Numero,
            Estado = estado,
            CreatedAt = now,
            UpdatedAt = now,
        }, ct);

        return (Map(creada), null, 201);
    }

    public async Task<(bool Ok, string? Error, int Status)> CambiarEstadoAsync(int id, string estado, CancellationToken ct = default)
    {
        var mesa = await _mesas.GetByIdAsync(id, ct);
        if (mesa is null)
            return (false, "Mesa no encontrada.", 404);

        string normalizado;
        try
        {
            normalizado = SalonEstados.NormalizarEstadoMesa(estado);
        }
        catch (ArgumentException ex)
        {
            return (false, ex.Message, 400);
        }

        mesa.Estado = normalizado;
        await _mesas.ActualizarAsync(mesa, ct);
        return (true, null, 200);
    }

    public async Task<(bool Ok, string? Error, int Status)> EliminarAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var ok = await _mesas.EliminarAsync(id, ct);
            return ok ? (true, null, 204) : (false, "Mesa no encontrada.", 404);
        }
        catch (InvalidOperationException ex)
        {
            return (false, ex.Message, 400);
        }
    }

    private static MesaDto Map(Mesa m) => new(m.IdMesa, m.Numero, m.Estado);
}
