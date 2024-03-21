using Content.Shared.Tag;
using Robust.Shared.Audio;

namespace Content.Shared.DeltaV.Gavel;

/// <summary>
/// A gavel block can emit a hitting sound when interacted with using a gavel.
/// </summary>
[RegisterComponent]
public sealed partial class GavelBlockComponent : Component
{
    [ValidatePrototypeId<TagPrototype>]
    public const string GavelTag = "Gavel";

    [DataField("gavelSound"), ViewVariables(VVAccess.ReadOnly)]
    public SoundSpecifier GavelSound = new SoundPathSpecifier("/Audio/DeltaV/Items/gavel.ogg");
}
