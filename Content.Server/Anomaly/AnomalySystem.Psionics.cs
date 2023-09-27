using Content.Server.Abilities.Psionics;
using Content.Shared.Anomaly;
using Content.Shared.Anomaly.Components;
using Robust.Shared.Random;

namespace Content.Server.Anomaly;

public sealed partial class AnomalySystem
{
    [Dependency] private readonly SharedAnomalySystem _sharedAnomaly = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly DispelPowerSystem _dispel = default!;
    private void InitializePsionics()
    {
        SubscribeLocalEvent<AnomalyComponent, DispelledEvent>(OnDispelled);
    }

    private void OnDispelled(EntityUid uid, AnomalyComponent component, DispelledEvent args)
    {
        _dispel.DealDispelDamage(uid);
        _sharedAnomaly.ChangeAnomalyHealth(uid, 0 - _random.NextFloat(0.4f, 0.8f), component);
        args.Handled = true;
    }
}
