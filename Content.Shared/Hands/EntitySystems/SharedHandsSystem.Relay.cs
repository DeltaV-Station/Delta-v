using Content.Shared.Camera;
using Content.Shared.Hands.Components;
using Content.Shared.Movement.Systems;
<<<<<<< HEAD
=======
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Wieldable;
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30

namespace Content.Shared.Hands.EntitySystems;

public abstract partial class SharedHandsSystem
{
    private void InitializeRelay()
    {
        SubscribeLocalEvent<HandsComponent, GetEyeOffsetRelayedEvent>(RelayEvent);
        SubscribeLocalEvent<HandsComponent, GetEyePvsScaleRelayedEvent>(RelayEvent);
        SubscribeLocalEvent<HandsComponent, RefreshMovementSpeedModifiersEvent>(RelayEvent);
<<<<<<< HEAD
=======

        // By-ref events.
        SubscribeLocalEvent<HandsComponent, ExtinguishEvent>(RefRelayEvent);
        SubscribeLocalEvent<HandsComponent, ProjectileReflectAttemptEvent>(RefRelayEvent);
        SubscribeLocalEvent<HandsComponent, HitScanReflectAttemptEvent>(RefRelayEvent);
        SubscribeLocalEvent<HandsComponent, WieldAttemptEvent>(RefRelayEvent);
        SubscribeLocalEvent<HandsComponent, UnwieldAttemptEvent>(RefRelayEvent);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
    }

    private void RelayEvent<T>(Entity<HandsComponent> entity, ref T args) where T : EntityEventArgs
    {
        var ev = new HeldRelayedEvent<T>(args);
        foreach (var held in EnumerateHeld(entity, entity.Comp))
        {
            RaiseLocalEvent(held, ref ev);
        }
    }
}
