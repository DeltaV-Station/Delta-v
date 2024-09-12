using Content.Server.Destructible.Thresholds; // TODO: shared
using Content.Server.StationEvents.Events;
using Content.Shared.Dataset;
using Content.Shared.Random.Helpers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Utility;

namespace Content.Server.StationEvents.Components;

/// <summary>
/// Makes a GALPOL announcement and creates a report some time after an antag spawns.
/// Removed after this is done.
/// </summary>
[RegisterComponent, Access(typeof(FugitiveRule))]
[AutoGenerateComponentPause]
public sealed partial class FugitiveRuleComponent : Component
{
    [DataField]
    public LocId Announcement = "station-event-fugitive-hunt-announcement";

    [DataField]
    public LocId Sender = "fugitive-announcement-GALPOL";

    [DataField]
    public Color Color = Color.Yellow;

    /// <summary>
    /// Report paper to spawn. Its content is generated from the fugitive.
    /// </summary>
    [DataField]
    public EntProtoId ReportPaper = "PaperFugitiveReport";

    /// <summary>
    /// How long to wait after the antag spawns before announcing it.
    /// </summary>
    [DataField]
    public TimeSpan AnnounceDelay = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Station to give the report to.
    /// </summary>
    [DataField]
    public EntityUid? Station;

    /// <summary>
    /// The report generated for the spawned fugitive.
    /// </summary>
    [DataField]
    public string Report = string.Empty;

    /// <summary>
    /// When the announcement will be made, if an antag has spawned yet.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan? NextAnnounce;

    /// <summary>
    /// Config to use to generate crimes.
    /// </summary>
    [DataField(required: true)]
    public RandomCrimes Crimes = new();
}

[DataDefinition]
public partial struct RandomCrimes
{
    /// <summary>
    /// Dataset to pick crime strings from.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<LocalizedDatasetPrototype> Dataset;

    /// <summary>
    /// Number of unique crimes they can be charged with.
    /// Does not affect the counts of each crime.
    /// </summary>
    [DataField(required: true)]
    public MinMax Crimes = new(1, 1);

    /// <summary>
    /// Counts of each crime that can be rolled.
    /// </summary>
    [DataField(required: true)]
    public MinMax Counts = new(1, 1);

    /// <summary>
    /// Get the localized string and count of each random crime.
    /// <summary>
    public IEnumerable<(string, int)> Pick(IPrototypeManager proto, IRobustRandom random)
    {
        var crimeTypes = proto.Index(Dataset);
        var crimes = new HashSet<LocId>();
        var total = Crimes.Next(random);
        while (crimes.Count < total)
        {
            crimes.Add(random.Pick(crimeTypes));
        }

        foreach (var crime in crimes)
        {
            var count = Counts.Next(random);
            yield return (Loc.GetString(crime), count);
        }
    }
}
