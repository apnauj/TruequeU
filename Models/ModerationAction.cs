using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TruequeU.Enums;

namespace TruequeU.Models;

public class ModerationAction
{
    [Key]
    public Guid Id { get; private set; }

    [Required]
    public Guid AdminId { get; set; }

    [ForeignKey(nameof(AdminId))]
    public User Admin { get; set; } = null!;

    public Guid? TargetListingId { get; set; }

    [ForeignKey(nameof(TargetListingId))]
    public Listing? TargetListing { get; set; }

    public Guid? TargetUserId { get; set; }

    [ForeignKey(nameof(TargetUserId))]
    public User? TargetUser { get; set; }

    [Required]
    public ModerationActionType Action { get; set; }

    [Required, MaxLength(500)]
    public string Reason { get; set; }

    public DateTime CreatedAt { get; private set; }

    private ModerationAction() { }

    public ModerationAction(Guid adminId, ModerationActionType action, string reason, Guid? targetListingId = null, Guid? targetUserId = null)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("El motivo de la acción de moderación es obligatorio.");

        if (targetListingId is null && targetUserId is null)
            throw new ArgumentException("Debe especificar un listing o un usuario como objetivo.");

        Id = Guid.NewGuid();
        AdminId = adminId;
        Action = action;
        Reason = reason;
        TargetListingId = targetListingId;
        TargetUserId = targetUserId;
        CreatedAt = DateTime.UtcNow;
    }
}
