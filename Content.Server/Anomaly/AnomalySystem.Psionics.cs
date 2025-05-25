using Content.Server.Abilities.Psionics;
using Content.Shared.Anomaly;
using Content.Shared.Anomaly.Components;
using Robust.Shared.Random;

namespace Content.Server.Anomaly;

public sealed partial class AnomalySystem
{
    [Dependency] private readonly DispelPowerSystem _dispel = default!;

    private void InitializePsionics()
    {
        SubscribeLocalEvent<AnomalyComponent, DispelledEvent>(OnDispelled);
    }

    //Nyano - Summary: gives dispellable behavior to Anomalies.
    private void OnDispelled(Entity<AnomalyComponent> ent, ref DispelledEvent args)
    {
        _dispel.DealDispelDamage(ent);
        ChangeAnomalyHealth(ent, 0 - _random.NextFloat(0.4f, 0.8f), ent.Comp);
        args.Handled = true;
    }
}
