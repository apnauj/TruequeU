using TruequeU.Models;
using TruequeU.Models.DTOs;

namespace TruequeU.Interfaces
{
    public interface IListingService
    {

        Task<ListingCreateDTO> Create(ListingCreateDTO lisitng);
    }
}
