using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpPost]
    public async Task<ActionResult<ReportReadDto>> Create([FromBody] ReportCreateDto dto)
    {
        var reporterId = GetCurrentUserId();

        try
        {
            var report = await _reportService.CreateAsync(reporterId, dto);
            return CreatedAtAction(nameof(GetById), new { id = report.Id }, report);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<ReportReadDto>>> GetAll()
    {
        var reports = await _reportService.GetAllAsync();
        return Ok(reports);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ReportReadDto>> GetById(Guid id)
    {
        var report = await _reportService.GetByIdAsync(id);

        if (report is null)
            return NotFound();

        return Ok(report);
    }

    [HttpPatch("{id:guid}/resolve")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ReportReadDto>> Resolve(Guid id, [FromBody] ResolveReportDto dto)
    {
        var adminId = GetCurrentUserId();

        try
        {
            var report = await _reportService.ResolveAsync(id, adminId, dto.ResolutionNote);
            return Ok(report);
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

public class ResolveReportDto
{
    [Required, MaxLength(500)]
    public string ResolutionNote { get; set; } = string.Empty;
}
