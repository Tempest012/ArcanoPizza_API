using ArcanoPizza_API.Model;

namespace ArcanoPizza_API.Data.Interface;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> GetByCorreoNormalizedAsync(string correo, CancellationToken ct = default);
}
