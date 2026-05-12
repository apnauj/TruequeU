using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using TruequeU.Enums;

namespace TruequeU.Models;

public class User : IdentityUser<Guid>
{
    [MaxLength(100)]
    public string? FullName { get; set; }

    [MaxLength(100)]
    public string? Program { get; set; }

    [Range(0, 5)]
    public double Rating { get; set; }

    public UserState State { get; set; }

    [Url, MaxLength(2048)]
    public string? AvatarUrl { get; set; }

    [MaxLength(500)]
    public string? Bio { get; set; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public DateTime? LastLogin { get; set; }

    public ICollection<Listing> Listings { get; set; } = [];

    public ICollection<Favorite> Favorites { get; set; } = [];
}