using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Abilities;

/// <summary>
/// Gives the player an action to sneak under tables at a slower move speed.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CrawlUnderObjectsComponent : Component
{
    [DataField]
    public EntityUid? ToggleHideAction;

    [DataField(required: true)]
    public EntProtoId ActionProto;

    [DataField, AutoNetworkedField]
    public bool Enabled;

    /// <summary>
    ///     List of fixtures that had their collision mask changed.
    ///     Required for re-adding the collision mask.
    /// </summary>
    [DataField]
    public List<(string key, int originalMask)> ChangedFixtures = new();

    [DataField]
    public float SneakSpeedModifier = 0.7f;
}

[Serializable, NetSerializable]
public enum SneakingVisuals : byte
{
    Sneaking
}

public sealed partial class ToggleCrawlingStateEvent : InstantActionEvent;

[ByRefEvent]
public readonly record struct CrawlingUpdatedEvent(bool Enabled, CrawlUnderObjectsComponent Comp);
