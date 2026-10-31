using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;

namespace Content.Shared._starcup.Metabolism;

/// <summary>
/// starcup: Specifies which reagents a metabolizer organ is permitted to metabolize.
/// Adding this causes the metabolizing organ to be incapable of metabolizing anything not specified.
/// </summary>
[RegisterComponent]
public sealed partial class MetabolizerWhitelistComponent : Component
{
    [DataField(required: true)]
    public HashSet<ProtoId<ReagentPrototype>> ReagentWhitelist;
}
