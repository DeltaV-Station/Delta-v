using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Xenoarchaeology.Artifact.Components;

/// <summary>
/// Contains description / tips about the effect.
/// </summary>
[RegisterComponent]
public sealed partial class XAEDetailsComponent : Component
{
    /// <summary>
    /// Unique description of the effect.
    /// </summary>
    [DataField(required: true)]
    public LocId SpecificTip;

    /// <summary>
    /// Vague description of the effect - may be shared by other effects.
    /// </summary>
    [DataField]
    public LocId? VagueTip = null;

    /// <summary>
    /// Whether to permit this effect from being hidden until unlocked.
    /// </summary>
    [DataField]
    public bool AllowLockedEffectHiding = true;
}
