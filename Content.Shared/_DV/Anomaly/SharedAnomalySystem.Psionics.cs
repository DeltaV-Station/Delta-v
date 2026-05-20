using Content.Shared._DV.CosmicCult;
using Content.Shared._DV.Psionics.Events;
using Content.Shared._DV.Psionics.Systems.PsionicPowers;
using Content.Shared.Anomaly.Components;

namespace Content.Shared.Anomaly;

public abstract partial class SharedAnomalySystem
{
    [Dependency] private readonly SharedDispelPowerSystem _dispel = default!;

    private void InitializePsionics()
    {
        SubscribeLocalEvent<AnomalyComponent, DispelledEvent>(OnDispelled);
    }

    //Nyano - Summary: gives dispellable behavior to Anomalies.
    private void OnDispelled(Entity<AnomalyComponent> anomaly, ref DispelledEvent args)
    {
        if (HasComp<CosmicCultExamineComponent>(anomaly)) // begone nyanocode interference with cosmic cult
            return;

        _dispel.DealDispelDamage(anomaly, dispeller: args.Dispeller);
        ChangeAnomalyHealth(anomaly, 0 - Random.NextFloat(0.4f, 0.8f), anomaly.Comp);
        args.Handled = true;
    }
}
