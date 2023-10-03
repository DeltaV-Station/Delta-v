using Robust.Shared.Serialization.TypeSerializers.Implementations;

namespace Content.Server.ReverseEngineering;

[RegisterComponent]
public sealed partial class ActiveReverseEngineeringMachineComponent : Component
{
    /// <summary>
    /// When did the scanning start?
    /// </summary>
    [DataField("startTime", customTypeSerializer: typeof(TimespanSerializer))]
    public TimeSpan StartTime;

    /// <summary>
    /// What is being scanned?
    /// </summary>
    [ViewVariables]
    public EntityUid Item;
}
