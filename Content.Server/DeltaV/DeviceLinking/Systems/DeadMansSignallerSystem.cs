using Content.Server.DeviceLinking.Components;
using Content.Server.DeviceLinking.Systems;
using Content.Shared.Hands;
using Content.Shared.Item.ItemToggle;

namespace Content.Server.DeltaV.DeviceLinking.Systems;

public sealed class DeadMansSignallerSystem : EntitySystem
{
    [Dependency] private readonly DeviceLinkSystem _link = default!;
    [Dependency] private readonly ItemToggleSystem _toggle = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SignallerComponent, GotUnequippedHandEvent>(DeadMans);
    }

    private void DeadMans(EntityUid uid, SignallerComponent component, GotUnequippedHandEvent args)
    {
        if (_toggle.IsActivated(uid))
        {
            if (HasComp<DeadMansSignallerComponent>(args.Unequipped))
                _link.InvokePort(uid, component.Port);
        }
    }
}
