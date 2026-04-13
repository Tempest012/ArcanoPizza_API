using ArcanoPizza_API.Model;

namespace ArcanoPizza_API.Services;

public interface IJwtTokenService
{
    string CreateAccessToken(Usuario usuario);
    int GetAccessTokenLifetimeSeconds();
}
