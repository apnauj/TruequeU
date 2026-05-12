using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TruequeU.Models.DTOs;

namespace TruequeU.Interfaces;

public interface IConversationService
{
    Task<ConversationReadDto> CreateConversationAsync(Guid buyerId, ConversationCreateDto dto);
    Task<ConversationReadDto?> GetConversationByIdAsync(Guid id, Guid viewerId);
    Task<IEnumerable<ConversationReadDto>> GetUserConversationsAsync(Guid userId);
    Task<bool> DeleteConversationAsync(Guid id);
}
