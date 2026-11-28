using Content.Client.Radio.Ui;
using Content.Shared.Radio;
using Content.Shared.Radio.Components;
using Content.Shared.Radio.EntitySystems;
using Robust.Client.GameObjects;

namespace Content.Client.Radio.EntitySystems;

public sealed class RadioDeviceSystem : SharedRadioDeviceSystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        // BEGIN DeltaV - Update intercom UI on RadioMicrophone/Speaker component statechange
        base.Initialize();
        SubscribeLocalEvent<RadioMicrophoneComponent, AfterAutoHandleStateEvent>(OnAfterHandleState);
        SubscribeLocalEvent<RadioSpeakerComponent, AfterAutoHandleStateEvent>(OnAfterHandleState);
        // END DeltaV
        SubscribeLocalEvent<IntercomComponent, AfterAutoHandleStateEvent>(OnAfterHandleState);
    }

    // DeltaV - Made generic. `component` unused, but required for subscriptions.
    private void OnAfterHandleState<TComp>(EntityUid uid, TComp component, AfterAutoHandleStateEvent args) where TComp : IComponent
    {
        // BEGIN DeltaV
        IntercomComponent? intercom = null;
        if (!Resolve(uid, ref intercom, false))
            return;

        var ent = new Entity<IntercomComponent>(uid, intercom);
        // END DeltaV

        if (_ui.TryGetOpenUi<IntercomBoundUserInterface>(ent.Owner, IntercomUiKey.Key, out var bui))
            bui.Update(ent);
    }
}
