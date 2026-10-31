using Content.Shared.Chat.Prototypes;
using Content.Shared.Damage;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._starcup.Damage;

[RegisterComponent, Access(typeof(CriticalDamageSystem)), AutoGenerateComponentPause]
public sealed partial class CriticalDamageComponent : Component
{
    /// <summary>
    /// The damage that the mob will regularly take while in critical condition.
    /// </summary>
    [DataField(required: true)]
    [ViewVariables(VVAccess.ReadWrite)]
    public DamageSpecifier Damage = default!;

    /// <summary>
    /// The next time that this mob will process damage.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextUpdate;

    /// <summary>
    /// The interval between updates
    /// </summary>
    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(2);

    [DataField]
    public TimeSpan EmoteCooldown = TimeSpan.FromSeconds(8);

    [ViewVariables]
    public TimeSpan LastEmoteTime;

    /// <summary>
    ///     The emote that intermittently plays while in crit.
    /// </summary>
    [DataField]
    public ProtoId<EmotePrototype> Emote;

    /// <summary>
    /// How many cycles in a row has the mob been taking critical damage?
    /// </summary>
    [ViewVariables]
    public int CritDamageCycles = 0;
}
