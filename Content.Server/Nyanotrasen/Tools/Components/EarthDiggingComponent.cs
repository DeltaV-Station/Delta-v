using System.Threading;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Content.Shared.Tools;

namespace Content.Server.Tools.Components;

[RegisterComponent]
public sealed class EarthDiggingComponent : Component
{
    [ViewVariables]
    [DataField("toolComponentNeeded")]
    public bool ToolComponentNeeded = true;

    [ViewVariables]
    [DataField("qualityNeeded", customTypeSerializer:typeof(PrototypeIdSerializer<ToolQualityPrototype>))]
    public string QualityNeeded = "Digging";

    [ViewVariables]
    [DataField("delay")]
    public float Delay = 2f;

    /// <summary>
    /// Used for do_afters.
    /// </summary>
    public CancellationTokenSource? CancelToken = null;
}
