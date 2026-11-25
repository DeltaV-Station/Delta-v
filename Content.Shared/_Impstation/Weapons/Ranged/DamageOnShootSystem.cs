using Content.Shared.Administration.Logs;
using Content.Shared.Damage.Components;
using Content.Shared.Database;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Robust.Shared.Random;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Ranged.Systems;

namespace Content.Shared.Damage.Systems;

public sealed class DamageOnShootSystem : EntitySystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DamageOnShootComponent, GunShotEvent>(OnGunShot);
    }

    /// <summary>
    /// Damages the user that fires this weapon. If the user does not have
    /// proper protection, the user will be damaged when firing.
    /// </summary>
    /// <param name="entity">The entity being interacted with</param>
    /// <param name="args">Contains the user that interacted with the entity</param>
    private void OnGunShot(Entity<DamageOnShootComponent> entity, ref GunShotEvent args)
    {
        // Stop the interaction if the user attempts to interact with the object before the timer is finished

        if (!entity.Comp.IsDamageActive)
            return;

        var totalDamage = entity.Comp.Damage;

        if (!entity.Comp.IgnoreResistances)
        {
            // try to get damage on interact protection from either the inventory slots of the entity
            _inventorySystem.TryGetInventoryEntity<DamageOnShootProtectionComponent>(args.User, out var protectiveEntity);

            // or checking the entity for  the comp itself if the inventory didn't work
            if (protectiveEntity.Comp == null && TryComp<DamageOnShootProtectionComponent>(args.User, out var protectiveComp))
                protectiveEntity = (args.User, protectiveComp);


            // if protectiveComp isn't null after all that, it means the user has protection,
            // so let's calculate how much they resist
            if (protectiveEntity.Comp != null)
            {
                totalDamage = DamageSpecifier.ApplyModifierSet(totalDamage, protectiveEntity.Comp.DamageProtection);
            }
        }

        var didSelfDamage = _damageableSystem.TryChangeDamage(args.User, totalDamage);

        if (didSelfDamage)
        {
            // Record this interaction and determine when a user is allowed to interact with this entity again
            entity.Comp.LastInteraction = _gameTiming.CurTime;
            entity.Comp.NextInteraction = _gameTiming.CurTime + TimeSpan.FromSeconds(entity.Comp.InteractTimer);

            _adminLogger.Add(LogType.Damaged, $"{ToPrettyString(args.User):user} injured their hand by shooting a {ToPrettyString(entity):gun} and received {totalDamage.GetTotal():damage} damage");
            _audioSystem.PlayPredicted(entity.Comp.DamageSound, entity, args.User);

            if (entity.Comp.PopupText != null)
                _popupSystem.PopupClient(Loc.GetString(entity.Comp.PopupText), args.User, args.User);

            // Attempt to paralyze the user after they have taken damage
            if (_random.Prob(entity.Comp.StunChance))
                _stun.TryUpdateParalyzeDuration(args.User, entity.Comp.StunSeconds);
        }
    }
}
