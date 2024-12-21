﻿using Content.Server.Popups;
using Content.Shared.DoAfter;
using Content.Shared.Verbs;
using Content.Shared.Forensics;
using Content.Shared.Examine;
using Robust.Shared.Utility;
using Content.Shared.IdentityManagement;

namespace Content.Server.Forensics
{
    public sealed class ScentTrackerSystem : EntitySystem
    {
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
        public override void Initialize()
        {
            SubscribeLocalEvent<ScentTrackerComponent, GetVerbsEvent<InnateVerb>>(AddVerbs);
            SubscribeLocalEvent<ScentTrackerComponent, ScentTrackerDoAfterEvent>(TrackScentDoAfter);
            SubscribeLocalEvent<ForensicsComponent, ExaminedEvent>((uid, _, args) => OnExamine(uid, args));
        }

        private void AddVerbs(EntityUid uid, ScentTrackerComponent component, GetVerbsEvent<InnateVerb> args)
        {
            TrackScentVerb(uid, component, args);
            StopTrackScentVerb(uid, component, args);
        }

        private void TrackScentVerb(EntityUid uid, ScentTrackerComponent component, GetVerbsEvent<InnateVerb> args)
        {
            if (!args.CanInteract
                || !args.CanAccess
                || args.User == args.Target)
                return;

            InnateVerb verbTrackScent = new()
            {
                Act = () => AttemptTrackScent(uid, args.Target, component),
                Text = Loc.GetString("track-scent"),
                Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/psionic_invisibility.png")),
                Priority = 1
            };
            args.Verbs.Add(verbTrackScent);
        }

        private void AttemptTrackScent(EntityUid user, EntityUid target, ScentTrackerComponent component)
        {
            if (!HasComp<ScentTrackerComponent>(user))
                return;

            var doAfterEventArgs = new DoAfterArgs(EntityManager, user, component.SniffDelay, new ScentTrackerDoAfterEvent(), user, target: target)
            {
                BreakOnUserMove = true,
                BreakOnDamage = true,
                BreakOnTargetMove = true
            };

            _popupSystem.PopupEntity(Loc.GetString("start-tracking-scent", ("user", Identity.Name(user, EntityManager)), ("target", Identity.Name(target, EntityManager))), user);
            _doAfterSystem.TryStartDoAfter(doAfterEventArgs);
        }

        private void TrackScentDoAfter(Entity<ScentTrackerComponent> entity, ref ScentTrackerDoAfterEvent args)
        {
            if (args.Handled
                || args.Cancelled
                || args.Args.Target == null)
                return;

            TrackScent(args.Args.User, args.Args.Target.Value);

            args.Handled = true;
        }

        private void StopTrackScentVerb(EntityUid uid, ScentTrackerComponent component, GetVerbsEvent<InnateVerb> args)
        {
            if (args.User != args.Target
                || component.Scent == string.Empty)
                return;

            InnateVerb verbStopTrackScent = new()
            {
                Act = () => StopTrackScent(uid, component),
                Text = Loc.GetString("stop-track-scent"),
                Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/psionic_invisibility.png")),
                Priority = 2
            };
            args.Verbs.Add(verbStopTrackScent);
        }

        private void OnExamine(EntityUid uid, ExaminedEvent args)
        {
            if (!TryComp<ScentTrackerComponent>(args.Examiner, out var component)
                || !TryComp<ForensicsComponent>(args.Examined, out var forcomp))
                return;

            if (forcomp.Scent != string.Empty && component.Scent == forcomp.Scent)
                args.PushMarkup(Loc.GetString("examined-scent"), -1);
        }

        #region Utilities
        public void TrackScent(EntityUid uid, EntityUid target)
        {
            if (!TryComp<ScentTrackerComponent>(uid, out var component)
                || !TryComp<ForensicsComponent>(target, out var forcomp))
                return;

            if (forcomp.Scent != string.Empty)
            {
                component.Scent = forcomp.Scent;
                _popupSystem.PopupEntity(Loc.GetString("tracking-scent", ("target", Identity.Name(target, EntityManager))), uid, uid);
            }
            else
                _popupSystem.PopupEntity(Loc.GetString("no-scent"), uid, uid);

            Dirty(uid, component);
        }

        public void StopTrackScent(EntityUid uid, ScentTrackerComponent component)
        {
            if (!HasComp<ScentTrackerComponent>(uid))
                return;

            component.Scent = string.Empty;
            _popupSystem.PopupEntity(Loc.GetString("stopped-tracking-scent"), uid, uid);

            Dirty(uid, component);
        }

        #endregion
    }
}
