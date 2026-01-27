// File to store DeltaV-specific database models

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Content.Server.Database;

// ReSharper disable once InconsistentNaming
public static class DVModel
{
    /// <summary>
    /// Stores tip dismissal preferences for a player, tracking which tips they've marked
    /// as "Don't show again".
    /// </summary>
    [Table("dv_seen_tips")]
    [Index(nameof(PlayerUserId), nameof(TipProtoId), IsUnique = true)]
    public class SeenTip
    {
        public int Id { get; set; }

        /// <summary>
        /// The player's user ID (GUID). References the Player table.
        /// </summary>
        public Guid PlayerUserId { get; set; }

        /// <summary>
        /// The prototype ID of the tip that was dismissed.
        /// </summary>
        [MaxLength(64)]
        public string TipProtoId { get; set; } = string.Empty;

        /// <summary>
        /// When the tip was dismissed.
        /// </summary>
        public DateTime DismissedAt { get; set; }
    }
}
