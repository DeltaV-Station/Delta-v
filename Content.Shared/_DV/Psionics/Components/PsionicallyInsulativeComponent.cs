using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Psionics.Components;

/// <summary>
/// Clothing with this component insulates the wearer upon being worn.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PsionicallyInsulativeComponent : Component
{
    /// <summary>
    /// Whether the wearer can still use their own psionics.
    /// </summary>
    /// <example>The mantis' jacket is insulated and allows passthrough, which protects them from psionics while allowing them to use their own.</example>
    [DataField]
    public bool AllowsPsionicUsage;

    /// <summary>
    /// Whether the wearer is shielded from psionic influences.
    /// </summary>
    /// <example>The security headcage stops the wearer from using their own psionics, but can still be affected by others.</example>
    [DataField]
    public bool ShieldsFromPsionics = true;

    /// <summary>
    /// Whether it's active when it's equipped in pockets.
    /// </summary>
    [DataField]
    public bool ActiveInPocket;

    /// <summary>
    /// If yes, it'll be destroyed in a noöspheric fry event.
    /// </summary>
    [DataField]
    public bool CanBeFried;

    /// <summary>
    /// What sound it makes when a noöspheric fry event happens.
    /// </summary>
    [DataField]
    public SoundSpecifier? FrySound = new SoundPathSpecifier("/Audio/Effects/lightburn.ogg");
}
