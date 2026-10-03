using Content.Shared._DV.Chemistry.Components;
using Content.Shared._DV.Chemistry.Events;
using Content.Shared.Access.Systems;

namespace Content.Shared._DV.Chemistry.Systems;

public sealed partial class InjectionRequiresTargetAccessSystem : EntitySystem
{
    [Dependency] private readonly AccessReaderSystem _access = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InjectionRequiresTargetAccessComponent, BeforeInjectEvent>(OnBeforeInject);
    }

    private void OnBeforeInject(Entity<InjectionRequiresTargetAccessComponent> injector, ref BeforeInjectEvent args)
    {
        if (args.Cancelled)
            return;

        if (_access.IsAllowed(args.TargetGettingInjected, injector))
            return;

        args.Cancel();
        if (injector.Comp.PopupMessage != null)
            args.OverrideMessage = Loc.GetString(injector.Comp.PopupMessage, ("target", args.TargetGettingInjected));
    }
}
