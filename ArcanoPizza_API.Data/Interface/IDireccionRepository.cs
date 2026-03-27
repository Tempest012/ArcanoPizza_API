using ArcanoPizza_API.Model;

namespace ArcanoPizza_API.Data.Interface;

public interface IDireccionRepository
{
    Task<IReadOnlyList<Direccion>> GetByUsuarioAsync(int idUsuario, CancellationToken ct = default);
    Task<Direccion?> GetByIdForUsuarioAsync(int idDireccion, int idUsuario, CancellationToken ct = default);
    Task<Direccion> AddAsync(Direccion entity, CancellationToken ct = default);
}
