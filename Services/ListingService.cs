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


        public async Task<ListingResponseDto> CreateAsync(ListingCreateDTO dto, Guid ownerID)
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
            await _context.SaveChangesAsync();
            var response = new ListingResponseDto(listing);

            return response;

        }


        public async Task<List<ListingResponseDto>> GetByOwnerId(Guid ownerId)
        {
            var lisitngs = await _context.Listing.Where(l => l.OwnerId == ownerId).ToListAsync();

            return lisitngs.Select(l => new ListingResponseDto(l)).ToList();

        }





       

    }
}
