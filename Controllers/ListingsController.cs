using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TruequeU.Interfaces;
using TruequeU.Models.DTOs;

namespace TruequeU.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ListingsController : ControllerBase
{
    private readonly IListingService _listingService;
    private readonly ILogger<ListingsController> _logger;

    public ListingsController(IListingService listingService, ILogger<ListingsController> logger)
    {
        _listingService = listingService ?? throw new ArgumentNullException(nameof(listingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<ListingResponseDto>>> GetAll()
    {
        _logger.LogDebug("Fetching all listings");
        var listings = await _listingService.GetAllAsync().ConfigureAwait(false);
        return Ok(listings);
    }

    [HttpGet("my")]
    public async Task<ActionResult<List<ListingResponseDto>>> GetMyListings()
    {
        var ownerId = GetCurrentUserId();
        var listings = await _listingService.GetByOwnerIdAsync(ownerId).ConfigureAwait(false);
        return Ok(listings);
    }

    [HttpPost]
    public async Task<ActionResult<ListingResponseDto>> Create([FromBody] ListingCreateDTO dto)
    {
        var ownerId = GetCurrentUserId();
        var listing = await _listingService.CreateAsync(dto, ownerId).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetAll), new { id = listing.Id }, listing);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ListingResponseDto>> Update(Guid id, [FromBody] ListingUpdateDTO dto)
    {
        var ownerId = GetCurrentUserId();

        try
        {
            var listing = await _listingService.UpdateAsync(dto, id, ownerId).ConfigureAwait(false);
            return Ok(listing);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Update failed for listing {ListingId}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ownerId = GetCurrentUserId();

        try
        {
            var deleted = await _listingService.SoftDeleteAsync(id, ownerId).ConfigureAwait(false);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Delete failed for listing {ListingId}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/sold")]
    public async Task<ActionResult<ListingResponseDto>> MarkAsSold(Guid id)
    {
        var ownerId = GetCurrentUserId();

        try
        {
            var listing = await _listingService.MarkAsSoldAsync(id, ownerId).ConfigureAwait(false);
            return Ok(listing);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "MarkAsSold failed for listing {ListingId}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/reserved")]
    public async Task<ActionResult<ListingResponseDto>> MarkAsReserved(Guid id)
    {
        var ownerId = GetCurrentUserId();

        try
        {
            var listing = await _listingService.MarkAsReservedAsync(id, ownerId).ConfigureAwait(false);
            return Ok(listing);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "MarkAsReserved failed for listing {ListingId}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/available")]
    public async Task<ActionResult<ListingResponseDto>> MarkAsAvailable(Guid id)
    {
        var ownerId = GetCurrentUserId();

        try
        {
            var listing = await _listingService.MarkAsAvailableAsync(id, ownerId).ConfigureAwait(false);
            return Ok(listing);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "MarkAsAvailable failed for listing {ListingId}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/images")]
    public async Task<ActionResult<ListingImageDto>> AddImage(Guid id, [FromBody] ImageUrlDto dto)
    {
        var ownerId = GetCurrentUserId();

        try
        {
            var image = await _listingService.AddImageAsync(id, ownerId, dto.Url).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetAll), new { id = image.Id }, image);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Add image failed for listing {ListingId}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{listingId:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> RemoveImage(Guid listingId, Guid imageId)
    {
        var ownerId = GetCurrentUserId();

        try
        {
            var removed = await _listingService.RemoveImageAsync(listingId, ownerId, imageId).ConfigureAwait(false);
            if (!removed)
                return NotFound();

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Remove image {ImageId} failed for listing {ListingId}", imageId, listingId);
            return BadRequest(new { error = ex.Message });
        }
    }

    private Guid GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userId!);
    }
}
