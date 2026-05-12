using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TruequeU.Models.DTOs;

namespace TruequeU.Interfaces;

public interface IMessageService
{
    Task<MessageReadDto> SendMessageAsync(Guid conversationId, Guid senderId, MessageCreateDto dto);
    Task<IEnumerable<MessageReadDto>> GetMessagesAsync(Guid conversationId);
    Task MarkAsReadAsync(Guid messageId);
    Task MarkAllAsReadAsync(Guid conversationId, Guid userId);
}
