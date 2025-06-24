using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;

namespace Content.Server._DV.GlimmerWisp;

/// <summary>
/// Adds a verb to drain life from a crit mob that matches a whitelist.
/// Successfully draining a mob rejuvenates you completely.
/// </summary>
[RegisterComponent, Access(typeof(LifeDrainerSystem))]
public sealed partial class LifeDrainerComponent : Component
{
    /// <summary>
    /// Damage to give to the target when draining is complete
    /// </summary>
    [DataField(required: true)]
    public DamageSpecifier Damage = new();

    /// <summary>
    /// Mobs have to match this whitelist to be drained.
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// The time that it takes to drain an entity.
    /// </summary>
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(8.35f);

    /// <summary>
    /// Sound played while draining a mob.
    /// </summary>
    [DataField]
    public SoundSpecifier DrainSound = new SoundPathSpecifier("/Audio/_DV/Effects/clang2.ogg");

    /// <summary>
    /// Sound played after draining is complete.
    /// </summary>
    [DataField]
    public SoundSpecifier FinishSound = new SoundPathSpecifier("/Audio/Effects/guardian_inject.ogg");

    [DataField]
    public EntityUid? DrainStream;

    /// <summary>
    /// A current drain doafter in progress.
    /// </summary>
    [DataField]
    public DoAfterId? DoAfter;

    /// <summary>
    /// What mob is being targeted for draining.
    /// When draining stops the AI will try to drain this target again until successful.
    /// </summary>
    [DataField]
    public EntityUid? Target;
}
