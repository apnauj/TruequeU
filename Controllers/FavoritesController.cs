using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruequeU.Interfaces;
using TruequeU.Models.DTOs;

namespace TruequeU.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    [HttpPost("listings/{id:guid}/favorite")]
    public async Task<ActionResult<FavoriteResponseDto>> AddFavorite(Guid id)
    {
        var userId = GetCurrentUserId();

        try
        {
            var favorite = await _favoriteService.AddFavoriteAsync(id, userId);
            return CreatedAtAction(nameof(GetMyFavorites), new { id = favorite.Id }, favorite);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("listings/{id:guid}/favorite")]
    public async Task<IActionResult> RemoveFavorite(Guid id)
    {
        var userId = GetCurrentUserId();

        try
        {
            var removed = await _favoriteService.RemoveFavoriteAsync(id, userId);
            if (!removed)
                return NotFound();

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("favorites")]
    public async Task<ActionResult<List<FavoriteResponseDto>>> GetMyFavorites()
    {
        var userId = GetCurrentUserId();

        try
        {
            var favorites = await _favoriteService.GetUserFavoritesAsync(userId);
            return Ok(favorites);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private Guid GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userId!);
    }
}
