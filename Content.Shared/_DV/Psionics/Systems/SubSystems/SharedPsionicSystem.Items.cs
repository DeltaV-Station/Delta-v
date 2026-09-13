using System.Linq;
using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Events;
using Content.Shared.Damage.Events;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs.Components;
using Content.Shared.StatusEffectNew;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._DV.Psionics.Systems;

public abstract partial class SharedPsionicSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] protected readonly SharedAudioSystem Audio = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

    private void InitializeItems()
    {
        SubscribeLocalEvent<PsionicallyInsulativeComponent, GotEquippedEvent>(OnInsulativeGearEquipped);
        SubscribeLocalEvent<PsionicallyInsulativeComponent, GotUnequippedEvent>(OnInsulativeGearUnequipped);


        SubscribeLocalEvent<PsionicallyInsulativeComponent, InventoryRelayedEvent<PsionicPowerUseAttemptEvent>>(OnPowerUseAttempt);
        SubscribeLocalEvent<PsionicallyInsulativeComponent, InventoryRelayedEvent<TargetedByPsionicPowerEvent>>(OnTargetedByPsionicPower);

        SubscribeLocalEvent<AntiPsionicWeaponComponent, MeleeHitEvent>(OnAntiPsionicMeleeHit);
        SubscribeLocalEvent<AntiPsionicWeaponComponent, StaminaMeleeHitEvent>(OnAntiPsionicStamHit);
    }

    private void OnInsulativeGearEquipped(Entity<PsionicallyInsulativeComponent> gear, ref GotEquippedEvent args)
    {
        if (_timing.ApplyingState)
            return;

        if (!gear.Comp.AllowsPsionicUsage)
        {
            var ev = new PsionicSuppressedEvent(args.Equipee);
            RaiseLocalEvent(args.Equipee, ref ev);
        }
        if (gear.Comp.ShieldsFromPsionics)
        {
            var ev = new PsionicShieldedEvent(args.Equipee);
            RaiseLocalEvent(args.Equipee, ref ev);
        }
    }

    private void OnInsulativeGearUnequipped(Entity<PsionicallyInsulativeComponent> gear, ref GotUnequippedEvent args)
    {
        if (_timing.ApplyingState)
            return;

        if (!gear.Comp.AllowsPsionicUsage && CanUsePsionicAbility(args.Equipee))
        {
            var ev = new PsionicStoppedSuppressedEvent(args.Equipee);
            RaiseLocalEvent(args.Equipee, ref ev);
        }
        if (gear.Comp.ShieldsFromPsionics && CanBeTargeted(args.Equipee, showPopup: false))
        {
            var ev = new PsionicStoppedShieldedEvent(args.Equipee);
            RaiseLocalEvent(args.Equipee, ref ev);
        }
    }

    #region EventHandling
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
            var ev = new AntiPsionicWeaponHitEvent();
            RaiseLocalEvent(target, ref ev);

            if (HasComp<PsionicComponent>(target))
            {
                Audio.PlayPredicted(weapon.Comp.HitSound, target, args.User);
                args.ModifiersList.Add(weapon.Comp.Modifiers);

                if (Random.Prob(weapon.Comp.DisableChance))
                    _statusEffects.TryUpdateStatusEffectDuration(target, PsionicsDisabledProtoId, TimeSpan.FromSeconds(10));
            }
            else if (HasComp<MobStateComponent>(target) && weapon.Comp.Punish && Random.Prob(weapon.Comp.PunishChance))
            {
                _stuttering.DoStutter(args.User, TimeSpan.FromMinutes(5), false);
                _stun.TryKnockdown(args.User, TimeSpan.FromSeconds(5), false, drop: false);
                _jittering.DoJitter(args.User, TimeSpan.FromSeconds(5), false);
            }
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
