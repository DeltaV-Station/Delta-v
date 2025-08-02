using Content.Server.Radio.Components;
using Content.Shared._DV.AACTablet;
using Content.Shared._starcup.AACTablet;

namespace Content.Server._DV.AACTablet;

public sealed partial class AACTabletSystem
{
    private HashSet<string> GetAvailableChannels(EntityUid entity)
    {
        var channels = new HashSet<string>();

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
