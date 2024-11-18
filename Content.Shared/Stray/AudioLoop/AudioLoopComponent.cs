//Audio/Items/drill_use.ogg
using System;
using System.Collections.Generic;
using System.Numerics;
using Robust.Shared.Audio.Effects;
using Robust.Shared.Audio.Sources;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameObjects;
using Content.Shared.Stray.AudioLoop;
using Robust.Shared.GameStates;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.ViewVariables;

namespace Content.Shared.Stray.AudioLoop;

/// <summary>
/// Stores the audio data for an audio entity.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), Access(typeof(SharedAudioLoopSystem))]
public sealed partial class AudioLoopComponent : Component
{
    [DataField("sound",required: true)]
    public SoundSpecifier sound { get; private set; }
    public EntityUid ent;
    public AudioComponent auC;
    [AutoNetworkedField]
    public bool act = false;
}
