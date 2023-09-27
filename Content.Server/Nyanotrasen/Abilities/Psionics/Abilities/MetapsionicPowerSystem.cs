using Content.Shared.Actions;
using Content.Shared.Abilities.Psionics;
using Content.Shared.StatusEffect;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Content.Shared.Mind;
using Content.Shared.Actions.Events;

namespace Content.Server.Abilities.Psionics
{
    public sealed class MetapsionicPowerSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly EntityLookupSystem _lookup = default!;
        [Dependency] private readonly SharedPopupSystem _popups = default!;
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly SharedMindSystem _mindSystem = default!;


        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<MetapsionicPowerComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<MetapsionicPowerComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<MetapsionicPowerComponent, MetapsionicPowerActionEvent>(OnPowerUsed);
        }

        private void OnInit(EntityUid uid, MetapsionicPowerComponent component, ComponentInit args)
        {
            //Don't know how to deal with TryIndex.

            var action = Spawn(MetapsionicPowerComponent.MetapsionicActionPrototype);
            if (!_mindSystem.TryGetMind(uid, out var mindId, out var mind) ||
                mind.OwnedEntity is not { } ownedEntity)
            {
                return;
            }
            component.MetapsionicPowerAction = new InstantActionComponent();
            _actions.AddAction(mind.OwnedEntity.Value, action, null);

            if (TryComp<PsionicComponent>(uid, out var psionic) && psionic.PsionicAbility == null)
                psionic.PsionicAbility = component.MetapsionicPowerAction;
        }

        private void OnShutdown(EntityUid uid, MetapsionicPowerComponent component, ComponentShutdown args)
        {
            _actions.RemoveAction(uid, MetapsionicPowerComponent.MetapsionicActionPrototype, null);
        }

        private void OnPowerUsed(EntityUid uid, MetapsionicPowerComponent component, MetapsionicPowerActionEvent args)
        {
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
            _psionics.LogPowerUsed(uid, "metapsionic pulse", 2, 4);

            args.Handled = true;
        }
    }
}
