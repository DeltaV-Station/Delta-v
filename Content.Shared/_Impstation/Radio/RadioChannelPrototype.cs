using Robust.Shared.Prototypes;

namespace Content.Shared.Radio;

public sealed partial class RadioChannelPrototype : IPrototype
{
    /// <summary>
    /// ImpStation original. If a channel is readOnly, then headsets cannot send messages through it.
    /// Intercomms still can.
    /// </summary>
    [DataField("intercomOnly")]
    public bool IntercomOnly = false;
}