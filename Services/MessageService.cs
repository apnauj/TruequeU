using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TruequeU.Interfaces;
using TruequeU.Models;
using TruequeU.Models.DTOs;
using TruequeU.Persistence;

namespace TruequeU.Services;

public class MessageService : IMessageService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MessageService> _logger;

    public MessageService(ApplicationDbContext context, ILogger<MessageService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<MessageReadDto> SendMessageAsync(Guid conversationId, Guid senderId, MessageCreateDto dto)
    {
        _logger.LogDebug("User {SenderId} sending message to conversation {ConversationId}", senderId, conversationId);

        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.Id == conversationId)
            .ConfigureAwait(false);

        if (conversation == null)
            throw new InvalidOperationException("La conversación no existe.");

        if (conversation.BuyerId != senderId && conversation.SellerId != senderId)
            throw new InvalidOperationException("No tienes permiso para enviar mensajes en esta conversación.");

        var message = new Message(conversationId, senderId, dto.Content);

        _context.Messages.Add(message);
        conversation.LastMessageAt = message.SentAt;

        await _context.SaveChangesAsync().ConfigureAwait(false);

        _logger.LogInformation("Message {MessageId} sent in conversation {ConversationId} by user {SenderId}", message.Id, conversationId, senderId);

        return new MessageReadDto
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            SenderId = message.SenderId,
            Content = message.Content,
            SentAt = message.SentAt,
            IsRead = message.IsRead
        };
    }

    public async Task<IEnumerable<MessageReadDto>> GetMessagesAsync(Guid conversationId)
    {
        return await _context.Messages
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.SentAt)
            .Select(m => new MessageReadDto
            {
                Id = m.Id,
                ConversationId = m.ConversationId,
                SenderId = m.SenderId,
                Content = m.Content,
                SentAt = m.SentAt,
                IsRead = m.IsRead
            })
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task MarkAsReadAsync(Guid messageId)
    {
        await _context.Messages
            .Where(m => m.Id == messageId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(m => m.IsRead, true))
            .ConfigureAwait(false);

        _logger.LogDebug("Message {MessageId} marked as read", messageId);
    }

    public async Task MarkAllAsReadAsync(Guid conversationId, Guid userId)
    {
        await _context.Messages
            .Where(m => m.ConversationId == conversationId
                        && m.SenderId != userId
                        && !m.IsRead)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(m => m.IsRead, true))
            .ConfigureAwait(false);

        _logger.LogDebug("All unread messages in conversation {ConversationId} marked as read for user {UserId}", conversationId, userId);
    }
}
