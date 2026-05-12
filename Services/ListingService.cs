using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
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

        public async Task<ListingResponseDto> MarkAsSoldAsync(Guid listingId, Guid ownerId)
        {

            if (listingId == Guid.Empty)
            {
                throw new ArgumentException("ListingId is required.", nameof(listingId));
            }


            if (ownerId == Guid.Empty)
            {
                throw new ArgumentException("OwnerId is required.", nameof(ownerId));
            }




            var listing_exist = await _context.Listing.
                              FirstOrDefaultAsync(l => l.Id == listingId && l.OwnerId == ownerId); 
           


            if (listing_exist is null)
            {
                throw new InvalidOperationException("Listing does not exist or does not belong to the current user.");
            }

            if (listing_exist.State == ListingState.Disable) throw new InvalidOperationException("Listing is disabled");

            if (listing_exist.State == ListingState.Sold) throw new InvalidOperationException("Listing is alrady sold");

            listing_exist.State = ListingState.Sold;

            await _context.SaveChangesAsync();

            return new ListingResponseDto(listing_exist);


        }


        public async Task<ListingResponseDto> MarkAsReservedAsync(Guid listingId, Guid ownerId)
        {

            if (listingId == Guid.Empty)
            {
                throw new ArgumentException("ListingId is required.", nameof(listingId));
            }


            if (ownerId == Guid.Empty)
            {
                throw new ArgumentException("OwnerId is required.", nameof(ownerId));
            }




            var listing_exist = await _context.Listing.
                              FirstOrDefaultAsync(l => l.Id == listingId && l.OwnerId == ownerId); 
           


            if (listing_exist is null)
            {
                throw new InvalidOperationException("Listing does not exist or does not belong to the current user.");
            }

            if (listing_exist.State == ListingState.Disable) throw new InvalidOperationException("Listing is disabled");

            if (listing_exist.State == ListingState.Sold) throw new InvalidOperationException("Listing is alrady sold");

            if (listing_exist.State == ListingState.Reserved) throw new InvalidOperationException("Listing is alrady reserved");

            listing_exist.State = ListingState.Reserved;

            await _context.SaveChangesAsync();

            return new ListingResponseDto(listing_exist);


        }



        public async Task<ListingResponseDto> MarkAsAvailableAsync(Guid listingId, Guid ownerId)
        {

            if (listingId == Guid.Empty)
            {
                throw new ArgumentException("ListingId is required.", nameof(listingId));
            }


            if (ownerId == Guid.Empty)
            {
                throw new ArgumentException("OwnerId is required.", nameof(ownerId));
            }




            var listing_exist = await _context.Listing.
                              FirstOrDefaultAsync(l => l.Id == listingId && l.OwnerId == ownerId); 
            


            if (listing_exist is null)
            {
                throw new InvalidOperationException("Listing does not exist or does not belong to the current user.");
            }

            if (listing_exist.State == ListingState.Disable) throw new InvalidOperationException("Listing is disabled");

            if (listing_exist.State == ListingState.Sold) throw new InvalidOperationException("Lisitng is alrady sold");

            if (listing_exist.State == ListingState.Available) throw new InvalidOperationException("Listing is alrady Available");

            listing_exist.State = ListingState.Available;

            await _context.SaveChangesAsync();

            return new ListingResponseDto(listing_exist);


        }











    }
}
