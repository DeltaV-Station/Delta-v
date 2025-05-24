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
        ent.Comp.LastDrainedTime = GameTiming.CurTime;

        // TODO: Unique victims
        // TODO: Heal the drainer as they metabolize the Blood?? Done via other events?
        // TODO: Attempt to steal any Psionic abilities

        Dirty(ent);
    }
}
