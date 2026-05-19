namespace TruequeU.Models.DTOs;

public record ListingImageDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public string? AltText { get; set; }
    public int DisplayOrder { get; set; }
}
