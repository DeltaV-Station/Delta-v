using Content.Server.StationEvents.Events;
using Robust.Shared.Prototypes;

namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(MidRoundAntagRule))]
public sealed partial class MidRoundAntagRuleComponent : Component
{
    [DataField("antags")]
    public List<EntProtoId> MidRoundAntags = new()
    {
        "SpawnPointGhostRatKing",
        //"SpawnPointGhostVampSpider",
        //"SpawnPointGhostFugitive",
        "SpawnPointGhostEvilTwin"
    };
}
