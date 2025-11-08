using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Weapons.Ranged.Components;

/// <summary>
/// Alters the accuracy and firerate of the gun after a DoAfter, immoblizing the wielder.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GunBipodComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("minAngle"), AutoNetworkedField]
    public Angle MinAngle = Angle.FromDegrees(-43);

    /// <summary>
    /// Angle bonus applied upon the bipod being used.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("maxAngle"), AutoNetworkedField]
    public Angle MaxAngle = Angle.FromDegrees(-43);

    /// <summary>
    /// Recoil bonuses applied upon the bipod being used.
    /// Higher angle decay bonus, quicker recovery.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Angle AngleDecay = Angle.FromDegrees(0);

    /// <summary>
    /// Recoil bonuses applied upon the bipod being used.
    /// Lower angle increase bonus (negative numbers), slower buildup.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Angle AngleIncrease = Angle.FromDegrees(0);

    /// <summary>
    /// Firerate bonus applied upon the bipod being used.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float FireRateIncrease = 3f;

    /// <summary>
    /// Time to set up the bipod.
    /// </summary>
    [DataField]
    public TimeSpan SetupDelay = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Is the bipod set up?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsSetup;

    [DataField]
    public EntProtoId BipodToggleAction = "ActionToggleBipod";

    [DataField, AutoNetworkedField]
    public EntityUid? BipodToggleActionEntity;

    /// <summary>
    /// The time when the Bipod has begun being set up.
    /// Used to stop it from firing while the bipod is being set up.
    /// </summary>
    /// <returns></returns>
    [AutoNetworkedField]
    public TimeSpan BipodSetupTime;
}
