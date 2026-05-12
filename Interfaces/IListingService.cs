using TruequeU.Models;
using TruequeU.Models.DTOs;

namespace TruequeU.Interfaces
{
    public interface IListingService
    {

        
        Task<ListingResponseDto> CreateAsync(ListingCreateDTO listing, Guid ownerID);


        Task<List<ListingResponseDto>> GetByOwnerId(Guid ownerId);
    }
}
