using Content.Shared._DV.Body;
using Content.Shared._DV.Light;
using Content.Shared.Damage;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Shared._DV.Body;

public abstract class SharedLightLevelHealthSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LightLevelHealthComponent, RefreshMovementSpeedModifiersEvent>(OnGetMoveSpeedModifiers);
    }



    public int CurrentThreshold(float lightLevel, LightLevelHealthComponent comp)
    {
        bool lowLight = lightLevel < comp.DarkThreshold;
        bool highLight = lightLevel > comp.LightThreshold;
        return lowLight && !highLight ? -1
             : !lowLight && highLight ? 1
             : 0;
    }

    public void TryDealDamage(Entity<LightLevelHealthComponent> target, DamageSpecifier damage)
    {
        if (damage.AnyPositive())
        {
            _audio.PlayPvs(target.Comp.SizzleSoundPath, target);
        }
        _damageable.TryChangeDamage(target, damage, true, false);
    }

    private void OnGetMoveSpeedModifiers(Entity<LightLevelHealthComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (!TryComp<LightReactiveComponent>(ent, out var lightReactive))
            return;

        if (lightReactive.CurrentLightLevel < ent.Comp.DarkThreshold)
            args.ModifySpeed(ent.Comp.DarkMovementSpeedMultiplier);
        else if (lightReactive.CurrentLightLevel > ent.Comp.LightThreshold)
            args.ModifySpeed(ent.Comp.LightMovementSpeedMultiplier);
    }
}
