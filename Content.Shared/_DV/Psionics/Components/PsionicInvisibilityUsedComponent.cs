using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Psionics.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class PsionicInvisibilityUsedComponent : Component
{
    /// <summary>
    /// The sound that plays when going invisible.
    /// </summary>
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Effects/toss.ogg");
}
