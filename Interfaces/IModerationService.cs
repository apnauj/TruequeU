using TruequeU.Models;

namespace TruequeU.Interfaces;

public interface IModerationService
{
    Task<ModerationAction> HideListingAsync(Guid adminId, Guid listingId, string reason);
    Task<ModerationAction> UnhideListingAsync(Guid adminId, Guid listingId, string reason);
    Task<ModerationAction> SuspendUserAsync(Guid adminId, Guid targetUserId, string reason);
    Task<ModerationAction> UnsuspendUserAsync(Guid adminId, Guid targetUserId, string reason);
    Task<(List<ModerationAction> Actions, int TotalCount)> GetModerationHistoryAsync(int page = 1, int pageSize = 20);
}
