using Content.Server._DV.DeviceLinking.Components;
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

        SubscribeLocalEvent<DeadMansSignallerComponent, GotUnequippedHandEvent>(DeadMans);
    }

    private void DeadMans(EntityUid uid, DeadMansSignallerComponent component, GotUnequippedHandEvent args)
    {
        if (_toggle.IsActivated(uid))
        {
            _link.InvokePort(uid, component.Port);
        }
    }
}
