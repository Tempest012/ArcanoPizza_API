using ArcanoPizza_API.Data.IServices;
using ArcanoPizza_API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArcanoPizza_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest dto, CancellationToken ct)
    {
        var outcome = await _auth.RegisterAsync(dto, ct);
        return outcome.StatusCode switch
        {
            200 => Ok(outcome.Response!),
            400 => BadRequest(),
            409 => Conflict(),
            _ => Problem(),
        };
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest dto, CancellationToken ct)
    {
        var outcome = await _auth.LoginAsync(dto, ct);
        return outcome.StatusCode switch
        {
            200 => Ok(outcome.Response!),
            400 => BadRequest(outcome.Body),
            401 => Unauthorized(),
            _ => Problem(),
        };
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshRequest dto, CancellationToken ct)
    {
        var outcome = await _auth.RefreshAsync(dto, ct);
        return outcome.StatusCode switch
        {
            200 => Ok(outcome.Response!),
            400 => BadRequest(),
            401 => Unauthorized(),
            _ => Problem(),
        };
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<ActionResult> Logout([FromBody] RefreshRequest dto, CancellationToken ct)
    {
        var outcome = await _auth.LogoutAsync(dto, ct);
        return outcome.StatusCode switch
        {
            400 => BadRequest(),
            204 => NoContent(),
            _ => Problem(),
        };
    }
}
