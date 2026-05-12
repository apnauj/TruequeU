using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TruequeU.Interfaces;
using TruequeU.Models;
using TruequeU.Models.DTOs;
using TruequeU.Persistence;

namespace TruequeU.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public UserService(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
            return false;

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    public async Task<UserReadDto?> UpdateUserAsync(Guid id, UserUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
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

        await _userManager.UpdateAsync(user);

        return new UserReadDto
        {
            Id = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            Bio = user.Bio,
            Rating = user.Rating
        };
    }

    public async Task<UserReadDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user is null)
            return null;

        return new UserReadDto
        {
            Id = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
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
                Bio = user.Bio,
                Rating = user.Rating
            })
            .ToListAsync(cancellationToken);
    }
}