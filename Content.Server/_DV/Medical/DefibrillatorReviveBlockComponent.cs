namespace Content.Server._DV.Medical;

/// <summary>
/// The component added to an entity (mid-round) if they cannot be revived by a defibrillator anymore.
/// </summary>
[RegisterComponent]
public sealed partial class DefibrillatorReviveBlockComponent: Component
{
    /// <summary>
    /// How many zaps are needed to receive the final "patient cannot be revived" message
    /// </summary>
    [ViewVariables]
    public int ZapsNeeded = 1;
}
