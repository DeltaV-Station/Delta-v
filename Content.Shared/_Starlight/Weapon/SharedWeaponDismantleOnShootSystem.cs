using System.Numerics;
using Content.Shared._Starlight.Weapon.Components;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Random;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.IoC;
using Robust.Shared.GameObjects;

namespace Content.Shared._Starlight.Weapon;

public abstract partial class SharedWeaponDismantleOnShootSystem : EntitySystem
{
    [Dependency] protected readonly ThrowingSystem Throwing = default!;
    [Dependency] protected readonly DamageableSystem Damageable = default!;
    [Dependency] protected readonly SharedAudioSystem Audio = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<WeaponDismantleOnShootComponent, AmmoShotEvent>(OnGunShot);
    }

    public bool DismantleCheck(Entity<WeaponDismantleOnShootComponent> ent, ref AmmoShotEvent args)
    {
        //roll to see if we explode or not
        var random = IoCManager.Resolve<IRobustRandom>();
        //1.0f means always true, 0.0f means always false
        if (!random.Prob(ent.Comp.DismantleChance))
            return false;

        return true;
    }

    private void OnGunShot(Entity<WeaponDismantleOnShootComponent> ent, ref AmmoShotEvent args)
    {
        if (DismantleCheck(ent, ref args) == false)
            return;

        // we need the user (shooter) to proceed
        if (!args.Shooter.HasValue)
            return;

        var shooter = args.Shooter.Value;

        // apply the damage to the shooter (expects Entity<DamageableComponent?>)
        Damageable.TryChangeDamage((shooter, null), ent.Comp.SelfDamage, origin: shooter);

        Audio.PlayPredicted(ent.Comp.DismantleSound, shooter, shooter);

        // get the user's transform
        var userPosition = Transform(shooter).Coordinates;

        if (!TryComp<GunComponent>(ent, out var gunComponent))
            return;

        var toCoordinates = gunComponent.ShootCoordinates;

        if (toCoordinates == null)
            return;

        //loop through all of the items
        var random = IoCManager.Resolve<IRobustRandom>();
        foreach (var item in ent.Comp.items)
        {
            for (var i = 0; i < item.Amount; i++)
            {
                //roll to see if we destroy the item or not
                if (!random.Prob(item.SpawnProbability))
                    continue;

                //get the item entity
                var itemEntity = Spawn(item.PrototypeId, userPosition);

                var direction = toCoordinates.Value.Position;
                //normalize it
                direction = Vector2.Normalize(direction);
                //multiply it by the distance
                direction *= ent.Comp.DismantleDistance;
                //rotate it by the angle
                direction = item.LaunchAngle.RotateVec(direction);

                //roll for random angle modifier
                double randomAngle = random.NextDouble(-item.AngleRandomness.Degrees, item.AngleRandomness.Degrees);
                //rotate it by the random angle
                direction = Angle.FromDegrees(randomAngle).RotateVec(direction);

                var throwDirection = new EntityCoordinates(shooter, direction);

                Throwing.TryThrow(itemEntity, throwDirection, ent.Comp.DismantleDistance, compensateFriction: true);
            }
        }

        //now we need to destroy the gun
        //get the gun entity
        PredictedQueueDel(ent.Owner);
    }
}
