using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Content.Server.Database;

//TODO
[Table("discord_accounts")]
public sealed class DiscordAccount
{
    [Key]
    public ulong Id { get; set; }

    public DiscordLinkedAccount LinkedAccount { get; set; } = default!;
    public List<DiscordLinkedAccountLogs> LinkedAccountLogs { get; set; } = default!;
}

[Table("discord_linked_accounts")]
public sealed class DiscordLinkedAccount
{
    [Key]
    public Guid PlayerId { get; set; }

    public Player Player { get; set; } = default!;

    public ulong DiscordId { get; set; }

    public DiscordAccount Discord { get; set; } = default!;
}

[Table("patreon_tiers")]
public sealed class DeltaVPatronTier
{
    [Key]
    public int Id { get; set; }

    public bool ShowOnCredits { get; set; }

    public string Name { get; set; } = default!;

    public ulong DiscordRole { get; set; }

    public int Priority { get; set; }

    public List<DeltaVPatron> Patrons { get; set; } = default!;
}

// TODO: Discord Linking - Do we name this patreon_patrons? I dont think its referencing the discord ID at all
[Table("patreon_patrons")]
[Index(nameof(TierId))]
public sealed class DeltaVPatron
{
    [Key]
    public Guid PlayerId { get; set; }

    public Player Player { get; set; } = default!;

    public int TierId { get; set; }

    // TODO: Discord Linking
    public DeltaVPatronTier Tier { get; set; } = default!;
}

[Table("discord_linking_codes")]
[Index(nameof(Code))]
public sealed class DiscordLinkingCodes
{
    [Key]
    public Guid PlayerId { get; set; }

    public Player Player { get; set; } = default!;

    public Guid Code { get; set; }

    public DateTime CreationTime { get; set; }
}

[Table("discord_linked_accounts_logs")]
[Index(nameof(PlayerId))]
[Index(nameof(DiscordId))]
[Index(nameof(At))]
public sealed class DiscordLinkedAccountLogs
{
    [Key]
    public int Id { get; set; }

    public Guid PlayerId { get; set; }

    public Player Player { get; set; } = default!;

    public ulong DiscordId { get; set; }

    public DiscordAccount Discord { get; set; } = default!;

    public DateTime At { get; set; }
}
