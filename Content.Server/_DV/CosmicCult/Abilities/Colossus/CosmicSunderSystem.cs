using Content.Shared._DV.CosmicCult;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Stunnable;
using Robust.Shared.Timing;

namespace Content.Server._DV.CosmicCult.Abilities;

public sealed class CosmicSunderSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

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
        _stun.TryStun(ent, ent.Comp.AttackWait, true);

        comp.Attacking = true;
        comp.AttackHoldTimer = comp.AttackWait + _timing.CurTime;
        Spawn(comp.Attack1Vfx, args.Target);

        var detonator = Spawn(comp.TileDetonations, args.Target);
        EnsureComp<CosmicTileDetonatorComponent>(detonator, out var detonateComp);
        detonateComp.DetonationTimer = _timing.CurTime;
    }
}
