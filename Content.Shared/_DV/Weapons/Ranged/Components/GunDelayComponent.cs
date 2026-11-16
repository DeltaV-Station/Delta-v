using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Audio;

namespace Content.Shared._DV.Weapons.Ranged.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class GunDelayComponent : Component
{
    [DataField, AutoNetworkedField]
    public float? Delay;

    /// <summary>
    /// When the gun will fire
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan FireTime = TimeSpan.Zero;

    [DataField, AutoNetworkedField]
    public float MinimumDelay = 1;

    [DataField, AutoNetworkedField]
    public float MaximumDelay = 1;

    /// <summary>
    /// Has the gun been "Lit" meaning that extra attempts should be blocked to not change the fire time?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Lit = false;

    /// <summary>
    /// Should the gun be allowed to fire?
    /// This is only set to true when the system calls AttemptShoot so it doesn't block it
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Firing = false;

    [DataField, AutoNetworkedField]
    public EntityUid? User;

    [DataField]
    public SoundSpecifier StartSound = new SoundPathSpecifier("/Audio/Items/Flare/flare_on.ogg");

    [DataField]
    public SoundSpecifier LoopSound = new SoundPathSpecifier("/Audio/Items/Flare/flare_burn.ogg");

    [DataField]
    public EntityUid? PlayingSound;
}
