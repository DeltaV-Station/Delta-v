using Content.Server._DV.Projectiles.Components;
using Content.Server._DV.Projectiles.Events;
using Content.Shared.Tag;
using Content.Shared.Whitelist;

namespace Content.Server._DV.Projectiles.Systems;

public sealed class PiercingProjectileSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PiercingProjectileComponent, ProjectilePierceEvent>(OnPierce);
    }

    private void OnPierce(Entity<PiercingProjectileComponent> bullet, ref ProjectilePierceEvent args)
    {
        // If the target doesn't have any tags to stop the bullet from piercing, it's automatically true.
        if (_whitelist.IsWhitelistFail(bullet.Comp.PierceCounterWhitelist, args.Target))
        {
            args.Pierced = true;
            return;
        }
        // If it does have the tag to stop it and enough health to count as "strongly armored", it'll block the bullet.
        // Mobs return a required Damage amount of Float.MaxValue. Therefore we need to check for absurdly high values.
        if (bullet.Comp.HealthThreshold < args.RequiredDamage && args.RequiredDamage < 20000000)
            return;

        if (bullet.Comp.Direction == null) // Get the direction of the bullet to determine which walls count.
            bullet.Comp.Direction = Transform(bullet).LocalRotation.GetCardinalDir();

        var xTarget = Transform(args.Target);

        var targetPosition = bullet.Comp.Direction is Direction.East or Direction.West
            ? xTarget.Coordinates.X // If the bullet is going horizontal, wall rows are vertical.
            : xTarget.Coordinates.Y; // And when vertical, the wall rows are horizontal

        // If the wall is part of a wall-row that the bullet already pierced, pierce it too and ignore it for the counter.
        if (bullet.Comp.IgnoreRowCoordinate != null
            && Math.Abs(targetPosition - bullet.Comp.IgnoreRowCoordinate.Value) < 0.25)
        {
            args.Pierced = true;
            return;
        }

        bullet.Comp.PierceCounter++;

        if (bullet.Comp.PierceCounter > bullet.Comp.MaxPierceNumberThreshold)
            return;

        // Save the row-position of the last wall it pierced, so we can ignore any other wall part of the same row.
        bullet.Comp.IgnoreRowCoordinate = targetPosition;
        args.Pierced = true;
    }
}
