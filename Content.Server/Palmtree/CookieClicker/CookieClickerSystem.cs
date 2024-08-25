using Content.Server.Palmtree.CookieClicker;
using Content.Shared.Interaction;

namespace Content.Server.Palmtree.CookieClicker.CounterSystem
{
    public class CookieClickerSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<IncrementorComponent, AfterInteractEvent>(OnAfterInteract);
        }
        private void OnAfterInteract(EntityUid uid, IncrementorComponent component, AfterInteractEvent args)
        {
            if (!args.CanReach || args.Target == null || args.User == args.Target || !TryComp(args.Target, out ClickCounterComponent? counter))
            {
                return;
            }
            counter.count++;
        }
    }
}
