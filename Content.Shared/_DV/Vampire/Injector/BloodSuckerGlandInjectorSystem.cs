using Content.Shared.Interaction;
using Content.Shared.Popups;

namespace Content.Shared._DV.Vampire.Injector
{
    public sealed class BloodSuckerGlandInjectorSystem : EntitySystem
    {
        [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
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
            if (bloodSuckerComponent.InjectWhenSuck)
                return;

            bloodSuckerComponent.InjectWhenSuck = true;
            bloodSuckerComponent.InjectReagent = component.InjectReagent;
            bloodSuckerComponent.UnitsToInject = component.UnitsToInject;
            component.Used = true;
            QueueDel(uid);

            _popupSystem.PopupEntity(Loc.GetString("bloodsucker-glands-throb"), args.Target.Value, args.Target.Value);
        }
    }
}
