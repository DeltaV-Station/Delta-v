using Content.Shared._DV.DogWhistle.EntitySystems;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.DogWhistle.Components;

/// <summary>
/// Marks that this entity creates dog whistle sounds when used.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedDogWhistleSystem))]
public sealed partial class DogWhistleComponent : Component
{
    /// <summary>
    /// The sound to emit when the whistle is used.
    /// </summary>
    [DataField]
    public SoundSpecifier Sound = new SoundCollectionSpecifier("TrenchWhistle");

    /// <summary>
    /// Action to use when this whistle is equipped by a user.
    /// </summary>
    [DataField]
    public EntProtoId ToggleAction = "ActionToggleDogWhistle";

    /// <summary>
    /// Entity created for this action when equipped.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? ToggleActionEntid = null;
}
