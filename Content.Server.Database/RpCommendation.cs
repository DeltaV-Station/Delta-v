// Commendation (RP Rating) database model for "Чумацький Шлях" server

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Content.Server.Database;

/// <summary>
/// Stores RP commendation records — one player giving a "like" to another at round end.
/// Table: rp_commendations
/// </summary>
[Table("rp_commendations")]
[Index(nameof(ReceiverUserId))]
[Index(nameof(Timestamp))]
[Index(nameof(RoundId), nameof(SenderUserId), IsUnique = true)] // one vote per sender per round
public class RpCommendation
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("rp_commendation_id")]
    public int Id { get; set; }

    /// <summary>
    /// The round in which the commendation was given.
    /// </summary>
    [Column("round_id")]
    public int RoundId { get; set; }

    /// <summary>
    /// UserId (GUID) of the player who sent the commendation.
    /// </summary>
    [Column("sender_user_id")]
    public Guid SenderUserId { get; set; }

    /// <summary>
    /// UserId (GUID) of the player who received the commendation.
    /// </summary>
    [Column("receiver_user_id")]
    public Guid ReceiverUserId { get; set; }

    /// <summary>
    /// When the commendation was recorded.
    /// </summary>
    [Column("time")]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// If true, this commendation was given by an admin via /rp_award command
    /// rather than by a player vote.
    /// </summary>
    [Column("is_admin_award")]
    public bool IsAdminGrant { get; set; }

    /// <summary>
    /// Optional reason (used by admin grants).
    /// </summary>
    [Column("reason")]
    [MaxLength(256)]
    public string? Reason { get; set; }
}
