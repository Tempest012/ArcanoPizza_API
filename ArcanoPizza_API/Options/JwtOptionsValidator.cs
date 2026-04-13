using Microsoft.Extensions.Options;

namespace ArcanoPizza_API.Options;

public sealed class JwtOptionsValidator : IValidateOptions<JwtOptions>
{
    public ValidateOptionsResult Validate(string? name, JwtOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Issuer))
            return ValidateOptionsResult.Fail("Jwt:Issuer es obligatorio.");
        if (string.IsNullOrWhiteSpace(options.Audience))
            return ValidateOptionsResult.Fail("Jwt:Audience es obligatorio.");
        if (string.IsNullOrWhiteSpace(options.SigningKey) || options.SigningKey.Length < 32)
            return ValidateOptionsResult.Fail("Jwt:SigningKey debe tener al menos 32 caracteres.");
        if (options.AccessTokenMinutes < 1)
            return ValidateOptionsResult.Fail("Jwt:AccessTokenMinutes debe ser >= 1.");
        if (options.RefreshTokenDays < 1)
            return ValidateOptionsResult.Fail("Jwt:RefreshTokenDays debe ser >= 1.");

        return ValidateOptionsResult.Success;
    }
}
