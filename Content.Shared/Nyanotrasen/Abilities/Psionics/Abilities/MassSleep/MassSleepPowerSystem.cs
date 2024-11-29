using Content.Shared.Actions;
using Content.Shared.Bed.Sleep;
using Content.Shared.Magic.Events;
using Content.Shared.Damage;
using Content.Shared.Mobs.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Content.Shared.Mind;
using Content.Shared.Actions.Events;
using Content.Shared.StatusEffect;

namespace Content.Shared.Abilities.Psionics
{
    public sealed class MassSleepPowerSystem : EntitySystem
    {
        public ProtoId<StatusEffectPrototype> StatusEffectKey = "ForcedSleep";
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly EntityLookupSystem _lookup = default!;
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<MassSleepPowerComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<MassSleepPowerComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<MassSleepPowerComponent, MassSleepPowerActionEvent>(OnPowerUsed);
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
            var duration = 5; // Duration of the mass sleep
            foreach (var entity in _lookup.GetEntitiesInRange(args.Target, component.Radius))
            {
                if (HasComp<MobStateComponent>(entity) && entity != uid && !HasComp<PsionicInsulationComponent>(entity))
                {
                    if (TryComp<DamageableComponent>(entity, out var damageable) && damageable.DamageContainerID == "Biological")
                        _statusEffects.TryAddStatusEffect<ForcedSleepingComponent>(entity, StatusEffectKey, TimeSpan.FromSeconds(duration), false);
                }
            }
            _psionics.LogPowerUsed(uid, "mass sleep");
            args.Handled = true;
        }
    }
}
