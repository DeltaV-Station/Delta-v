using Content.Server.Tesla.Components;
using Robust.Shared.Timing;

namespace Content.Server.Tesla.EntitySystems;

/// <summary>
/// Manages the passive energy drain for the Tesla.
/// </summary>
public sealed class TeslaPassiveDrainSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly TeslaEnergyBallSystem _teslaSys = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<TeslaEnergyBallComponent>();

        while (query.MoveNext(out var uid, out var component))
        {
            if (curTime < component.NextUpdateTime)
                continue;

            component.NextUpdateTime = curTime + TimeSpan.FromSeconds(1);
            _teslaSys.AdjustEnergy(uid, component, -component.PassiveEnergyDrainRate);
        }
    }
}
