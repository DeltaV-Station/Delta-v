using Robust.Shared.Prototypes;
using Content.Server.Chemistry.Components;
using Content.Server.Popups;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Verbs;

namespace Content.Server.Chemistry.EntitySystems
{
    public sealed class SolutionRegenerationSwitcherSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly SolutionContainerSystem _solutionSystem = default!;
        [Dependency] private readonly PopupSystem _popups = default!;

        private ISawmill _sawmill = default!;

        public override void Initialize()
        {
            base.Initialize();

            _sawmill = Logger.GetSawmill("chemistry");

            SubscribeLocalEvent<SolutionRegenerationSwitcherComponent, GetVerbsEvent<AlternativeVerb>>(AddSwitchVerb);
        }

        private void AddSwitchVerb(EntityUid uid, SolutionRegenerationSwitcherComponent component, GetVerbsEvent<AlternativeVerb> args)
        {
            if (!args.CanInteract || !args.CanAccess)
                return;

            if (component.Options.Count <= 1)
                return;

            AlternativeVerb verb = new()
            {
                Act = () =>
                {
                    SwitchReagent(uid, component, args.User);
                },
                Text = Loc.GetString("autoreagent-switch"),
                Priority = 2
            };
            args.Verbs.Add(verb);
        }

        private void SwitchReagent(EntityUid uid, SolutionRegenerationSwitcherComponent component, EntityUid user)
        {
            if (!TryComp<SolutionRegenerationComponent>(uid, out var solutionRegenerationComponent))
            {
                _sawmill.Warning($"{ToPrettyString(uid)} has no SolutionRegenerationComponent.");
                return;
            }

            if (component.CurrentIndex + 1 == component.Options.Count)
                component.CurrentIndex = 0;
            else
                component.CurrentIndex++;

            if (!_solutionSystem.TryGetSolution(uid, solutionRegenerationComponent.Solution, out var solution))
            {
                _sawmill.Error($"Can't get SolutionRegeneration.Solution for {ToPrettyString(uid)}");
                return;
            }

            var newSolution = component.Options[component.CurrentIndex];
            var primaryId = newSolution.GetPrimaryReagentId();
            if (primaryId == null)
            {
                _sawmill.Error($"Can't get PrimaryReagentId for {ToPrettyString(uid)} on index {component.CurrentIndex}.");
                return;
            }
            ReagentPrototype? proto;

            //Only reagents with spritePath property can change appearance of transformable containers!
            if (!string.IsNullOrWhiteSpace(primaryId?.Prototype))
            {
                if (!_prototypeManager.TryIndex(primaryId.Value.Prototype, out proto))
                {
                    _sawmill.Error($"Can't get get reagent prototype {primaryId} for {ToPrettyString(uid)}");
                    return;
                }
            }
            else return;

            // Empty out the current solution.
            if (!component.KeepSolution)
                solution.RemoveAllSolution();

            // Replace the generating solution with the newly selected solution.
            var generated = solutionRegenerationComponent.Generated;
            generated.RemoveAllSolution();
            _solutionSystem.TryAddSolution(uid, generated, newSolution);

            _popups.PopupEntity(Loc.GetString("autoregen-switched", ("reagent", proto.LocalizedName)), user, user);
        }
    }
}
