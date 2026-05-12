using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruequeU.Interfaces;
using TruequeU.Models.DTOs;

namespace TruequeU.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ListingsController : ControllerBase
{
    private readonly IListingService _listingService;

    public ListingsController(IListingService listingService)
    {
        _listingService = listingService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<ListingResponseDto>>> GetAll()
    {
        var listings = await _listingService.GetAllAsync();
        return Ok(listings);
    }

    [HttpGet("my")]
    public async Task<ActionResult<List<ListingResponseDto>>> GetMyListings()
    {
        var ownerId = GetCurrentUserId();
        var listings = await _listingService.GetByOwnerIdAsync(ownerId);
        return Ok(listings);
    }

    [HttpPost]
    public async Task<ActionResult<ListingResponseDto>> Create([FromBody] ListingCreateDTO dto)
    {
        var ownerId = GetCurrentUserId();
        var listing = await _listingService.CreateAsync(dto, ownerId);
        return CreatedAtAction(nameof(GetAll), new { id = listing.Id }, listing);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ListingResponseDto>> Update(Guid id, [FromBody] ListingUpdateDTO dto)
    {
        var ownerId = GetCurrentUserId();

        try
        {
            var listing = await _listingService.UpdateAsync(dto, id, ownerId);
            return Ok(listing);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ownerId = GetCurrentUserId();

        try
        {
            var deleted = await _listingService.SoftDeleteAsync(id, ownerId);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("{id:guid}/sold")]
    public async Task<ActionResult<ListingResponseDto>> MarkAsSold(Guid id)
    {
        var ownerId = GetCurrentUserId();

        try
        {
            var listing = await _listingService.MarkAsSoldAsync(id, ownerId);
            return Ok(listing);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("{id:guid}/reserved")]
    public async Task<ActionResult<ListingResponseDto>> MarkAsReserved(Guid id)
    {
        var ownerId = GetCurrentUserId();

        try
        {
            var listing = await _listingService.MarkAsReservedAsync(id, ownerId);
            return Ok(listing);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("{id:guid}/available")]
    public async Task<ActionResult<ListingResponseDto>> MarkAsAvailable(Guid id)
    {
        var ownerId = GetCurrentUserId();

        try
        {
            var listing = await _listingService.MarkAsAvailableAsync(id, ownerId);
            return Ok(listing);
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
