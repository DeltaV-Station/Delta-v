using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Bed.Sleep;
using Content.Shared.Damage;
using Content.Shared.Mobs.Components;
using Robust.Shared.Prototypes;
using Content.Shared.Actions.Events;
using Content.Shared.Popups;
using Content.Shared.Psionics.Events;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;

namespace Content.Shared.Abilities.Psionics
{
    public sealed class MassSleepPowerSystem : EntitySystem
    {
        public ProtoId<StatusEffectPrototype> StatusEffectKey = "ForcedSleep";
        [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly EntityLookupSystem _lookup = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly SharedPopupSystem _popup = default!;
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<MassSleepPowerComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<MassSleepPowerComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<MassSleepPowerComponent, MassSleepPowerActionEvent>(OnPowerUsed);
            SubscribeLocalEvent<MassSleepPowerComponent, MassSleepDoAfterEvent>(OnMassSleepDoAfter);
        }

        private void OnInit(EntityUid uid, MassSleepPowerComponent component, ComponentInit args)
        {
            _actions.AddAction(uid, ref component.MassSleepActionEntity, component.MassSleepActionId );
            _actions.TryGetActionData( component.MassSleepActionEntity, out var actionData );
            if (actionData is { UseDelay: not null })
                _actions.StartUseDelay(component.MassSleepActionEntity);
            if (TryComp<PsionicComponent>(uid, out var psionic) && psionic.PsionicAbility == null)
                psionic.PsionicAbility = component.MassSleepActionEntity;
        }

        private void OnShutdown(EntityUid uid, MassSleepPowerComponent component, ComponentShutdown args)
        {
            _actions.RemoveAction(uid, component.MassSleepActionEntity);
        }

        private void OnPowerUsed(EntityUid uid, MassSleepPowerComponent component, MassSleepPowerActionEvent args)
        {
            var ev = new MassSleepDoAfterEvent();
            var doAfterArgs = new DoAfterArgs(EntityManager, uid, component.UseDelay, ev, uid)
            {
                Hidden = true,
                BreakOnDamage = true
            };

            foreach (var entity in _lookup.GetEntitiesInRange(args.Performer, component.WarningRadius))
            {
                if (HasComp<MobStateComponent>(entity) && entity != uid && !HasComp<PsionicInsulationComponent>(entity))
                {
                    _popup.PopupEntity(Loc.GetString("psionic-power-mass-sleep-warning"),
                        entity,
                        entity,
                        PopupType.LargeCaution);
                }
            }

            _statusEffects.TryAddStatusEffect<SlowedDownComponent>(uid, "SlowedDown", component.UseDelay, true);

            _doAfter.TryStartDoAfter(doAfterArgs, out var doAfterId);
            component.DoAfter = doAfterId;

            _psionics.LogPowerUsed(uid, "mass sleep");
            args.Handled = true;
        }

        private void OnMassSleepDoAfter(EntityUid uid, MassSleepPowerComponent component, MassSleepDoAfterEvent args)
        {
            if (args.Handled)
                return;
            if (args.Cancelled)
            {
                _statusEffects.TryRemoveStatusEffect(uid, "SlowedDown");
                return;
            }

            foreach (var entity in _lookup.GetEntitiesInRange(args.User, component.Radius))
            {
                if (HasComp<MobStateComponent>(entity) && entity != uid && !HasComp<PsionicInsulationComponent>(entity))
                {
                    if (TryComp<DamageableComponent>(entity, out var damageable) && damageable.DamageContainerID == "Biological")
                        _statusEffects.TryAddStatusEffect<ForcedSleepingComponent>(entity, StatusEffectKey, TimeSpan.FromSeconds(component.Duration), false);
                }
            }

            component.DoAfter = null;
        }
    }
}
