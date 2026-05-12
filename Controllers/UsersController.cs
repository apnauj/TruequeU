using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruequeU.Interfaces;
using TruequeU.Models.DTOs;

namespace TruequeU.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserReadDto>>> GetAll()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("profile")]
    public async Task<ActionResult<UserReadDto>> GetMyProfile()
    {
        var userId = GetCurrentUserId();
        var user = await _userService.GetUserByIdAsync(userId);

        if (user is null)
            return NotFound();

        return Ok(user);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserReadDto>> GetById(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);

        if (user is null)
            return NotFound();

        return Ok(user);
    }

    [HttpPut("profile")]
    public async Task<ActionResult<UserReadDto>> UpdateProfile([FromBody] UserUpdateDto dto)
    {
        var userId = GetCurrentUserId();
        var user = await _userService.UpdateUserAsync(userId, dto);

        if (user is null)
            return NotFound();

        return Ok(user);
    }

    [HttpDelete("profile")]
    public async Task<IActionResult> DeleteProfile()
    {
        var userId = GetCurrentUserId();
        var deleted = await _userService.DeleteUserAsync(userId);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userId!);
    }
}
