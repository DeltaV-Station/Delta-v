using System.Linq;
using System.Numerics;
using Content.Client.Stealth;
using Content.Shared._DV.Overlays.Components;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Stealth.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client._Goobstation.Overlays;

public sealed class SharkVisionOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private readonly TransformSystem _transform;
    private readonly StealthSystem _stealth;
    private readonly ContainerSystem _container;
    private readonly SharedSolutionContainerSystem _solution;
    private readonly SpriteSystem _sprite;

    public override bool RequestScreenTexture => true;
    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    private readonly List<SharkVisionRenderEntry> _entries = [];

    private EntityUid? _lightEntity;

    public float LightRadius;

    public SharkVisionComponent? Comp;

    public SharkVisionOverlay()
    {
        IoCManager.InjectDependencies(this);

        _container = _entity.System<ContainerSystem>();
        _transform = _entity.System<TransformSystem>();
        _stealth = _entity.System<StealthSystem>();
        _solution = _entity.System<SharedSolutionContainerSystem>();
        _sprite = _entity.System<SpriteSystem>();

        ZIndex = -1;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture is null || Comp is null)
            return;

        var worldHandle = args.WorldHandle;
        var eye = args.Viewport.Eye;

        if (eye == null)
            return;

        var player = _player.LocalEntity;

        if (!_entity.TryGetComponent(player, out TransformComponent? playerXform))
            return;

        var accumulator = Math.Clamp(Comp.PulseAccumulator, 0f, Comp.PulseTime);
        var alpha = Comp.PulseTime <= 0f ? 1f : float.Lerp(1f, 0f, accumulator / Comp.PulseTime);

        var mapId = eye.Position.MapId;
        var eyeRot = eye.Rotation;

        GetVisionEntities(Comp.BloodPrototypes, mapId, eyeRot);

        foreach (var entry in _entries)
        {
            Render(entry.Ent, entry.Map, worldHandle, entry.EyeRot, Comp.Color, alpha);
        }

        worldHandle.SetTransform(Matrix3x2.Identity);
    }

    private void GetVisionEntities(ProtoId<ReagentPrototype>[] bloodPrototypes, MapId mapId, Angle eyeRot)
    {
        _entries.Clear();
        var entities = _entity.EntityQueryEnumerator<SolutionContainerManagerComponent, SpriteComponent, TransformComponent>();
        while (entities.MoveNext(out var uid, out var body, out var sprite, out var xform))
        {
            if (!CanSee(uid, sprite))
                continue;

            // Luckily, players are also SolutionContainerManagerComponent, so let's check for a bloodstream and check for bleeds
            if (_entity.TryGetComponent<BloodstreamComponent>(uid, out var bloodstream) && bloodstream.BleedAmount <= 0)
                continue;

            // Should always be true but I'm just simplifying a future if statement
            if (!_entity.TryGetComponent<SolutionContainerManagerComponent>(uid, out var solutionContainer))
                continue;

            if (solutionContainer.Containers == null)
                continue;

            var bloodFound = false;
            // Someone calculate the Big O notation of this lmao
            foreach (var individualContainer in solutionContainer.Containers)
            {
                if (_solution.TryGetSolution((uid, solutionContainer), individualContainer, out var _, out var solution))
                {
                    var reagentsInContainer = solution.GetReagentPrototypes(_proto).ToDictionary();
                    bloodFound = reagentsInContainer.Keys.Any(reagentKey => bloodPrototypes.Any(x => reagentKey == x));
                }

                if (bloodFound)
                    break; // We don't need to keep searching if the entity has blood in at least one container
            }

            if (!bloodFound)
                continue;

            var entity = uid;
            // Parent container check
            if (_container.TryGetOuterContainer(uid, xform, out var container))
            {
                var owner = container.Owner;

                if (_entity.TryGetComponent<SpriteComponent>(owner, out var ownerSprite)
                    && _entity.TryGetComponent<TransformComponent>(owner, out var ownerXform))
                {
                    entity = owner;
                    sprite = ownerSprite;
                    xform = ownerXform;
                }
            }

            if (_entries.Any(e => e.Ent.Owner == entity))
                continue;

            _entries.Add(new SharkVisionRenderEntry((entity, sprite, xform), mapId, eyeRot));
        }
    }

    private void Render(Entity<SpriteComponent, TransformComponent> ent,
        MapId? map,
        DrawingHandleWorld handle,
        Angle eyeRot,
        Color color,
        float alpha)
    {
        var (uid, sprite, xform) = ent;
        if (xform.MapID != map || !CanSee(uid, sprite))
            return;

        var position = _transform.GetWorldPosition(xform);
        var rotation = _transform.GetWorldRotation(xform);

        var originalColor = sprite.Color;
        _sprite.SetColor((ent, ent.Comp1), color.WithAlpha(alpha));
        _sprite.RenderSprite((ent, ent.Comp1), handle, eyeRot, rotation, position);
        _sprite.SetColor((ent, ent.Comp1), originalColor);
    }

    private bool CanSee(EntityUid uid, SpriteComponent sprite)
    {
        return sprite.Visible && (!_entity.TryGetComponent(uid, out StealthComponent? stealth) ||
                                  _stealth.GetVisibility(uid, stealth) > 0.5f);
    }

    public void ResetLight(bool checkFirstTimePredicted = true)
    {
        if (_lightEntity == null || checkFirstTimePredicted && !_timing.IsFirstTimePredicted)
            return;

        _entity.DeleteEntity(_lightEntity);
        _lightEntity = null;
    }
}

public record struct SharkVisionRenderEntry(
    Entity<SpriteComponent, TransformComponent> Ent,
    MapId? Map,
    Angle EyeRot);
