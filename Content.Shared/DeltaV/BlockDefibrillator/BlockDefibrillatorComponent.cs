using Robust.Shared.GameStates;

namespace Content.Shared.DeltaV.BlockDefibrillator;

[RegisterComponent, NetworkedComponent]
public sealed partial class BlockDefibrillatorComponent : Component
{

    [DataField]
    public string OnBlockPopupMessage = "The defibrillator cannot find a pulse through the armor!";
}
