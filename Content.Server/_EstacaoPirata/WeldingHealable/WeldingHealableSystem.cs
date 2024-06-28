using System.Diagnostics;
using Content.Server._EstacaoPirata.WeldingHealing;
using Content.Server.Administration.Logs;
using Content.Server.Stack;
using Content.Server.Tools.Components;
using Content.Shared._EstacaoPirata.WeldingHealing;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Tools;
using Content.Shared.Stacks;
using SharedToolSystem = Content.Shared.Tools.Systems.SharedToolSystem;

namespace Content.Server._EstacaoPirata.WeldingHealable
{
    public sealed class WeldingHealableSystem : SharedWeldingHealableSystem
    {
        [Dependency] private readonly SharedToolSystem _toolSystem = default!;
        [Dependency] private readonly DamageableSystem _damageableSystem = default!;
        [Dependency] private readonly SharedPopupSystem _popup = default!;
        [Dependency] private readonly IAdminLogManager _adminLogger= default!;
        [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer= default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<WeldingHealableComponent, InteractUsingEvent>(Repair);
            SubscribeLocalEvent<WeldingHealableComponent, SiliconRepairFinishedEvent>(OnRepairFinished);
        }

        private void OnRepairFinished(EntityUid uid, WeldingHealableComponent healableComponentcomponent, SiliconRepairFinishedEvent args)
        {
            if (args.Cancelled)
                return;

            if (args.Used == null)
                return;

            if(!EntityManager.TryGetComponent(args.Target, out DamageableComponent? damageable))
                return;

            if(!EntityManager.TryGetComponent(args.Used, out WeldingHealingComponent? component))
                return;

            if (damageable.DamageContainerID != null)
            {
                if (!component.DamageContainers.Contains(damageable.DamageContainerID))
                    return;
            }

            var damageChanged = _damageableSystem.TryChangeDamage(uid, component.Damage, true, false, origin: args.User);


            if (!HasDamage(damageable, component))
                return;

            if (TryComp(args.Used, out WelderComponent? welder) &&
                TryComp(args.Used, out SolutionContainerManagerComponent? solutionContainer))
            {
                if (!_solutionContainer.ResolveSolution(((EntityUid) args.Used, solutionContainer), welder.FuelSolutionName, ref welder.FuelSolution, out var solution))
                    return;
                _solutionContainer.RemoveReagent(welder.FuelSolution.Value, welder.FuelReagent, component.FuelCost);
            }

            var str = Loc.GetString("comp-repairable-repair",
                ("target", uid),
                ("tool", args.Used!));
            _popup.PopupEntity(str, uid, args.User);


            if (args.Used.HasValue)
            {
                args.Handled = _toolSystem.UseTool(args.Used.Value, args.User, uid, args.Delay, component.QualityNeeded, new SiliconRepairFinishedEvent
                {
                    Delay = args.Delay
                });
            }
        }



        private async void Repair(EntityUid uid, WeldingHealableComponent healableComponent, InteractUsingEvent args)
        {
            if (args.Handled)
                return;

            if(!EntityManager.TryGetComponent(args.Used, out WeldingHealingComponent? component))
                return;

            if(!EntityManager.TryGetComponent(args.Target, out DamageableComponent? damageable))
                return;

            if (damageable.DamageContainerID != null)
            {
                if (!component.DamageContainers.Contains(damageable.DamageContainerID))
                    return;
            }
            if (!HasDamage(damageable, component))
                return;

            if (!_toolSystem.HasQuality(args.Used, component.QualityNeeded))
                return;

            float delay = component.DoAfterDelay;

            // Add a penalty to how long it takes if the user is repairing itself
            if (args.User == args.Target)
            {
                if (!component.AllowSelfHeal)
                    return;

                delay *= component.SelfHealPenalty;
            }

            // Run the repairing doafter
            args.Handled = _toolSystem.UseTool(args.Used, args.User, args.Target, delay, component.QualityNeeded, new SiliconRepairFinishedEvent
            {
                Delay = delay,
            });

        }
        private bool HasDamage(DamageableComponent component, WeldingHealingComponent healable)
        {
            var damageableDict = component.Damage.DamageDict;
            var healingDict = healable.Damage.DamageDict;
            foreach (var type in healingDict)
            {
                if (damageableDict[type.Key].Value > 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
