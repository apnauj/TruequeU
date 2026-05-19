using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TruequeU.Enums;
using TruequeU.Interfaces;
using TruequeU.Models;
using TruequeU.Models.DTOs;
using TruequeU.Persistence;

namespace TruequeU.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ReportService> _logger;

    public ReportService(ApplicationDbContext context, ILogger<ReportService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ReportReadDto> CreateAsync(Guid reporterId, ReportCreateDto dto)
    {
        _logger.LogDebug("User {ReporterId} creating report against user {ReportedUserId}", reporterId, dto.ReportedUserId);

        var report = new Report(reporterId, dto.ReportedUserId, dto.Reason, dto.Comment, dto.ReportedListingId);

        _context.Reports.Add(report);
        await _context.SaveChangesAsync().ConfigureAwait(false);

        _logger.LogInformation("Report {ReportId} created by user {ReporterId}", report.Id, reporterId);

        return await GetByIdAsync(report.Id).ConfigureAwait(false)
               ?? throw new InvalidOperationException("El reporte no se pudo crear.");
    }

    public async Task<IEnumerable<ReportReadDto>> GetAllAsync()
    {
        return await _context.Reports
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReportReadDto
            {
                Id = r.Id,
                ReporterId = r.ReporterId,
                ReporterName = r.Reporter.UserName!,
                ReportedUserId = r.ReportedUserId,
                ReportedUserName = r.ReportedUser.UserName!,
                ReportedListingId = r.ReportedListingId,
                Reason = r.Reason,
                Comment = r.Comment,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
                ResolvedByUserId = r.ResolvedByUserId,
                ResolvedByUserName = r.ResolvedByUser != null ? r.ResolvedByUser.UserName : null,
                ResolvedAt = r.ResolvedAt,
                ResolutionNote = r.ResolutionNote
            })
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<ReportReadDto?> GetByIdAsync(Guid id)
    {
        return await _context.Reports
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new ReportReadDto
            {
                Id = r.Id,
                ReporterId = r.ReporterId,
                ReporterName = r.Reporter.UserName!,
                ReportedUserId = r.ReportedUserId,
                ReportedUserName = r.ReportedUser.UserName!,
                ReportedListingId = r.ReportedListingId,
                Reason = r.Reason,
                Comment = r.Comment,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
                ResolvedByUserId = r.ResolvedByUserId,
                ResolvedByUserName = r.ResolvedByUser != null ? r.ResolvedByUser.UserName : null,
                ResolvedAt = r.ResolvedAt,
                ResolutionNote = r.ResolutionNote
            })
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public async Task<ReportReadDto> ResolveAsync(Guid id, Guid adminId, string resolutionNote)
    {
        var report = await _context.Reports.FindAsync(id).ConfigureAwait(false)
            ?? throw new InvalidOperationException("El reporte no existe.");

        if (report.Status != ReportStatus.Open)
            throw new InvalidOperationException("El reporte ya ha sido resuelto.");

        report.Status = ReportStatus.Closed;
        report.ResolvedByUserId = adminId;
        report.ResolvedAt = DateTime.UtcNow;
        report.ResolutionNote = resolutionNote;

        await _context.SaveChangesAsync().ConfigureAwait(false);

        _logger.LogInformation("Report {ReportId} resolved by admin {AdminId}", id, adminId);

        return (await GetByIdAsync(id).ConfigureAwait(false))!;
    }
}
