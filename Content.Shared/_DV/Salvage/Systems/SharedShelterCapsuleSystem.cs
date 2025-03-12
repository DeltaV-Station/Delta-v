using Content.Shared._DV.Salvage.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Robust.Shared.Timing;

namespace Content.Shared._DV.Salvage.Systems;

/// <summary>
/// Handles interaction for shelter capsules.
/// Room spawning is done serverside.
/// </summary>
public abstract class SharedShelterCapsuleSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShelterCapsuleComponent, UseInHandEvent>(OnUse);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<ShelterCapsuleComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.NextSpawn is not {} nextSpawn || now < nextSpawn)
                continue;

            comp.User = null;
            comp.NextSpawn = null;
            if (TrySpawnRoom((uid, comp)) is {} id)
            {
                var msg = Loc.GetString(id, ("capsule", uid));
                _popup.PopupEntity(msg, uid, PopupType.LargeCaution);
            }
        }
    }

    /// <summary>
    /// Spawn the room, returning a locale string for an error. It gets "capsule" passed.
    /// </summary>
    protected virtual LocId? TrySpawnRoom(Entity<ShelterCapsuleComponent> ent)
    {
        return null;
    }

    private void OnUse(Entity<ShelterCapsuleComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled || ent.Comp.NextSpawn != null)
            return;

        args.Handled = true;

        var msg = Loc.GetString("shelter-capsule-warning", ("capsule", ent));
        _popup.PopupPredicted(msg, ent, args.User, PopupType.LargeCaution);

        ent.Comp.User = args.User;
        ent.Comp.NextSpawn = _timing.CurTime + ent.Comp.Delay;
    }
}
