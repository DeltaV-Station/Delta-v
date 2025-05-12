using Content.Server._DV.Objectives.Systems;
using Content.Server.Objectives.Systems;
using Content.Shared._DV.Traitor;
using Content.Shared.Charges.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Mind;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Objectives.Components;
using Content.Shared.Popups;
using Content.Shared.Salvage.Fulton;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Server._DV.Traitor;

public sealed class ExtractionFultonSystem : SharedExtractionFultonSystem
{
    [Dependency] private readonly ExtractConditionSystem _extractCondition = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MobStateSystem _mob = default!;
    [Dependency] private readonly RansomConditionSystem _ransomCondition = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedChargesSystem _charges = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedFultonSystem _fulton = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExtractionFultonComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<ExtractionFultonComponent, ExtractionFultonDoAfterEvent>(OnDoAfter);
    }

    private void OnAfterInteract(Entity<ExtractionFultonComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || args.Target is not {} target)
            return;

        args.Handled = true;

        AttachFulton(ent, target, args.User);
    }

    protected override void AttachFulton(Entity<ExtractionFultonComponent> ent, EntityUid target, EntityUid user)
    {
        if (_mind.GetMind(user) is not {} mindId || !TryComp<MindComponent>(mindId, out var mind))
            return;

        if (HasComp<FultonedComponent>(target))
        {
            Popup.PopupEntity(Loc.GetString("fulton-fultoned"), target, user);
            return;
        }

        if (_charges.IsEmpty(ent.Owner))
        {
            Popup.PopupEntity(Loc.GetString("emag-no-charges"), ent, user);
            return;
        }

        if (!CanExtractPopup((mindId, mind), user, target))
            return;

        if (FindBeacon(ent, target) is not {} beacon)
        {
            Log.Error($"No beacon found accepting {ToPrettyString(target)} from {ToPrettyString(ent)}");
            Popup.PopupEntity(Loc.GetString("extraction-fulton-no-destination"), ent, user);
            return;
        }

        var ev = new ExtractionFultonDoAfterEvent(GetNetEntity(beacon));
        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user, ent.Comp.ApplyDelay, ev, eventTarget: ent, target: target, used: ent)
        {
            BreakOnMove = true,
            NeedHand = true
        });
    }

    private void OnDoAfter(Entity<ExtractionFultonComponent> ent, ref ExtractionFultonDoAfterEvent args)
    {
        if (args.Cancelled || args.Target is not {} target || GetEntity(args.Beacon) is not {} beacon)
            return;

        if (!_charges.TryUseCharge(ent.Owner))
            return;

        var duration = HasComp<MobStateComponent>(target)
            ? ent.Comp.MobDelay
            : ent.Comp.ItemDelay;

        // this is checked when extracted to only complete this persons objective
        EnsureComp<ExtractingComponent>(target).Mind = _mind.GetMind(args.User);

        var comp = AddComp<FultonedComponent>(target);
        comp.Beacon = beacon;
        comp.NextFulton = _timing.CurTime + duration;
        comp.FultonDuration = duration;
        comp.Removeable = true;
        _fulton.UpdateAppearance(target, comp);
        Dirty(target, comp);
        _audio.PlayPvs(ent.Comp.FultonSound, target);

        // TODO: make mobs beep while fultoned
    }

    private bool CanExtractPopup(Entity<MindComponent?> mind, EntityUid user, EntityUid target)
    {
        if (Transform(target).Anchored)
        {
            Popup.PopupEntity(Loc.GetString("extraction-fulton-anchored"), target, user);
            return false;
        }

        if (_extractCondition.FindObjective(mind, target) != null)
            return true;

        if (_ransomCondition.FindObjective(mind, target) != null)
        {
            if (!_mob.IsAlive(target))
            {
                Popup.PopupEntity(Loc.GetString("extraction-fulton-dead"), target, user);
                return false;
            }

            return true;
        }

        Popup.PopupEntity(Loc.GetString("extraction-fulton-not-target"), target, user);
        return false;
    }
}
