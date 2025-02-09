using Robust.Shared.GameStates;

namespace Content.Shared._DV.Traitor;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedSyndicateFultonSystem))]
public sealed class SyndicateFultonComponent : Component
{
    /// <summary>
    /// Kidnaps people instead of extracting items.
    /// Controls which objective to complete.
    /// </summary>
    [DataField]
    public bool Sophont;
}
