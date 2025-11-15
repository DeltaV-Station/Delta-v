using Content.Server.Anomaly.Components;
using Content.Shared.Anomaly.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Random;

namespace Content.Server.Anomaly.Effects;
public sealed class ShuffleParticlesAnomalySystem : EntitySystem
{
    [Dependency] private readonly AnomalySystem _anomaly = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ShuffleParticlesAnomalyComponent, AnomalyPulseEvent>(OnPulse, after: [typeof(AnomalySystem)]); // DeltaV - Add after: to change particles only after triggering effects
        SubscribeLocalEvent<ShuffleParticlesAnomalyComponent, StartCollideEvent>(OnStartCollide, after: [typeof(AnomalySystem)]); // DeltaV - Add after: to change particles only after triggering effects
    }

    private void OnStartCollide(Entity<ShuffleParticlesAnomalyComponent> ent, ref StartCollideEvent args)
    {
        if (!TryComp<AnomalyComponent>(ent, out var anomaly))
            return;

        if (!HasComp<AnomalousParticleComponent>(args.OtherEntity))
            return;

        if (ent.Comp.ShuffleOnParticleHit && _random.Prob(ent.Comp.Prob))
            _anomaly.ShuffleParticlesEffect((ent, anomaly));
    }

    private void OnPulse(Entity<ShuffleParticlesAnomalyComponent> ent, ref AnomalyPulseEvent args)
    {
        if (!TryComp<AnomalyComponent>(ent, out var anomaly))
            return;

        if (ent.Comp.ShuffleOnPulse && _random.Prob(ent.Comp.Prob))
        {
            _anomaly.ShuffleParticlesEffect((ent, anomaly));
        }
    }
}

