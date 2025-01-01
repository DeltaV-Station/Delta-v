using Content.Shared._DV.Antag;
using Content.Shared.Random;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Antag;

/// <summary>
///     Component holds what operations are possible and their weights.
/// </summary>
[RegisterComponent, Access(typeof(NukieOperationSystem))]
public sealed partial class NukieOperationComponent : Component
{
    /// <summary>
    ///     The different nukie operations.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<WeightedRandomPrototype> Operations;

    /// <summary>
    ///     The chosen operation. Is set after the first nukie spawns.
    /// </summary>
    [DataField]
    public ProtoId<NukieOperationPrototype>? ChosenOperation;
}

/// <summary>
///     Event to get update the nuke code paper to not actually have the code anymore.
/// </summary>
[ByRefEvent]
public record struct GetNukeCodePaperWriting(string? ToWrite);
