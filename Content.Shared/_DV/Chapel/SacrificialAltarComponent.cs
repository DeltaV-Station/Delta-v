using Content.Shared.Destructible.Thresholds;
using Content.Shared.DoAfter;
using Content.Shared.EntityTable;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Chapel;

/// <summary>
/// Altar that lets you sacrifice psionics to lower glimmer by a large amount.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedSacrificialAltarSystem))]
public sealed partial class SacrificialAltarComponent : Component
{
    /// <summary>
    /// DoAfter for an active sacrifice.
    /// </summary>
    [DataField]
    public DoAfterId? DoAfter;

    /// <summary>
    /// How long it takes to sacrifice someone once they die.
    /// This is the window to interrupt a sacrifice if you want glimmer to stay high, or need the psionic to be revived.
    /// </summary>
    [DataField]
    public TimeSpan SacrificeTime = TimeSpan.FromSeconds(8.35);

    [DataField]
    public SoundSpecifier SacrificeSound = new SoundPathSpecifier("/Audio/_DV/Effects/clang2.ogg");

    [DataField]
    public EntityUid? SacrificeStream;

    /// <summary>
    /// Random amount to reduce glimmer by.
    /// </summary>
    [DataField]
    public MinMax GlimmerReduction = new(30, 60);

    [DataField]
    public ProtoId<EntityTablePrototype> RewardPool = "PsionicSacrificeRewards";
}

[Serializable, NetSerializable]
public sealed partial class SacrificeDoAfterEvent : SimpleDoAfterEvent;
