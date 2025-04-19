using Content.Server.Antag;
using Content.Server.Antag.Components;
using Content.Server.GameTicking.Rules.Components;
using Content.Shared._DV.Traitor;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Mind;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nuke;
using Content.Shared.Popups;

namespace Content.Server._DV.Nuke;

/// <summary>
/// When a syndie extracts the nuke disk, gives it to nukies as soon as possible.
/// If nukies are taking years and a sleeper steals it, an arbitrary nukie gets it.
/// If there are no nukies it waits until a loneop spawns.
/// </summary>
public sealed class NukeDiskSystem : EntitySystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly MobStateSystem _mob = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NukeDiskComponent, FultonedEvent>(OnFultoned);
        SubscribeLocalEvent<TeleportDiskRuleComponent, AfterAntagEntitySelectedEvent>(OnAntagEntSelected);
    }

    private void OnFultoned(Entity<NukeDiskComponent> ent, ref FultonedEvent args)
    {
        // no free win for using salv fultons
        if (!HasComp<ExtractingComponent>(ent))
            return;

        // just incase another system somehow doesn't do it
        RemCompDeferred<ExtractingComponent>(ent);

        ent.Comp.Extracted = true;
        // give it to an arbitrary nukie if a sleeper/whatever steals it
        // everyone wins
        if (FindLivingNukie() is {} target)
            TeleportDisk(ent, target);
    }

    private void OnAntagEntSelected(Entity<TeleportDiskRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        if (FindExtractedDisk() is not {} disk)
            return;

        // this nukie is arbitrary but its probably definitely a loneop anyway
        TeleportDisk(disk, args.EntityUid);
    }

    /// <summary>
    /// Tries to give the disk to a living nukie.
    /// </summary>
    public void TeleportDisk(Entity<NukeDiskComponent> ent, EntityUid target)
    {
        if (!ent.Comp.Extracted)
            return;

        ent.Comp.Extracted = false; // no repeated teleports

        _adminLogger.Add(LogType.Teleport, LogImpact.High, $"Teleported {ToPrettyString(ent):disk} to {ToPrettyString(target)} because it was extracted by a syndie");
        _hands.PickupOrDrop(target, ent);
        _popup.PopupEntity(Loc.GetString("nuke-disk-teleported", ("disk", ent)), target, target);
    }

    /// <summary>
    /// Find a nuke disk that has been stolen by a syndie via extraction fulton.
    /// </summary>
    public Entity<NukeDiskComponent>? FindExtractedDisk()
    {
        var query = EntityQueryEnumerator<NukeDiskComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Extracted)
                return (uid, comp);
        }

        return null;
    }

    /// <summary>
    /// Find a living nukie mob to give the disk to.
    /// </summary>
    public EntityUid? FindLivingNukie()
    {
        var query = EntityQueryEnumerator<NukeopsRuleComponent, AntagSelectionComponent>();
        while (query.MoveNext(out _, out _, out var comp))
        {
            foreach (var (mindId, _) in comp.AssignedMinds)
            {
                if (TryComp<MindComponent>(mindId, out var mind) &&
                    GetEntity(mind.OriginalOwnedEntity) is {} mob &&
                    _mob.IsAlive(mob))
                {
                    return mob;
                }
            }
        }

        return null;
    }
}
