using Robust.Shared.Analyzers;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._DV.Autoclave;

/// <summary>
///     A component that will cause powered locker entities to clean their contents periodically
/// </summary>
[RegisterComponent, AutoGenerateComponentPause, Access(typeof(AutoclaveSystem))]
public sealed partial class AutoclaveComponent : Component
{
    /// <summary>
    ///     The next time that items inside the autoclave will be cleaned
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextUpdate;

    /// <summary>
    ///     How often to clean contained items
    /// </summary>
    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(1);
}
