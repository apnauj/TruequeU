using Humanizer;
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


        public async Task<ListingResponseDto> UpdateAsync(ListingUpdateDTO dto, Guid listingId, Guid ownerId)
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

            if(listing_exist.State == ListingState.Disable)
            {
                throw new InvalidCastException("Disable listing cannot be update");
            }


            if (listing_exist.State == ListingState.Sold)
            {
                throw new InvalidOperationException("Sold listings cannot be updated.");
            }

            if (dto.Title is not null)
            {
                listing_exist.Title = dto.Title;
            }

            if (dto.Description is not null)
            {
                listing_exist.Description = dto.Description;
            }

            if (dto.Price.HasValue)
            {
                listing_exist.Price = dto.Price.Value;
            }

            if (dto.Category.HasValue)
            {
                listing_exist.Category = dto.Category.Value;
            }

            if (dto.Condition.HasValue)
            {
                listing_exist.Condition = dto.Condition.Value;
            }

            if (dto.CampusLocation is not null)
            {
                listing_exist.CampusLocation = dto.CampusLocation;
            }

            await _context.SaveChangesAsync();

            return new ListingResponseDto(listing_exist);


        }


        public async Task<bool> SoftDeleteAsync(Guid listingId, Guid ownerId)
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

            if(listing_exist.State = ListingState.Disable)
            {
                throw new InvalidOperationException("Listing  was alrady deleted");
            }

            listing_exist.State = ListingState.Disable;

            await _context.SaveChangesAsync();
            return true; 

        }











    }
}
