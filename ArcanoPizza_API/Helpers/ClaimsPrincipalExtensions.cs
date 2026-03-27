using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ArcanoPizza_API.Helpers;

internal static class ClaimsPrincipalExtensions
{
    public static int GetUsuarioId(this ClaimsPrincipal user)
    {
        var v = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(v) || !int.TryParse(v, out var id))
            throw new InvalidOperationException("Token sin identificador de usuario.");
        return id;
    }
}
