using ArcanoPizza_API.DTOs;

namespace ArcanoPizza_API.Data.IServices;

public interface IAuthService
{
    Task<AuthOutcome> RegisterAsync(RegisterRequest dto, CancellationToken ct);
    Task<AuthOutcome> LoginAsync(LoginRequest dto, CancellationToken ct);
    Task<AuthOutcome> RefreshAsync(RefreshRequest dto, CancellationToken ct);
    Task<AuthOutcome> LogoutAsync(RefreshRequest dto, CancellationToken ct);
}
