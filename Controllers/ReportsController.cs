using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TruequeU.Authorization;
using TruequeU.Interfaces;
using TruequeU.Models.DTOs;

namespace TruequeU.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
    {
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost]
    public async Task<ActionResult<ReportReadDto>> Create([FromBody] ReportCreateDto dto)
    {
        var reporterId = GetCurrentUserId();

        try
        {
            var report = await _reportService.CreateAsync(reporterId, dto).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetById), new { id = report.Id }, report);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Report creation failed: invalid argument");
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Report creation failed for reporter {ReporterId}", reporterId);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Roles = RoleConstants.Admin)]
    public async Task<ActionResult<IEnumerable<ReportReadDto>>> GetAll()
    {
        var reports = await _reportService.GetAllAsync().ConfigureAwait(false);
        return Ok(reports);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = RoleConstants.Admin)]
    public async Task<ActionResult<ReportReadDto>> GetById(Guid id)
    {
        var report = await _reportService.GetByIdAsync(id).ConfigureAwait(false);

        if (report is null)
            return NotFound();

        return Ok(report);
    }

    [HttpPatch("{id:guid}/resolve")]
    [Authorize(Roles = RoleConstants.Admin)]
    public async Task<ActionResult<ReportReadDto>> Resolve(Guid id, [FromBody] ResolveReportDto dto)
    {
        var adminId = GetCurrentUserId();

        try
        {
            var report = await _reportService.ResolveAsync(id, adminId, dto.ResolutionNote).ConfigureAwait(false);
            return Ok(report);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Report resolution failed for report {ReportId}", id);
            return BadRequest(new { error = ex.Message });
        }
    }

    private Guid GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userId!);
    }
}

