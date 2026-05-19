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

    public async Task<ListingResponseDto?> GetByIdAsync(Guid id)
    {
        var listing = await _context.Listings
            .Include(l => l.Images)
            .FirstOrDefaultAsync(l => l.Id == id)
            .ConfigureAwait(false);

        return listing is null ? null : new ListingResponseDto(listing);
    }

    public async Task<ListingResponseDto> CreateAsync(ListingCreateDTO dto, Guid ownerId)
    {
        _logger.LogDebug("Creating listing for owner {OwnerId} with {ImageCount} images", ownerId, dto.ImageUrls.Count);

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

        for (int i = 0; i < dto.ImageUrls.Count; i++)
        {
            var image = new ListingImage(
                dto.ImageUrls[i],
                listing.Id,
                isPrimary: i == 0,
                displayOrder: i
            );
            listing.Images.Add(image);
        }

        await _context.SaveChangesAsync().ConfigureAwait(false);

        _logger.LogInformation("Listing {ListingId} created by user {OwnerId} with {ImageCount} images", listing.Id, ownerId, dto.ImageUrls.Count);

        return new ListingResponseDto(listing);
    }

    public async Task<List<ListingResponseDto>> GetByOwnerIdAsync(Guid ownerId)
    {
        if (ownerId == Guid.Empty)
        {
            throw new ArgumentException("OwnerId is required.", nameof(ownerId));
        }

        var listings = await _context.Listings
            .Include(l => l.Images)
            .Where(l => l.OwnerId == ownerId)
            .ToListAsync()
            .ConfigureAwait(false);

        return listings.Select(l => new ListingResponseDto(l)).ToList();
    }

    public async Task<PagedResult<ListingResponseDto>> GetAllAsync(ListingFilterDto filter)
    {
        var query = _context.Listings
            .Include(l => l.Images)
            .Where(l => l.State != ListingState.Disable)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim();
            query = query.Where(l =>
                l.Title.Contains(keyword) ||
                l.Description.Contains(keyword));
        }

        if (filter.Category.HasValue)
            query = query.Where(l => l.Category == filter.Category.Value);

        if (filter.MinPrice.HasValue)
            query = query.Where(l => l.Price >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(l => l.Price <= filter.MaxPrice.Value);

        if (filter.Condition.HasValue)
            query = query.Where(l => l.Condition == filter.Condition.Value);

        if (filter.State.HasValue)
            query = query.Where(l => l.State == filter.State.Value);

        if (filter.PostedAfter.HasValue)
            query = query.Where(l => l.CreatedAt >= filter.PostedAfter.Value);

        var totalCount = await query.CountAsync().ConfigureAwait(false);

        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync()
            .ConfigureAwait(false);

        return new PagedResult<ListingResponseDto>
        {
            Items = items.Select(l => new ListingResponseDto(l)).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
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

    public async Task<ListingImageDto> AddImageAsync(Guid listingId, Guid ownerId, string imageUrl)
    {
        if (listingId == Guid.Empty)
            throw new ArgumentException("ListingId is required.", nameof(listingId));

        if (ownerId == Guid.Empty)
            throw new ArgumentException("OwnerId is required.", nameof(ownerId));

        var listing = await _context.Listings
            .Include(l => l.Images)
            .FirstOrDefaultAsync(l => l.Id == listingId && l.OwnerId == ownerId)
            .ConfigureAwait(false);

        if (listing is null)
            throw new InvalidOperationException("Listing does not exist or does not belong to the current user.");

        if (listing.State == ListingState.Disable || listing.State == ListingState.Sold)
            throw new InvalidOperationException("Cannot add images to a disabled or sold listing.");

        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("Image URL is required.", nameof(imageUrl));

        var displayOrder = listing.Images.Count;
        var image = new ListingImage(imageUrl, listingId, displayOrder: displayOrder);
        listing.Images.Add(image);

        await _context.SaveChangesAsync().ConfigureAwait(false);

        _logger.LogInformation("Image {ImageId} added to listing {ListingId} by user {OwnerId}", image.Id, listingId, ownerId);

        return new ListingImageDto
        {
            Id = image.Id,
            Url = image.Url,
            IsPrimary = image.IsPrimary,
            AltText = image.AltText,
            DisplayOrder = image.DisplayOrder
        };
    }

    public async Task<bool> RemoveImageAsync(Guid listingId, Guid ownerId, Guid imageId)
    {
        if (listingId == Guid.Empty)
            throw new ArgumentException("ListingId is required.", nameof(listingId));

        if (ownerId == Guid.Empty)
            throw new ArgumentException("OwnerId is required.", nameof(ownerId));

        var listing = await _context.Listings
            .Include(l => l.Images)
            .FirstOrDefaultAsync(l => l.Id == listingId && l.OwnerId == ownerId)
            .ConfigureAwait(false);

        if (listing is null)
            throw new InvalidOperationException("Listing does not exist or does not belong to the current user.");

        if (listing.State == ListingState.Disable || listing.State == ListingState.Sold)
            throw new InvalidOperationException("Cannot remove images from a disabled or sold listing.");

        if (listing.Images.Count <= 3)
            throw new InvalidOperationException("A listing must have at least 3 images.");

        var image = listing.Images.FirstOrDefault(i => i.Id == imageId);
        if (image is null)
            return false;

        listing.Images.Remove(image);
        _context.ListingImages.Remove(image);

        await _context.SaveChangesAsync().ConfigureAwait(false);

        _logger.LogInformation("Image {ImageId} removed from listing {ListingId} by user {OwnerId}", imageId, listingId, ownerId);

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
