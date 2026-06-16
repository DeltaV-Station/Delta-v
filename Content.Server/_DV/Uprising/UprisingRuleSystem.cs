using Content.Server.GameTicking.Rules;
using Content.Server.Nuke;
using Content.Server.Roles;
using Content.Shared._DV.Roles;
using Content.Shared.Chat;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.GameTicking.Components;

namespace Content.Server._DV.Uprising;

public sealed class UprisingRuleSystem : GameRuleSystem<UprisingRuleComponent>
{
    [Dependency] private readonly SharedChatSystem _chat = default!;
    [Dependency] private readonly NukeSystem _nuke = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LoyalistRoleComponent, GetBriefingEvent>(OnGetLoyalistBriefing);
        SubscribeLocalEvent<InsurgentRoleComponent, GetBriefingEvent>(OnGetInsurgentBriefing);
    }

    protected override void Started(EntityUid uid, UprisingRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        component.NukeAnnouncementAt = Timing.CurTime + component.NukeAnnouncementDelay;
        component.NukeTimeAt = Timing.CurTime + component.NukeTimeDelay;
        component.FirstWarningAt = Timing.CurTime + component.FirstWarning;
        component.ImpendingWarningAt = Timing.CurTime + component.ImpendingWarning;
        component.FinalWarningAt = Timing.CurTime + component.FinalWarning;
    }

    protected override void ActiveTick(EntityUid uid, UprisingRuleComponent component, GameRuleComponent gameRule, float frameTime)
    {
        base.ActiveTick(uid, component, gameRule, frameTime);

        if (component.NukeAnnouncementAt is { } nukeAnnouncement && nukeAnnouncement <= Timing.CurTime)
        {
            var time = component.NukeTimeAt.HasValue ? component.NukeTimeAt.Value - Timing.CurTime : component.NukeTimeDelay;
            _chat.DispatchGlobalAnnouncement(Loc.GetString("uprising-announcement-nuke-announcement", ("time", (int)Math.Round(time.TotalMinutes))));
            component.NukeAnnouncementAt = null;
        }

        if (component.FirstWarningAt is { } firstWarning && firstWarning <= Timing.CurTime)
        {
            var time = component.NukeTimeAt.HasValue ? component.NukeTimeAt.Value - Timing.CurTime : component.FirstWarning;
            _chat.DispatchGlobalAnnouncement(Loc.GetString("uprising-announcement-first-warning", ("time", (int)Math.Round(time.TotalMinutes))));
            component.FirstWarningAt = null;
        }

        if (component.ImpendingWarningAt is { } impendingWarning && impendingWarning <= Timing.CurTime)
        {
            var time = component.NukeTimeAt.HasValue ? component.NukeTimeAt.Value - Timing.CurTime : component.ImpendingWarning;
            _chat.DispatchGlobalAnnouncement(Loc.GetString("uprising-announcement-impending-warning", ("time", (int)Math.Round(time.TotalMinutes))));
            component.ImpendingWarningAt = null;
        }

        if (component.FinalWarningAt is { } finalWarning && finalWarning <= Timing.CurTime)
        {
            var time = component.NukeTimeAt.HasValue ? component.NukeTimeAt.Value - Timing.CurTime : component.FinalWarning;
            _chat.DispatchGlobalAnnouncement(Loc.GetString("uprising-announcement-impending-warning", ("time", (int)Math.Round(time.TotalMinutes))));
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
                _nuke.ArmBomb(bombUid, bomb);
            }
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
}
