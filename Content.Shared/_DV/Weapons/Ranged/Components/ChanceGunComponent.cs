using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.Weapons.Ranged.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class ChanceGunComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Broken = false;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan FireTime = TimeSpan.Zero;

    [DataField, AutoNetworkedField]
    public bool Lit = false;

    [DataField, AutoNetworkedField]
    public bool Fired = false;

    [DataField, AutoNetworkedField]
    public EntityUid? User;

    [DataField]
    public SoundSpecifier StartSound = new SoundPathSpecifier("/Audio/Items/Flare/flare_on.ogg");

    [DataField]
    public SoundSpecifier LoopSound = new SoundPathSpecifier("/Audio/Items/Flare/flare_burn.ogg");

    [DataField]
    public EntityUid? PlayingSound;
}
