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

namespace Content.Shared.Abilities.Psionics
{
    public sealed class MassSleepPowerSystem : EntitySystem
    {
        public ProtoId<StatusEffectPrototype> StatusEffectKey = "ForcedSleep";
        [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly EntityLookupSystem _lookup = default!;
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

        private void OnInit(Entity<MassSleepPowerComponent> ent, ref ComponentInit args)
        {
            _actions.AddAction(ent, ref ent.Comp.MassSleepActionEntity, ent.Comp.MassSleepActionId );
            _actions.TryGetActionData( ent.Comp.MassSleepActionEntity, out var actionData );
            if (actionData is { UseDelay: not null })
                _actions.StartUseDelay(ent.Comp.MassSleepActionEntity);
            if (TryComp<PsionicComponent>(ent, out var psionic) && psionic.PsionicAbility == null)
                psionic.PsionicAbility = ent.Comp.MassSleepActionEntity;
        }

        private void OnShutdown(Entity<MassSleepPowerComponent> ent, ref ComponentShutdown args)
        {
            _actions.RemoveAction(ent, ent.Comp.MassSleepActionEntity);
        }

        private void OnPowerUsed(Entity<MassSleepPowerComponent> ent, ref MassSleepPowerActionEvent args)
        {
            var ev = new MassSleepDoAfterEvent();
            var doAfterArgs = new DoAfterArgs(EntityManager, ent, ent.Comp.UseDelay, ev, ent)
            {
                BreakOnDamage = true
            };

            foreach (var entity in _lookup.GetEntitiesInRange(args.Performer, ent.Comp.WarningRadius))
            {
                if (HasComp<MobStateComponent>(entity) && entity != (EntityUid)ent && !HasComp<PsionicInsulationComponent>(entity))
                {
                    _popup.PopupEntity(Loc.GetString("psionic-power-mass-sleep-warning"),
                        entity,
                        entity,
                        PopupType.LargeCaution);
                }
            }

            _statusEffects.TryAddStatusEffect<SlowedDownComponent>(ent, "SlowedDown", ent.Comp.UseDelay, true);

            _doAfter.TryStartDoAfter(doAfterArgs, out var doAfterId);
            ent.Comp.DoAfter = doAfterId;

            _psionics.LogPowerUsed(ent, "mass sleep");
            args.Handled = true;
        }

        private void OnMassSleepDoAfter(Entity<MassSleepPowerComponent> ent, ref MassSleepDoAfterEvent args)
        {
            if (args.Handled)
                return;

            if (args.Cancelled)
            {
                _statusEffects.TryRemoveStatusEffect(ent, "SlowedDown");
                return;
            }

            foreach (var entity in _lookup.GetEntitiesInRange(args.User, ent.Comp.Radius))
            {
                if (HasComp<MobStateComponent>(entity) && entity != (EntityUid)ent && !HasComp<PsionicInsulationComponent>(entity))
                {
                    if (TryComp<DamageableComponent>(entity, out var damageable) && damageable.DamageContainerID == "Biological")
                        _statusEffects.TryAddStatusEffect<ForcedSleepingComponent>(entity, StatusEffectKey, TimeSpan.FromSeconds(ent.Comp.Duration), false);
                }
            }

            ent.Comp.DoAfter = null;
        }
    }
}
