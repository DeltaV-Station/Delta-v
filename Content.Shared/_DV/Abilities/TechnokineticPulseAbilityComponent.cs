using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Abilities;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TechnokineticPulseAbilityComponent : Component
{
    /// <summary>
    /// The radius in which to EMP
    /// </summary>
    [DataField]
    public float Range = 1.0f;

    /// <summary>
    /// The amount of power to drain from batteries
    /// </summary>
    [DataField]
    public float EnergyConsumption = 20000;

    /// <summary>
    /// The duration for which devices are disabled.
    /// </summary>
    [DataField]
    public TimeSpan DisableDuration = TimeSpan.FromSeconds(20f);

    /// <summary>
    /// The action that triggers the technokinetic pulse ability.
    /// </summary>
    [DataField]
    public EntProtoId TechnokineticPulseActionId = "ActionTechnokineticPulse";

    /// <summary>
    /// Standing reference to the action entity, if it exists.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? TechnokineticPulseActionEntity;
}

public sealed partial class TechnokineticPulseActionEvent : InstantActionEvent;
