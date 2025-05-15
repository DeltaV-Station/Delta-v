using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Content.Shared.Salvage.Fulton;
using Content.Shared.Verbs;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Traitor;

public abstract class SharedExtractionFultonSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] protected readonly SharedPopupSystem Popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExtractionFultonComponent, GetVerbsEvent<UtilityVerb>>(OnGetVerbs);
        SubscribeLocalEvent<FultonedComponent, ContainerGettingInsertedAttemptEvent>(OnInsertAttempt);
    }

    private void OnGetVerbs(Entity<ExtractionFultonComponent> ent, ref GetVerbsEvent<UtilityVerb> args)
    {
        var target = args.Target;
        var user = args.User;
        args.Verbs.Add(new UtilityVerb()
        {
            Act = () => AttachFulton(ent, target, user),
            Text = Loc.GetString("extraction-fulton-verb-text"),
            Disabled = FindBeacon(ent, target) == null
        });
    }

    private void OnInsertAttempt(Entity<FultonedComponent> ent, ref ContainerGettingInsertedAttemptEvent args)
    {
        args.Cancel();
        if (_net.IsServer)
            Popup.PopupEntity(Loc.GetString("extraction-fulton-remove-first"), ent);
    }

    protected virtual void AttachFulton(Entity<ExtractionFultonComponent> ent, EntityUid target, EntityUid user)
    {
    }

    public EntityUid? FindBeacon(Entity<ExtractionFultonComponent> ent, EntityUid target)
    {
        // TODO: whitelist for the fulton to support non-traitor uses
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
