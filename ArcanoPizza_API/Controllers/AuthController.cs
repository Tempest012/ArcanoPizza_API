using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.DTOs;
using ArcanoPizza_API.Model;
using ArcanoPizza_API.Options;
using ArcanoPizza_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ArcanoPizza_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private const string DefaultRol = "Cliente";

    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher<Usuario> _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly JwtOptions _jwtOptions;

    public AuthController(
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

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.NombreUsuario)
            || string.IsNullOrWhiteSpace(dto.Correo)
            || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest();

        var correo = dto.Correo.Trim().ToLowerInvariant();
        if (await _usuarioRepository.GetByCorreoNormalizedAsync(correo, ct) is not null)
            return Conflict();

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

        return Ok(BuildAuthResponse(usuario, refreshRaw));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest dto, CancellationToken ct)
    {
        var correo = (dto.Correo ?? dto.Email)?.Trim();
        if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { mensaje = "Correo y contraseña son obligatorios." });

        var usuario = await _usuarioRepository.GetByCorreoNormalizedAsync(correo, ct);
        if (usuario?.PasswordHash is null)
            return Unauthorized();

        var verify = _passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, dto.Password);
        if (verify == PasswordVerificationResult.Failed)
            return Unauthorized();

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

        return Ok(BuildAuthResponse(usuario, refreshRaw));
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshRequest dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.RefreshToken))
            return BadRequest();

        var hash = RefreshTokenHasher.Sha256Hex(dto.RefreshToken);
        var existing = await _refreshTokenRepository.GetActiveWithUsuarioByTokenHashAsync(hash, ct);
        if (existing is null)
            return Unauthorized();

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

        return Ok(BuildAuthResponse(existing.Usuario, refreshRaw));
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<ActionResult> Logout([FromBody] RefreshRequest dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.RefreshToken))
            return BadRequest();

        var hash = RefreshTokenHasher.Sha256Hex(dto.RefreshToken);
        var existing = await _refreshTokenRepository.GetActiveWithUsuarioByTokenHashAsync(hash, ct);
        if (existing is null)
            return NoContent();

        existing.RevokedAt = DateTime.UtcNow;
        await _refreshTokenRepository.UpdateAsync(existing, ct);
        return NoContent();
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
