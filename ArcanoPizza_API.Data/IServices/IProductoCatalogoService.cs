using ArcanoPizza_API.DTOs;

namespace ArcanoPizza_API.Data.IServices;

public interface IProductoCatalogoService
{
    /// <returns>Lista de productos activos o null si hubo error de persistencia.</returns>
    Task<IReadOnlyList<ProductoDto>?> GetActivosAsync(CancellationToken ct);
}
