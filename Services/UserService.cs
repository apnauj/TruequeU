using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TruequeU.Interfaces;
using TruequeU.Models;
using TruequeU.Models.DTOs;
using TruequeU.Persistence;

namespace TruequeU.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<UserService> _logger;

    public UserService(ApplicationDbContext context, UserManager<User> userManager, ILogger<UserService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString()).ConfigureAwait(false);
        if (user is null)
            return false;

        var result = await _userManager.DeleteAsync(user).ConfigureAwait(false);

        if (result.Succeeded)
            _logger.LogInformation("User {UserId} deleted", id);

        return result.Succeeded;
    }

    public async Task<UserReadDto?> UpdateUserAsync(Guid id, UserUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString()).ConfigureAwait(false);
        if (user is null)
            return null;

        if (!string.IsNullOrEmpty(dto.FullName))
            user.FullName = dto.FullName;
        if (!string.IsNullOrEmpty(dto.Bio))
            user.Bio = dto.Bio;
        if (!string.IsNullOrEmpty(dto.Program))
            user.Program = dto.Program;
        if (!string.IsNullOrEmpty(dto.AvatarUrl))
            user.AvatarUrl = dto.AvatarUrl;

        await _userManager.UpdateAsync(user).ConfigureAwait(false);

        _logger.LogInformation("User {UserId} profile updated", id);

        return new UserReadDto
        {
            Id = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            FullName = user.FullName,
            Program = user.Program,
            Bio = user.Bio,
            Rating = user.Rating
        };
    }

    public async Task<UserReadDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
            return null;

        return new UserReadDto
        {
            Id = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            FullName = user.FullName,
            Program = user.Program,
            Bio = user.Bio,
            Rating = user.Rating
        };
    }

    public async Task<IEnumerable<UserReadDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .Select(user => new UserReadDto
            {
                Id = user.Id,
                Username = user.UserName!,
                Email = user.Email!,
                FullName = user.FullName,
                Program = user.Program,
                Bio = user.Bio,
                Rating = user.Rating
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}