using Content.Shared._DV.CosmicCult;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Stunnable;
using Robust.Shared.Timing;

namespace Content.Server._DV.CosmicCult.Abilities;

public sealed class CosmicSunderSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SharedStunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicColossusComponent, EventCosmicColossusSunder>(OnColossusSunder);
    }

    private void OnColossusSunder(Entity<CosmicColossusComponent> ent, ref EventCosmicColossusSunder args)
    {
        args.Handled = true;

        var comp = ent.Comp;
        _appearance.SetData(ent, ColossusVisuals.Status, ColossusStatus.Action);
        _transform.SetCoordinates(ent, args.Target);
        _transform.AnchorEntity(ent);
        _stun.TryAddStunDuration(ent, ent.Comp.AttackWait);

        comp.Attacking = true;
        comp.AttackHoldTimer = comp.AttackWait + _timing.CurTime;
        Spawn(comp.Attack1Vfx, args.Target);

        var detonator = Spawn(comp.TileDetonations, args.Target);
        EnsureComp<CosmicTileDetonatorComponent>(detonator, out var detonateComp);
        detonateComp.DetonationTimer = _timing.CurTime;
    }
}
