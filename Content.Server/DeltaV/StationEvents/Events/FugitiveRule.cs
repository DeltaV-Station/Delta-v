using Content.Server.Antag;
using Content.Server.Communications;
using Content.Server.GameTicking.Components; // TODO: Shared when upstream merged
using Content.Server.Paper;
using Content.Server.StationEvents.Components;
using Content.Shared.Ghost;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Popups;
using Content.Shared.Random.Helpers;
using Robust.Shared.Physics.Components;
using Robust.Shared.Utility;

namespace Content.Server.StationEvents.Events;

public sealed class FugitiveRule : StationEventSystem<FugitiveRuleComponent>
{
    [Dependency] private readonly PaperSystem _paper = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FugitiveRuleComponent, AfterAntagEntitySelectedEvent>(OnEntitySelected);
    }

    protected override void ActiveTick(EntityUid uid, FugitiveRuleComponent comp, GameRuleComponent rule, float frameTime)
    {
        if (comp.NextAnnounce is not {} next || next > Timing.CurTime)
            return;

        var announcement = Loc.GetString(comp.Announcement);
        var sender = Loc.GetString(comp.Sender);
        ChatSystem.DispatchGlobalAnnouncement(announcement, sender: sender, colorOverride: comp.Color);

        // send the report to every comms console on the station
        var query = EntityQueryEnumerator<TransformComponent, CommunicationsConsoleComponent>();
        var consoles = new List<TransformComponent>();
        while (query.MoveNext(out var console, out var xform, out _))
        {
            if (StationSystem.GetOwningStation(console, xform) != comp.Station || HasComp<GhostComponent>(console))
                continue;

            consoles.Add(xform);
        }

        foreach (var xform in consoles)
        {
            var report = Spawn(comp.ReportPaper, xform.Coordinates);
            _paper.SetContent(report, comp.Report);
        }

        // prevent any possible funnies
        comp.NextAnnounce = null;

        RemCompDeferred(uid, comp);
    }

    private void OnEntitySelected(Entity<FugitiveRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        if (ent.Comp.NextAnnounce != null)
        {
            Log.Error("Fugitive rule spawning multiple fugitives isn't supported, sorry.");
            return;
        }

        var fugi = args.EntityUid;
        ent.Comp.Report = GenerateReport(fugi, ent.Comp).ToMarkup();
        ent.Comp.Station = StationSystem.GetOwningStation(fugi);
        ent.Comp.NextAnnounce = Timing.CurTime + ent.Comp.AnnounceDelay;

        _popup.PopupEntity(Loc.GetString("fugitive-spawn"), fugi, fugi);
    }

    private FormattedMessage GenerateReport(EntityUid uid, FugitiveRuleComponent rule)
    {
        var report = new FormattedMessage();
        report.PushMarkup(Loc.GetString("fugitive-report-title", ("name", uid)));
        report.PushNewline();
        report.PushMarkup(Loc.GetString("fugitive-report-first-line", ("name", uid)));
        report.PushNewline();

        if (!TryComp<HumanoidAppearanceComponent>(uid, out var humanoid))
        {
            report.AddMarkup(Loc.GetString("fugitive-report-inhuman", ("name", uid)));
            return report;
        }

        var species = PrototypeManager.Index(humanoid.Species);

        report.PushMarkup(Loc.GetString("fugitive-report-morphotype", ("species", Loc.GetString(species.Name))));
        report.PushMarkup(Loc.GetString("fugitive-report-age", ("age", humanoid.Age)));
        report.PushMarkup(Loc.GetString("fugitive-report-sex", ("sex", humanoid.Sex.ToString())));

        if (TryComp<PhysicsComponent>(uid, out var physics))
            report.PushMarkup(Loc.GetString("fugitive-report-weight", ("weight", Math.Round(physics.FixturesMass))));

        report.PushNewline();
        report.PushMarkup(Loc.GetString("fugitive-report-crimes-header"));

        // generate some random crimes to avoid this situation
        // "officer what are my charges?"
        // "uh i dunno a piece of paper said to arrest you thats it"
        AddCharges(report, rule);

        report.PushNewline();
        report.AddMarkup(Loc.GetString("fugitive-report-last-line"));

        return report;
    }

    private void AddCharges(FormattedMessage report, FugitiveRuleComponent rule)
    {
        var crimeTypes = PrototypeManager.Index(rule.CrimesDataset);
        var crimes = new HashSet<LocId>();
        var total = RobustRandom.Next(rule.MinCrimes, rule.MaxCrimes + 1);
        while (crimes.Count < total)
        {
            crimes.Add(RobustRandom.Pick(crimeTypes));
        }

        foreach (var crime in crimes)
        {
            var count = RobustRandom.Next(rule.MinCounts, rule.MaxCounts + 1);
            report.PushMarkup(Loc.GetString("fugitive-report-crime", ("crime", Loc.GetString(crime)), ("count", count)));
        }
    }
}
