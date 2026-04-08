using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events;
using Content.Shared._DV.Psionics.Events.PowerActionEvents;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Popups;
using Content.Shared.Revenant.Components;
using Content.Shared.StatusEffect;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._DV.Psionics.Systems.PsionicPowers;

/// <summary>
/// This enables psionic users to dispel noospheric beings and actions.
/// </summary>
public abstract class SharedDispelPowerSystem : BasePsionicPowerSystem<DispelPowerComponent, DispelPowerActionEvent>
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly StatusEffectNew.StatusEffectsSystem _statusEffectsNew = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DispellableComponent, DispelledEvent>(OnDispelled);
        SubscribeLocalEvent<DamageOnDispelComponent, DispelledEvent>(OnDmgDispelled);
        SubscribeLocalEvent<DispelPowerComponent, PsionicStoppedShieldedEvent>(OnPsionicShieldingStopped, null,[typeof(SharedPsionicSystem)]);
        // Upstream stuff we're just gonna handle here
        SubscribeLocalEvent<RevenantComponent, DispelledEvent>(OnRevenantDispelled);
    }

    protected override void OnPowerInit(Entity<DispelPowerComponent> power, ref MapInitEvent args)
    {
        base.OnPowerInit(power, ref args);
        // Dispell psionics can now see invisible entities to dispell them.
        Psionic.SetCanSeePsionicInvisiblity(power.Owner, true);
    }

    protected override void OnPowerUsed(Entity<DispelPowerComponent> psionic, ref DispelPowerActionEvent args)
    {
        if (!Psionic.CanBeTargeted(args.Target, hasAggressor: args.Performer))
            return;

        var ev = new DispelledEvent(args.Performer, args.Target);
        RaiseLocalEvent(args.Target, ev);

        if (ev.Handled)
            AfterPowerUsed(psionic, args.Performer);
    }

    protected override void OnPsionicallySuppressed(Entity<DispelPowerComponent> power, ref PsionicSuppressedEvent args)
    {
        if (Timing.ApplyingState)
            return;
        // Don't let them see if they're suppressed.
        Psionic.SetCanSeePsionicInvisiblity(args.Victim, false);
    }

    protected override void OnStoppedPsionicallySuppressed(Entity<DispelPowerComponent> psionic, ref PsionicStoppedSuppressedEvent args)
    {
        if (Timing.ApplyingState)
            return;
        // Let mah people SEE
        Psionic.SetCanSeePsionicInvisiblity(args.Victim, true);
    }

    /// <summary>
    /// This is necessary to avoid dispel users to lose their invisibility sight.
    /// This fires after the System where losing psionic shielding makes you unable to see invisible entities.
    /// </summary>
    /// <param name="psionic">The psionic who lost their shielding.</param>
    /// <param name="args">The event.</param>
    private void OnPsionicShieldingStopped(Entity<DispelPowerComponent> psionic, ref PsionicStoppedShieldedEvent args)
    {
        if (_statusEffectsNew.HasStatusEffect(psionic, "StatusEffectPsionicsDisabled"))
            return;
        // Losing the shielding causes you to lose vision of the invisible. This is to prevent that.
        Psionic.SetCanSeePsionicInvisiblity(args.Shielded, true);
    }

    protected override void OnMindBroken(Entity<DispelPowerComponent> psionic, ref PsionicMindBrokenEvent args)
    {
        base.OnMindBroken(psionic, ref args);
        // If the mindbreak was successful, make 'em blind.
        if (!psionic.Comp.Deleted)
            return;

        Psionic.SetCanSeePsionicInvisiblity(psionic, false);
    }

    private void OnDispelled(Entity<DispellableComponent> dispellable, ref DispelledEvent args)
    {
        QueueDel(dispellable);
        Spawn("Ash", Transform(dispellable).Coordinates);
        Popup.PopupPredicted(Loc.GetString("psionic-burns-up", ("item", dispellable)), dispellable, args.Dispeller, PopupType.MediumCaution);
        _audio.PlayPredicted(dispellable.Comp.DispelSound, dispellable.Owner, args.Dispeller);
        args.Handled = true;
    }

    private void OnDmgDispelled(Entity<DamageOnDispelComponent> damaged, ref DispelledEvent args)
    {
        var damage = damaged.Comp.Damage;
        var modifier = Random.NextFloat(damaged.Comp.Variance, 1 + damaged.Comp.Variance);

        damage *= modifier;
        DealDispelDamage(damaged, damage, args.Dispeller, damaged.Comp.DispelSound);
        args.Handled = true;
    }

    private void OnRevenantDispelled(Entity<RevenantComponent> revenant, ref DispelledEvent args)
    {
        DealDispelDamage(revenant, dispeller: args.Dispeller);
        // TODO: Port over the new StatusEffectSystem when upstream ports over the Corporeal status effect to the new system.
        _statusEffects.TryAddStatusEffect(revenant, "Corporeal", TimeSpan.FromSeconds(30), false, "Corporeal");
        args.Handled = true;
    }

    public void DealDispelDamage(EntityUid dispelled, DamageSpecifier? damage = null, EntityUid? dispeller = null, SoundSpecifier? sound = null)
    {
        if (Deleted(dispelled))
            return;

        Popup.PopupPredicted(Loc.GetString("psionic-burn-resist", ("item", dispelled)), dispelled, dispeller, PopupType.SmallCaution);
        _audio.PlayPredicted(sound, dispelled, dispeller);

        if (damage == null)
        {
            damage = new DamageSpecifier();
            damage.DamageDict.Add("Blunt", 100);
        }

        _damageable.TryChangeDamage(dispelled, damage, ignoreResistances: true);
    }
}
