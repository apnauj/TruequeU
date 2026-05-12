using TruequeU.Models;
using TruequeU.Models.DTOs;

namespace TruequeU.Interfaces
{
    public interface IListingService
    {

        Task<ListingResponseDto> Create(ListingCreateDTO lisitng, Guid ownerID);
    }
}
