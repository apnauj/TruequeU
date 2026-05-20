using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TruequeU.Enums;
using TruequeU.Interfaces;
using TruequeU.Models;
using TruequeU.Models.DTOs;
using TruequeU.Persistence;

namespace TruequeU.Services;

public class ConversationService : IConversationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ConversationService> _logger;

    public ConversationService(ApplicationDbContext context, ILogger<ConversationService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ConversationReadDto> CreateConversationAsync(Guid buyerId, ConversationCreateDto dto)
    {
        _logger.LogDebug("Creating conversation for buyer {BuyerId} on listing {ListingId}", buyerId, dto.ListingId);

        var buyer = await _context.Users.FindAsync(buyerId).ConfigureAwait(false);
        if (buyer?.State == UserState.Suspended)
            throw new InvalidOperationException("Suspended users cannot start conversations.");

        var listing = await _context.Listings
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == dto.ListingId)
            .ConfigureAwait(false);

        if (listing == null)
            throw new InvalidOperationException("El artículo no existe.");

        if (listing.State != ListingState.Available)
            throw new InvalidOperationException("El artículo no está disponible para iniciar una conversación.");

        if (listing.OwnerId == buyerId)
            throw new InvalidOperationException("No puedes iniciar una conversación con tu propio artículo.");

        bool exists = await _context.Conversations.AnyAsync(c =>
            c.BuyerId == buyerId && c.ListingId == dto.ListingId)
            .ConfigureAwait(false);

        if (exists)
            throw new InvalidOperationException("Ya existe una conversación para este artículo.");

        var conversation = new Conversation(dto.ListingId, buyerId, listing.OwnerId);
        var message = new Message(conversation.Id, buyerId, dto.Content);

        _context.Conversations.Add(conversation);
        _context.Messages.Add(message);
        await _context.SaveChangesAsync().ConfigureAwait(false);

        _logger.LogInformation("Conversation {ConversationId} created between buyer {BuyerId} and seller {SellerId}", conversation.Id, buyerId, listing.OwnerId);

        return new ConversationReadDto
        {
            Id = conversation.Id,
            ListingId = conversation.ListingId,
            BuyerId = conversation.BuyerId,
            SellerId = conversation.SellerId,
            CreatedAt = conversation.CreatedAt,
            LastMessageAt = conversation.LastMessageAt,
            LastMessageContent = dto.Content,
            UnreadCount = 0
        };
    }

    public async Task<ConversationReadDto?> GetConversationByIdAsync(Guid id, Guid viewerId)
    {
        return await _context.Conversations
            .AsNoTracking()
            .Where(c => c.Id == id && (c.BuyerId == viewerId || c.SellerId == viewerId))
            .Select(c => new ConversationReadDto
            {
                Id = c.Id,
                ListingId = c.ListingId,
                BuyerId = c.BuyerId,
                SellerId = c.SellerId,
                CreatedAt = c.CreatedAt,
                LastMessageAt = c.LastMessageAt,
                LastMessageContent = c.Messages
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => m.Content)
                    .FirstOrDefault(),
                UnreadCount = c.Messages.Count(m => !m.IsRead && m.SenderId != viewerId)
            })
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<ConversationReadDto>> GetUserConversationsAsync(Guid userId)
    {
        return await _context.Conversations
            .AsNoTracking()
            .Where(c => c.BuyerId == userId || c.SellerId == userId)
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .Select(c => new ConversationReadDto
            {
                Id = c.Id,
                ListingId = c.ListingId,
                BuyerId = c.BuyerId,
                SellerId = c.SellerId,
                CreatedAt = c.CreatedAt,
                LastMessageAt = c.LastMessageAt,
                LastMessageContent = c.Messages
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => m.Content)
                    .FirstOrDefault(),
                UnreadCount = c.Messages.Count(m => !m.IsRead && m.SenderId != userId)
            })
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<bool> DeleteConversationAsync(Guid id, Guid userId)
    {
        var conversation = await _context.Conversations.FindAsync(id).ConfigureAwait(false);
        if (conversation == null) return false;

        if (conversation.BuyerId != userId && conversation.SellerId != userId)
            return false;

        _context.Conversations.Remove(conversation);
        await _context.SaveChangesAsync().ConfigureAwait(false);

        _logger.LogInformation("Conversation {ConversationId} deleted", id);

        return true;
    }
}
