using Content.Shared.Implants;
using Content.Shared.NPC.Systems;
using Robust.Shared.Containers;

namespace Content.Shared.DeltaV.Implants.AddFactions;

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

        _npc.AddFactions(target, ent.Comp.Factions);
    }

    // TODO: Update this function to actually remove the factions correctly when removal of the implant is supported.
    private void OnRemove(Entity<AddFactionsImplantComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {

    }
}
