using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Pager;

/// <summary>
///     Sends out a page when pressed.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(PagerButtonSystem))]
public sealed partial class PagerButtonComponent : Component
{
    [DataField]
    public SoundSpecifier PressSound;
}
