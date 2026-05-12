using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TruequeU.Models;

public class ListingImage
{
    [Key]
    public Guid Id { get; private set; }

    [Required, Url]
    public string Url { get; set; }

    public bool IsPrimary { get; set; }

    [MaxLength(150)]
    public string? AltText { get; set; }

    public int DisplayOrder { get; set; }

    [Required]
    public Guid ListingId { get; set; }

    [ForeignKey(nameof(ListingId))]
    public Listing Listing { get; set; } = null!;
    
    private ListingImage() { }
    
    public ListingImage(
        string url, 
        Guid listingId, 
        bool isPrimary = false, 
        int displayOrder = 0, 
        string? altText = null)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("La URL de la imagen es obligatoria.");

        Id = Guid.NewGuid();
        Url = url;
        ListingId = listingId;
        IsPrimary = isPrimary;
        DisplayOrder = displayOrder;
        AltText = altText;
    }
}