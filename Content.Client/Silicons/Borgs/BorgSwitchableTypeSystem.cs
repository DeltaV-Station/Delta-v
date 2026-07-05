using Content.Client.PDA; // DeltaV
using Content.Shared._CD.Silicons.Borgs; // CosmicDrift
using Content.Shared.Movement.Components;
using Content.Shared.Silicons.Borgs;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Client.GameObjects;
using Robust.Client.ResourceManagement;  // CosmicDrift
using Robust.Shared.Serialization.TypeSerializers.Implementations; // CosmicDrift
using Robust.Shared.Timing; // CosmicDrift

namespace Content.Client.Silicons.Borgs;

/// <summary>
/// Client side logic for borg type switching. Sets up primarily client-side visual information.
/// </summary>
/// <seealso cref="SharedBorgSwitchableTypeSystem"/>
/// <seealso cref="BorgSwitchableTypeComponent"/>
public sealed partial class BorgSwitchableTypeSystem : SharedBorgSwitchableTypeSystem // DeltaV - made partial
{
    [Dependency] private readonly BorgSystem _borgSystem = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly IGameTiming _timing = default!; // CosmicDrift - borg subtypes

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BorgSwitchableTypeComponent, AfterAutoHandleStateEvent>(AfterStateHandler);
        SubscribeLocalEvent<BorgSwitchableTypeComponent, ComponentStartup>(OnComponentStartup);
    }

    private void OnComponentStartup(Entity<BorgSwitchableTypeComponent> ent, ref ComponentStartup args)
    {
        UpdateEntityAppearance(ent);
    }

    private void AfterStateHandler(Entity<BorgSwitchableTypeComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        UpdateEntityAppearance(ent);
    }

    protected override void UpdateEntityAppearance(
        Entity<BorgSwitchableTypeComponent> entity,
        BorgTypePrototype prototype)
    {
        // Begin Afterlight Addition - added checks to stop sprite state errors
        if (!_timing.IsFirstTimePredicted)
            return;

        if (TryComp<BorgSwitchableSubtypeComponent>(entity, out var subtype) &&
            subtype.BorgSubtype != null)
        {
            var ev = new TypeTryingToUpdateVisualsEvent();
            RaiseLocalEvent(entity, ref ev);
            return;
        }
        // End Afterlight Additions - added checks to stop sprite state errors
        if (TryComp(entity, out SpriteComponent? sprite))
        {
            _sprite.LayerSetRsiState((entity, sprite), BorgVisualLayers.Body, prototype.SpriteBodyState);
            _sprite.LayerSetRsiState((entity, sprite), BorgVisualLayers.LightStatus, prototype.SpriteToggleLightState);
        }

        if (TryComp(entity, out BorgChassisComponent? chassis))
        {
            _borgSystem.SetMindStates(
                (entity.Owner, chassis),
                prototype.SpriteHasMindState,
                prototype.SpriteNoMindState);

            if (TryComp(entity, out AppearanceComponent? appearance))
            {
                // Queue update so state changes apply.
                _appearance.QueueUpdate(entity, appearance);
            }
        }

        // DeltaV - borg pdas
        if (TryComp<PdaBorderColorComponent>(entity, out var pdaBorders))
        {
            pdaBorders.BorderColor = prototype.PdaBorderColor ?? pdaBorders.BorderColor;
            pdaBorders.AccentHColor = prototype.PdaAccentHorizontalColor ?? pdaBorders.AccentHColor;
            pdaBorders.AccentVColor = prototype.PdaAccentVerticalColor ?? pdaBorders.AccentVColor;
        }
        // DeltaV - borg pdas

        // Start CosmicDrift Changes - borg subtypes
        if (prototype.SpriteBodyMovementState is { } movementState)
        {
            var spriteMovement = EnsureComp<SpriteMovementComponent>(entity);
            spriteMovement.NoMovementLayers.Clear();
            spriteMovement.NoMovementLayers["movement"] = new PrototypeLayerData
            {
                State = prototype.SpriteBodyState,
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
        // End CosmicDrift Changes - borg subtypes
        base.UpdateEntityAppearance(entity, prototype);
    }
}
