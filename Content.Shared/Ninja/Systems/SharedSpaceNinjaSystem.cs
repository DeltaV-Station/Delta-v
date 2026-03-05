using Content.Shared.Ninja.Components;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Popups;
using System.Diagnostics.CodeAnalysis;
using Content.Shared.Damage.Systems; // DeltaV
using Content.Shared.Stealth.Components; // DeltaV
using Robust.Shared.Prototypes; // DeltaV

namespace Content.Shared.Ninja.Systems;

/// <summary>
/// Provides shared ninja API, handles being attacked revealing ninja and stops guns from shooting.
/// </summary>
public abstract class SharedSpaceNinjaSystem : EntitySystem
{
    [Dependency] protected readonly SharedNinjaSuitSystem Suit = default!;
    [Dependency] protected readonly SharedPopupSystem Popup = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!; // DeltaV

    public EntityQuery<SpaceNinjaComponent> NinjaQuery;

    public override void Initialize()
    {
        base.Initialize();

        NinjaQuery = GetEntityQuery<SpaceNinjaComponent>();

        // SubscribeLocalEvent<SpaceNinjaComponent, AttackedEvent>(OnNinjaAttacked); // DeltaV - Handled by the DamageChangedEvent
        SubscribeLocalEvent<SpaceNinjaComponent, MeleeAttackEvent>(OnNinjaAttack);
        SubscribeLocalEvent<SpaceNinjaComponent, ShotAttemptedEvent>(OnShotAttempted);

        SubscribeLocalEvent<SpaceNinjaComponent, DamageChangedEvent>(OnNinjaAttacked); // DeltaV - Reveal the ninja on damage
    }

    public bool IsNinja([NotNullWhen(true)] EntityUid? uid)
    {
        return NinjaQuery.HasComp(uid);
    }

    /// <summary>
    /// Set the ninja's worn suit entity
    /// </summary>
    public void AssignSuit(Entity<SpaceNinjaComponent> ent, EntityUid? suit)
    {
        if (ent.Comp.Suit == suit)
            return;

        ent.Comp.Suit = suit;
        Dirty(ent, ent.Comp);
    }

    /// <summary>
    /// Set the ninja's worn gloves entity
    /// </summary>
    public void AssignGloves(Entity<SpaceNinjaComponent> ent, EntityUid? gloves)
    {
        if (ent.Comp.Gloves == gloves)
            return;

        ent.Comp.Gloves = gloves;
        Dirty(ent, ent.Comp);
    }

    /// <summary>
    /// Bind a katana entity to a ninja, letting it be recalled and dash.
    /// Does nothing if the player is not a ninja or already has a katana bound.
    /// </summary>
    public void BindKatana(Entity<SpaceNinjaComponent?> ent, EntityUid katana)
    {
        if (!NinjaQuery.Resolve(ent, ref ent.Comp, false) || ent.Comp.Katana != null)
            return;

        ent.Comp.Katana = katana;
        Dirty(ent, ent.Comp);
    }

    /// <summary>
    /// Gets the user's battery and tries to use some charge from it, returning true if successful.
    /// Serverside only.
    /// </summary>
    public virtual bool TryUseCharge(EntityUid user, float charge)
    {
        return false;
    }

    // DeltaV - Handled by the DamageChangedEvent
    /// <summary>
    /// Handle revealing ninja if cloaked when attacked.
    /// </summary>
    //private void OnNinjaAttacked(Entity<SpaceNinjaComponent> ent, ref AttackedEvent args)
    //{
    //    TryRevealNinja(ent, disable: true);
    //}
    // END DeltaV

    /// <summary>
    /// DeltaV - Handle revealing ninja if cloaked when attacked by a hitscan attack.
    /// </summary>
    private void OnNinjaAttacked(Entity<SpaceNinjaComponent> ent, ref DamageChangedEvent args)
    {
        // If there's no damage delta, just return
        if (args.DamageDelta is not { } damage)
            return;

        // Don't reveal on (most) healing
        if (!args.DamageIncreased)
            return;

        // If the damage doesn't have a source, we need to check the type, in case it 
        // was a grenade or explosion. We want to ignore airloss and toxin damage types.
        if (!args.Origin.HasValue)
        {
            // If there are any negative values, its probably natual or chem healing, so don't reveal. It might be an OD from medicine.
            if (!damage.AnyPositive())
                return;

            // Check the damage types for damage types that should reveal (brute, burns)
            // Basically, we want to ignore most indirect forms of damage (airloss, toxins)
            var damageGroups = damage.GetDamagePerGroup(_prototypeManager);
            if (!damageGroups.ContainsKey("Brute") && !damageGroups.ContainsKey("Burn")) // This feels a bit dirty, oh well.
                return;
        }

        // Only reveal on damage at least the minumum. This prevents tiny ticks of damage (e.g. from malign rifts pulses)
        if (damage.GetTotal() < ent.Comp.MinimumRevealDamage)
            return;

        // Yea, now reveal that son of a bitch >:3
        TryRevealNinja(ent, disable: true);
    }

    /// <summary>
    /// Handle revealing ninja if cloaked when attacking.
    /// Only reveals, there is no cooldown.
    /// </summary>
    private void OnNinjaAttack(Entity<SpaceNinjaComponent> ent, ref MeleeAttackEvent args)
    {
        TryRevealNinja(ent, disable: false);
    }

    private void TryRevealNinja(Entity<SpaceNinjaComponent> ent, bool disable)
    {
        // DeltaV - Reveal ninja on damage
        if (ent.Comp.Suit is not {} uid)
            return;

        if (!TryComp<NinjaSuitComponent>(ent.Comp.Suit, out var suit))
            return;

        if (!HasComp<StealthComponent>(ent)) // Only attempt to reveal if stealthed
            return;
        // END DeltaV
        Suit.RevealNinja((uid, suit), ent, disable: disable);
    }

    /// <summary>
    /// Require ninja to fight with HONOR, no guns!
    /// </summary>
    private void OnShotAttempted(Entity<SpaceNinjaComponent> ent, ref ShotAttemptedEvent args)
    {
        Popup.PopupClient(Loc.GetString("gun-disabled"), ent, ent);
        args.Cancel();
    }
}
