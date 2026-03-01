using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Implants.Components; // DeltaV
using Content.Shared.Radio;
using Content.Shared.Radio.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._EE.Radio;

public sealed class IntrinsicRadioKeySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IntrinsicRadioTransmitterComponent, EncryptionChannelsChangedEvent>(OnTransmitterChannelsChanged);
        SubscribeLocalEvent<ActiveRadioComponent, EncryptionChannelsChangedEvent>(OnReceiverChannelsChanged);
    }

    private void OnTransmitterChannelsChanged(EntityUid uid, IntrinsicRadioTransmitterComponent component, EncryptionChannelsChangedEvent args)
    {
        UpdateChannels(uid, args.Component, ref component.Channels);
    }

    private void OnReceiverChannelsChanged(EntityUid uid, ActiveRadioComponent component, EncryptionChannelsChangedEvent args)
    {
        UpdateChannels(uid, args.Component, ref component.Channels);
    }

    private void UpdateChannels(EntityUid uid, EncryptionKeyHolderComponent keyHolderComp, ref HashSet<ProtoId<RadioChannelPrototype>> channels) // DeltaV - passthrough uid
    {
        channels.Clear();
        channels.UnionWith(keyHolderComp.Channels);

        // Begin DeltaV Additions - Ensure radio implants continue to function
        if (TryComp<ImplantedComponent>(uid, out var implantedComp))
        {
            foreach (var implant in implantedComp.ImplantContainer.ContainedEntities)
            {
                if (!TryComp<RadioImplantComponent>(implant, out var radio))
                    continue;

                /*
                    Active added channels should already contain everything this implant
                    has added, so we can simply just add them back.
                */
                channels.UnionWith(radio.ActiveAddedChannels);
            }
        }
        // End DeltaV Additions
    }
}
