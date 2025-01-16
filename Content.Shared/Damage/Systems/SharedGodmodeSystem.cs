using Content.Shared.Damage.Components;
using Content.Shared.Rejuvenate;
using Content.Shared.Slippery;
using Content.Shared.StatusEffect;
using Content.Shared.Body.Systems; // Shitmed Change

namespace Content.Shared.Damage.Systems;

public abstract class SharedGodmodeSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;

    [Dependency] private readonly SharedBodySystem _bodySystem = default!; // Shitmed Change

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GodmodeComponent, BeforeDamageChangedEvent>(OnBeforeDamageChanged);
        SubscribeLocalEvent<GodmodeComponent, BeforeStatusEffectAddedEvent>(OnBeforeStatusEffect);
        SubscribeLocalEvent<GodmodeComponent, BeforeStaminaDamageEvent>(OnBeforeStaminaDamage);
        SubscribeLocalEvent<GodmodeComponent, SlipAttemptEvent>(OnSlipAttempt);
    }

    private void OnSlipAttempt(EntityUid uid, GodmodeComponent component, SlipAttemptEvent args)
    {
        args.NoSlip = true;
    }

    private void OnBeforeDamageChanged(EntityUid uid, GodmodeComponent component, ref BeforeDamageChangedEvent args)
    {
        args.Cancelled = true;
    }

    private void OnBeforeStatusEffect(EntityUid uid, GodmodeComponent component, ref BeforeStatusEffectAddedEvent args)
    {
        args.Cancelled = true;
    }

    private void OnBeforeStaminaDamage(EntityUid uid, GodmodeComponent component, ref BeforeStaminaDamageEvent args)
    {
        args.Cancelled = true;
    }

    public virtual void EnableGodmode(EntityUid uid, GodmodeComponent? godmode = null)
    {
        godmode ??= EnsureComp<GodmodeComponent>(uid);

        if (TryComp<DamageableComponent>(uid, out var damageable))
        {
            godmode.OldDamage = new DamageSpecifier(damageable.Damage);
        }

        // Rejuv to cover other stuff
        RaiseLocalEvent(uid, new RejuvenateEvent());

        foreach (var (id, _) in _bodySystem.GetBodyChildren(uid)) // Shitmed Change
            EnableGodmode(id);
    }

    public virtual void DisableGodmode(EntityUid uid, GodmodeComponent? godmode = null)
    {
        if (!Resolve(uid, ref godmode, false))
            return;

        if (TryComp<DamageableComponent>(uid, out var damageable) && godmode.OldDamage != null)
        {
            _damageable.SetDamage(uid, damageable, godmode.OldDamage);
        }

        RemComp<GodmodeComponent>(uid);

        foreach (var (id, _) in _bodySystem.GetBodyChildren(uid)) // Shitmed Change
            DisableGodmode(id);
    }

    /// <summary>
    ///     Toggles godmode for a given entity.
    /// </summary>
    /// <param name="uid">The entity to toggle godmode for.</param>
    /// <returns>true if enabled, false if disabled.</returns>
    public bool ToggleGodmode(EntityUid uid)
    {
        if (TryComp<GodmodeComponent>(uid, out var godmode))
        {
            DisableGodmode(uid, godmode);
            return false;
        }

        EnableGodmode(uid, godmode);
        return true;
    }
}
