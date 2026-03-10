using Content.Shared.Alert;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Starlight;

#region Shadekin
[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause, AutoGenerateComponentState]
public sealed partial class ShadekinComponent : Component
{
    [DataField]
    public ProtoId<AlertPrototype> ShadekinAlert = "Shadekin";

    [ViewVariables(VVAccess.ReadOnly), AutoPausedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    [DataField]
    public TimeSpan UpdateCooldown = TimeSpan.FromSeconds(1f);

    [AutoNetworkedField, ViewVariables]
    public ShadekinState CurrentState { get; set; } = ShadekinState.Dark;

    [DataField("thresholds", required: true)]
    public SortedDictionary<FixedPoint2, ShadekinState> Thresholds = new();

    /// <summary>
    /// whether to flicker lights or not. default on
    /// </summary>
    [DataField] public bool DoLightFlicker = true;
}

[Serializable, NetSerializable]
public enum ShadekinState : byte
{
    Invalid = 0,
    Dark = 1,
    Low = 2,
    Annoying = 3,
    High = 4,
    Extreme = 5

}
#endregion
