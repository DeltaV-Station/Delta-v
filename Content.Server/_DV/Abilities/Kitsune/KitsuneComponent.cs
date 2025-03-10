using Content.Shared.Polymorph;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Abilities.Kitsune;

/// <summary>
/// This component assigns the entity with a polymorph action
/// </summary>

[RegisterComponent]
public sealed partial class KitsuneComponent : Component
{
    [DataField] public ProtoId<PolymorphPrototype> KitsunePolymorphId = "KitsuneMorph";

    [DataField] public EntProtoId KitsuneAction = "ActionKitsuneMorph";

    [DataField] public EntityUid? KitsuneActionEntity;

    [DataField] public bool NoAction = false;

    /// <summary>
    /// The foxfire prototype to use.
    /// </summary>
    [DataField]
    public EntProtoId FoxfirePrototype = "Foxfire";
    [DataField]
    public EntProtoId FoxfireActionId = "ActionFoxfire";
    public EntityUid? FoxfireAction;
    public List<EntityUid> ActiveFoxFires = [];

}
