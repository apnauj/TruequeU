using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TruequeU.Models;
using TruequeU.Models.DTOs;

namespace TruequeU.Interfaces;

public interface IUserService
{
    Task<UserReadDto> CreateUserAsync(UserCreateDto user);
    Task<UserReadDto?> UpdateUserAsync(Guid id, UserUpdateDto user);
    Task<bool> DeleteUserAsync(Guid id);
    Task<UserReadDto?> GetUserByIdAsync(Guid id);
    Task<IEnumerable<UserReadDto>> GetAllUsersAsync();
}