using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;

namespace Content.Shared.Body.Events;

/// <summary>
/// Event called to get a list of reagents
/// that are whitelisted by a metabolizer organ.
/// </summary>
[ByRefEvent]
public readonly record struct MetabolismWhitelistEvent()
{
    public readonly List<ProtoId<ReagentPrototype>> Reagents = [];
}
