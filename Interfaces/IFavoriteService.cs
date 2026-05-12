using TruequeU.Models.DTOs;

namespace TruequeU.Interfaces
{
    public interface IFavoriteService
    {
        Task<FavoriteResponseDto> AddFavoriteAsync(Guid listingId, Guid userId);

        Task<bool> RemoveFavoriteAsync(Guid listingId, Guid userId);

        Task<List<FavoriteResponseDto>> GetUserFavoritesAsync(Guid userId);
    }
}
