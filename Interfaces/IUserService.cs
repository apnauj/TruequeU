using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TruequeU.Models;
using TruequeU.Models.DTOs;

namespace TruequeU.Interfaces;

public interface IUserService
{
    Task<UserReadDto?> UpdateUserAsync(Guid id, UserUpdateDto user, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserReadDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserReadDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);
}