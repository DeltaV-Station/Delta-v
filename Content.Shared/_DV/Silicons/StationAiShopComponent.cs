using Content.Shared.Actions;
using Content.Shared.Chemistry.Components;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Silicons;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StationAiShopComponent : Component
{
    [DataField]
    public EntProtoId ShopActionId = "ActionStationAiOpenShop";

    [DataField, AutoNetworkedField]
    public EntityUid? ShopAction;
}

public sealed partial class StationAiShopActionEvent : InstantActionEvent;

/// <summary>
/// Toggles the RGB light controller on the given entity
/// </summary>
public sealed partial class StationAiRgbLightingActionEvent : EntityTargetActionEvent;

/// <summary>
/// Replaces the light bulb in the target with the given prototype
/// </summary>
public sealed partial class StationAiLightSynthesizerActionEvent : EntityTargetActionEvent
{
    [DataField(required: true)]
    public EntProtoId BulbPrototype;
    [DataField(required: true)]
    public EntProtoId TubePrototype;
}

/// <summary>
/// Plays the given sound coming from the target entity
/// </summary>
public sealed partial class StationAiPlaySoundActionEvent : EntityTargetActionEvent
{
    [DataField(required: true)]
    public SoundSpecifier Sound;
}

/// <summary>
/// Changes the target entity's health by the given damage specifier
/// </summary>
public sealed partial class StationAiHealthChangeActionEvent : EntityTargetActionEvent
{
    [DataField(required: true)]
    public DamageSpecifier Damage;
}

/// <summary>
/// Spawns the given entity at the target location
/// </summary>
public sealed partial class StationAiSpawnEntityActionEvent : WorldTargetActionEvent
{
    [DataField(required: true)]
    public EntProtoId Entity;
}

/// <summary>
/// Triggers the given smoke effect at the target location
/// </summary>
public sealed partial class StationAiSmokeActionEvent : WorldTargetActionEvent
{
    /// <summary>
    /// How long the smoke stays for, after it has spread.
    /// </summary>
    [DataField]
    public float Duration = 10;

    /// <summary>
    /// How much the smoke will spread.
    /// </summary>
    [DataField(required: true)]
    public int SpreadAmount;

    /// <summary>
    /// Smoke entity to spawn.
    /// Defaults to smoke but you can use foam if you want.
    /// </summary>
    [DataField]
    public EntProtoId<SmokeComponent> SmokePrototype = "Smoke";

    /// <summary>
    /// Solution to add to each smoke cloud.
    /// </summary>
    /// <remarks>
    /// When using repeating trigger this essentially gets multiplied so dont do anything crazy like omnizine or lexorin.
    /// </remarks>
    [DataField]
    public Solution Solution = new();
}
