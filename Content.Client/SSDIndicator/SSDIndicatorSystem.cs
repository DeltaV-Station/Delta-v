using Content.Shared._DV.Mind; // DeltaV
using Content.Shared.CCVar; // DeltaV
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.NPC;
using Content.Shared.SSDIndicator;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Client.SSDIndicator;

/// <summary>
///     Handles displaying SSD indicator as status icon
/// </summary>
public sealed class SSDIndicatorSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly Shared.SSDIndicator.SSDIndicatorSystem _shared = default!; // DeltaV - SSD Recency, don't want to rename the upstream class

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SSDIndicatorComponent, GetStatusIconsEvent>(OnGetStatusIcon);
    }

    private void OnGetStatusIcon(EntityUid uid, SSDIndicatorComponent component, ref GetStatusIconsEvent args)
    {
        if (component.IsSSD &&
            _cfg.GetCVar(CCVars.ICShowSSDIndicator) &&
            !_mobState.IsDead(uid) &&
            !HasComp<ActiveNPCComponent>(uid) &&
            HasComp<MindExaminableComponent>(uid))
        {
            // Begin DeltaV Additions
            var ev = new ShowSSDIndicatorEvent();
            RaiseLocalEvent(uid, ref ev);
            if (ev.Hidden)
                return;

            // SSD Recency Indicator
            var stage = _shared.GetStage(new Entity<SSDIndicatorComponent>(uid, component));
            var icon = stage switch
            {
                SsdStage.VeryRecent => component.VeryRecentIcon,
                SsdStage.Recent => component.RecentIcon,
                SsdStage.Cryoable => component.Icon,
                _ => throw new InvalidOperationException($"{ToPrettyString(uid)} has an invalid SSD stage {stage}."),
            };

            args.StatusIcons.Add(_prototype.Index(icon));
            // End DeltaV Additions

            // args.StatusIcons.Add(_prototype.Index(component.Icon)); // DeltaV - commented out. status icon now added above
        }
    }
}
