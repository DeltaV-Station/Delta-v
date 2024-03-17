using Content.Server.StationEvents.Events;
using Robust.Shared.Prototypes;

namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(MidRoundAntagRule))]
public sealed partial class MidRoundAntagRuleComponent : Component
{
    /// <summary>
    /// Spawner to create at a random mid round antag marker.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId Spawner = string.Empty;
}
