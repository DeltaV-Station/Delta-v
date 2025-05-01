namespace Content.Shared._DV.Traitor;

/// <summary>
/// Added to an entity being extracted with a syndie fulton.
/// Used to control whos objectives get completed.
/// </summary>
[RegisterComponent, Access(typeof(SharedExtractionFultonSystem))]
public sealed partial class ExtractingComponent : Component
{
    /// <summary>
    /// Mind of the player that extracted it.
    /// </summary>
    [DataField]
    public EntityUid? Mind;
}
