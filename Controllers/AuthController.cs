using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TruequeU.Authorization;
using TruequeU.Configuration;
using TruequeU.Enums;
using TruequeU.Models;
using TruequeU.Models.DTOs;

namespace TruequeU.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private const string AuthCookieName = "auth_token";

    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthController> _logger;
    private readonly IWebHostEnvironment _environment;

    public AuthController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AuthController> logger,
        IWebHostEnvironment environment)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        _jwtSettings = jwtSettings?.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        _logger.LogDebug("Register attempt for email {Email}", dto.Email);

        var user = new User
        {
            UserName = dto.UserName.Trim(),
            Email = dto.Email.Trim(),
            FullName = dto.FullName,
            State = UserState.Active,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password).ConfigureAwait(false);

        if (!result.Succeeded)
        {
            _logger.LogWarning("Registration failed for {Email}: {Errors}", dto.Email, result.Errors);

            var duplicateEmail = result.Errors.FirstOrDefault(e => e.Code == "DuplicateEmail");
            if (duplicateEmail is not null)
                return Conflict(new { error = "Email is already taken." });

            var duplicateUserName = result.Errors.FirstOrDefault(e => e.Code == "DuplicateUserName");
            if (duplicateUserName is not null)
                return Conflict(new { error = "Username is already taken." });

            var firstError = result.Errors.FirstOrDefault();
            return BadRequest(new { error = firstError?.Description ?? "Registration failed." });
        }

        await _userManager.AddToRoleAsync(user, RoleConstants.User).ConfigureAwait(false);

        var token = await GenerateJwtTokenAsync(user).ConfigureAwait(false);
        SetAuthCookie(token);

        _logger.LogInformation("User {UserId} registered successfully", user.Id);

        return Ok(new AuthResponseDto
        {
            Expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpireMinutes),
            UserId = user.Id,
            UserName = user.UserName!,
            Email = user.Email!
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        _logger.LogDebug("Login attempt for email {Email}", dto.Email);

        var user = await _userManager.FindByEmailAsync(dto.Email.Trim()).ConfigureAwait(false);

        if (user is null)
        {
            _logger.LogWarning("Login failed: user not found for email {Email}", dto.Email);
            return Unauthorized(new { error = "Credenciales inválidas." });
        }

        if (user.State == UserState.Suspended)
        {
            _logger.LogWarning("Login refused: user {UserId} is suspended", user.Id);
            return Unauthorized(new { error = "Tu cuenta ha sido suspendida." });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(
            user, dto.Password, lockoutOnFailure: false).ConfigureAwait(false);

        if (!result.Succeeded)
        {
            _logger.LogWarning("Login failed: invalid password for user {UserId}", user.Id);
            return Unauthorized(new { error = "Credenciales inválidas." });
        }

        user.LastLogin = DateTime.UtcNow;
        await _userManager.UpdateAsync(user).ConfigureAwait(false);

        var token = await GenerateJwtTokenAsync(user).ConfigureAwait(false);
        SetAuthCookie(token);

        _logger.LogInformation("User {UserId} logged in successfully", user.Id);

        return Ok(new AuthResponseDto
        {
            Expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpireMinutes),
            UserId = user.Id,
            UserName = user.UserName!,
            Email = user.Email!
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(AuthCookieName);
        return Ok(new { message = "Logged out successfully." });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserReadDto>> GetCurrentUser()
    {
        var userId = GetCurrentUserId();

        var user = await _userManager.FindByIdAsync(userId.ToString()).ConfigureAwait(false);

        if (user is null)
            return NotFound();

        return Ok(new UserReadDto
        {
            Id = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            FullName = user.FullName,
            Program = user.Program,
            Bio = user.Bio,
            Rating = user.Rating
        });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
    {
        _logger.LogDebug("Forgot password request for email {Email}", dto.Email);

        var user = await _userManager.FindByEmailAsync(dto.Email.Trim()).ConfigureAwait(false);

        if (user is null)
        {
            _logger.LogWarning("Forgot password: user not found for email {Email}", dto.Email);
            return Ok(new { message = "Si el correo existe, se ha enviado un enlace de restablecimiento." });
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user).ConfigureAwait(false);

        return Ok(new { token });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto)
    {
        _logger.LogDebug("Reset password request for email {Email}", dto.Email);

        var user = await _userManager.FindByEmailAsync(dto.Email.Trim()).ConfigureAwait(false);

        if (user is null)
        {
            _logger.LogWarning("Reset password: user not found for email {Email}", dto.Email);
            return BadRequest(new { error = "Solicitud inválida." });
        }

        var result = await _userManager.ResetPasswordAsync(
            user, dto.Token, dto.NewPassword).ConfigureAwait(false);

        if (!result.Succeeded)
        {
            _logger.LogWarning("Reset password failed for email {Email}: {Errors}", dto.Email, result.Errors);
            return BadRequest(new { error = "Invalid password reset request." });
        }

        _logger.LogInformation("Password reset successful for user {UserId}", user.Id);

        return Ok(new { message = "Contraseña restablecida exitosamente." });
    }

    private void SetAuthCookie(string token)
    {
        var isProduction = _environment.IsProduction();
        Response.Cookies.Append(AuthCookieName, token, new CookieOptions
        {
            HttpOnly = true,
            SameSite = isProduction ? SameSiteMode.Strict : SameSiteMode.Lax,
            Secure = isProduction,
            Expires = DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.ExpireMinutes),
            Path = "/"
        });
    }

    private async Task<string> GenerateJwtTokenAsync(User user)
    {
        var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.Key));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpireMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private Guid GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userId!);
    }
}
