using Content.Server.PowerCell;
using Content.Shared.Prying.Components;

namespace Content.Server._DV.Tools;

public sealed class PryingRequiresPowerSystem : EntitySystem
{
    [Dependency] private readonly PowerCellSystem _power = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PryingRequiresPowerComponent, BeforePryEvent>(OnBeforePry);
    }

    private void OnBeforePry(EntityUid uid, PryingRequiresPowerComponent component, ref BeforePryEvent args)
    {
        if (!_power.TryUseCharge(uid, component.UseCost))
            args.Cancelled = true;
    }
}
