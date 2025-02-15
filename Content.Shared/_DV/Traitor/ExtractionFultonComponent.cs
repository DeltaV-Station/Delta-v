using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Traitor;

/// <summary>
/// Fulton that can be used for traitor extraction and ransom objectives.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedExtractionFultonSystem))]
public sealed partial class ExtractionFultonComponent : Component
{
    [DataField]
    public TimeSpan ApplyDelay = TimeSpan.FromSeconds(3);

    /// <summary>
    /// How long it takes for a stolen item to get sent to the vault.
    /// </summary>
    [DataField]
    public TimeSpan ItemDelay = TimeSpan.FromSeconds(30);

    /// <summary>
    /// How long it takes for a mob to get sent to jail.
    /// </summary>
    [DataField]
    public TimeSpan MobDelay = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Sound that gets played when the fulton is applied.
    /// </summary>
    [DataField]
    public SoundSpecifier? FultonSound = new SoundPathSpecifier("/Audio/Items/Mining/fultext_deploy.ogg");
}
