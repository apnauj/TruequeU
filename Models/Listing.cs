using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using TruequeU.Enums;

namespace TruequeU.Models;

public class Listing
{
    [Key]
    public Guid Id { get; private set; }
    
    [Required, MaxLength(100)]
    public string Title { get; set; }
    
    [Required, MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    [Required, Precision(18,2), Range(0.0, double.MaxValue)]
    public decimal Price { get; set; }
    
    [Required]
    public Category Category { get; set; }
    
    [Required]
    public ItemCondition Condition { get; set; }
    
    [Required, MaxLength(100)]
    public string CampusLocation { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; set; }
    
    public ListingState State { get; set; } = ListingState.Available;
    
    [Required]
    public Guid OwnerId { get; set; }

    [ForeignKey(nameof(OwnerId))] 
    public User Owner { get; set; } = null!;
    
    public ICollection<ListingImage> Images { get; set; } = new HashSet<ListingImage>();

    public ICollection<Conversation> Conversations { get; set; } = new HashSet<Conversation>();
    
    private Listing(){}

    public Listing(
        string title, 
        string description, 
        decimal price, 
        Category category, 
        ItemCondition condition, 
        string campusLocation, 
        Guid ownerId)
    {
        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        Price = price;
        Category = category;
        Condition = condition;
        CampusLocation = campusLocation;
        OwnerId = ownerId;
        
        CreatedAt = DateTime.UtcNow;
        State = ListingState.Available; 
    }
}




