using Content.Server.Damage.Components;
using Content.Shared.Damage;
using Content.Shared.Throwing;
using Content.Shared._DV.Chemistry.Systems; // DeltaV - Beergoggles enable safe throw
using Content.Shared.Nutrition.Components; // DeltaV - Beergoggles enable safe throw

namespace Content.Server.Damage.Systems
{
    /// <summary>
    /// Damages the thrown item when it lands.
    /// </summary>
    public sealed class DamageOnLandSystem : EntitySystem
    {
        [Dependency] private readonly Shared.Damage.Systems.DamageableSystem _damageableSystem = default!;
        [Dependency] private readonly SafeSolutionThrowerSystem _safesolthrower = default!; // DeltaV - Beergoggles enable safe throw

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<DamageOnLandComponent, LandEvent>(DamageOnLand);
        }

        private void DamageOnLand(EntityUid uid, DamageOnLandComponent component, ref LandEvent args)
        {
            // DeltaV - start of Beergoggles enable safe throw
            if (args.User is { } user && TryComp<EdibleComponent>(uid, out var edible) && edible.Edible == "Drink") // TODO: Probably create an event for this.
            {
                if (_safesolthrower.GetSafeThrow(user))
                    return;
            }
            // DeltaV - end of Beergoggles enable safe throw
            _damageableSystem.TryChangeDamage(uid, component.Damage, component.IgnoreResistances);
        }
    }
}
