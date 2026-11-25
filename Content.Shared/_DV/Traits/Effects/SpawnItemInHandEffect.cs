using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Traits.Effects;

/// <summary>
/// Effect that spawns an item and attempts to place it in the player's hand.
/// If the player cannot hold the item, it is spawned at their feet.
/// </summary>
public sealed partial class SpawnItemInHandEffect : BaseTraitEffect
{
    /// <summary>
    /// The entity prototype to spawn.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId Item = string.Empty;

    public override void Apply(TraitEffectContext ctx)
    {
        // This effect needs to be applied server-side where we have access to
        // SharedHandsSystem. The actual spawning logic is handled by the server TraitSystem.
        // This class just holds the data.
    }
}
