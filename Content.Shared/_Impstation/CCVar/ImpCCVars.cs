using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared._Impstation.CCVar;

// ReSharper disable once InconsistentNaming
[CVarDefs]
public sealed class ImpCCVars : CVars
{
    /// <summary>
    /// The number of shared moods to give thaven by default.
    /// </summary>
    public static readonly CVarDef<uint> ThavenSharedMoodCount =
        CVarDef.Create<uint>("thaven.shared_mood_count", 1, CVar.SERVERONLY);

    public static readonly CVarDef<int> CosmicCultistEntropyValue =
        CVarDef.Create("cosmiccult.cultist_entropy_value", 7, CVar.SERVER, "How much entropy a convert is worth towards the next monument tier");

    public static readonly CVarDef<int> CosmicCultTargetConversionPercent =
        CVarDef.Create("cosmiccult.target_conversion_percent", 40, CVar.SERVER, "How much of the crew the cult is aiming to convert for a tier 3 monument");

    public static readonly CVarDef<int> CosmicCultStewardVoteTimer =
        CVarDef.Create("cosmiccult.steward_vote_timer", 40, CVar.SERVER, "How long the timer for the cult's stewardship vote lasts.");

    public static readonly CVarDef<int> CosmicCultT2RevealDelaySeconds =
        CVarDef.Create<int>("cosmiccult.t2_reveal_delay_seconds", 120, CVar.SERVER, "The delay between the monument getting upgraded to tier 2 and the crew learning of that fact. the monument cannot be upgraded again in this time.");

    public static readonly CVarDef<int> CosmicCultT3RevealDelaySeconds =
        CVarDef.Create<int>("cosmiccult.t3_reveal_delay_seconds", 60, CVar.SERVER, "The delay between the monument getting upgraded to tier 3 and the crew learning of that fact. the monument cannot be upgraded again in this time.");

    public static readonly CVarDef<int> CosmicCultFinaleDelaySeconds =
        CVarDef.Create<int>("cosmiccult.extra_entropy_for_finale", 150, CVar.SERVER, "The delay between the monument getting upgraded to tier 3 and the finale starting");
}
