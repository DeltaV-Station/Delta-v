using Content.Server.Popups;
using Content.Shared._DV.CosmicCult.Components.Examine;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared._DV.CosmicCult;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Systems;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Server._DV.CosmicCult;

public sealed class CosmicGlyphSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedCosmicCultSystem _cosmicCult = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private readonly HashSet<Entity<CosmicCultComponent>> _cultists = [];
    private readonly HashSet<Entity<HumanoidAppearanceComponent>> _humanoids = [];

    public override void Initialize()
    {
        SubscribeLocalEvent<CosmicGlyphComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<CosmicGlyphComponent, ActivateInWorldEvent>(OnUseGlyph);
        SubscribeLocalEvent<CosmicGlyphComponent, ComponentStartup>(OnGlyphCreated);
        SubscribeLocalEvent<CosmicGlyphComponent, EraseGlyphEvent>(EraseGlyph);
    }

    #region Base trigger

    private void OnExamine(Entity<CosmicGlyphComponent> uid, ref ExaminedEvent args)
    {
        if (_cosmicCult.EntityIsCultist(args.Examiner))
        {
            args.PushMarkup(Loc.GetString("cosmic-examine-glyph-cultcount", ("COUNT", uid.Comp.RequiredCultists)));
        }
        else
        {
            args.PushMarkup(Loc.GetString("cosmic-examine-text-glyphs"));
        }
    }

    private void OnGlyphCreated(Entity<CosmicGlyphComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.Timer = _timing.CurTime + ent.Comp.SpawnTime;
    }

    public void EraseGlyph(Entity<CosmicGlyphComponent> ent, ref EraseGlyphEvent args)
    {
        _appearance.SetData(ent, GlyphVisuals.Status, GlyphStatus.Despawning);
        ent.Comp.State = GlyphStatus.Despawning;
        ent.Comp.Timer = _timing.CurTime + ent.Comp.DespawnTime;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var glyphQuery = EntityQueryEnumerator<CosmicGlyphComponent>();
        while (glyphQuery.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime >= comp.Timer && (comp.State == GlyphStatus.Spawning || comp.State == GlyphStatus.Cooldown))
            {
                _appearance.SetData(uid, GlyphVisuals.Status, GlyphStatus.Ready);
                comp.State = GlyphStatus.Ready;
                return;
            }
            if (_timing.CurTime >= comp.Timer && comp.State == GlyphStatus.Active)
            {
                ActivateGlyph(new Entity<CosmicGlyphComponent>(uid, comp));
            }
            if (_timing.CurTime >= comp.Timer && comp.State == GlyphStatus.Despawning)
            {
                QueueDel(uid);
            }
        }
    }

    private void OnUseGlyph(Entity<CosmicGlyphComponent> uid, ref ActivateInWorldEvent args)
    {
        var tgtpos = Transform(uid).Coordinates;
        var userCoords = Transform(args.User).Coordinates;
        if (args.Handled || !userCoords.TryDistance(EntityManager, tgtpos, out var distance) || distance > uid.Comp.ActivationRange || !_cosmicCult.EntityIsCultist(args.User) || uid.Comp.State != GlyphStatus.Ready)
            return;
        var cultists = GatherCultists(uid, uid.Comp.ActivationRange);
        if (cultists.Count < uid.Comp.RequiredCultists)
        {
            _popup.PopupEntity(Loc.GetString("cult-glyph-not-enough-cultists"), uid, args.User);
            return;
        }

        var ev = new CheckGlyphConditionsEvent(args.User, cultists);
        RaiseLocalEvent(uid, ev);
        if (ev.Cancelled) return;

        args.Handled = true;
        uid.Comp.User = args.User;
        _appearance.SetData(uid, GlyphVisuals.Status, GlyphStatus.Active);
        uid.Comp.State = GlyphStatus.Active;
        uid.Comp.Timer = _timing.CurTime + uid.Comp.ActivationTime;
    }

    private void ActivateGlyph(Entity<CosmicGlyphComponent> ent)
    {
        if (!ent.Comp.EraseOnUse)
        {
            _appearance.SetData(ent, GlyphVisuals.Status, GlyphStatus.Cooldown);
            ent.Comp.State = GlyphStatus.Cooldown;
            ent.Comp.Timer = _timing.CurTime + ent.Comp.CooldownTime;
        }

        if (ent.Comp.User is not { } user) return;
        var cultists = GatherCultists(ent, ent.Comp.ActivationRange);
        var tryInvokeEv = new TryActivateGlyphEvent(user, cultists);
        RaiseLocalEvent(ent, tryInvokeEv);
        if (tryInvokeEv.Cancelled || cultists.Count < ent.Comp.RequiredCultists)
        {
            //TODO: SFX and/or VFX for failed activation?
            return;
        }

        var damage = ent.Comp.ActivationDamage / cultists.Count;
        foreach (var cultist in cultists)
        {
            _damageable.TryChangeDamage(cultist, damage, true);
        }

        var tgtpos = Transform(ent).Coordinates;
        _audio.PlayPvs(ent.Comp.GylphSFX, tgtpos, AudioParams.Default.WithVolume(+1f));
        Spawn(ent.Comp.GylphVFX, tgtpos);
        ent.Comp.User = null;
        var ev = new EraseGlyphEvent();
        if (ent.Comp.EraseOnUse) EraseGlyph(ent, ref ev); // This is probably not the correct way to do it.
    }
    #endregion

    #region Housekeeping
    /// <summary>
    ///     Gets all cultists/constructs near a glyph.
    /// </summary>
    public HashSet<Entity<CosmicCultComponent>> GatherCultists(EntityUid uid, float range)
    {
        _cultists.Clear();
        _lookup.GetEntitiesInRange<CosmicCultComponent>(Transform(uid).Coordinates, range, _cultists);
        _cultists.RemoveWhere(entity => !_mobState.IsAlive(entity) || _container.IsEntityInContainer(entity));
        return _cultists;
    }

    /// <summary>
    ///     Gets all the humanoids near a glyph.
    /// </summary>
    /// <param name="uid">The glyph.</param>
    /// <param name="range">Radius for a lookup.</param>
    /// <param name="exclude">Filter to exclude from return.</param>
    public HashSet<Entity<HumanoidAppearanceComponent>> GetTargetsNearGlyph(EntityUid uid, float range, Predicate<Entity<HumanoidAppearanceComponent>>? exclude = null)
    {
        _humanoids.Clear();
        _lookup.GetEntitiesInRange<HumanoidAppearanceComponent>(Transform(uid).Coordinates, range, _humanoids);
        if (exclude != null)
            _humanoids.RemoveWhere(exclude);
        _humanoids.RemoveWhere(target => HasComp<CosmicBlankComponent>(target) || HasComp<CosmicLapseComponent>(target)); // We never want these.

        return _humanoids;
    }
    #endregion
}
