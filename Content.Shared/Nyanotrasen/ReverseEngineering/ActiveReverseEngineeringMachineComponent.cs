using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.ReverseEngineering;

/// <summary>
/// Added to RE machines when they are actively scanning an item.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedReverseEngineeringSystem))]
[AutoGenerateComponentPause, AutoGenerateComponentState]
public sealed partial class ActiveReverseEngineeringMachineComponent : Component
{
    /// <summary>
    /// When is the next probe roll due for?
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan NextProbe = TimeSpan.Zero;
}
