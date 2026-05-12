using TruequeU.Interfaces;
using TruequeU.Models.DTOs;
using TruequeU.Persistence;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TruequeU.Models;
using Microsoft.EntityFrameworkCore;
using TruequeU.Enums;

namespace TruequeU.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ReportReadDto> CreateReportAsync(ReportCreateDto reportCreateDto)
        {
            var report = new Report(
                reportCreateDto.ReporterId,
                reportCreateDto.ReportedUserId,
                reportCreateDto.Reason,
                reportCreateDto.Comment,
                reportCreateDto.ReportedListingId
            );

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return new ReportReadDto(report);
        }

        public async Task<ReportReadDto?> GetReportByIdAsync(Guid reportId)
        {
            var report = await _context.Reports.FindAsync(reportId);

            return report is null ? null : new ReportReadDto(report);
        }

        public async Task<IEnumerable<ReportReadDto>> GetAllReportsAsync()
        {
            var reports = await _context.Reports.ToListAsync();

            return reports.Select(r => new ReportReadDto(r));
        }

        public async Task<ReportReadDto?> UpdateReportAsync(Guid reportId, ReportUpdateDto reportUpdateDto)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null)
            {
                return null;
            }

            report.Status = reportUpdateDto.Status;
            report.ResolvedByUserId = reportUpdateDto.ResolvedByUserId;
            report.ResolvedAt = reportUpdateDto.ResolvedAt;
            report.ResolutionNote = reportUpdateDto.ResolutionNote;

            await _context.SaveChangesAsync();

            return new ReportReadDto(report);
        }

        public async Task<bool> DeleteReportAsync(Guid reportId)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null)
            {
                return false;
            }

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}