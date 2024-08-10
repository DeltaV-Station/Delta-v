using Content.Server.CriminalRecords.Systems;
using Content.Server.StationEvents.Components;
using Content.Server.StationRecords.Systems;
using Content.Shared.CriminalRecords;
using Content.Shared.Roles;
using Content.Shared.Security;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.DeltaV.Prisoner;

/// <summary>
/// Gives prisoners a roundstart randomized criminal record and detained status.
/// </summary>
public sealed class PrisonerRecordSystem : EntitySystem
{
    [Dependency] private readonly CriminalRecordsSystem _criminalRecords = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly StationRecordsSystem _records = default!;

    /// <summary>
    /// Only give this job a criminal record.
    /// </summary>
    public ProtoId<JobPrototype> Prisoner = "Prisoner";

    public RandomCrimes Crimes = new()
    {
        Dataset = "FugitiveCrimes",
        Crimes = new(1, 6),
        Counts = new(1, 8)
    };

    public override void Initialize()
    {
        base.Initialize();

        // make sure this is ran after criminal records system so it exists
        SubscribeLocalEvent<AfterGeneralRecordCreatedEvent>(OnGeneralRecordCreated, after: [typeof(CriminalRecordsSystem)]);
    }

    private void OnGeneralRecordCreated(AfterGeneralRecordCreatedEvent args)
    {
        if (args.Record.JobPrototype != Prisoner)
            return;

        if (!_records.TryGetRecord<CriminalRecord>(args.Key, out var criminal))
        {
            Log.Error($"No criminal record found for {args.Key} somehow!");
            return;
        }

        // Prisoners spawn in perma so start off detained
        _criminalRecords.OverwriteStatus(args.Key, criminal, SecurityStatus.Detained, null);
        var start = TimeSpan.Zero;
        criminal.History.Add(new CrimeHistory(start, Loc.GetString("criminal-records-prisoner-record-header")));
        foreach (var (crime, count) in Crimes.Pick(_proto, _random))
        {
            criminal.History.Add(new CrimeHistory(start, Loc.GetString("fugitive-report-crime", ("crime", crime), ("count", count))));
        }
        _records.Synchronize(args.Key);
    }
}
