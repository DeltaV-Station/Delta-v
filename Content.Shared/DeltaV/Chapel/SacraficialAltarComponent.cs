using Content.Shared.Destructible.Thresholds;
using Content.Shared.DoAfter;
using Content.Shared.EntityTable;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.DeltaV.Chapel;

/// <summary>
/// Altar that lets you sacrafice psionics to lower glimmer by a large amount.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedSacraficialAltarSystem))]
public sealed partial class SacraficialAltarComponent : Component
{
    /// <summary>
    /// DoAfter for an active sacrafice.
    /// </summary>
    [DataField]
    public DoAfterId? DoAfter;

    /// <summary>
    /// How long it takes to sacrafice someone once they die.
    /// This is the window to interrupt a sacrafice if you want glimmer to stay high, or need the psionic to be revived.
    /// </summary>
    [DataField]
    public TimeSpan SacraficeTime = TimeSpan.FromSeconds(8.35);

    [DataField]
    public SoundSpecifier SacraficeSound = new SoundPathSpecifier("/Audio/DeltaV/Effects/clang2.ogg");

    [DataField]
    public EntityUid? SacraficeStream;

    /// <summary>
    /// Random amount to reduce glimmer by.
    /// </summary>
    [DataField]
    public MinMax GlimmerReduction = new(30, 60);

    [DataField]
    public ProtoId<EntityTablePrototype> RewardPool = "PsionicSacraficeRewards";
}

[Serializable, NetSerializable]
public sealed partial class SacraficeDoAfterEvent : SimpleDoAfterEvent;
