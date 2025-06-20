using Content.Server.Atmos.EntitySystems;
using Content.Shared._DV.Projectiles;

namespace Content.Server._DV.Projectiles;

public sealed class PressureProjectileSystem : SharedPressureProjectileSystem
{
    [Dependency] private readonly AtmosphereSystem _atmos = default!;

    protected override float GetPressure(Entity<PressureProjectileComponent> ent)
    {
        return _atmos.GetContainingMixture(ent.Owner)?.Pressure ?? 0f;
    }
}
