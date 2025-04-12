using Content.Server.Popups;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared._DV.CosmicCult.Components.Examine;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Systems;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Containers;

namespace Content.Server._DV.CosmicCult;

public sealed class CosmicGlyphSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CosmicGlyphComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<CosmicGlyphComponent, ActivateInWorldEvent>(OnUseGlyph);
    }

    #region Base trigger

    private void OnExamine(Entity<CosmicGlyphComponent> uid, ref ExaminedEvent args)
    {
        if (HasComp<CosmicCultComponent>(args.Examiner))
            args.PushMarkup(Loc.GetString("cosmic-examine-glyph-cultcount", ("COUNT", uid.Comp.RequiredCultists)));
        else
            args.PushMarkup(Loc.GetString("cosmic-examine-text-glyphs"));
    }

    private void OnUseGlyph(Entity<CosmicGlyphComponent> uid, ref ActivateInWorldEvent args)
    {
        Log.Debug("Glyph event triggered!");
        var tgtpos = Transform(uid).Coordinates;
        var userCoords = Transform(args.User).Coordinates;
        if (args.Handled || !userCoords.TryDistance(EntityManager, tgtpos, out var distance) || distance > uid.Comp.ActivationRange || !HasComp<CosmicCultComponent>(args.User))
            return;
        var cultists = GatherCultists(uid, uid.Comp.ActivationRange);
        if (cultists.Count < uid.Comp.RequiredCultists)
        {
            _popup.PopupEntity(Loc.GetString("cult-glyph-not-enough-cultists"), uid, args.User);
            return;
        }

        args.Handled = true;
        var damageSpecifier = new DamageSpecifier();
        var tryInvokeEv = new TryActivateGlyphEvent(args.User, cultists);
        RaiseLocalEvent(uid, tryInvokeEv);
        if (tryInvokeEv.Cancelled)
            return;

        damageSpecifier.DamageDict.Add("Asphyxiation", uid.Comp.ActivationDamage / cultists.Count);
        foreach (var cultist in cultists)
        {
            DealDamage(cultist, damageSpecifier);
        }

        _audio.PlayPvs(uid.Comp.GylphSFX, tgtpos, AudioParams.Default.WithVolume(+1f));
        Spawn(uid.Comp.GylphVFX, tgtpos);
        QueueDel(uid);
    }

    #endregion

    #region Housekeeping

    private void DealDamage(EntityUid user, DamageSpecifier? damage)
    {
        if (damage is null)
            return;
        // So the original DamageSpecifier will not be changed.
        var newDamage = new DamageSpecifier(damage);
        _damageable.TryChangeDamage(user, newDamage, true);
    }

    /// <summary>
    ///     Gets all cultists/constructs near a glyph.
    /// </summary>
    public HashSet<EntityUid> GatherCultists(EntityUid uid, float range)
    {
        var entities = _lookup.GetEntitiesInRange(Transform(uid).Coordinates, range);
        entities.RemoveWhere(entity => !HasComp<CosmicCultComponent>(entity) || !_mobState.IsAlive(entity) || _container.IsEntityInContainer(entity));
        return entities;
    }

    /// <summary>
    ///     Gets all the humanoids near a glyph.
    /// </summary>
    /// <param name="uid">The glyph.</param>
    /// <param name="range">Radius for a lookup.</param>
    /// <param name="exclude">Filter to exclude from return.</param>
    public HashSet<Entity<HumanoidAppearanceComponent>> GetTargetsNearGlyph(EntityUid uid, float range, Predicate<Entity<HumanoidAppearanceComponent>>? exclude = null)
    {
        var possibleTargets = _lookup.GetEntitiesInRange<HumanoidAppearanceComponent>(Transform(uid).Coordinates, range);
        if (exclude != null)
            possibleTargets.RemoveWhere(exclude);
        possibleTargets.RemoveWhere(target => HasComp<CosmicBlankComponent>(target) || HasComp<CosmicLapseComponent>(target)); // We never want these.

        return possibleTargets;
    }

    #endregion
}
