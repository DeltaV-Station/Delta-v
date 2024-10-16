using Content.Server.Tesla.Components;
using Robust.Shared.Timing;

namespace Content.Server.Tesla.EntitySystems;

/// <summary>
/// Manages the passive energy drain for the Tesla.
/// </summary>
public sealed partial class TeslaEnergyBallSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Update(float frameTime)
    {
        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<TeslaEnergyBallComponent>();

        while (query.MoveNext(out var uid, out var component))
        {
            if (curTime < component.NextUpdateTime)
                continue;

            component.NextUpdateTime = curTime + TimeSpan.FromSeconds(1);
            AdjustEnergy(uid, component, -component.PassiveEnergyDrainRate);
        }
    }
}
