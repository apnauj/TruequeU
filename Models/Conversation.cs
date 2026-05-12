using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TruequeU.Models;

[Index(nameof(BuyerId), nameof(ListingId), IsUnique = true)]
public class Conversation
{
    [Key]
    public Guid Id { get; private set; }

    [Required]
    public Guid ListingId { get; set; }
    
    [ForeignKey(nameof(ListingId))]
    public Listing Listing { get; set; } = null!;

    [Required]
    public Guid BuyerId { get; set; }

    [ForeignKey(nameof(BuyerId))]
    public User Buyer { get; set; } = null!;

    [Required]
    public Guid SellerId { get; set; }

    [ForeignKey(nameof(SellerId))]
    public User Seller { get; set; } = null!;

    [Required]
    public DateTime CreatedAt { get; private set; }

    public DateTime? LastMessageAt { get; set; }
    
    public ICollection<Message> Messages { get; set; } = new HashSet<Message>();

    private Conversation() {}

    public Conversation(Guid listingId, Guid buyerId, Guid sellerId)
    {
        Id = Guid.NewGuid();
        ListingId = listingId;
        BuyerId = buyerId;
        SellerId = sellerId;
        CreatedAt = DateTime.UtcNow;
        LastMessageAt = DateTime.UtcNow;
    }
}
