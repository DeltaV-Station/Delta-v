using Content.Server.Stunnable.Components;
using Content.Shared.Movement.Systems;
using JetBrains.Annotations;
using Content.Shared.Throwing;
using Robust.Shared.Physics.Events;
// Begin DeltaV
using Content.Shared.Whitelist;
using Content.Shared.Damage.Events;
using Content.Shared.Damage.Systems;
// End DeltaV

namespace Content.Server.Stunnable.Systems;

[UsedImplicitly]
internal sealed class StunOnCollideSystem : EntitySystem
{
    [Dependency] private readonly StunSystem _stunSystem = default!;
    [Dependency] private readonly MovementModStatusSystem _movementMod = default!;

    // Begin DeltaV
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedStaminaSystem _sharedStaminaSystem = default!;
    // End DeltaV

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StunOnCollideComponent, StartCollideEvent>(HandleCollide);
        SubscribeLocalEvent<StunOnCollideComponent, ThrowDoHitEvent>(HandleThrow);
    }

    private void TryDoCollideStun(Entity<StunOnCollideComponent> ent, EntityUid target)
    {
        // Begin DeltaV Additions
        if (!_whitelist.IsWhitelistFailOrNull(ent.Comp.Blacklist, target))
            return;

        // If the target is heavily protected, just deal stamina damage, no knockdown etc.
        if (ent.Comp.UseProtectionThreshold)
        {
            var beforeStaminaDamageEvent = new BeforeStaminaDamageEvent(100f, FromMelee: false);
            RaiseLocalEvent(target, ref beforeStaminaDamageEvent);

            if (beforeStaminaDamageEvent.Value < ent.Comp.ProtectionThreshold)
            {
                _sharedStaminaSystem.TakeStaminaDamage(target, ent.Comp.StaminaDamage);
                return;
            }
        }
        // End DeltaV Additions

        _stunSystem.TryKnockdown(target, ent.Comp.KnockdownAmount, ent.Comp.Refresh, ent.Comp.AutoStand, ent.Comp.Drop, true);

        if (ent.Comp.Refresh)
        {
            _stunSystem.TryUpdateStunDuration(target, ent.Comp.StunAmount);

            _movementMod.TryUpdateMovementSpeedModDuration(
                target,
                MovementModStatusSystem.TaserSlowdown,
                ent.Comp.SlowdownAmount,
                ent.Comp.WalkSpeedModifier,
                ent.Comp.SprintSpeedModifier
            );
        }
        else
        {
            _stunSystem.TryAddStunDuration(target, ent.Comp.StunAmount);
            _movementMod.TryAddMovementSpeedModDuration(
                target,
                MovementModStatusSystem.TaserSlowdown,
                ent.Comp.SlowdownAmount,
                ent.Comp.WalkSpeedModifier,
                ent.Comp.SprintSpeedModifier
            );
        }
    }

    private void HandleCollide(Entity<StunOnCollideComponent> ent, ref StartCollideEvent args)
    {
        if (args.OurFixtureId != ent.Comp.FixtureID)
            return;

        TryDoCollideStun(ent, args.OtherEntity);
    }

    private void HandleThrow(Entity<StunOnCollideComponent> ent, ref ThrowDoHitEvent args)
    {
        TryDoCollideStun(ent, args.Target);
    }
}
