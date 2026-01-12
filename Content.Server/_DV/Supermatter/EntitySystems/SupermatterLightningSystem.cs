using System.Linq;
using Content.Server._DV.Supermatter.Components;
using Content.Server.Lightning;
using Content.Shared._Impstation.Supermatter.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._DV.Supermatter.EntitySystems;

/// <summary>
/// This handles generating lightning strikes caused as side effects of a supermatter crystal.
/// </summary>
public sealed class SupermatterLightningSystem : EntitySystem
{
    [Dependency] private readonly LightningSystem _lightning = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    protected override string SawmillName => "supermatter.lightning";

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<SupermatterLightningComponent, SupermatterComponent>();

        while (query.MoveNext(out var uid, out var lightningComp, out var supermatter))
        {
            if(!lightningComp.Enabled)
                continue;

            var isDamageValid = lightningComp.EnableDamageThreshold && lightningComp.DamageThresholds.Any(kvp => kvp.Key <= supermatter.Damage);
            var isPowerValid = lightningComp.EnablePowerThresholds && lightningComp.PowerThresholds.Any(kvp => kvp.Key <= supermatter.Power);

            if (!isDamageValid && !isPowerValid)
            {
                if(lightningComp.NextZapTime.HasValue) lightningComp.NextZapTime = null;

                continue;
            }

            if (!lightningComp.NextZapTime.HasValue || _timing.CurTime > lightningComp.NextZapTime.Value)
            {
                ShootLightning(uid, lightningComp, supermatter);

                lightningComp.NextZapTime = _timing.CurTime + lightningComp.ZapInterval + TimeSpan.FromSeconds(_random.NextFloat(-lightningComp.ZapIntervalVariance, lightningComp.ZapIntervalVariance));
            }
        }
    }

    /// <summary>
    /// Shoot lightning bolts depending on accumulated power.
    /// </summary>
    private void ShootLightning(EntityUid uid, SupermatterLightningComponent comp, SupermatterComponent sm)
    {
        var zapCount = 0;
        var zapRange = Math.Clamp(sm.Power / comp.LightningRangePowerScaling, comp.LightningRangeMin, comp.LightningRangeMax);

        var lightningPrototype = comp.LightningPrototype;

        foreach (var (threshold, data) in comp.DamageThresholds)
        {
            if (threshold > sm.Damage) break;

            if(data.Chance.HasValue && !_random.Prob(data.Chance.Value))
                continue;

            zapCount += data.Zaps;
        }

        foreach (var (threshold, data) in comp.PowerThresholds)
        {
            if (threshold > sm.Power) break;

            if(data.LightningPrototype.HasValue)
                lightningPrototype = data.LightningPrototype.Value;

            if(data.Chance.HasValue && !_random.Prob(data.Chance.Value))
                continue;

            zapCount += data.Zaps;
        }

        if (zapCount >= 1)
            _lightning.ShootRandomLightnings(uid, zapRange, zapCount, lightningPrototype, hitCoordsChance: comp.ZapHitCoordinatesChance);
    }
}
