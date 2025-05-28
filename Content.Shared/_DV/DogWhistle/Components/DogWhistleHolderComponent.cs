using Content.Shared._DV.DogWhistle.EntitySystems;

namespace Content.Shared._DV.DogWhistle.Components;

/// <summary>
/// Marks that this entity currently has a dog whistle equipped and may give orders via pointing.
/// Since we're using pointing for this, we rely on this component to mark entities that need to have special
/// handling for their pointing.
/// </summary>
[RegisterComponent]
[Access(typeof(SharedDogWhistleSystem))]
public sealed partial class DogWhistleHolderComponent : Component
{
    /// <summary>
    /// The whistle that this holder has equipped on their body.
    /// </summary>
    public EntityUid Whistle;
}
