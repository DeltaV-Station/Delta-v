using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Psionics.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class DispellableComponent : Component
{
    /// <summary>
    /// The sound that occurs when being dispelled.
    /// </summary>
    [DataField]
    public SoundSpecifier DispelSound = new SoundPathSpecifier("/Audio/Effects/lightburn.ogg");
}
