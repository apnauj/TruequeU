using TruequeU.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace TruequeU.Interfaces
{
    public interface IReportService
    {
        Task<ReportReadDto> CreateReportAsync(ReportCreateDto reportCreateDto);
        Task<ReportReadDto?> GetReportByIdAsync(Guid reportId);
        Task<IEnumerable<ReportReadDto>> GetAllReportsAsync();
        Task<ReportReadDto?> UpdateReportAsync(Guid reportId, ReportUpdateDto reportUpdateDto);
        Task<bool> DeleteReportAsync(Guid reportId);
    }
}