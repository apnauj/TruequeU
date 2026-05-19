using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TruequeU.Enums;
using TruequeU.Interfaces;
using TruequeU.Models;
using TruequeU.Models.DTOs;
using TruequeU.Persistence;

namespace TruequeU.Services;

public class ListingService : IListingService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ListingService> _logger;

    public ListingService(ApplicationDbContext context, ILogger<ListingService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ListingResponseDto> CreateAsync(ListingCreateDTO dto, Guid ownerId)
    {
        _logger.LogDebug("Creating listing for owner {OwnerId}", ownerId);

        var listing = new Listing(
            dto.Title,
            dto.Description,
            dto.Price,
            dto.Category,
            dto.Condition,
            dto.CampusLocation,
            ownerId
        );

        _context.Listings.Add(listing);
        await _context.SaveChangesAsync().ConfigureAwait(false);

        _logger.LogInformation("Listing {ListingId} created by user {OwnerId}", listing.Id, ownerId);

        return new ListingResponseDto(listing);
    }

    public async Task<List<ListingResponseDto>> GetByOwnerIdAsync(Guid ownerId)
    {
        if (ownerId == Guid.Empty)
        {
            throw new ArgumentException("OwnerId is required.", nameof(ownerId));
        }

        var listings = await _context.Listings
            .Where(l => l.OwnerId == ownerId)
            .ToListAsync()
            .ConfigureAwait(false);

        return listings.Select(l => new ListingResponseDto(l)).ToList();
    }

    public async Task<List<ListingResponseDto>> GetAllAsync()
    {
        var listings = await _context.Listings
            .Where(l => l.State != ListingState.Disable)
            .ToListAsync()
            .ConfigureAwait(false);

        return listings.Select(l => new ListingResponseDto(l)).ToList();
    }

    public async Task<ListingResponseDto> MarkAsSoldAsync(Guid listingId, Guid ownerId)
    {
        var listing = await GetAndValidateOwnedListingAsync(listingId, ownerId).ConfigureAwait(false);

        if (listing.State == ListingState.Disable)
            throw new InvalidOperationException("Listing is disabled");

        if (listing.State == ListingState.Sold)
            throw new InvalidOperationException("Listing is already sold");

        listing.State = ListingState.Sold;

        await _context.SaveChangesAsync().ConfigureAwait(false);

        _logger.LogInformation("Listing {ListingId} marked as sold by owner {OwnerId}", listingId, ownerId);

        return new ListingResponseDto(listing);
    }

    public async Task<ListingResponseDto> MarkAsReservedAsync(Guid listingId, Guid ownerId)
    {
        var listing = await GetAndValidateOwnedListingAsync(listingId, ownerId).ConfigureAwait(false);

        if (listing.State == ListingState.Disable)
            throw new InvalidOperationException("Listing is disabled");

        if (listing.State == ListingState.Sold)
            throw new InvalidOperationException("Listing is already sold");

        if (listing.State == ListingState.Reserved)
            throw new InvalidOperationException("Listing is already reserved");

        listing.State = ListingState.Reserved;

        await _context.SaveChangesAsync().ConfigureAwait(false);

        _logger.LogInformation("Listing {ListingId} marked as reserved by owner {OwnerId}", listingId, ownerId);

        return new ListingResponseDto(listing);
    }

    public async Task<ListingResponseDto> MarkAsAvailableAsync(Guid listingId, Guid ownerId)
    {
        var listing = await GetAndValidateOwnedListingAsync(listingId, ownerId).ConfigureAwait(false);

        if (listing.State == ListingState.Disable)
            throw new InvalidOperationException("Listing is disabled");

        if (listing.State == ListingState.Sold)
            throw new InvalidOperationException("Listing is already sold");

        if (listing.State == ListingState.Available)
            throw new InvalidOperationException("Listing is already Available");

        listing.State = ListingState.Available;

        await _context.SaveChangesAsync().ConfigureAwait(false);

        _logger.LogInformation("Listing {ListingId} marked as available by owner {OwnerId}", listingId, ownerId);

        return new ListingResponseDto(listing);
    }

    public async Task<ListingResponseDto> UpdateAsync(ListingUpdateDTO dto, Guid listingId, Guid ownerId)
    {
        var listing = await GetAndValidateOwnedListingAsync(listingId, ownerId).ConfigureAwait(false);

        if (listing.State == ListingState.Disable)
        {
            throw new InvalidOperationException("Disabled listings cannot be updated.");
        }

        if (listing.State == ListingState.Sold)
        {
            throw new InvalidOperationException("Sold listings cannot be updated.");
        }

        if (dto.Title is not null)
        {
            listing.Title = dto.Title;
        }

        if (dto.Description is not null)
        {
            listing.Description = dto.Description;
        }

        if (dto.Price.HasValue)
        {
            listing.Price = dto.Price.Value;
        }

        if (dto.Category.HasValue)
        {
            listing.Category = dto.Category.Value;
        }

        if (dto.Condition.HasValue)
        {
            listing.Condition = dto.Condition.Value;
        }

        if (dto.CampusLocation is not null)
        {
            listing.CampusLocation = dto.CampusLocation;
        }

        await _context.SaveChangesAsync().ConfigureAwait(false);

        _logger.LogInformation("Listing {ListingId} updated by owner {OwnerId}", listingId, ownerId);

        return new ListingResponseDto(listing);
    }

    public async Task<bool> SoftDeleteAsync(Guid listingId, Guid ownerId)
    {
        var listing = await GetAndValidateOwnedListingAsync(listingId, ownerId).ConfigureAwait(false);

        if (listing.State == ListingState.Disable)
        {
            throw new InvalidOperationException("Listing was already deleted");
        }

        listing.State = ListingState.Disable;

        await _context.SaveChangesAsync().ConfigureAwait(false);

        _logger.LogInformation("Listing {ListingId} soft-deleted by owner {OwnerId}", listingId, ownerId);

        return true;
    }

    private async Task<Listing> GetAndValidateOwnedListingAsync(Guid listingId, Guid ownerId)
    {
        if (listingId == Guid.Empty)
        {
            throw new ArgumentException("ListingId is required.", nameof(listingId));
        }

        if (ownerId == Guid.Empty)
        {
            throw new ArgumentException("OwnerId is required.", nameof(ownerId));
        }

        var listing = await _context.Listings
            .FirstOrDefaultAsync(l => l.Id == listingId && l.OwnerId == ownerId)
            .ConfigureAwait(false);

        if (listing is null)
        {
            throw new InvalidOperationException("Listing does not exist or does not belong to the current user.");
        }

        return listing;
    }
}
