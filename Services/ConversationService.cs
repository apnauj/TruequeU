using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TruequeU.Enums;
using TruequeU.Interfaces;
using TruequeU.Models;
using TruequeU.Models.DTOs;
using TruequeU.Persistence;

namespace TruequeU.Services;

public class ConversationService : IConversationService
{
    private readonly ApplicationDbContext _context;

    public ConversationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ConversationReadDto> CreateConversationAsync(Guid buyerId, ConversationCreateDto dto)
    {
        var listing = await _context.Listings
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == dto.ListingId);

        if (listing == null)
            throw new InvalidOperationException("El artículo no existe.");

        if (listing.State != ListingState.Available)
            throw new InvalidOperationException("El artículo no está disponible para iniciar una conversación.");

        if (listing.OwnerId == buyerId)
            throw new InvalidOperationException("No puedes iniciar una conversación con tu propio artículo.");

        bool exists = await _context.Conversations.AnyAsync(c =>
            c.BuyerId == buyerId && c.ListingId == dto.ListingId);

        if (exists)
            throw new InvalidOperationException("Ya existe una conversación para este artículo.");

        var conversation = new Conversation(dto.ListingId, buyerId, listing.OwnerId);

        var message = new Message(conversation.Id, buyerId, dto.Content);

        _context.Conversations.Add(conversation);
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

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
            .Where(c => c.Id == id)
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
            .FirstOrDefaultAsync();
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
            .ToListAsync();
    }

    public async Task<bool> DeleteConversationAsync(Guid id)
    {
        var conversation = await _context.Conversations.FindAsync(id);
        if (conversation == null) return false;

        _context.Conversations.Remove(conversation);
        await _context.SaveChangesAsync();

        return true;
    }
}
