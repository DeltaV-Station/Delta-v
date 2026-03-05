using System.Linq;
using Content.Shared._DV.Abilities.Psionics;
using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Events;
using Content.Shared.Damage;
using Content.Shared.Damage.Events;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Popups;
using Content.Shared.StatusEffectNew;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Shared._DV.Psionics.Systems;

public abstract partial class SharedPsionicSystem
{
    [Dependency] protected readonly SharedAudioSystem Audio = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

    private void InitializeItems()
    {
        SubscribeLocalEvent<PsionicallyInsulativeComponent, GotEquippedEvent>(OnInsulativeGearEquipped);
        SubscribeLocalEvent<PsionicallyInsulativeComponent, GotUnequippedEvent>(OnInsulativeGearUnequipped);

        SubscribeLocalEvent<PsionicallyInsulativeComponent, InventoryRelayedEvent<PsionicPowerUseAttemptEvent>>(OnPowerUseAttempt);
        SubscribeLocalEvent<PsionicallyInsulativeComponent, InventoryRelayedEvent<CheckPsionicInsulativeGearEvent>>(OnPsionicGearChecked);
        SubscribeLocalEvent<PsionicallyInsulativeComponent, InventoryRelayedEvent<TargetedByPsionicPowerEvent>>(OnTargetedByPsionicPower);

        SubscribeLocalEvent<AntiPsionicWeaponComponent, MeleeHitEvent>(OnAntiPsionicMeleeHit);
        SubscribeLocalEvent<AntiPsionicWeaponComponent, StaminaMeleeHitEvent>(OnAntiPsionicStamHit);
    }

    private void OnInsulativeGearEquipped(Entity<PsionicallyInsulativeComponent> gear, ref GotEquippedEvent args)
    {
        RefreshPsionicAbilities(args.Equipee);
    }

    private void OnInsulativeGearUnequipped(Entity<PsionicallyInsulativeComponent> gear, ref GotUnequippedEvent args)
    {
        RefreshPsionicAbilities(args.Equipee);
    }

    private void RefreshPsionicAbilities(EntityUid user)
    {
        if (!TryComp<PsionicComponent>(user, out var psionic))
            return;

        var ev = new CheckPsionicInsulativeGearEvent();
        RaiseLocalEvent(user, ref ev);
    }

    #region EventHandling
    private void OnPsionicGearChecked(Entity<PsionicallyInsulativeComponent> gear, ref InventoryRelayedEvent<CheckPsionicInsulativeGearEvent> args)
    {
        args.Args.GearPresent = true;
        // If one gear blocks psionic usage, psionics cannot be used.
        args.Args.AllowsPsionicUsage &= gear.Comp.AllowsPsionicUsage;
        // If one gear shields from psionics, they're shielded.
        args.Args.ShieldsFromPsionics |= gear.Comp.ShieldsFromPsionics;

    }

    private void OnPowerUseAttempt(Entity<PsionicallyInsulativeComponent> gear, ref InventoryRelayedEvent<PsionicPowerUseAttemptEvent> args)
    {
        // If one gear blocks psionic usage, psionics cannot be used.
        args.Args.CanUsePower &= gear.Comp.AllowsPsionicUsage;
    }

    private void OnTargetedByPsionicPower(Entity<PsionicallyInsulativeComponent> gear, ref InventoryRelayedEvent<TargetedByPsionicPowerEvent> args)
    {
        // If one gear shields from psionics, they're shielded.
        args.Args.IsShielded |= gear.Comp.ShieldsFromPsionics;
    }
    #endregion

    #region AntiPsionicWeaponry
    private void OnAntiPsionicMeleeHit(Entity<AntiPsionicWeaponComponent> weapon, ref MeleeHitEvent args)
    {
        foreach (var target in args.HitEntities)
        {
            if (HasComp<PsionicComponent>(target))
            {
                Audio.PlayPredicted(weapon.Comp.HitSound, target, args.User);
                args.ModifiersList.Add(weapon.Comp.Modifiers);

                if (Random.Prob(weapon.Comp.DisableChance))
                    _statusEffects.TryUpdateStatusEffectDuration(target, PsionicsDisabledProtoId, TimeSpan.FromSeconds(10));
            }

            // if (TryComp<MindSwappedComponent>(target, out var swapped))
            // {
            //     _mindSwapPowerSystem.Swap(target, swapped.OriginalEntity, true);
            //     return;
            // }

            if (!weapon.Comp.Punish
                || !HasComp<PotentialPsionicComponent>(target)
                || HasComp<PsionicComponent>(target)
                || !Random.Prob(weapon.Comp.PunishChance))
                continue;

            _stuttering.DoStutter(args.User, TimeSpan.FromMinutes(5), false);
            _stun.TryKnockdown(args.User, TimeSpan.FromSeconds(5), false, drop: false);
            _jittering.DoJitter(args.User, TimeSpan.FromSeconds(5), false);
        }
    }

    private void OnAntiPsionicStamHit(Entity<AntiPsionicWeaponComponent> weapon, ref StaminaMeleeHitEvent args)
    {
        if (args.HitList.Any(targetStamina => HasComp<PsionicComponent>(targetStamina.Entity)))
        {
            args.Multiplier *= weapon.Comp.StaminaDamageMultiplier;
        }
    }
    #endregion
}
