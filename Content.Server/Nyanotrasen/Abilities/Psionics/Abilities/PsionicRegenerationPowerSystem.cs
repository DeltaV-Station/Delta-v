using Robust.Shared.Audio;
using Robust.Shared.Player;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.DoAfter;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions;
using Content.Shared.Chemistry.Components;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Content.Shared.Psionics.Events;
using Content.Shared.Examine;
using static Content.Shared.Examine.ExamineSystemShared;
using Robust.Shared.Timing;
using Content.Shared.Actions.Events;
using Robust.Server.Audio;

namespace Content.Server.Abilities.Psionics
{
    public sealed class PsionicRegenerationPowerSystem : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
        [Dependency] private readonly AudioSystem _audioSystem = default!;
        [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;


        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<PsionicRegenerationPowerComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<PsionicRegenerationPowerComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<PsionicRegenerationPowerComponent, PsionicRegenerationPowerActionEvent>(OnPowerUsed);
            SubscribeLocalEvent<PsionicRegenerationPowerComponent, MobStateChangedEvent>(OnMobStateChangedEvent);
            SubscribeLocalEvent<PsionicRegenerationPowerComponent, DispelledEvent>(OnDispelled);
            SubscribeLocalEvent<PsionicRegenerationPowerComponent, PsionicRegenerationDoAfterEvent>(OnDoAfter);
        }

        private void OnInit(EntityUid uid, PsionicRegenerationPowerComponent component, ComponentInit args)
        {
            _actions.AddAction(uid, ref component.PsionicRegenerationActionEntity, component.PsionicRegenerationActionId );
            _actions.TryGetActionData( component.PsionicRegenerationActionEntity, out var actionData );
            if (actionData is { UseDelay: not null })
                _actions.StartUseDelay(component.PsionicRegenerationActionEntity);
            if (TryComp<PsionicComponent>(uid, out var psionic))
            {
                psionic.ActivePowers.Add(component);
                psionic.PsychicFeedback.Add(component.RegenerationFeedback);
            }
        }

        private void OnPowerUsed(EntityUid uid, PsionicRegenerationPowerComponent component, PsionicRegenerationPowerActionEvent args)
        {
            var ev = new PsionicRegenerationDoAfterEvent(_gameTiming.CurTime);
            var doAfterArgs = new DoAfterArgs(EntityManager, uid, component.UseDelay, ev, uid);

            //Prevent the power from ignoring its own cooldown
            _actions.TryGetActionData(component.PsionicRegenerationActionEntity, out var actionData);
            var curTime = _gameTiming.CurTime;
            if (actionData != null && actionData.Cooldown.HasValue && actionData.Cooldown.Value.End > curTime)
                return;

            _doAfterSystem.TryStartDoAfter(doAfterArgs, out var doAfterId);

            component.DoAfter = doAfterId;

            _popupSystem.PopupEntity(Loc.GetString("psionic-regeneration-begin", ("entity", uid)),
                uid,
                // TODO: Use LoS-based Filter when one is available.
                Filter.Pvs(uid).RemoveWhereAttachedEntity(entity => !ExamineSystemShared.InRangeUnOccluded(uid, entity, ExamineRange, null)),
                true,
                PopupType.Medium);

            _audioSystem.PlayPvs(component.SoundUse, uid, AudioParams.Default.WithVolume(8f).WithMaxDistance(1.5f).WithRolloffFactor(3.5f));
            _psionics.LogPowerUsed(uid, "psionic regeneration");
            args.Handled = true;
        }

        /// <summary>
        /// Regenerators automatically activate upon crit, provided the power was off cooldown at that exact point in time.
        /// Self-rescusitation is also far more costly, and extremely obvious
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="component"></param>
        /// <param name="args"></param>
        private void OnMobStateChangedEvent(EntityUid uid, PsionicRegenerationPowerComponent component, MobStateChangedEvent args)
        {
            if (HasComp<PsionicInsulationComponent>(uid))
                return;

            if (args.NewMobState is MobState.Critical)
            {
                _actions.TryGetActionData(component.PsionicRegenerationActionEntity, out var actionData);
                var curTime = _gameTiming.CurTime;
                if (actionData != null && actionData.Cooldown.HasValue && actionData.Cooldown.Value.End > curTime)
                    return;

                if (actionData is { UseDelay: not null })
                {
                    component.SelfRevive = true;
                    _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, uid, component.UseDelay, new PsionicRegenerationDoAfterEvent(_gameTiming.CurTime), uid, args.Target, uid)
                    {
                        BreakOnUserMove = false,
                        BreakOnTargetMove = false,
                        BreakOnWeightlessMove = false,
                        BreakOnDamage = false,
                        RequireCanInteract = false,
                    });
                    _popupSystem.PopupEntity(Loc.GetString("psionic-regeneration-self-revive", ("entity", uid)),
                        uid,
                        // TODO: Use LoS-based Filter when one is available.
                        Filter.Pvs(uid).RemoveWhereAttachedEntity(entity => !ExamineSystemShared.InRangeUnOccluded(uid, entity, ExamineRange, null)),
                        true,
                        PopupType.MediumCaution);
                    _audioSystem.PlayPvs(component.SoundUse, uid, AudioParams.Default.WithVolume(8f).WithMaxDistance(1.5f).WithRolloffFactor(3.5f));
                    _psionics.LogPowerUsed(uid, "psionic regeneration", 20, 40);
                    _actions.StartUseDelay(component.PsionicRegenerationActionEntity);
                }
            }
        }

        private void OnShutdown(EntityUid uid, PsionicRegenerationPowerComponent component, ComponentShutdown args)
        {
            _actions.RemoveAction(uid, component.PsionicRegenerationActionEntity);

            if (TryComp<PsionicComponent>(uid, out var psionic))
            {
                psionic.ActivePowers.Remove(component);
                psionic.PsychicFeedback.Remove(component.RegenerationFeedback);
            }
        }

        private void OnDispelled(EntityUid uid, PsionicRegenerationPowerComponent component, DispelledEvent args)
        {
            if (component.DoAfter == null)
                return;

            _doAfterSystem.Cancel(component.DoAfter);
            component.DoAfter = null;

            args.Handled = true;
        }

        private void OnDoAfter(EntityUid uid, PsionicRegenerationPowerComponent component, PsionicRegenerationDoAfterEvent args)
        {
            component.DoAfter = null;

            if (!TryComp<BloodstreamComponent>(uid, out var stream))
                return;

            // DoAfter has no way to run a callback during the process to give
            // small doses of the reagent, so we wait until either the action
            // is cancelled (by being dispelled) or complete to give the
            // appropriate dose. A timestamp delta is used to accomplish this.
            var percentageComplete = Math.Min(1f, (_gameTiming.CurTime - args.StartedAt).TotalSeconds / component.UseDelay);

            var solution = new Solution();
            solution.AddReagent("PsionicRegenerationEssence", FixedPoint2.New(component.EssenceAmount * percentageComplete));
            _bloodstreamSystem.TryAddToChemicals(uid, solution, stream);
            if (component.SelfRevive == true)
            {
                var critSolution = new Solution();
                critSolution.AddReagent("Epinephrine", 10);
                _bloodstreamSystem.TryAddToChemicals(uid, critSolution, stream);
                component.SelfRevive = false;
            }
        }
    }
}
