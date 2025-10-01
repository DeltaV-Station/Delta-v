using Content.Shared.Polymorph;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Abilities.Kitsune;

/// <summary>
/// This component assigns the entity with a polymorph action
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class KitsuneComponent : Component
{
    [DataField] public ProtoId<PolymorphPrototype> KitsunePolymorphId = "KitsuneMorph";

    [DataField] public EntProtoId KitsuneAction = "ActionKitsuneMorph";

    [DataField, AutoNetworkedField] public EntityUid? KitsuneActionEntity;

    /// <summary>
    /// The foxfire prototype to use.
    /// </summary>
    [DataField] public EntProtoId FoxfirePrototype = "Foxfire";

    [DataField] public EntProtoId FoxfireActionId = "ActionFoxfire";

    [DataField, AutoNetworkedField] public EntityUid? FoxfireAction;

    [DataField, AutoNetworkedField] public List<EntityUid> ActiveFoxFires = [];

    [DataField, AutoNetworkedField] public Color? Color;

    /// <summary>
    /// Represents a light coming from a light source.
    /// As such it has its value maximised while not touching hue or saturation.
    /// </summary>
    [DataField, AutoNetworkedField] public Color? ColorLight;
}

[Serializable, NetSerializable]
public enum KitsuneColorVisuals : byte
{
    Color,
    Layer
}
