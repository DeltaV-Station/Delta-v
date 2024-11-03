using Robust.Shared.Prototypes;

namespace Content.Shared.DeltaV.CartridgeLoader.Cartridges;

[Prototype("crimeAssistPage")]
public sealed partial class CrimeAssistPage : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = "";

    [DataField]
    public ProtoId<CrimeAssistPage>? OnStart;

    [DataField]
    public LocId? LocKey;

    [DataField]
    public ProtoId<CrimeAssistPage>? OnYes;

    [DataField]
    public ProtoId<CrimeAssistPage>? OnNo;

    [DataField]
    public LocId? LocKeyTitle;

    [DataField]
    public LocId? LocKeyDescription;

    [DataField]
    public LocId? LocKeySeverity;

    [DataField]
    public LocId? LocKeyPunishment;
}
