using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Shitmed.BodyEffects;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentPause]
public sealed partial class MechanismEffectComponent : Component
{
    /// <summary>
    /// Components added to a body when this mechanism is enabled.
    /// Gets removed when the mechanism is disabled after.
    /// </summary>
    [DataField, AlwaysPushInheritance]
    public ComponentRegistry? Added;

    /// <summary>
    /// Components removed from a body when this mechanism is disabled.
    /// Gets added back with these values when this mechanism is enabled, not the previous values.
    /// </summary>
    [DataField, AlwaysPushInheritance]
    public ComponentRegistry? Removed;

    /// <summary>
    ///     The components that are active on the part and will be refreshed every 5s
    /// </summary>
    [DataField]
    public ComponentRegistry Active = new();

    /// <summary>
    ///     How long to wait between each refresh.
    ///     Effects can only last at most this long once the organ is removed.
    /// </summary>
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(5);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;
}
