using Content.Shared._DV.Implants.Components;
using Content.Shared._DV.Implants.Events;
using Content.Shared.Implants;
using Content.Shared.Popups;

namespace Content.Shared._DV.Implants.Systems;

/// <summary>
/// This handles the check for exclusive implants.
/// </summary>
public sealed class DVExclusiveImplantSystem : EntitySystem
{
    [Dependency] private SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<DVExclusiveImplantComponent, AddImplantAttemptEvent>(OnImplantAttempt);
        SubscribeLocalEvent<DVExclusiveImplantComponent, ImplantRelayEvent<DVExclusiveImplantAttemptEvent>>(OnExclusiveAttempt);
    }

    private void OnImplantAttempt(Entity<DVExclusiveImplantComponent> implant, ref AddImplantAttemptEvent args)
    {
        var ev = new DVExclusiveImplantAttemptEvent(implant.Comp.ExclusiveKey);
        RaiseLocalEvent(args.Target, ref ev);

        if (ev.Cancelled)
          args.Cancel();
    }

    private void OnExclusiveAttempt(Entity<DVExclusiveImplantComponent> implant, ref ImplantRelayEvent<DVExclusiveImplantAttemptEvent> args)
    {
        if (implant.Comp.ExclusiveKey.Equals(args.Event.ExclusiveKey))
            args.Event.Cancel();
    }
}
