using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TruequeU.Interfaces;
using TruequeU.Models;

namespace TruequeU.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ModerationController : ControllerBase
{
    private readonly IModerationService _moderationService;
    private readonly ILogger<ModerationController> _logger;

    public ModerationController(IModerationService moderationService, ILogger<ModerationController> logger)
    {
        _moderationService = moderationService ?? throw new ArgumentNullException(nameof(moderationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("listings/{id:guid}/hide")]
    public async Task<ActionResult<ModerationAction>> HideListing(Guid id, [FromBody] ModerationRequestDto dto)
    {
        var adminId = GetCurrentUserId();

        try
        {
            var action = await _moderationService.HideListingAsync(adminId, id, dto.Reason).ConfigureAwait(false);
            return Ok(action);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Hide listing {ListingId} failed", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("listings/{id:guid}/unhide")]
    public async Task<ActionResult<ModerationAction>> UnhideListing(Guid id, [FromBody] ModerationRequestDto dto)
    {
        var adminId = GetCurrentUserId();

        try
        {
            var action = await _moderationService.UnhideListingAsync(adminId, id, dto.Reason).ConfigureAwait(false);
            return Ok(action);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Unhide listing {ListingId} failed", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("users/{id:guid}/suspend")]
    public async Task<ActionResult<ModerationAction>> SuspendUser(Guid id, [FromBody] ModerationRequestDto dto)
    {
        var adminId = GetCurrentUserId();

        try
        {
            var action = await _moderationService.SuspendUserAsync(adminId, id, dto.Reason).ConfigureAwait(false);
            return Ok(action);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Suspend user {UserId} failed", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("users/{id:guid}/unsuspend")]
    public async Task<ActionResult<ModerationAction>> UnsuspendUser(Guid id, [FromBody] ModerationRequestDto dto)
    {
        var adminId = GetCurrentUserId();

        try
        {
            var action = await _moderationService.UnsuspendUserAsync(adminId, id, dto.Reason).ConfigureAwait(false);
            return Ok(action);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Unsuspend user {UserId} failed", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("history")]
    public async Task<ActionResult> GetHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var (actions, totalCount) = await _moderationService.GetModerationHistoryAsync(page, pageSize)
            .ConfigureAwait(false);

        return Ok(new { actions, totalCount, page, pageSize });
    }

    private Guid GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userId!);
    }
}

public class ModerationRequestDto
{
    [Required, MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}
