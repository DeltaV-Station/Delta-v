using Content.Server.StationEvents.Events;

namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(MidRoundAntagRule))]
public sealed partial class MidRoundAntagRuleComponent : Component
{
    [DataField("antags")]
    public IReadOnlyList<string> MidRoundAntags = new[]
    {
        "SpawnPointGhostRatKing",
        //"SpawnPointGhostVampSpider",//Has Arachne as a prereq
        "SpawnPointGhostFugitive", //Yea this is temporarily the Fugitive event until the others are working
        //"MobEvilTwinSpawn"
    };
}
