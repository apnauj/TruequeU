using Mono.TextTemplating;
using System.Reflection;
using TruequeU.Enums;

namespace TruequeU.Models.DTOs
{
    public record ListingResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; } 
        public Category Category { get; set; }
        public ItemCondition Condition { get; set; }
        public string CampusLocation { get; set; }
        public Guid OwnerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public ListingState State { get; set; }

        public ListingResponseDto(Listing listing)
        {

            Id = listing.Id;
            Title = listing.Title;
            Description = listing.Description;
            Price = listing.Price;
            Category = listing.Category;
            Condition = listing.Condition;
            CampusLocation = listing.CampusLocation;
            OwnerId = listing.OwnerId;
            CreatedAt = listing.CreatedAt;
            State = listing.State;


        }





    }
}
