using Content.Shared.Radio.Components; // Delta-V
using Content.Shared._DV.AACTablet;
using Robust.Shared.Prototypes; // Delta-V
using Content.Shared.Radio; // Delta-V

namespace Content.Server._DV.AACTablet;

public sealed partial class AACTabletSystem
{
    private HashSet<ProtoId<RadioChannelPrototype>> GetAvailableChannels(EntityUid entity) // Delta-V
    {
        var channels = new HashSet<ProtoId<RadioChannelPrototype>>(); // Delta-V

        // Get all the intrinsic radio channels (IPCs, implants)
        if (TryComp(entity, out ActiveRadioComponent? intrinsicRadio))
            channels.UnionWith(intrinsicRadio.Channels);

        // Get the user's headset channels, if any
        if (TryComp(entity, out WearingHeadsetComponent? headset)
            && TryComp(headset.Headset, out ActiveRadioComponent? headsetRadio))
            channels.UnionWith(headsetRadio.Channels);

        return channels;
    }

    private void OnBoundUIOpened(Entity<AACTabletComponent> ent, ref BoundUIOpenedEvent args)
    {
        var state = new AACTabletBuiState(GetAvailableChannels(args.Actor));
        _userInterface.SetUiState(args.Entity, AACTabletKey.Key, state);
    }
}
