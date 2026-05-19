using TruequeU.Models;
using TruequeU.Models.DTOs;

namespace TruequeU.Interfaces;

public interface IListingService
{
    Task<ListingResponseDto> CreateAsync(ListingCreateDTO listing, Guid ownerId);

    Task<List<ListingResponseDto>> GetByOwnerIdAsync(Guid ownerId);

    Task<List<ListingResponseDto>> GetAllAsync();

    Task<ListingResponseDto> MarkAsSoldAsync(Guid listingId, Guid ownerId);

    Task<ListingResponseDto> MarkAsReservedAsync(Guid listingId, Guid ownerId);

    Task<ListingResponseDto> MarkAsAvailableAsync(Guid listingId, Guid ownerId);

    Task<ListingResponseDto> UpdateAsync(ListingUpdateDTO listing, Guid listingId, Guid ownerId);

    Task<bool> SoftDeleteAsync(Guid listingId, Guid ownerId);

    Task<ListingImageDto> AddImageAsync(Guid listingId, Guid ownerId, string imageUrl);

    Task<bool> RemoveImageAsync(Guid listingId, Guid ownerId, Guid imageId);
}
