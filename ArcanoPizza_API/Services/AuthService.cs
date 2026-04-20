using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.Data.IServices;
using ArcanoPizza_API.DTOs;
using ArcanoPizza_API.Model;
using ArcanoPizza_API.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ArcanoPizza_API.Services;

public class AuthService : IAuthService
{
    private const string DefaultRol = "Cliente";

    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher<Usuario> _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        IUsuarioRepository usuarioRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher<Usuario> passwordHasher,
        IJwtTokenService jwtTokenService,
        IOptions<JwtOptions> jwtOptions)
    {
        _usuarioRepository = usuarioRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthOutcome> RegisterAsync(RegisterRequest dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.NombreUsuario)
            || string.IsNullOrWhiteSpace(dto.Correo)
            || string.IsNullOrWhiteSpace(dto.Password))
        {
            return new AuthOutcome { StatusCode = 400 };
        }

        var correo = dto.Correo.Trim().ToLowerInvariant();
        if (await _usuarioRepository.GetByCorreoNormalizedAsync(correo, ct) is not null)
            return new AuthOutcome { StatusCode = 409 };

        var now = DateTime.UtcNow;
        var usuario = new Usuario
        {
            NombreUsuario = dto.NombreUsuario.Trim(),
            Correo = correo,
            Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim(),
            Rol = DefaultRol,
            CreatedAt = now,
            UpdatedAt = now,
        };
        usuario.PasswordHash = _passwordHasher.HashPassword(usuario, dto.Password);

        usuario = await _usuarioRepository.AddAsync(usuario, ct);

        var (refreshRaw, refreshHash) = RefreshTokenHasher.GeneratePair();
        var refreshEntity = new RefreshToken
        {
            FkIdUsuario = usuario.IdUsuario,
            TokenHash = refreshHash,
            ExpiresAt = now.AddDays(_jwtOptions.RefreshTokenDays),
            CreatedAt = now,
        };
        await _refreshTokenRepository.AddAsync(refreshEntity, ct);

        return new AuthOutcome
        {
            StatusCode = 200,
            Response = BuildAuthResponse(usuario, refreshRaw),
        };
    }

    public async Task<AuthOutcome> LoginAsync(LoginRequest dto, CancellationToken ct)
    {
        var correo = (dto.Correo ?? dto.Email)?.Trim();
        if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return new AuthOutcome
            {
                StatusCode = 400,
                Body = new { mensaje = "Correo y contraseña son obligatorios." },
            };
        }

        var usuario = await _usuarioRepository.GetByCorreoNormalizedAsync(correo, ct);
        if (usuario?.PasswordHash is null)
            return new AuthOutcome { StatusCode = 401 };

        var verify = _passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, dto.Password);
        if (verify == PasswordVerificationResult.Failed)
            return new AuthOutcome { StatusCode = 401 };

        var now = DateTime.UtcNow;
        var (refreshRaw, refreshHash) = RefreshTokenHasher.GeneratePair();
        var refreshEntity = new RefreshToken
        {
            FkIdUsuario = usuario.IdUsuario,
            TokenHash = refreshHash,
            ExpiresAt = now.AddDays(_jwtOptions.RefreshTokenDays),
            CreatedAt = now,
        };
        await _refreshTokenRepository.AddAsync(refreshEntity, ct);

        return new AuthOutcome
        {
            StatusCode = 200,
            Response = BuildAuthResponse(usuario, refreshRaw),
        };
    }

    public async Task<AuthOutcome> RefreshAsync(RefreshRequest dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.RefreshToken))
            return new AuthOutcome { StatusCode = 400 };

        var hash = RefreshTokenHasher.Sha256Hex(dto.RefreshToken);
        var existing = await _refreshTokenRepository.GetActiveWithUsuarioByTokenHashAsync(hash, ct);
        if (existing is null)
            return new AuthOutcome { StatusCode = 401 };

        var now = DateTime.UtcNow;
        var (refreshRaw, refreshHash) = RefreshTokenHasher.GeneratePair();
        var newEntity = new RefreshToken
        {
            FkIdUsuario = existing.FkIdUsuario,
            TokenHash = refreshHash,
            ExpiresAt = now.AddDays(_jwtOptions.RefreshTokenDays),
            CreatedAt = now,
        };

        await _refreshTokenRepository.RotateAsync(existing, newEntity, ct);

        return new AuthOutcome
        {
            StatusCode = 200,
            Response = BuildAuthResponse(existing.Usuario, refreshRaw),
        };
    }

    public async Task<AuthOutcome> LogoutAsync(RefreshRequest dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.RefreshToken))
            return new AuthOutcome { StatusCode = 400 };

        var hash = RefreshTokenHasher.Sha256Hex(dto.RefreshToken);
        var existing = await _refreshTokenRepository.GetActiveWithUsuarioByTokenHashAsync(hash, ct);
        if (existing is null)
            return new AuthOutcome { StatusCode = 204 };

        existing.RevokedAt = DateTime.UtcNow;
        await _refreshTokenRepository.UpdateAsync(existing, ct);
        return new AuthOutcome { StatusCode = 204 };
    }

    private AuthResponse BuildAuthResponse(Usuario usuario, string refreshRaw)
    {
        var access = _jwtTokenService.CreateAccessToken(usuario);
        return new AuthResponse(
            access,
            refreshRaw,
            _jwtTokenService.GetAccessTokenLifetimeSeconds(),
            new AuthUserDto(usuario.IdUsuario, usuario.NombreUsuario, usuario.Correo, usuario.Rol));
    }
}
