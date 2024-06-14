using Content.Shared.ActionBlocker;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Popups;
using Content.Shared.Psionics.Events;
using Content.Shared.Rejuvenate;
using Content.Shared.Verbs;
using Content.Shared.Whitelist;
using Content.Server.NPC.Components;
using Content.Server.NPC.Systems;
using Content.Server.Carrying;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Server.DeltaV.GlimmerWisp;

public sealed class LifeDrainerSystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly MobStateSystem _mob = default!;
    [Dependency] private readonly NPCRetaliationSystem _retaliation = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LifeDrainerComponent, GetVerbsEvent<InnateVerb>>(OnGetVerbs);
        SubscribeLocalEvent<LifeDrainerComponent, LifeDrainDoAfterEvent>(OnDrain);
    }

    private void OnGetVerbs(Entity<LifeDrainerComponent> ent, ref GetVerbsEvent<InnateVerb> args)
    {
        var target = args.Target;
        if (!args.CanAccess ||
            !CanDrain(ent, target))
            return;

        args.Verbs.Add(new InnateVerb()
        {
            Act = () =>
            {
                TryDrain(ent, target);
            },
            Text = Loc.GetString("verb-life-drain"),
            Icon = new SpriteSpecifier.Texture(new ("/Textures/Nyanotrasen/Icons/verbiconfangs.png")),
            Priority = 2
        });
    }

    private void OnDrain(Entity<LifeDrainerComponent> ent, ref LifeDrainDoAfterEvent args)
    {
        var (uid, comp) = ent;
        comp.DrainStream = _audio.Stop(comp.DrainStream);
        if (!comp.IsDraining || args.Handled || args.Args.Target is not {} target)
            return;

        comp.IsDraining = false;
        comp.Target = null;

        // attack whoever interrupted the draining
        if (args.Cancelled)
        {
            if (!TryComp<NPCRetaliationComponent>(ent, out var retaliation))
                return;

            var ret = (ent.Owner, retaliation);
            if (TryComp<PullableComponent>(target, out var pullable) && pullable.Puller is {} puller)
                _retaliation.TryRetaliate(ret, puller);

            if (TryComp<BeingCarriedComponent>(target, out var carried))
                _retaliation.TryRetaliate(ret, carried.Carrier);

            return;
        }

        _popup.PopupEntity(Loc.GetString("life-drain-second-end", ("drainer", uid)), target, target, PopupType.LargeCaution);
        _popup.PopupEntity(Loc.GetString("life-drain-third-end", ("drainer", uid), ("target", target)), target, Filter.PvsExcept(target), true, PopupType.LargeCaution);

        var rejuv = new RejuvenateEvent();
        RaiseLocalEvent(uid, rejuv);

        _audio.PlayPvs(comp.FinishSound, uid);

        _damageable.TryChangeDamage(target, comp.Damage, true, origin: uid);
    }

    public bool CanDrain(Entity<LifeDrainerComponent> ent, EntityUid target)
    {
        var (uid, comp) = ent;
        return !comp.IsDraining &&
            uid != target &&
            _whitelist.IsWhitelistPass(comp.Whitelist, target) &&
            _mob.IsCritical(target);
    }

    public bool TryDrain(Entity<LifeDrainerComponent> ent, EntityUid target)
    {
        var (uid, comp) = ent;
        if (!CanDrain(ent, target) || !_actionBlocker.CanInteract(uid, target))
            return false;

        comp.IsDraining = true;
        comp.Target = target;

        _popup.PopupEntity(Loc.GetString("life-drain-second-start", ("drainer", uid)), target, target, PopupType.LargeCaution);
        _popup.PopupEntity(Loc.GetString("life-drain-third-start", ("drainer", uid), ("target", target)), target, Filter.PvsExcept(target), true, PopupType.LargeCaution);

        if (_audio.PlayPvs(comp.DrainSound, target) is {} stream)
            comp.DrainStream = stream.Item1;

        var ev = new LifeDrainDoAfterEvent();
        var args = new DoAfterArgs(EntityManager, uid, comp.Delay, ev, target: target, eventTarget: uid)
        {
            BreakOnMove = true,
            MovementThreshold = 2f,
            NeedHand = false
        };

        return _doAfter.TryStartDoAfter(args);
    }

    public void ClearTarget(LifeDrainerComponent comp)
    {
        comp.Target = null;
    }
}

[Serializable]
public sealed partial class LifeDrainDoAfterEvent : SimpleDoAfterEvent;
