using TruequeU.Enums;

namespace TruequeU.Models.DTOs
{
    public record FavoriteResponseDto
    {
        public Guid Id { get; set; }
        public Guid ListingId { get; set; }
        public Guid UserId { get; set; }
        public DateTime FavoritedAt { get; set; }
        public string ListingTitle { get; set; } = string.Empty;
        public decimal ListingPrice { get; set; }
        public Category ListingCategory { get; set; }
        public ListingState ListingState { get; set; }

        public FavoriteResponseDto(Favorite favorite)
        {
            Id = favorite.Id;
            ListingId = favorite.ListingId;
            UserId = favorite.UserId;
            FavoritedAt = favorite.FavoritedAt;
            ListingTitle = favorite.Listing.Title;
            ListingPrice = favorite.Listing.Price;
            ListingCategory = favorite.Listing.Category;
            ListingState = favorite.Listing.State;
        }
    }
}
