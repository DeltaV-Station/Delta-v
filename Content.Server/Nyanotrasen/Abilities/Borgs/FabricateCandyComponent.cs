using Robust.Shared.Audio;

namespace Content.Server.Abilities.Borgs;

[RegisterComponent]
public sealed partial class FabricateCandyComponent : Component
{
    [DataField]
    public EntityUid? LollipopAction;

    [DataField]
    public EntityUid? GumballAction;

    /// <summary>
    /// The sound played when fabricating candy.
    /// </summary>
    [DataField]
    public SoundSpecifier FabricationSound = new SoundPathSpecifier("/Audio/Machines/machine_vend.ogg")
    {
        Params = new AudioParams
        {
            Volume = -2f
        }
    };
}
