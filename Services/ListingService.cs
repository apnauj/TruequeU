using TruequeU.Interfaces;
using TruequeU.Models;
using TruequeU.Models.DTOs;

namespace TruequeU.Services
{
    public class ListingService : IListingService
    {
        private readonly ApplicationDbContext _context;
        public ListingService(ApplicationDbContext c)
        {
            _context = c;
        }


        public async Task<ListingResponseDto> Create(ListingCreateDTO dto, Guid ownerID)
        {
            var listing = new Listing(
                dto.Title,
                dto.Description,
                dto.Price,
                dto.Category,
                dto.Condition,
                dto.CampusLocation,
                ownerID
                );

            _context.Listing.Add(listing);


            var response = new ListingResponseDto(listing);

            await _context.SaveChangesAsync();

            return response;

        }

    }
}
