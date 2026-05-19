using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TruequeU.Interfaces;
using TruequeU.Models.DTOs;

namespace TruequeU.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;
    private readonly ILogger<FavoritesController> _logger;

    public FavoritesController(IFavoriteService favoriteService, ILogger<FavoritesController> logger)
    {
        _favoriteService = favoriteService ?? throw new ArgumentNullException(nameof(favoriteService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("listings/{id:guid}/favorite")]
    public async Task<ActionResult<FavoriteResponseDto>> AddFavorite(Guid id)
    {
        var userId = GetCurrentUserId();

        try
        {
            var favorite = await _favoriteService.AddFavoriteAsync(id, userId).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetMyFavorites), new { id = favorite.Id }, favorite);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Add favorite failed for user {UserId} on listing {ListingId}", userId, id);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("listings/{id:guid}/favorite")]
    public async Task<IActionResult> RemoveFavorite(Guid id)
    {
        var userId = GetCurrentUserId();

        try
        {
            var removed = await _favoriteService.RemoveFavoriteAsync(id, userId).ConfigureAwait(false);
            if (!removed)
                return NotFound();

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Remove favorite failed for user {UserId} on listing {ListingId}", userId, id);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("favorites")]
    public async Task<ActionResult<List<FavoriteResponseDto>>> GetMyFavorites()
    {
        var userId = GetCurrentUserId();

        try
        {
            var favorites = await _favoriteService.GetUserFavoritesAsync(userId).ConfigureAwait(false);
            return Ok(favorites);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Get favorites failed for user {UserId}", userId);
            return BadRequest(new { error = ex.Message });
        }
    }

    private Guid GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userId!);
    }
}
