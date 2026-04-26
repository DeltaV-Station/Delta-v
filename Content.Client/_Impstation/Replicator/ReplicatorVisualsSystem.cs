using Content.Client.DamageState;
using Content.Shared.CombatMode;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared._Impstation.Replicator;
using Robust.Client.GameObjects;

namespace Content.Client._Impstation.Replicator;

public sealed class ReplicatorVisualsSystem : EntitySystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ReplicatorComponent, AppearanceChangeEvent>(OnAppearanceChange);
        SubscribeLocalEvent<ReplicatorComponent, ToggleCombatActionEvent>(OnToggleCombat);
        SubscribeLocalEvent<ReplicatorComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnToggleCombat(Entity<ReplicatorComponent> ent, ref ToggleCombatActionEvent args)
    {
        if (TryComp<SpriteComponent>(ent, out var sprite))
            _appearance.OnChangeData(ent, sprite);
    }

    private void OnAppearanceChange(Entity<ReplicatorComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null || !TryComp<CombatModeComponent>(ent, out var combat))
            return;

        if (!args.Sprite.LayerMapTryGet(ReplicatorVisuals.Combat, out var layerIndex) ||
            !args.Sprite.LayerMapTryGet(DamageStateVisualLayers.Base, out var baseIndex))
            return;

        if (!args.Sprite.TryGetLayer(layerIndex, out var combatLayer) ||
            !args.Sprite.TryGetLayer(baseIndex, out var baseLayer))
            return;

        args.Sprite.LayerSetVisible(layerIndex, _mobState.IsAlive(ent) && combat.IsInCombatMode);
        combatLayer.SetAnimationTime(baseLayer.AnimationTime);
        combatLayer.AnimationFrame = baseLayer.AnimationFrame;
        combatLayer.AnimationTimeLeft = baseLayer.AnimationTimeLeft;
    }

    private void OnMobStateChanged(Entity<ReplicatorComponent> ent, ref MobStateChangedEvent args)
    {
        if (TryComp<SpriteComponent>(ent, out var sprite))
            _appearance.OnChangeData(ent, sprite);
    }
}
