using Content.Shared.Light.Components;

namespace Content.Server._DV.StationEvents.Components;

/// <summary>
/// This is used to configure the flickering lights game rule.
/// </summary>
[RegisterComponent]
public sealed partial class FlickeringLightsRuleComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? AffectedStation;

    /// <summary>
    /// A dictionary of affected lights, and whether it should reset the <see cref="PoweredLightComponent.IgnoreGhostsBoo"/> value.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<EntityUid, bool> AffectedEntities = new();

    [DataField]
    public float LightFlickerChance = 0.25f;
}
