using Content.Server.Damage.Components;
using Content.Shared.Damage;
using Content.Shared.Throwing;
using Content.Shared._DV.Chemistry.Systems; // DeltaV - Beergoggles enable safe throw

namespace Content.Server.Damage.Systems
{
    /// <summary>
    /// Damages the thrown item when it lands.
    /// </summary>
    public sealed class DamageOnLandSystem : EntitySystem
    {
        [Dependency] private readonly DamageableSystem _damageableSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<DamageOnLandComponent, LandEvent>(DamageOnLand);
        }

        private void DamageOnLand(EntityUid uid, DamageOnLandComponent component, ref LandEvent args)
        {
            // DeltaV - start of Beergoggles enable safe throw
            if (args.User.HasValue)
            {
                var safeThrowEvent = new SafeSolutionThrowEvent();
                RaiseLocalEvent(args.User.Value, safeThrowEvent);
                if (safeThrowEvent.SafeThrow)
                {
                    return;
                }
            }
            // DeltaV - end of Beergoggles enable safe throw
            _damageableSystem.TryChangeDamage(uid, component.Damage, component.IgnoreResistances);
        }
    }
}
