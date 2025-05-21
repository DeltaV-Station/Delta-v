using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._DV.CosmicCult.Components;

/// <summary>
/// Component for Cosmic Cult's entropic colossus.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CosmicColossusComponent : Component
{
    [DataField] public SoundSpecifier DeathSFX = new SoundPathSpecifier("/Audio/Animals/space_dragon_roar.ogg");

    [DataField] public SoundSpecifier TileSFX = new SoundPathSpecifier("/Audio/_DV/CosmicCult/tile_detonate.ogg");

    [DataField] public SoundSpecifier IngressSFX = new SoundPathSpecifier("/Audio/_DV/CosmicCult/ability_ingress.ogg");

    [DataField] public SoundSpecifier DoAfterSFX = new SoundPathSpecifier("/Audio/Machines/airlock_creaking.ogg");

    [DataField] public EntProtoId CultVFX = "CosmicGenericVFX";

    [DataField] public TimeSpan IngressDoAfter = TimeSpan.FromSeconds(7);

    [DataField] public TimeSpan ReleaseDelay = TimeSpan.FromSeconds(1.5);
    [DataField] public TimeSpan Cleanup = TimeSpan.FromSeconds(5);
}

[Serializable, NetSerializable]
public enum ColossusVisuals : byte
{
    Status,
}

[Serializable, NetSerializable]
public enum ColossusStatus : byte
{
    Alive,
    Dead,
    Attacking,
}
