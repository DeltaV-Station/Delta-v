using Robust.Shared.Prototypes;

namespace Content.Shared._DV.CartridgeLoader.Cartridges;

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
    public CrimeSeverity? CrimeSeverity;

    [DataField]
    public LocId? LocKeyPunishment;
}

/// <summary>
/// The severity a crime is in, used for page results.
/// </summary>
public enum CrimeSeverity : byte
{
    Innocent,
    Misdemeanour,
    Felony,
    Capital
}
