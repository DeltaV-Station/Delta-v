using Content.Server.Administration.Logs;
using Content.Server.Cargo.Components;
using Content.Server.Stack;
using Content.Shared._EstacaoPirata.BlindHealing;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Stacks;

namespace Content.Server._EstacaoPirata.BlindHealing
{
    public sealed class BlindHealingSystem : SharedBlindHealingSystem
    {
        [Dependency] private readonly SharedPopupSystem _popup = default!;
        [Dependency] private readonly IAdminLogManager _adminLogger= default!;
        [Dependency] private readonly BlindableSystem _blindableSystem = default!;
        [Dependency] private readonly StackSystem _stackSystem = default!;
        [Dependency] private readonly DamageableSystem _damageableSystem = default!;
        [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<BlindHealingComponent, UseInHandEvent>(OnUse);
            SubscribeLocalEvent<BlindHealingComponent, AfterInteractEvent>(OnInteract);
            SubscribeLocalEvent<BlindHealingComponent, HealingDoAfterEvent>(OnHealingFinished);
        }

        private void OnHealingFinished(EntityUid uid, BlindHealingComponent component, HealingDoAfterEvent args)
        {
            Log.Info("event started!");

            if (args.Cancelled)
                return;

            if (args.Target == null)
                return;

            EntityUid target = (EntityUid) args.Target;

            if(!EntityManager.TryGetComponent(target, out BlindableComponent? blindcomp))
                return;

            if (blindcomp is { EyeDamage: 0 })
                return;

            if(EntityManager.TryGetComponent(uid, out StackComponent? stackComponent))
            {
                double price = 1;
                if(EntityManager.TryGetComponent(uid, out StackPriceComponent? stackPrice))
                {
                    price = stackPrice.Price;
                }
                _stackSystem.SetCount(uid, (int) (_stackSystem.GetCount(uid, stackComponent) - price), stackComponent);

            }

            _blindableSystem.AdjustEyeDamage((target, blindcomp), -blindcomp!.EyeDamage);

            _adminLogger.Add(LogType.Healed, $"{ToPrettyString(args.User):user} repaired {ToPrettyString(uid):target}'s vision");

            var str = Loc.GetString("comp-repairable-repair",
                ("target", uid),
                ("tool", args.Used!));
            _popup.PopupEntity(str, uid, args.User);

        }

        private bool TryHealBlindness(EntityUid uid, EntityUid user, EntityUid target, float delay)
        {
            var doAfterEventArgs =
                new DoAfterArgs(EntityManager, user, delay, new HealingDoAfterEvent(), uid, target: target, used: uid)
                {
                    NeedHand = true,
                    BreakOnMove = true,
                    BreakOnWeightlessMove = false,
                };

            _doAfter.TryStartDoAfter(doAfterEventArgs);
            return true;
        }

        private void OnInteract(EntityUid uid, BlindHealingComponent component, ref AfterInteractEvent args)
        {

            if (args.Handled)
                return;

            if(!TryComp(args.User, out DamageableComponent? damageable))
                return;

            if (damageable.DamageContainerID != null)
            {
                if (!component.DamageContainers.Contains(damageable.DamageContainerID))
                {
                    return;
                }
            }


            if(!TryComp(args.User, out BlindableComponent? blindcomp))
                return;

            if (blindcomp is { EyeDamage: 0 })
                return;

            float delay = component.DoAfterDelay;

            if (args.User == args.Target)
            {
                if (!component.AllowSelfHeal)
                    return;
                delay *= component.SelfHealPenalty;
            }

            TryHealBlindness(uid, args.User, args.User, delay);
        }

        private void OnUse(EntityUid uid, BlindHealingComponent component, ref UseInHandEvent args)
        {
            if (args.Handled)
                return;

            if(!TryComp(args.User, out DamageableComponent? damageable))
                return;

            if (damageable.DamageContainerID != null)
            {
                if (!component.DamageContainers.Contains(damageable.DamageContainerID))
                {
                    return;
                }
            }

            if(!TryComp(args.User, out BlindableComponent? blindcomp))
                return;

            if (blindcomp is { EyeDamage: 0 })
                return;

            if (!component.AllowSelfHeal)
                return;

            float delay = component.DoAfterDelay;
            delay *= component.SelfHealPenalty;

            TryHealBlindness(uid, args.User, args.User, delay);

        }
    }
}
