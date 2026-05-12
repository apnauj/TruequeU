using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TruequeU.Models.DTOs;

namespace TruequeU.Interfaces;

public interface IReportService
{
    Task<ReportReadDto> CreateAsync(Guid reporterId, ReportCreateDto dto);
    Task<IEnumerable<ReportReadDto>> GetAllAsync();
    Task<ReportReadDto?> GetByIdAsync(Guid id);
    Task<ReportReadDto> ResolveAsync(Guid id, Guid adminId, string resolutionNote);
}
