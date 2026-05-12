using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TruequeU.Models;

[Index(nameof(UserId), nameof(ListingId), IsUnique = true)]
public class Favorite
{
    [Key]
    public Guid Id { get; private set; }

    [Required]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [Required]
    public Guid ListingId { get; set; }

    [ForeignKey(nameof(ListingId))]
    public Listing Listing { get; set; } = null!;

    public DateTime FavoritedAt { get; private set; }

    private Favorite() { }

    public Favorite(Guid userId, Guid listingId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        ListingId = listingId;
        FavoritedAt = DateTime.UtcNow;
    }
}
