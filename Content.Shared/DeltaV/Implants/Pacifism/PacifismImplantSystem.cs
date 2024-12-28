using Robust.Shared.Containers;
using Content.Shared.Implants;
using Content.Shared.CombatMode.Pacification;

namespace Content.Shared.DeltaV.Implants.Pacifism;

public sealed class PacifismImplantSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PacifismImplantComponent, ImplantImplantedEvent>(OnImplantImplantedEvent);
        SubscribeLocalEvent<PacifismImplantComponent, EntGotRemovedFromContainerMessage>(OnRemove);
    }

    private void OnImplantImplantedEvent(Entity<PacifismImplantComponent> ent, ref ImplantImplantedEvent args)
    {
        if (args.Implanted is not {} target)
            return;

        PacifiedComponent? pacifiedComponent;
        if (TryComp(target, out pacifiedComponent))
        {
            ent.Comp.StoredDisallowDisarm = pacifiedComponent.DisallowDisarm;
            ent.Comp.StoredDisallowAllCombat = pacifiedComponent.DisallowAllCombat;
        }
        else
        {
            EnsureComp<PacifiedComponent>(target, out pacifiedComponent);
        }
        pacifiedComponent.DisallowAllCombat = ent.Comp.DisallowAllCombat;
        pacifiedComponent.DisallowDisarm = ent.Comp.DisallowDisarm;

        Dirty(target, pacifiedComponent);
    }

    private void OnRemove(Entity<PacifismImplantComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        // If one is not null the other one won't be null either.
        if (ent.Comp.StoredDisallowDisarm != null && ent.Comp.StoredDisallowAllCombat != null)
        {
            if (!TryComp<PacifiedComponent>(args.Container.Owner, out var pacifiedComponent))
                return;
            // Put in the old values
            pacifiedComponent.DisallowDisarm = ent.Comp.StoredDisallowDisarm.Value;
            pacifiedComponent.DisallowAllCombat = ent.Comp.StoredDisallowAllCombat.Value;
        }
        else
        {
            RemCompDeferred<PacifiedComponent>(args.Container.Owner);
        }
    }
}
