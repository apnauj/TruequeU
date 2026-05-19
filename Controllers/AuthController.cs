using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TruequeU.Configuration;
using TruequeU.Enums;
using TruequeU.Models;
using TruequeU.Models.DTOs;

namespace TruequeU.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AuthController> logger)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        _jwtSettings = jwtSettings?.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            return BadRequest(result.Errors);
        }

        await _userManager.AddToRoleAsync(user, "User").ConfigureAwait(false);

        var token = await GenerateJwtTokenAsync(user).ConfigureAwait(false);

        _logger.LogInformation("User {UserId} registered successfully", user.Id);

        return Ok(new AuthResponseDto
        {
            Token = token,
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
            return Unauthorized("Credenciales inválidas.");
        }

        if (user.State == UserState.Suspended)
        {
            _logger.LogWarning("Login refused: user {UserId} is suspended", user.Id);
            return Unauthorized("Tu cuenta ha sido suspendida.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false).ConfigureAwait(false);

        if (!result.Succeeded)
        {
            _logger.LogWarning("Login failed: invalid password for user {UserId}", user.Id);
            return Unauthorized("Credenciales inválidas.");
        }

        user.LastLogin = DateTime.UtcNow;
        await _userManager.UpdateAsync(user).ConfigureAwait(false);

        var token = await GenerateJwtTokenAsync(user).ConfigureAwait(false);

        _logger.LogInformation("User {UserId} logged in successfully", user.Id);

        return Ok(new AuthResponseDto
        {
            Token = token,
            Expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpireMinutes),
            UserId = user.Id,
            UserName = user.UserName!,
            Email = user.Email!
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
            return Ok("Si el correo existe, se ha enviado un enlace de restablecimiento.");
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
            return BadRequest("Solicitud inválida.");
        }

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword).ConfigureAwait(false);

        if (!result.Succeeded)
        {
            _logger.LogWarning("Reset password failed for email {Email}: {Errors}", dto.Email, result.Errors);
            return BadRequest(result.Errors);
        }

        _logger.LogInformation("Password reset successful for user {UserId}", user.Id);

        return Ok("Contraseña restablecida exitosamente.");
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
}
