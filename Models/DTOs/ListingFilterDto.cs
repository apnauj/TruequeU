using TruequeU.Enums;

namespace TruequeU.Models.DTOs;

public class ListingFilterDto
{
    public string? Keyword { get; set; }

    public Category? Category { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public ItemCondition? Condition { get; set; }

    public ListingState? State { get; set; }

    public DateTime? PostedAfter { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
