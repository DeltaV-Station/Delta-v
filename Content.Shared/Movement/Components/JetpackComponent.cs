using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Movement.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class JetpackComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? JetpackUser;

    [ViewVariables(VVAccess.ReadWrite), DataField("moleUsage")]
    public float MoleUsage = 0.012f;

    [DataField] public EntProtoId ToggleAction = "ActionToggleJetpack";

    [DataField, AutoNetworkedField] public EntityUid? ToggleActionEntity;

    [ViewVariables(VVAccess.ReadWrite), DataField("acceleration")]
    public float Acceleration = 1f;

    [ViewVariables(VVAccess.ReadWrite), DataField("friction")]
    public float Friction = 0.25f; // same as off-grid friction

    [ViewVariables(VVAccess.ReadWrite), DataField("weightlessModifier")]
    public float WeightlessModifier = 1.2f;

    // DeltaV Start - Jetpacks automatically toggle on.
    /// <summary>
    /// When toggling the jetpack, this will turn true/false.
    /// Upon leaving a grid, this will determine if the jetpack activates.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool AutomaticMode;

    /// <summary>
    /// The user whose jetpack is waiting to activate.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? AutomaticUser;
    // DeltaV End - Jetpacks automatically toggle on.
}
