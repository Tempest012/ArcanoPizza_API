using ArcanoPizza_API.Model;

namespace ArcanoPizza_API.Data.Interface;

public interface IMesaRepository
{
    Task<IReadOnlyList<Mesa>> GetAllAsync(CancellationToken ct = default);
    Task<Mesa?> GetByIdAsync(int idMesa, CancellationToken ct = default);
    Task<Mesa?> GetByNumeroAsync(int numero, CancellationToken ct = default);
    Task<Mesa> CrearAsync(Mesa mesa, CancellationToken ct = default);
    Task ActualizarAsync(Mesa mesa, CancellationToken ct = default);
    Task<bool> EliminarAsync(int idMesa, CancellationToken ct = default);
}
