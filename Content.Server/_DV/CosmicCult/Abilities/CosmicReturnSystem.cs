using Content.Server._DV.CosmicCult.Components;
using Content.Shared._DV.CosmicCult;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared._DV.CosmicCult.Components.Examine;
using Content.Shared.Damage;
using Content.Shared.Mind;
using Content.Shared.Stunnable;

namespace Content.Server._DV.CosmicCult.Abilities;

public sealed class CosmicReturnSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicAstralBodyComponent, EventCosmicReturn>(OnCosmicReturn);
        SubscribeLocalEvent<CosmicGlyphAstralProjectionComponent, TryActivateGlyphEvent>(OnAstralProjGlyph);
    }

    private void OnAstralProjGlyph(Entity<CosmicGlyphAstralProjectionComponent> uid, ref TryActivateGlyphEvent args)
    {
        _damageable.TryChangeDamage(args.User, uid.Comp.ProjectionDamage, true);
        var projectionEnt = Spawn(uid.Comp.SpawnProjection, Transform(uid).Coordinates);
        if (_mind.TryGetMind(args.User, out var mindId, out var _))
            _mind.TransferTo(mindId, projectionEnt);
        EnsureComp<CosmicMarkBlankComponent>(args.User);
        EnsureComp<CosmicAstralBodyComponent>(projectionEnt, out var astralComp);
        var mind = Comp<MindComponent>(mindId);
        mind.PreventGhosting = true;
        astralComp.OriginalBody = args.User;
        _stun.TryKnockdown(args.User, TimeSpan.FromSeconds(2), true);
    }

    private void OnCosmicReturn(Entity<CosmicAstralBodyComponent> uid, ref EventCosmicReturn args) //This action is exclusive to the Glyph-created Astral Projection, and allows the user to return to their original body.
    {
        if (_mind.TryGetMind(args.Performer, out var mindId, out var _))
            _mind.TransferTo(mindId, uid.Comp.OriginalBody);
        var mind = Comp<MindComponent>(mindId);
        mind.PreventGhosting = false;
        QueueDel(uid);
        RemComp<CosmicMarkBlankComponent>(uid.Comp.OriginalBody);
    }
}
