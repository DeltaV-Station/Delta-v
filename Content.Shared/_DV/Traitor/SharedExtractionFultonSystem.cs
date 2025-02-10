using Content.Shared.DoAfter;
using Content.Shared.Whitelist;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Traitor;

public abstract class SharedExtractionFultonSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    public EntityUid? FindBeacon(Entity<ExtractionFultonComponent> ent, EntityUid target)
    {
        // TODO: whitelist for the fulton
        var query = EntityQueryEnumerator<ExtractionBeaconComponent>();
        while (query.MoveNext(out var uid, out var beacon))
        {
            if (ValidTarget(beacon, target))
                return uid;
        }

        return null;
    }

    /// <summary>
    /// Returns whether an extraction beacon can accept a given target entity.
    /// </summary>
    public bool ValidTarget(ExtractionBeaconComponent comp, EntityUid uid)
    {
        return _whitelist.IsWhitelistPassOrNull(comp.Whitelist, uid)
            && !_whitelist.IsBlacklistPass(comp.Blacklist, uid);
    }
}

/// <summary>
/// Raised on an entity after it has been fultoned to somewhere.
/// </summary>
[ByRefEvent]
public record struct FultonedEvent;

[Serializable, NetSerializable]
public sealed partial class ExtractionFultonDoAfterEvent : SimpleDoAfterEvent
{
    [DataField]
    public NetEntity? Beacon;

    public ExtractionFultonDoAfterEvent(NetEntity? beacon = null)
    {
        Beacon = beacon;
    }
}
