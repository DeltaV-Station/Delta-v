using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Audio;

namespace Content.Shared._DV.Weapons.Ranged.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class GunDelayComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Delay = 1;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan FireTime = TimeSpan.Zero;

    [DataField]
    public bool Random = false;

    [DataField, AutoNetworkedField]
    public float MinimumDelay = 1;

    [DataField, AutoNetworkedField]
    public float MaximumDelay = 1;

    [DataField, AutoNetworkedField]
    public bool Lit = false;

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
