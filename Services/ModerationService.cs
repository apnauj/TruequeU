using Microsoft.EntityFrameworkCore;
using TruequeU.Enums;
using TruequeU.Interfaces;
using TruequeU.Models;
using TruequeU.Persistence;

namespace TruequeU.Services;

public class ModerationService : IModerationService
{
    private readonly ApplicationDbContext _context;

    public ModerationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ModerationAction> HideListingAsync(Guid adminId, Guid listingId, string reason)
    {
        var listing = await _context.Listings.FindAsync(listingId);
        if (listing is null)
            throw new InvalidOperationException("El artículo no existe.");

        if (listing.State == ListingState.Disable)
            throw new InvalidOperationException("El artículo ya está oculto.");

        listing.State = ListingState.Disable;

        var action = new ModerationAction(adminId, ModerationActionType.HideListing, reason, targetListingId: listingId);
        _context.ModerationActions.Add(action);

        await _context.SaveChangesAsync();
        return action;
    }

    public async Task<ModerationAction> UnhideListingAsync(Guid adminId, Guid listingId, string reason)
    {
        var listing = await _context.Listings.FindAsync(listingId);
        if (listing is null)
            throw new InvalidOperationException("El artículo no existe.");

        if (listing.State != ListingState.Disable)
            throw new InvalidOperationException("El artículo no está oculto.");

        listing.State = ListingState.Available;

        var action = new ModerationAction(adminId, ModerationActionType.UnhideListing, reason, targetListingId: listingId);
        _context.ModerationActions.Add(action);

        await _context.SaveChangesAsync();
        return action;
    }

    public async Task<ModerationAction> SuspendUserAsync(Guid adminId, Guid targetUserId, string reason)
    {
        var user = await _context.Users.FindAsync(targetUserId);
        if (user is null)
            throw new InvalidOperationException("El usuario no existe.");

        if (user.State == UserState.Suspended)
            throw new InvalidOperationException("El usuario ya está suspendido.");

        user.State = UserState.Suspended;

        var action = new ModerationAction(adminId, ModerationActionType.SuspendUser, reason, targetUserId: targetUserId);
        _context.ModerationActions.Add(action);

        await _context.SaveChangesAsync();
        return action;
    }

    public async Task<ModerationAction> UnsuspendUserAsync(Guid adminId, Guid targetUserId, string reason)
    {
        var user = await _context.Users.FindAsync(targetUserId);
        if (user is null)
            throw new InvalidOperationException("El usuario no existe.");

        if (user.State != UserState.Suspended)
            throw new InvalidOperationException("El usuario no está suspendido.");

        user.State = UserState.Active;

        var action = new ModerationAction(adminId, ModerationActionType.UnsuspendUser, reason, targetUserId: targetUserId);
        _context.ModerationActions.Add(action);

        await _context.SaveChangesAsync();
        return action;
    }

    public async Task<(List<ModerationAction> Actions, int TotalCount)> GetModerationHistoryAsync(int page = 1, int pageSize = 20)
    {
        var query = _context.ModerationActions
            .AsNoTracking()
            .Include(a => a.Admin)
            .Include(a => a.TargetListing)
            .Include(a => a.TargetUser)
            .OrderByDescending(a => a.CreatedAt);

        var totalCount = await query.CountAsync();

        var actions = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (actions, totalCount);
    }
}
