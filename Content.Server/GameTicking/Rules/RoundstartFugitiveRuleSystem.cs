using Content.Server.Antag;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Roles;
using Content.Shared.Humanoid;
using Content.Server.Communications;
using Content.Server.Forensics;
using Content.Server.Station.Systems;
using Content.Server.StationEvents.Components;
using Content.Server.StationEvents.Events;
using Content.Shared.GameTicking.Components;
using Content.Shared.Ghost;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Inventory;
using Content.Shared.Paper;
using Content.Shared.Popups;
using Content.Shared.Random.Helpers;
using Content.Shared.Storage.EntitySystems;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.GameTicking.Rules;

/// <summary>
/// Copy of ThiefRuleSystem
/// </summary>
public sealed class RoundstartFugitiveRuleSystem : GameRuleSystem<RoundstartFugitiveRuleComponent>
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly PaperSystem _paper = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStorageSystem _storage = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundstartFugitiveRuleComponent, AfterAntagEntitySelectedEvent>(AfterAntagSelected);

        SubscribeLocalEvent<RoundstartFugitiveRoleComponent, GetBriefingEvent>(OnGetBriefing);

    }

    //Moved this bit of code down below so AfterAntagSelcted isn't duplicate

    // Greeting upon thief activation
    //private void AfterAntagSelected(Entity<RoundstartFugitiveRuleComponent> mindId,
    //    ref AfterAntagEntitySelectedEvent args)
    //{
    //    var ent = args.EntityUid;
    //    _antag.SendBriefing(ent, MakeBriefing(ent), null, null);
    //}

    // Character screen briefing
    private void OnGetBriefing(Entity<RoundstartFugitiveRoleComponent> role, ref GetBriefingEvent args)
    {
        var ent = args.Mind.Comp.OwnedEntity;

        if (ent is null)
            return;
        args.Append(MakeBriefing(ent.Value));
    }

    private string MakeBriefing(EntityUid ent)
    {
        var isHuman = HasComp<HumanoidAppearanceComponent>(ent);
        var briefing = isHuman
            ? Loc.GetString("roundstartfugitive-role-greeting-human")
            : Loc.GetString("roundstartfugitive-role-greeting-animal"); //Can thieves be animals???

        if (isHuman)
            briefing += "\n \n" + Loc.GetString("roundstartfugitive-role-greeting-equipment") + "\n";

        return briefing;
    }
    // Copy of parts from FugitiveRules below here, hopefully this will cause the Fugitive Fax event to trigger?

    protected override void ActiveTick(EntityUid uid, RoundstartFugitiveRuleComponent comp, GameRuleComponent rule, float frameTime)
    {
        if (comp.NextAnnounce is not {} next || next > Timing.CurTime)
            return;

        var announcement = Loc.GetString(comp.Announcement);
        var sender = Loc.GetString(comp.Sender);
        _chat.DispatchGlobalAnnouncement(announcement, sender: sender, colorOverride: comp.Color);

        // send the report to every comms console on the station
        var query = EntityQueryEnumerator<TransformComponent, CommunicationsConsoleComponent>();
        var consoles = new List<TransformComponent>();
        while (query.MoveNext(out var console, out var xform, out _))
        {
            if (_station.GetOwningStation(console, xform) != comp.Station || HasComp<GhostComponent>(console))
                continue;

            consoles.Add(xform);
        }

        foreach (var xform in consoles)
        {
            SpawnReport(comp, xform);
        }

        // prevent any possible funnies
        comp.NextAnnounce = null;

        RemCompDeferred(uid, comp);
    }

    private void AfterAntagSelected(Entity<RoundstartFugitiveRuleComponent> mindId, ref AfterAntagEntitySelectedEvent args)
    {
        {
            var ent = args.EntityUid;
            _antag.SendBriefing(ent, MakeBriefing(ent), null, null);
        }

        var (uid, comp) = mindId;
        if (comp.NextAnnounce != null)
        {
            Log.Error("Fugitive rule spawning multiple fugitives isn't supported, sorry.");
            return;
        }

        var fugi = args.EntityUid;
        comp.Report = GenerateReport(fugi, comp).ToMarkup();
        comp.Station = _station.GetOwningStation(fugi);
        comp.NextAnnounce = Timing.CurTime + comp.AnnounceDelay;

        // _popup.PopupEntity(Loc.GetString("fugitive-spawn"), fugi, fugi); //I think this does the 'You fall from the ceiling popup? If so, can be removed later

        // give the fugi a report so they know what their charges are
        var report = SpawnReport(comp, Transform(fugi));

        // try to insert it into their bag
        if (_inventory.TryGetSlotEntity(fugi, "back", out var backpack))
        {
            _storage.Insert(backpack.Value, report, out _, playSound: false);
        }
        else
        {
            // no bag somehow, at least pick it up
            _hands.TryPickup(fugi, report);
        }
    }

    private Entity<PaperComponent> SpawnReport(RoundstartFugitiveRuleComponent rule, TransformComponent xform)
    {
        var report = Spawn(rule.ReportPaper, xform.Coordinates);
        var paper = Comp<PaperComponent>(report);
        var ent = (report, paper);
        _paper.SetContent(ent, rule.Report);
        return ent;
    }

    private FormattedMessage GenerateReport(EntityUid uid, RoundstartFugitiveRuleComponent rule)
    {
        var report = new FormattedMessage();
        report.PushMarkup(Loc.GetString("fugitive-report-title"));
        report.PushNewline();
        report.PushMarkup(Loc.GetString("fugitive-report-first-line"));
        report.PushNewline();

        if (!TryComp<HumanoidAppearanceComponent>(uid, out var humanoid))
        {
            report.AddMarkup(Loc.GetString("fugitive-report-inhuman", ("name", uid)));
            return report;
        }

        var species = _proto.Index(humanoid.Species); //IPrototype or protoype

        report.PushMarkup(Loc.GetString("fugitive-report-morphotype", ("species", Loc.GetString(species.Name))));
        report.PushMarkup(Loc.GetString("fugitive-report-age", ("age", humanoid.Age)));
        report.PushMarkup(Loc.GetString("fugitive-report-sex", ("sex", humanoid.Sex.ToString())));

        if (TryComp<PhysicsComponent>(uid, out var physics))
            report.PushMarkup(Loc.GetString("fugitive-report-weight", ("weight", Math.Round(physics.FixturesMass))));

        // add a random identifying quality that officers can use to track them down - DISABLED
        //report.PushMarkup(RobustRandom.Next(0, 2) switch
        //{
        //    0 => Loc.GetString("fugitive-report-detail-dna", ("dna", GetDNA(uid))),
        //    _ => Loc.GetString("fugitive-report-detail-prints", ("prints", GetPrints(uid)))
        //});

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

    private string GetDNA(EntityUid uid)
    {
        return CompOrNull<DnaComponent>(uid)?.DNA ?? "?";
    }

    private string GetPrints(EntityUid uid)
    {
        return CompOrNull<FingerprintComponent>(uid)?.Fingerprint ?? "?";
    }

    private void AddCharges(FormattedMessage report, RoundstartFugitiveRuleComponent rule)
    {
        var crimeTypes = _proto.Index(rule.CrimesDataset);
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


