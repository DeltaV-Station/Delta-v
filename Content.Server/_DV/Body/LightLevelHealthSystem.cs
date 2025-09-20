using Content.Shared._DV.Body;
using Content.Shared._DV.Light;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Robust.Shared.Timing;

namespace Content.Server._DV.Body;

public sealed class LightLevelHealthSystem : SharedLightLevelHealthSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;
    [Dependency] private readonly SharedLightReactiveSystem _lightReactive = default!;

    private TimeSpan _nextUpdate = TimeSpan.MinValue;
    public override void Update(float frameTime)
    {
        if (_timing.CurTime < _nextUpdate)
            return;
        _nextUpdate = _timing.CurTime + TimeSpan.FromSeconds(1);

        var query = EntityQueryEnumerator<LightLevelHealthComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_mobState.IsDead(uid) && !comp.HealWhenDead)
                continue; // Don't apply damage if the mob is dead, also don't heal if dead unless specified.
            // Get the light level at the entity's position
            var lightLevel = _lightReactive.GetLightLevel(uid, true);

            int currentThreshold = CurrentThreshold(lightLevel, comp);
            if (currentThreshold != comp.CurrentThreshold)
            {
                comp.CurrentThreshold = currentThreshold;
                _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);
            }

            if (currentThreshold == 0)
                continue; // No damage or healing to apply

            var damage = currentThreshold == -1 ? comp.DarkDamage : comp.LightDamage;
            if (damage.AnyPositive() && _mobState.IsDead(uid))
                continue; // Don't apply damage if the mob is dead

            TryDealDamage(new(uid, comp), damage);
        }
    }
}
