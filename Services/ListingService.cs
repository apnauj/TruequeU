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


        public async Task<ListingCreateDTO> Create(ListingCreateDTO dto, Guid ownerID)
        {
            var listing = new Listing(
                dto.title,
                dto.description,
                dto.price,
                dto.category,
                dto.condition,
                dto.campusLocation,
                ownerID
                );

            _context.Listing.Add(listing);

            await _context.SaveChangesAsync();

            return dto;

        }

    }
}
