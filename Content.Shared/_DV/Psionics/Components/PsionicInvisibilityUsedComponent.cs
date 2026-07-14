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

    /// <summary>
    /// They are fully invisible. This only makes their sprite appear shaded so they know they're invisible.
    /// </summary>
    public float InvisibilityStrength = 0.66f;

    /// <summary>
    /// How long the user will be stunned when leaving invisibility.
    /// </summary>
    public TimeSpan StunDuration = TimeSpan.FromSeconds(4);
}
