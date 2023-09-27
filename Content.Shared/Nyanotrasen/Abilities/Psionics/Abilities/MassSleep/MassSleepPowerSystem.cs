using Content.Shared.Actions;
using Content.Shared.Bed.Sleep;
using Content.Shared.Magic.Events;
using Content.Shared.Damage;
using Content.Shared.Mobs.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Content.Shared.Mind;
using Content.Shared.Actions.Events;

namespace Content.Shared.Abilities.Psionics
{
    public sealed class MassSleepPowerSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly EntityLookupSystem _lookup = default!;
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly SharedMindSystem _mindSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<MassSleepPowerComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<MassSleepPowerComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<MassSleepPowerComponent, MassSleepPowerActionEvent>(OnPowerUsed);
        }

        private void OnInit(EntityUid uid, MassSleepPowerComponent component, ComponentInit args)
        {
            //Don't know how to deal with TryIndex.

            var action = Spawn(MassSleepPowerComponent.MassSleepActionPrototype);
            if (!_mindSystem.TryGetMind(uid, out var mindId, out var mind) ||
                mind.OwnedEntity is not { } ownedEntity)
            {
                return;
            }
            component.MassSleepPowerAction = new WorldTargetActionComponent();
            _actions.AddAction(mind.OwnedEntity.Value, action, null);

            if (TryComp<PsionicComponent>(uid, out var psionic) && psionic.PsionicAbility == null)
                psionic.PsionicAbility = component.MassSleepPowerAction;
        }

        private void OnShutdown(EntityUid uid, MassSleepPowerComponent component, ComponentShutdown args)
        {
            _actions.RemoveAction(uid, MassSleepPowerComponent.MassSleepActionPrototype, null);
        }

        private void OnPowerUsed(EntityUid uid, MassSleepPowerComponent component, MassSleepPowerActionEvent args)
        {
            foreach (var entity in _lookup.GetEntitiesInRange(args.Target, component.Radius))
            {
                if (HasComp<MobStateComponent>(entity) && entity != uid && !HasComp<PsionicInsulationComponent>(entity))
                {
                    if (TryComp<DamageableComponent>(entity, out var damageable) && damageable.DamageContainerID == "Biological")
                        EnsureComp<SleepingComponent>(entity);
                }
            }
            _psionics.LogPowerUsed(uid, "mass sleep");
            args.Handled = true;
        }
    }
}
