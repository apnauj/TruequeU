using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TruequeU.Enums;

namespace TruequeU.Models;

public class User
{
    [Key]
    public Guid Id { get; private set; }
    
    [Required, MaxLength(50)]
    public string Username { get; set; }
    
    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; }
    
    [Required]
    public string PasswordHash { get; set; }
    
    [MaxLength(100)]
    public string? FullName { get; set; }

    [MaxLength(100)]
    public string? Program { get; set; }

    [Range(0, 5)]
    public double Rating { get; set; }

    public UserState State { get; set; }
    
    public string? AvatarUrl { get; set; } 
    
    [MaxLength(500)]
    public string? Bio { get; set; }

    [Required] 
    public DateTime CreatedAt { get; private set; }
    
    public DateTime? LastLogin { get; set; }
    
    public bool IsEmailVerified { get; set; }
    
    public ICollection<Listing> Listings { get; set; } = new HashSet<Listing>();
    
    public ICollection<Favorite> Favorites { get; set; } = new HashSet<Favorite>();
    
    private User() { }
    
    public User(string username, string email, string passwordHash, string? fullName = null)
    {
        Id = Guid.NewGuid();
        Username = username;
        Email = email.ToLower().Trim(); 
        PasswordHash = passwordHash;
        FullName = fullName;
        
        State = UserState.Active;
        CreatedAt = DateTime.UtcNow;
        IsEmailVerified = false;
    }
}