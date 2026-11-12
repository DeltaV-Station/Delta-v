using System.Diagnostics.CodeAnalysis;
using Content.Server.Antag;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Nuke;
using Content.Server.Pinpointer;
using Content.Server.Roles;
using Content.Shared._DV.Roles;
using Content.Shared._DV.Uprising;
using Content.Shared.Chat;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DoAfter;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Content.Shared.Objectives.Systems;
using Content.Shared.Roles;
using Content.Shared.Verbs;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Server._DV.Uprising;

public sealed class UprisingRuleSystem : GameRuleSystem<UprisingRuleComponent>
{
    [Dependency] private readonly SharedChatSystem _chat = default!;
    [Dependency] private readonly NukeSystem _nuke = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly NavMapSystem _navMap = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedObjectivesSystem _objectives = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LoyalistRoleComponent, GetBriefingEvent>(OnGetLoyalistBriefing);
        SubscribeLocalEvent<InsurgentRoleComponent, GetBriefingEvent>(OnGetInsurgentBriefing);

        SubscribeLocalEvent<UprisingVictoryObjectiveComponent, ObjectiveGetProgressEvent>(OnUprisingVictoryProgress);

        SubscribeLocalEvent<UprisingConsoleComponent, GetVerbsEvent<AlternativeVerb>>(OnGetConsoleVerbs);
        SubscribeLocalEvent<UprisingConsoleComponent, UprisingArmDoAfter>(OnArm);
        SubscribeLocalEvent<UprisingConsoleComponent, UprisingDisarmDoAfter>(OnDisarm);
    }

    private void OnUprisingVictoryProgress(Entity<UprisingVictoryObjectiveComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        if (GetRule() is not { } rule)
            return;

        if (rule.Comp1.SideWinsAt is var (side, _) && ent.Comp.Side == side)
            args.Progress = 0.5f;
        else if (rule.Comp1.SideWon == ent.Comp.Side)
            args.Progress = 1f;
        else
            args.Progress = 0f;
    }

    protected override void Started(EntityUid uid, UprisingRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        component.NukeAnnouncementAt = Timing.CurTime + component.NukeAnnouncementDelay;
        component.NukeTimeAt = Timing.CurTime + component.NukeTimeDelay;
        component.FirstWarningAt = component.NukeTimeAt - component.FirstWarning;
        component.ImpendingWarningAt = component.NukeTimeAt - component.ImpendingWarning;
        component.FinalWarningAt = component.NukeTimeAt - component.FinalWarning;
    }

    protected override void ActiveTick(EntityUid uid, UprisingRuleComponent component, GameRuleComponent gameRule, float frameTime)
    {
        base.ActiveTick(uid, component, gameRule, frameTime);

        if (component.NukeAnnouncementAt is { } nukeAnnouncement && nukeAnnouncement <= Timing.CurTime)
        {
            var time = component.NukeTimeAt.HasValue ? component.NukeTimeAt.Value - Timing.CurTime : component.NukeTimeDelay;
            _chat.DispatchGlobalAnnouncement(Loc.GetString("uprising-announcement-nuke-announcement", ("time", (int)Math.Round(time.TotalMinutes))), colorOverride: Color.Gold);
            component.NukeAnnouncementAt = null;
        }

        if (component.FirstWarningAt is { } firstWarning && firstWarning <= Timing.CurTime)
        {
            var time = component.NukeTimeAt.HasValue ? component.NukeTimeAt.Value - Timing.CurTime : component.FirstWarning;
            _chat.DispatchGlobalAnnouncement(Loc.GetString("uprising-announcement-first-warning", ("time", (int)Math.Round(time.TotalMinutes))), colorOverride: Color.Gold);
            component.FirstWarningAt = null;
        }

        if (component.ImpendingWarningAt is { } impendingWarning && impendingWarning <= Timing.CurTime)
        {
            var time = component.NukeTimeAt.HasValue ? component.NukeTimeAt.Value - Timing.CurTime : component.ImpendingWarning;
            _chat.DispatchGlobalAnnouncement(Loc.GetString("uprising-announcement-impending-warning", ("time", (int)Math.Round(time.TotalMinutes))), colorOverride: Color.Gold);
            component.ImpendingWarningAt = null;
        }

        if (component.FinalWarningAt is { } finalWarning && finalWarning <= Timing.CurTime)
        {
            var time = component.NukeTimeAt.HasValue ? component.NukeTimeAt.Value - Timing.CurTime : component.FinalWarning;
            _chat.DispatchGlobalAnnouncement(Loc.GetString("uprising-announcement-final-warning", ("time", (int)Math.Round(time.TotalMinutes))), colorOverride: Color.Gold);
            component.FinalWarningAt = null;
        }

        if (component.NukeTimeAt is { } nukeTime && nukeTime <= Timing.CurTime)
        {
            component.NukeTimeAt = null;
            var query = EntityQueryEnumerator<NukeComponent>();
            while (query.MoveNext(out var bombUid, out var bomb))
            {
                _nuke.SetRemainingTime(bombUid, component.NukeDuration, bomb);
                _itemSlots.SetLock(bombUid, bomb.DiskSlot, false);
                _itemSlots.TryEject(bombUid, bomb.DiskSlot, null, out _, true);
                _itemSlots.SetLock(bombUid, bomb.DiskSlot, true);
                _nuke.ArmBomb(bombUid, bomb, "uprising-announcement-nuke-armed");
            }
        }

        if (component.SideWinsAt is var (winningSide, winningTime) && winningTime <= Timing.CurTime)
        {
            component.SideWinsAt = null;
            var query = EntityQueryEnumerator<NukeComponent>();
            while (query.MoveNext(out var bombUid, out var bomb))
            {
                _nuke.DisarmBomb(bombUid, bomb);
            }

            component.SideWon = winningSide;
            _chat.DispatchGlobalAnnouncement(
                Loc.GetString($"uprising-announcement-victory-announcement-{winningSide}"),
                Loc.GetString($"uprising-announcement-victory-announcement-{winningSide}.sender"),
                colorOverride: Color.FromXaml(Loc.GetString($"uprising-announcement-victory-announcement-{winningSide}.color")));

            GameTicker.EndRound();
        }
    }

    protected override void AppendRoundEndText(EntityUid uid,
        UprisingRuleComponent component,
        GameRuleComponent gameRule,
        ref RoundEndTextAppendEvent args)
    {
        if (component.SideWinsAt is var (partialSide, _))
        {
            args.AddLine(Loc.GetString($"uprising-outcome-{partialSide}-partial"));
        }
        else if (component.SideWon is { } winningSide)
        {
            args.AddLine(Loc.GetString($"uprising-outcome-{winningSide}-full"));
        }
        else
        {
            args.AddLine(Loc.GetString("uprising-outcome-none"));
        }

        args.AddLine(Loc.GetString("uprising-leads"));

        var antags = _antag.GetAntagIdentifiers(uid);
        var leads = new HashSet<EntityUid>();

        foreach (var (mind, sessionData, name) in antags)
        {
            leads.Add(mind);

            if (_role.MindHasRole<UprisingRoleComponent>(mind, out var it))
                args.AddLine(Loc.GetString($"uprising-leads-user.{it.Value.Comp2.Side}", ("name", name), ("user", sessionData.UserName)));
            else
                args.AddLine(Loc.GetString("uprising-leads-user.Unknown", ("name", name), ("user", sessionData.UserName)));
        }
        args.AddLine(string.Empty);

        var lines = new Dictionary<UprisingSide, List<string>>();
        foreach (var value in Enum.GetValues<UprisingSide>())
        {
            lines[value] = new();
        }

        var minds = EntityQueryEnumerator<MindComponent>();
        while (minds.MoveNext(out var mindUid, out var mindComponent))
        {
            if (leads.Contains(mindUid))
                continue;

            if (!_role.MindHasRole<UprisingRoleComponent>((mindUid, mindComponent), out var role) || mindComponent.OriginalOwnerUserId is not { } owner)
                continue;

            if (!_player.TryGetPlayerData(owner, out var data))
                continue;

            lines[role.Value.Comp2.Side].Add(Loc.GetString($"uprising-members-user", ("name", mindComponent.CharacterName ?? string.Empty), ("user", data.UserName)));
        }

        foreach (var value in Enum.GetValues<UprisingSide>())
        {
            args.AddLine(Loc.GetString($"uprising-members.{value}"));
            foreach (var line in lines[value])
            {
                args.AddLine(line);
            }
            args.AddLine(string.Empty);
        }
    }

    private void OnGetLoyalistBriefing(Entity<LoyalistRoleComponent> ent, ref GetBriefingEvent args)
    {
        args.Append(Loc.GetString("antag-Loyalist.briefing"));
    }

    private void OnGetInsurgentBriefing(Entity<InsurgentRoleComponent> ent, ref GetBriefingEvent args)
    {
        args.Append(Loc.GetString("antag-Insurgent.briefing"));
    }

    private Entity<UprisingRuleComponent, GameRuleComponent>? GetRule()
    {
        var enumerator = QueryActiveRules();
        while (enumerator.MoveNext(out var uid, out _, out var uprising, out var gamerule))
        {
            return (uid, uprising, gamerule);
        }

        return null;
    }

    private void StartWinning(UprisingSide side, EntityUid console)
    {
        if (GetRule() is not { } rule)
            return;

        var location = _navMap.GetNearestBeaconString(console);
        var announcement = Loc.GetString($"uprising-announcement-start-victory-announcement-{side}",
            ("time", (int) Math.Round(rule.Comp1.SideTimes[side].TotalSeconds)),
            ("location", FormattedMessage.RemoveMarkupOrThrow(location)));

        _chat.DispatchGlobalAnnouncement(
            announcement,
            Loc.GetString($"uprising-announcement-start-victory-announcement-{side}.sender"),
            colorOverride: Color.FromXaml(Loc.GetString($"uprising-announcement-start-victory-announcement-{side}.color")));

        rule.Comp1.SideWinsAt = (side, Timing.CurTime + rule.Comp1.SideTimes[side]);
    }

    private void StopWinning()
    {
        if (GetRule() is not { } rule || rule.Comp1.SideWinsAt is not var (side, time))
            return;

        rule.Comp1.SideTimes[side] = Timing.CurTime - time;

        _chat.DispatchGlobalAnnouncement(
            Loc.GetString($"uprising-announcement-interrupted-victory-announcement-{side}"),
            Loc.GetString($"uprising-announcement-interrupted-victory-announcement-{side}.sender"),
            colorOverride: Color.FromXaml(Loc.GetString($"uprising-announcement-interrupted-victory-announcement-{side}.color")));

        rule.Comp1.SideWinsAt = null;
    }

    private void OnGetConsoleVerbs(Entity<UprisingConsoleComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (GetRule() is not { } rule)
            return;

        var user = args.User;
        if (rule.Comp1.SideWinsAt is var (disarmSide, _))
        {
            args.Verbs.Add(new ()
            {
                Act = () => StartDisarming(ent, user),
                Text = Loc.GetString($"uprising-disarm-verb.{disarmSide}"),
            });
            return;
        }

        if (!CheckCanArm(args.User, out var armSide))
            return;

        args.Verbs.Add(new()
        {
            Act = () => StartArming(ent, user),
            Text = Loc.GetString($"uprising-arm-verb.{armSide}"),
        });
    }

    private bool CheckCanArm(EntityUid user, [NotNullWhen(true)] out UprisingSide? side)
    {
        side = null;
        if (_mind.GetMind(user) is not { } mind || !TryComp<MindComponent>(mind, out var mindComp))
            return false;

        if (!_role.MindHasRole<UprisingRoleComponent>(mind, out var uprisingRole))
            return false;

        foreach (var objective in mindComp.Objectives)
        {
            if (!HasComp<UprisingArmRequiredObjectiveComponent>(objective))
                continue;

            if (_objectives.GetProgress(objective, (mind, mindComp)) is not >= 1)
                return false;
        }

        side = uprisingRole.Value.Comp2.Side;
        return true;
    }

    private void StartDisarming(Entity<UprisingConsoleComponent> ent, EntityUid user)
    {
        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager,
            user,
            ent.Comp.Delay,
            new UprisingDisarmDoAfter(),
            ent.Owner));
    }

    private void StartArming(Entity<UprisingConsoleComponent> ent, EntityUid user)
    {
        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager,
            user,
            ent.Comp.Delay,
            new UprisingArmDoAfter(),
            ent.Owner));
    }

    private void OnArm(Entity<UprisingConsoleComponent> ent, ref UprisingArmDoAfter args)
    {
        if (args.Handled || args.Cancelled || !CheckCanArm(args.User, out var side))
            return;

        StartWinning(side.Value, ent);
    }

    private void OnDisarm(Entity<UprisingConsoleComponent> ent, ref UprisingDisarmDoAfter args)
    {
        if (args.Handled || args.Cancelled)
            return;

        StopWinning();
    }
}
