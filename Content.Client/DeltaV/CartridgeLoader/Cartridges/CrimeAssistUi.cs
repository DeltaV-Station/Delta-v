using Robust.Client.UserInterface;
using Content.Client.UserInterface.Fragments;
using Content.Shared.DeltaV.CartridgeLoader.Cartridges;
using Content.Shared.CartridgeLoader;
using Robust.Shared.Prototypes;

namespace Content.Client.DeltaV.CartridgeLoader.Cartridges;

public sealed partial class CrimeAssistUi : UIFragment
{
    private CrimeAssistUiFragment? _fragment;

    public override Control GetUIFragmentRoot()
    {
        return _fragment!;
    }

    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner)
    {
        _fragment = new CrimeAssistUiFragment();

        _fragment.OnSync += _ => SendSyncMessage(userInterface);
    }

    private void SendSyncMessage(BoundUserInterface userInterface)
    {
        var syncMessage = new CrimeAssistSyncMessageEvent();
        var message = new CartridgeUiMessage(syncMessage);
        userInterface.SendMessage(message);
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
    }

    [Prototype("crimeAssistPage")]
    public sealed partial class CrimeAssistPage : IPrototype
    {
        [ViewVariables]
        [IdDataField]
        public string ID { get; private set; } = "";

        [DataField("onStart")]
        public string? OnStart { get; private set; }

        [DataField("locKey")]
        public string? LocKey { get; private set; }

        [DataField("onYes")]
        public string? OnYes { get; private set; }

        [DataField("onNo")]
        public string? OnNo { get; private set; }

        [DataField("locKeyTitle")]
        public string? LocKeyTitle { get; private set; }

        [DataField("locKeyDescription")]
        public string? LocKeyDescription { get; private set; }

        [DataField("locKeySeverity")]
        public string? LocKeySeverity { get; private set; }

        [DataField("locKeyPunishment")]
        public string? LocKeyPunishment { get; private set; }
    }
}
