using System;

namespace TruequeU.Models.DTOs;

public record ConversationReadDto
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public string? LastMessageContent { get; set; }
    public int UnreadCount { get; set; }
}
