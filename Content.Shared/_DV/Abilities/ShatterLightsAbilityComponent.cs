using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Abilities;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ShatterLightsAbilityComponent : Component
{
    /// <summary>
    /// The radius in which lights will be broken.
    /// </summary>
    [DataField]
    public float Radius = 10f;

    /// <summary>
    /// If true, lights will only be broken if the entity has line of sight to them.
    /// </summary>
    [DataField]
    public bool LineOfSight = false;

    /// <summary>
    /// The action that triggers the shatter lights ability.
    /// </summary>
    [DataField]
    public EntProtoId ShatterLightsActionId = "ActionShatterLights";

    /// <summary>
    /// Standing reference to the action entity, if it exists.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? ShatterLightsActionEntity;

    /// <summary>
    /// The sound to play when the ability is used.
    /// </summary>
    [DataField]
    public SoundSpecifier AbilitySound = new SoundPathSpecifier("/Audio/_DV/Effects/creepyshriek.ogg");
}

public sealed partial class ShatterLightsActionEvent : InstantActionEvent;
