using TruequeU.Enums;
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


        public async Task<List<ListingResponseDto>> GetByOwnerIdAsync(Guid ownerId)
        {

            if (ownerId == Guid.Empty)
            {
                throw new ArgumentException("OwnerId is required.", nameof(ownerId));

            }
            var listings = await _context.Listing
                .Where(l => l.OwnerId == ownerId).ToListAsync();

            return listings.Select(l => new ListingResponseDto(l)).ToList();

        }

        public async Task<List<ListingResponseDto>> GetAllAsync()
        {
            var listings = await _context.Listing
                .Where(l => l.State != ListingState.Disable).ToListAsync();

            return listings.Select(l => new ListingResponseDto(l)).ToList();
        }

        





       

    }
}
