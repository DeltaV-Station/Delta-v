using System.ComponentModel.DataAnnotations;
using Content.Shared.Chat.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio;

namespace Content.Shared._EstacaoPirata.EmitBuzzWhileDamaged;

/// <summary>
/// This is used for controlling the cadence of the buzzing emitted by EmitBuzzOnCritSystem.
/// This component is used by mechanical species that can get to critical health.
/// </summary>
[RegisterComponent]
public sealed partial class EmitBuzzWhileDamagedComponent : Component
{
    [DataField("BuzzEmoteCooldown")]
    public TimeSpan BuzzEmoteCooldown { get; private set; } = TimeSpan.FromSeconds(8);

    [ViewVariables]
    public TimeSpan LastBuzzEmoteTime;

    [DataField]
    public ProtoId<EmotePrototype> BuzzEmote = "Bzzzzt...";

    [DataField("cycleDelay")]
    public float CycleDelay = 2.0f;

    public float AccumulatedFrametime;

    [DataField("sound")]
    public SoundSpecifier Sound = new SoundCollectionSpecifier("buzzes");
}
