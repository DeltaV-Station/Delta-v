using Content.Shared._DV.Psionics.Components;
using Content.Shared.Implants;

namespace Content.Shared._DV.Psionics.Systems;

public abstract partial class SharedPsionicSystem
{
    public void InitializeImplants()
    {
        SubscribeLocalEvent<DVImplantRequiresPsionicRollComponent, AddImplantAttemptEvent>(OnImplantAttempt);
        SubscribeLocalEvent<DVImplantRequiresPsionicRollComponent, ImplantImplantedEvent>(OnImplantAdded);
    }

    private void OnImplantAttempt(Entity<DVImplantRequiresPsionicRollComponent> implant, ref AddImplantAttemptEvent args)
    {
        if (PotentialQuery.TryComp(args.Target, out var comp) && comp.Rolled)
            args.Cancel();
    }

    private void OnImplantAdded(Entity<DVImplantRequiresPsionicRollComponent> implant, ref ImplantImplantedEvent args)
    {
        if (!PotentialQuery.TryComp(args.Implanted, out var comp))
            return;

        comp.Rolled = true;
    }
}
