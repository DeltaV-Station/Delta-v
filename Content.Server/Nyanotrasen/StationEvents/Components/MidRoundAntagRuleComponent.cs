using Content.Server.StationEvents.Events;

namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(MidRoundAntagRule))]
public sealed partial class MidRoundAntagRuleComponent : Component
{
    [DataField("antags")]
    public IReadOnlyList<string> MidRoundAntags = new[]
    {
        "SpawnPointGhostRatKing",
        "SpawnPointGhostVampSpider",
        "SpawnPointGhostFugitive",
        "MobEvilTwinSpawn"
    };
}
