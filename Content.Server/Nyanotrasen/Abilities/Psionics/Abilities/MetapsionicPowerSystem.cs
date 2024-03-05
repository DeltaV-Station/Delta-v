using Content.Shared.Actions;
using Content.Shared.Actions.Events;
using Content.Shared.Abilities.Psionics;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using static Content.Shared.Examine.ExamineSystemShared;
using Content.Shared.Popups;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Timing;
using Robust.Shared.Player;
using Content.Server.DoAfter;
using Content.Shared.Psionics.Events;
using Content.Server.Psionics;

namespace Content.Server.Abilities.Psionics
{
    public sealed class MetapsionicPowerSystem : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly EntityLookupSystem _lookup = default!;
        [Dependency] private readonly SharedPopupSystem _popups = default!;
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
        [Dependency] private readonly AudioSystem _audioSystem = default!;


        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<MetapsionicPowerComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<MetapsionicPowerComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<MetapsionicPowerComponent, WideMetapsionicPowerActionEvent>(OnWidePowerUsed);
            SubscribeLocalEvent<FocusedMetapsionicPowerActionEvent>(OnFocusedPowerUsed);
            SubscribeLocalEvent<MetapsionicPowerComponent, FocusedMetapsionicDoAfterEvent>(OnDoAfter);
        }

        private void OnInit(EntityUid uid, MetapsionicPowerComponent component, ComponentInit args)
        {
            if (!TryComp(uid, out ActionsComponent? comp))
                return;
            _actions.AddAction(uid, ref component.ActionWideMetapsionicEntity, component.ActionWideMetapsionic, component: comp);
            _actions.AddAction(uid, ref component.ActionFocusedMetapsionicEntity, component.ActionFocusedMetapsionic, component: comp);
            _actions.TryGetActionData(component.ActionWideMetapsionicEntity, out var actionData);
            if (actionData is { UseDelay: not null })
            {
                _actions.StartUseDelay(component.ActionWideMetapsionicEntity);
                _actions.StartUseDelay(component.ActionFocusedMetapsionicEntity);
            }
            if (TryComp<PsionicComponent>(uid, out var psionic))
            {
                psionic.ActivePowers.Add(component);
                psionic.PsychicFeedback.Add(component.MetapsionicFeedback);
                psionic.Amplification += 0.1f;
                psionic.Dampening += 0.5f;
            }

        }

        private void UpdateActions(EntityUid uid, MetapsionicPowerComponent? component = null)
        {
            if (!Resolve(uid, ref component))
                return;
            _actions.StartUseDelay(component.ActionWideMetapsionicEntity);
            _actions.StartUseDelay(component.ActionFocusedMetapsionicEntity);
        }

        private void OnShutdown(EntityUid uid, MetapsionicPowerComponent component, ComponentShutdown args)
        {
            _actions.RemoveAction(uid, component.ActionWideMetapsionicEntity);
            _actions.RemoveAction(uid, component.ActionFocusedMetapsionicEntity);

            if (TryComp<PsionicComponent>(uid, out var psionic))
            {
                psionic.ActivePowers.Remove(component);
                psionic.PsychicFeedback.Remove(component.MetapsionicFeedback);
                psionic.Amplification -= 0.1f;
                psionic.Dampening -= 0.5f;
            }
        }

        private void OnWidePowerUsed(EntityUid uid, MetapsionicPowerComponent component, WideMetapsionicPowerActionEvent args)
        {
            if (!TryComp<PsionicComponent>(uid, out var psionic))
                return;

            foreach (var entity in _lookup.GetEntitiesInRange(uid, component.Range))
            {
                if (HasComp<PsionicComponent>(entity) && entity != uid && !HasComp<PsionicInsulationComponent>(entity) &&
                    !(HasComp<ClothingGrantPsionicPowerComponent>(entity) && Transform(entity).ParentUid == uid))
                {
                    _popups.PopupEntity(Loc.GetString("metapsionic-pulse-success"), uid, uid, PopupType.LargeCaution);
                    args.Handled = true;
                    return;
                }
            }
            _popups.PopupEntity(Loc.GetString("metapsionic-pulse-failure"), uid, uid, PopupType.Large);
            _psionics.LogPowerUsed(uid, "metapsionic pulse", (int) MathF.Round(psionic.Amplification / psionic.Dampening * 2), (int) MathF.Round(psionic.Amplification / psionic.Dampening * 4));
            UpdateActions(uid, component);
            args.Handled = true;
        }

        private void OnFocusedPowerUsed(FocusedMetapsionicPowerActionEvent args)
        {
            if (!TryComp<PsionicComponent>(args.Performer, out var psionic))
                return;

            if (HasComp<PsionicInsulationComponent>(args.Target))
                return;

            if (!TryComp<MetapsionicPowerComponent>(args.Performer, out var component))
                return;

            var ev = new FocusedMetapsionicDoAfterEvent(_gameTiming.CurTime);

            _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, args.Performer, component.UseDelay, ev, args.Performer, args.Target, args.Performer)
            {
                BlockDuplicate = true,
                BreakOnUserMove = true,
                BreakOnTargetMove = true,
                BreakOnDamage = true,
            }, out var doAfterId);

            component.DoAfter = doAfterId;

            _popups.PopupEntity(Loc.GetString("focused-metapsionic-pulse-begin", ("entity", args.Performer)),
                args.Performer,
                // TODO: Use LoS-based Filter when one is available.
                Filter.Pvs(args.Performer).RemoveWhereAttachedEntity(entity => !ExamineSystemShared.InRangeUnOccluded(args.Performer, entity, ExamineRange, null)),
                true,
                PopupType.Medium);

            _audioSystem.PlayPvs(component.SoundUse, component.Owner, AudioParams.Default.WithVolume(8f).WithMaxDistance(1.5f).WithRolloffFactor(3.5f));
            _psionics.LogPowerUsed(args.Performer, "focused metapsionic pulse", (int) MathF.Round(psionic.Amplification / psionic.Dampening * 3), (int) MathF.Round(psionic.Amplification / psionic.Dampening * 6));
            args.Handled = true;

            UpdateActions(args.Performer, component);
        }

        private void OnDoAfter(EntityUid uid, MetapsionicPowerComponent component, FocusedMetapsionicDoAfterEvent args)
        {
            if (!TryComp<PsionicComponent>(args.Target, out var psychic))
                return;

            component.DoAfter = null;

            if (args.Target == null) return;

            if (args.Target == uid)
            {
                _popups.PopupEntity(Loc.GetString("metapulse-self", ("entity", args.Target)), uid, uid, PopupType.LargeCaution);
                return;
            }

            if (!HasComp<PotentialPsionicComponent>(args.Target))
            {
                _popups.PopupEntity(Loc.GetString("no-powers", ("entity", args.Target)), uid, uid, PopupType.LargeCaution);
                return;
            }

            if (HasComp<PotentialPsionicComponent>(args.Target) & !HasComp<PsionicComponent>(args.Target))
            {
                _popups.PopupEntity(Loc.GetString("psychic-potential", ("entity", args.Target)), uid, uid, PopupType.LargeCaution);
                return;
            }

            foreach (var psychicFeedback in psychic.PsychicFeedback)
            {
                _popups.PopupEntity(Loc.GetString(psychicFeedback, ("entity", args.Target)), uid, uid, PopupType.LargeCaution);
            }

        }
    }
}
