using System.Linq;
using System.Text.RegularExpressions;
using Content.Server.Hands.Systems;
using Content.Server.Medical.Components;
using Content.Shared._DV.Medical;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Labels.EntitySystems;
using Content.Shared.Paper;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.Medical;

public sealed partial class HealthAnalyzerSystem : EntitySystem
{
    private static readonly Regex TemplateInsert = new(@"\{([\w.]+)\}", RegexOptions.Compiled);

    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly LabelSystem _label = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly PaperSystem _paper = default!;
    [Dependency] private readonly SharedGameTicker _gameTicker = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public void InitializeReportPrinting()
    {
        SubscribeLocalEvent<HealthAnalyzerComponent, HealthAnalyzerPrintReportMessage>(OnPrint);
    }

    private void OnPrint(Entity<HealthAnalyzerComponent> entity, ref HealthAnalyzerPrintReportMessage args)
    {
        var printer = entity.Comp;
        // Prevent users from printing too quickly
        if (printer.PrintAllowedAfter >= _timing.CurTime)
            return;

        HealthAnalyzerComponent? analyzer = null;
        if (!Resolve(entity.Owner, ref analyzer))
            return;

        // The health analyzer UI disables the button when the patient is invalid or out of range
        if (analyzer.ScannedEntity is not { Valid: true } patient)
            return;

        var user = args.Actor;
        if (!IsInRange(patient, user, analyzer.MaxScanRange))
            return;

        // Create slip of paper according to template
        var paper = Spawn(printer.PrintTemplate, Transform(user).Coordinates);
        ComposePatientRecord(paper, user, patient);
        _label.Label(paper, GetEntityName(patient));
        _hands.PickupOrDrop(user, paper);
        _audio.PlayPvs(entity.Comp.PrintReportSound, user);

        // Start cooldown
        printer.PrintAllowedAfter = _timing.CurTime + printer.PrintCooldown;
    }

    private bool IsInRange(EntityUid patient, EntityUid user, float? maxScanRange)
    {
        if (maxScanRange == null)
        {
            return true;
        }

        return _transformSystem.InRange(
            (patient, Transform(patient)),
            (user, Transform(user)),
            maxScanRange.Value
        );
    }

    private void ComposePatientRecord(EntityUid uid, EntityUid responder, EntityUid patient)
    {
        PaperComponent? paper = null;
        DamageableComponent? damageable = null;
        if (!Resolve(uid, ref paper) || !Resolve(patient, ref damageable))
        {
            return;
        }

        var template = paper.Content;

        // Anything in this dictionary can be interpolated into the print template
        Dictionary<string, Func<string>> inserts = new()
        {
            { "patient.name", () => GetEntityName(patient) },
            { "patient.species", () => GetEntitySpecies(patient) },
            { "responder.name", () => GetEntityName(responder) },
            { "roundTime", () => (_timing.CurTime - _gameTicker.RoundStartTimeSpan).ToString(@"hh\:mm") },
            { "damageList", () => ComposeDamageList((patient, damageable)) },
        };

        var content = TemplateInsert.Replace(template,
            match =>
            {
                var key = match.Groups[1].Value;
                if (inserts.TryGetValue(key, out var value))
                {
                    return value.Invoke();
                }

                return match.Value;
            });

        _paper.SetContent((uid, paper), content);
    }

    private string ComposeDamageList(Entity<DamageableComponent> ent)
    {
        var damages = _damageable.GetPositiveDamage(ent);
        if (damages.GetTotal() <= 0)
        {
            return Loc.GetString("health-analyzer-printout-damage-none");
        }

        var report = new FormattedMessage();
        var groupDamages = damages.GetDamagePerGroup(_prototypes);

        var groups = groupDamages.OrderByDescending(entry => entry.Value);
        var damage = damages.DamageDict;
        foreach (var (groupId, groupDamage) in groups)
        {
            if (groupDamage <= 0)
            {
                continue;
            }

            var group = _prototypes.Index(groupId);

            // Group header
            var groupTitleText = Loc.GetString(
                "health-analyzer-printout-damage-group-text",
                ("damageGroup", group.LocalizedName),
                ("amount", groupDamage)
            );
            report.AddMarkupPermissive(groupTitleText);
            report.PushNewline();

            // List individual damage types
            foreach (var type in group.DamageTypes)
            {
                var amount = damage.GetValueOrDefault(type, 0);
                if (amount <= 0)
                {
                    continue;
                }

                report.AddMarkupPermissive(Loc.GetString(
                    "health-analyzer-printout-damage-type-text",
                    ("damageType", _prototypes.Index(type).LocalizedName),
                    ("amount", amount)
                ));
                report.PushNewline();
            }
        }

        return report.ToMarkup();
    }

    private string GetEntityName(EntityUid uid)
    {
        return HasComp<MetaDataComponent>(uid)
            ? Identity.Name(uid, EntityManager)
            : Loc.GetString("health-analyzer-window-entity-unknown-text");
    }

    private string GetEntitySpecies(EntityUid uid)
    {
        return Loc.GetString(
            TryComp<HumanoidProfileComponent>(uid, out var appearance)
                ? _prototypes.Index(appearance.Species).Name
                : "health-analyzer-window-entity-unknown-species-text"
        );
    }
}
