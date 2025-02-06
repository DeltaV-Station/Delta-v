using Content.Shared._DV.Biscuit;

namespace Content.Server._DV.Biscuit;

[RegisterComponent]
public sealed partial class BiscuitComponent : SharedBiscuitComponent
{
    [DataField]
    public bool Cracked { get; set; }
}
