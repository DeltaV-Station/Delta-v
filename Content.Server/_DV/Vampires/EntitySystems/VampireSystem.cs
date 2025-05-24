using Content.Server.Polymorph.Components;
using Content.Shared._DV.BloodDraining.Events;
using Content.Shared._DV.Vampires.Components;
using Content.Shared._DV.Vampires.EntitySystems;

namespace Content.Server._DV.Vampires.EntitySystems;

public sealed class VampireSystem : SharedVampireSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VampireComponent, BloodDrainedEvent>(OnBloodDrained);
    }

    private void OnBloodDrained(Entity<VampireComponent> ent, ref BloodDrainedEvent args)
    {
        var victim = args.Victim;

        // Ensure polymorphed victims only count once
        if (TryComp<PolymorphedEntityComponent>(victim, out var polymorphed))
            victim = polymorphed.Parent;

        if (ent.Comp.UniqueVictims.Add(victim))
            OnNewUniqueVictim(ent);

        ent.Comp.LastDrainedTime = GameTiming.CurTime;

        // TODO: Heal the drainer as they metabolize the Blood?? Done via other events?
        // TODO: Attempt to steal any Psionic abilities

        Dirty(ent);
    }

    private void OnNewUniqueVictim(Entity<VampireComponent> ent)
    {
        // TODO: Update bonuses
    }
}
