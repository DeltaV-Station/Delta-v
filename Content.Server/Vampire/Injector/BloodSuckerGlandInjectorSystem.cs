using Content.Server.Popups;
using Content.Shared.Interaction;
using Robust.Shared.Player;

namespace Content.Server.Vampiric
{
    public sealed class BloodSuckerGlandInjectorSystem : EntitySystem
    {
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<BloodSuckerGlandInjectorComponent, AfterInteractEvent>(OnAfterInteract);
        }

        private void OnAfterInteract(EntityUid uid, BloodSuckerGlandInjectorComponent component, AfterInteractEvent args)
        {
            if (component.Used)
                return;

            if (!args.CanReach)
                return;

            if (!TryComp<BloodSuckerComponent>(args.Target, out var bloodSuckerComponent))
                return;

            // They already have one.
            if (bloodSuckerComponent.InjectWhenSucc)
                return;

            bloodSuckerComponent.InjectWhenSucc = true;
            bloodSuckerComponent.InjectReagent = component.InjectReagent;
            bloodSuckerComponent.UnitsToInject = component.UnitsToInject;
            component.Used = true;
            QueueDel(uid);

            _popupSystem.PopupEntity(Loc.GetString("bloodsucker-glands-throb"), args.Target.Value, args.Target.Value);
        }
    }
}
