using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Chemistry.Effects;

/// <summary>
/// Removes all psionics from an entity.
/// </summary>
[UsedImplicitly]
public sealed partial class ChemRemovePsionics : EventEntityEffect<ChemRemovePsionics>
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-chem-remove-psionic", ("chance", Probability));
}

