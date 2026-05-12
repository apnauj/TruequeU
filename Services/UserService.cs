using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TruequeU.Interfaces;
using TruequeU.Models;
using TruequeU.Models.DTOs;

namespace TruequeU.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserReadDto> CreateUserAsync(UserCreateDto dto)
    {

        var newUser = new User(
            dto.Username, 
            dto.Email, 
            dto.PasswordHash, 
            dto.FullName
        );
        
        _context.Users.Add(newUser);
        await _context.SaveChangesAsync(); 
        
        return new UserReadDto
        {
            Id = newUser.Id,
            Username = newUser.Username,
            Email = newUser.Email,
            Bio = newUser.Bio,
            Rating = newUser.Rating
        };
        
    }

    public async Task<UserReadDto?> UpdateUserAsync(Guid id, UserUpdateDto dto)
    {
        var userEntity = await _context.Users.FindAsync(id);
        
        if (userEntity == null) return null; 
        
        if (!string.IsNullOrEmpty(dto.Username)) userEntity.Username = dto.Username;
        if (!string.IsNullOrEmpty(dto.FullName)) userEntity.FullName = dto.FullName;
        if (!string.IsNullOrEmpty(dto.Bio)) userEntity.Bio = dto.Bio;
        if (!string.IsNullOrEmpty(dto.Program)) userEntity.Program = dto.Program;
        if (!string.IsNullOrEmpty(dto.AvatarUrl)) userEntity.AvatarUrl = dto.AvatarUrl;
        
        await _context.SaveChangesAsync();
        
        return new UserReadDto
        {
            Id = userEntity.Id,
            Username = userEntity.Username,
            Email = userEntity.Email,
            Bio = userEntity.Bio,
            Rating = userEntity.Rating
        };
    }

    public Task<bool> DeleteUserAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<UserReadDto?> GetUserByIdAsync(Guid id)
    {
        var userEntity = await _context.Users.FindAsync(id);
        
        return new UserReadDto
        {
            Id = userEntity.Id,
            Username = userEntity.Username,
            Email = userEntity.Email,
            Bio = userEntity.Bio,
            Rating = userEntity.Rating
        };
    }

    public async Task<IEnumerable<UserReadDto>> GetAllUsersAsync()
    {
        return await _context.Users
            .AsNoTracking() 
            .Select(user => new UserReadDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Bio = user.Bio,
                Rating = user.Rating
            })
            .ToListAsync();
    }
}