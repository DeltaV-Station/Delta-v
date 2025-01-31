using Content.Shared.Implants;
using Content.Shared.NPC.Systems;
using Robust.Shared.Containers;

namespace Content.Shared._DV.Implants.AddFactions;

public sealed class AddFactionsImplantSystem : EntitySystem
{
    [Dependency] private readonly NpcFactionSystem _npc = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AddFactionsImplantComponent, ImplantImplantedEvent>(OnImplantImplantedEvent);
        SubscribeLocalEvent<AddFactionsImplantComponent, EntGotRemovedFromContainerMessage>(OnRemove);
    }

    private void OnImplantImplantedEvent(Entity<AddFactionsImplantComponent> ent, ref ImplantImplantedEvent args)
    {
        if (args.Implanted is not {} target)
            return;

        foreach (var faction in ent.Comp.Factions)
        {
            if (_npc.IsMember(target, faction)) // If it's already in that faction, skip this.
                continue;

            _npc.AddFaction(target, faction);
            ent.Comp.AddedFactions.Add(faction);
        }
    }

    private void OnRemove(Entity<AddFactionsImplantComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        foreach (var faction in ent.Comp.AddedFactions)
            _npc.RemoveFaction(args.Container.Owner, faction);

        ent.Comp.AddedFactions.Clear();
    }
}
