using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TruequeU.Enums;
using TruequeU.Interfaces;
using TruequeU.Models;
using TruequeU.Models.DTOs;
using TruequeU.Persistence;

namespace TruequeU.Services;

public class FavoriteService : IFavoriteService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FavoriteService> _logger;

    public FavoriteService(ApplicationDbContext context, ILogger<FavoriteService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<FavoriteResponseDto> AddFavoriteAsync(Guid listingId, Guid userId)
    {
        if (listingId == Guid.Empty)
            throw new ArgumentException("ListingId is required.", nameof(listingId));

        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));

        var listing = await _context.Listings
            .FirstOrDefaultAsync(l => l.Id == listingId)
            .ConfigureAwait(false);

        if (listing is null)
            throw new InvalidOperationException("Listing does not exist.");

        if (listing.State == ListingState.Disable)
            throw new InvalidOperationException("Cannot favorite a disabled listing.");

        var existingFavorite = await _context.Favorites
            .FirstOrDefaultAsync(f => f.ListingId == listingId && f.UserId == userId)
            .ConfigureAwait(false);

        if (existingFavorite is not null)
            throw new InvalidOperationException("Listing is already in favorites.");

        var favorite = new Favorite(userId, listingId);
        _context.Favorites.Add(favorite);
        await _context.SaveChangesAsync().ConfigureAwait(false);

        var created = await _context.Favorites
            .Include(f => f.Listing)
            .FirstAsync(f => f.Id == favorite.Id)
            .ConfigureAwait(false);

        _logger.LogInformation("User {UserId} favorited listing {ListingId}", userId, listingId);

        return new FavoriteResponseDto(created);
    }

    public async Task<bool> RemoveFavoriteAsync(Guid listingId, Guid userId)
    {
        if (listingId == Guid.Empty)
            throw new ArgumentException("ListingId is required.", nameof(listingId));

        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));

        var favorite = await _context.Favorites
            .FirstOrDefaultAsync(f => f.ListingId == listingId && f.UserId == userId)
            .ConfigureAwait(false);

        if (favorite is null)
            return false;

        _context.Favorites.Remove(favorite);
        await _context.SaveChangesAsync().ConfigureAwait(false);

        _logger.LogInformation("User {UserId} removed favorite from listing {ListingId}", userId, listingId);

        return true;
    }

    public async Task<List<FavoriteResponseDto>> GetUserFavoritesAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));

        var favorites = await _context.Favorites
            .Include(f => f.Listing)
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.FavoritedAt)
            .ToListAsync()
            .ConfigureAwait(false);

        return favorites.Select(f => new FavoriteResponseDto(f)).ToList();
    }
}
