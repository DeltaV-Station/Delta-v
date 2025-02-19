using Content.Shared._DV.Ailments;
using Robust.Shared.Analyzers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Ailments;

/// <summary>
///     Contains the set of possible ailment packs & the currently active ailments in those packs, similar to a damage container
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AilmentComponent : Component
{
    [DataField, ViewVariables, AutoNetworkedField]
    public List<ProtoId<AilmentPackPrototype>> Packs = default!;

    [DataField, ViewVariables, AutoNetworkedField]
    public Dictionary<ProtoId<AilmentPackPrototype>, ProtoId<AilmentPrototype>?> ActiveAilments = new();
}
