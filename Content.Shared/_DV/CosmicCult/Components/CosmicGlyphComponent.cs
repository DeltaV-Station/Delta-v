using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.CosmicCult.Components;

[RegisterComponent]
public sealed partial class CosmicGlyphComponent : Component
{
    [DataField] public string GlyphName = "base";
    [DataField] public int RequiredCultists = 1;
    [DataField] public float ActivationRange = 1.55f;

    /// <summary>
    ///     Damage dealt on glyph activation.
    /// </summary>
    [DataField] public float ActivationDamage;
    [DataField] public bool CanBeErased = true;
    [DataField] public EntProtoId GylphVFX = "CosmicGenericVFX";
    [DataField] public SoundSpecifier GylphSFX = new SoundPathSpecifier("/Audio/_DV/CosmicCult/glyph_trigger.ogg");
}

public sealed class TryActivateGlyphEvent(EntityUid user, HashSet<EntityUid> cultists) : CancellableEntityEventArgs
{
    public EntityUid User = user;
    public HashSet<EntityUid> Cultists = cultists;
}
