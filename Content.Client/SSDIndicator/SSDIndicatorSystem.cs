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
            ProtoId<SsdIconPrototype> icon;
            switch (component.Stage)
            {

                case SsdStage.VeryRecent:
                    icon = component.VeryRecentIcon;
                    break;
                case SsdStage.Recent:
                    icon = component.RecentIcon;
                    break;
                case SsdStage.Cryoable:
                    icon = component.Icon;
                    break;
                default:
                    Log.Error("Client SSDIndicatorSystem needs to be updated for new SsdStage. Falling back to default icon.");
                    icon = component.Icon;
                    break;
            }

            args.StatusIcons.Add(_prototype.Index(icon));
            // End DeltaV Additions

            // args.StatusIcons.Add(_prototype.Index(component.Icon)); // DeltaV - commented out. status icon now added above
        }
    }
}
