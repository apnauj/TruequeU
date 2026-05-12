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
        bool exists = await _context.Users.AnyAsync(u => 
            u.Email == dto.Email.ToLower() || 
            u.Username.ToLower() == dto.Username.ToLower());

        if (exists)
        {
            throw new InvalidOperationException("El usuario o correo electrónico ya se encuentra registrado.");
        }
        
        var newUser = new User(
            dto.Username, 
            dto.Email, 
            dto.Password, 
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
        bool exists = await _context.Users.AnyAsync(u => 
            u.Email == dto.Email.ToLower() || 
            u.Username.ToLower() == dto.Username.ToLower());

        if (exists)
        {
            throw new InvalidOperationException("El usuario o correo electrónico ya se encuentra registrado.");
        }
        var userEntity = await _context.Users.FindAsync(id);
        
        if (userEntity == null) return null; 
        
        if (!string.IsNullOrEmpty(dto.Username)) userEntity.Username = dto.Username;
        if (!string.IsNullOrEmpty(dto.FullName)) userEntity.FullName = dto.FullName;
        if (!string.IsNullOrEmpty(dto.Email)) userEntity.Email = dto.Email.Trim().ToLower();
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

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var userEntity = await _context.Users.AsNoTracking().FindAsync(id);
        if (userEntity == null) return false;

        _context.Users.Remove(userEntity);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<UserReadDto?> GetUserByIdAsync(Guid id)
    {
        var userEntity = await _context.Users.AsNoTracking().FirstOrDefaultAsync(id);
        if (userEntity == null) return null;
        
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