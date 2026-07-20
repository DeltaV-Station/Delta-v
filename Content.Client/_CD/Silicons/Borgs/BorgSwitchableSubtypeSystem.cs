using System.Linq;
using Content.Client.Silicons.Borgs;
using Content.Shared._CD.Silicons.Borgs;
using Content.Shared.Movement.Components;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Client._CD.Silicons.Borgs;

/// <summary>
/// Primarily handles the appearance aspects of the borg subtype.
/// </summary>
public sealed class BorgSwitchableSubtypeSystem : SharedBorgSwitchableSubtypeSystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly BorgSystem _borg = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BorgSwitchableSubtypeComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<BorgSwitchableSubtypeComponent, AfterAutoHandleStateEvent>(OnAutoHandleEvent);
    }

    private void OnAutoHandleEvent(Entity<BorgSwitchableSubtypeComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        SelectBorgSubtype(ent);
    }

    private void OnComponentStartup(Entity<BorgSwitchableSubtypeComponent> ent, ref ComponentStartup args)
    {
        SelectBorgSubtype(ent);
    }

    protected override void UpdateEntityAppearance(Entity<BorgSwitchableSubtypeComponent> entity, EntityPrototype borgSubtypePrototype)
    {
        // LOT of copy pasted code from BorgSwitchableTypeSystem, but is probably necessary unless the upstream code
        // is refactored

        if (!borgSubtypePrototype.TryGetComponent<BorgSubtypeDefinitionComponent>(out var borgSubtype, ComponentFactory))
            return;

        // get our required components
        var (owner, _) = entity;
        if (!TryComp<SpriteComponent>(entity, out var chassisSprite))
            return;

        // remove all existing layers
        for (int i = chassisSprite.AllLayers.Count() - 1; i >= 0; i--)
        {
            _sprite.RemoveLayer((entity, chassisSprite), i);
        }

        for (int i = 0; i < borgSubtype.LayerData.Length; i++)
        {
            var layerData = borgSubtype.LayerData[i];

            layerData.RsiPath = borgSubtype.SpritePath?.ToString();
            if (borgSubtype.Offset != null)
                layerData.Offset = borgSubtype.Offset;
            _sprite.AddLayer((owner, chassisSprite), layerData, i);
        }

        if (TryComp<BorgChassisComponent>(entity, out var chassis))
        {
            _borg.SetMindStates(
                (entity.Owner, chassis),
                borgSubtype.SpriteHasMindState,
                borgSubtype.SpriteNoMindState);

            if (TryComp(entity, out AppearanceComponent? appearance))
            {
                // Queue update so state changes apply.
                _appearance.QueueUpdate(entity, appearance);
            }
        }

        if (borgSubtype.SpriteBodyMovementState is { } movementState)
        {
            var spriteMovement = EnsureComp<SpriteMovementComponent>(entity);
            spriteMovement.NoMovementLayers.Clear();
            spriteMovement.NoMovementLayers["movement"] = new PrototypeLayerData
            {
                State = borgSubtype.SpriteBodyState,
            };
            spriteMovement.MovementLayers.Clear();
            spriteMovement.MovementLayers["movement"] = new PrototypeLayerData
            {
                State = movementState,
            };
        }
        else
        {
            RemComp<SpriteMovementComponent>(entity);
        }

        base.UpdateEntityAppearance(entity, borgSubtypePrototype);
    }
}
